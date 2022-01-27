using Molinos.Orquest.DriversImpl.ALPR.Models;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Molinos.Orquest.ModuloALPR.ALPR
{
    public class Alpr : IDisposable
    {
        private const int REALLY_BIG_PIXEL_WIDTH = 999999999;
        private bool is_initialized = false;
        private string country;
        private string config_file;
        private string runtime_dir;
        private string license_key;
        private int hardware_acceleration;
        private int gpu_id;
        private int batch_size;
        private IntPtr native_instance;
        public ILogger Log { get; set; }

        /// <summary>
        /// OpenALPR library constructor. The object must be initialized separately with a call to Initialize()
        /// </summary>
        /// <param name="country">Country used for recognition.  It may be a single country (e.g., "us") or a comma separated list</param>
        /// <param name="config_file">The path to the openalpr.conf file.  Leave it blank to use the file in the current directory.</param>
        /// <param name="runtime_dir">The path to the runtime_data directory.  Leave it blank to use the runtime_data in the current directory</param>
        /// <param name="license_key">The path to the license key. Leave blank to use licence key in system variable</param>
        /// <param name="hardware_acceleration">Indicates whether to use CPU or GPU</param>
        /// <param name="gpu_id">ID of the GPU to be used</param>
        /// <param name="batch_size">Batch size for GPU processing. Only need to change if using GPU</param>
        public Alpr(string country, ILogger log, string config_file = "", string runtime_dir = "", string license_key = "", int hardware_acceleration = 0, int gpu_id = 0, int batch_size = 1)
        {
            this.country = country;
            this.config_file = config_file;
            this.runtime_dir = runtime_dir;
            this.license_key = license_key;
            this.hardware_acceleration = hardware_acceleration;
            this.gpu_id = gpu_id;
            this.batch_size = batch_size;
            this.Log = log;
        }

        ~Alpr()
        {
            Dispose();
        }

        /// <summary>
        /// Release the memory associated with OpenALPR when you are done using it
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

            // Deinitialize the library
            if (this.is_initialized)
            {
                try
                {
                    // Destroy the native obj
                    openalpr_cleanup(native_instance);
                }
                catch (System.DllNotFoundException)
                {
                    // Ignore.  The library couldn't possibly be loaded anyway
                }
            }
            this.is_initialized = false;
        }
        /// <summary>
        /// Load the OpenALPR recognition data into memory.
        /// Initialization may take several seconds to load all of the various LP recognition data into memory
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool Initialize()
        {
            // Initialize the library

            if (is_initialized)
            {
                // Idempotent behavior
                return true;
            }

            this.is_initialized = true;

            try
            {
                String path = Path.Combine(AssemblyDirectory + "\\..\\ALPR\\dll");

                SetDllDirectory(path);

                if (hardware_acceleration == 0)
                {
                    native_instance = openalpr_init(country, config_file, runtime_dir, license_key);
                }
                else
                {
                    native_instance = openalpr_init_gpu(country, config_file, runtime_dir, license_key, hardware_acceleration, gpu_id, batch_size);
                }

                openalpr_set_detect_region(native_instance, 1);

            }
            catch (System.DllNotFoundException e)
            {
                Log.Error(e,"Could not find/load native library (libopenalpr.dll)");
                this.is_initialized = false;
                return false;
            }
            catch (Exception e)
            {
                this.is_initialized = false;
                Log.Error(e.Message);
            }

            return IsLoaded();
        }



        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        /// <summary>
        /// Verifies that the OpenALPR library has been initialized
        /// </summary>
        /// <returns>true if the library is ready to use, false if it has not been initialized to read plates</returns>
        public bool IsLoaded()
        {
            if (!this.is_initialized)
            {
                return false;
            }

            try
            {
                return openalpr_is_loaded(this.native_instance) != 0;
            }
            catch (System.DllNotFoundException e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        // Checks if the instance can receive commands (e.g., is initialized)
        // If not, it throws an exception
        private void check_initialization()
        {
            if (!IsLoaded())
            {
                Log.Error("OpenALPR Library cannot execute this request since it has not been initialized.");
                throw new InvalidOperationException("OpenALPR Library cannot execute this request since it has not been initialized.");
            }

        }

        /// <summary>
        /// Recognizes an encoded image (e.g., JPG, PNG, etc) provided as an array of bytes
        /// </summary>
        /// <param name="image_bytes">The raw bytes that compose the image</param>
        /// <returns>An AlprResponse object that contains the results of the recognition</returns>
        /// <exception cref="InvalidOperationException">Thrown when the library has not been initialized</exception>
        public AlprResponse Recognize(byte[] image_bytes)
        {
            check_initialization();

            NativeROI roi;
            roi.x = 0;
            roi.y = 0;
            roi.width = REALLY_BIG_PIXEL_WIDTH;
            roi.height = REALLY_BIG_PIXEL_WIDTH;

            IntPtr unmanagedArray = Marshal.AllocHGlobal(image_bytes.Length);
            Marshal.Copy(image_bytes, 0, unmanagedArray, image_bytes.Length);

            IntPtr resp_ptr = openalpr_recognize_encodedimage(this.native_instance, unmanagedArray, image_bytes.Length, roi);

            Marshal.FreeHGlobal(unmanagedArray);

            string json = Marshal.PtrToStringAnsi(resp_ptr);

            byte[] json_bytes = Encoding.Default.GetBytes(json);
            string utf8_json = Encoding.UTF8.GetString(json_bytes);

            AlprResponse response = JsonConvert.DeserializeObject<AlprResponse>(utf8_json);
            openalpr_free_response_string(resp_ptr);
            return response;
        }

        /// <summary>
        /// Sets the maximum number of results for OpenALPR to return with each recognition task
        /// </summary>
        /// <param name="top_n">An integer describing the maximum number of results to return</param>
        /// <exception cref="InvalidOperationException">Thrown when the library has not been initialized</exception>
        public void setTopN(int top_n)
        {
            check_initialization();

            openalpr_set_topn(this.native_instance, top_n);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeROI
        {
            public int x;
            public int y;
            public int width;
            public int height;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_init([MarshalAs(UnmanagedType.LPStr)] string country, [MarshalAs(UnmanagedType.LPStr)] string configFile, [MarshalAs(UnmanagedType.LPStr)] string runtimeDir, [MarshalAs(UnmanagedType.LPStr)] string licenseKey);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_init_gpu([MarshalAs(UnmanagedType.LPStr)] string country, [MarshalAs(UnmanagedType.LPStr)] string configFile, [MarshalAs(UnmanagedType.LPStr)] string runtimeDir, [MarshalAs(UnmanagedType.LPStr)] string licenseKey, int alprCHardwareAcceleration, int gpuId, int batchSize);

        [DllImport("libopenalpr.dll")]
        private static extern int openalpr_is_loaded(IntPtr instance);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_cleanup(IntPtr instance);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_country(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string country);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_prewarp(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string prewarp_config);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_mask(IntPtr instance, IntPtr pixelData, int bytesPerPixel, int imgWidth, int imgHeight);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_detect_region(IntPtr instance, int detectRegion);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_topn(IntPtr instance, int topN);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_set_default_region(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string region);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_recognize_rawimage(IntPtr instance, IntPtr pixelData, int bytesPerPixel, int imgWidth, int imgHeight, NativeROI roi);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_recognize_encodedimage(IntPtr instance, IntPtr bytes, long length, NativeROI roi);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_free_response_string(IntPtr response);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_create_image_batch();

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_add_encoded_image_to_batch(IntPtr batch, IntPtr bytes, long length, NativeROI roi);

        [DllImport("libopenalpr.dll")]
        private static extern IntPtr openalpr_recognize_batch(IntPtr instance, IntPtr batch);

        [DllImport("libopenalpr.dll")]
        private static extern void openalpr_release_image_batch(IntPtr batch);
    }
}
