# Define la URL de descarga del archivo MSI de msdeploy
$msdeployUrl = "https://download.microsoft.com/download/b/d/8/bd882ec4-12e0-481a-9b32-0fae8e3c0b78/webdeploy_amd64_en-US.msi"

# Define la ruta donde se descargará el archivo MSI
$msiPath = "$env:TEMP\msdeploy.msi"

# Descargar el archivo MSI
Write-Host "Descargando msdeploy desde $msdeployUrl..."
Invoke-WebRequest -Uri $msdeployUrl -OutFile $msiPath

# Verificar si el archivo fue descargado correctamente
if (Test-Path $msiPath) {
    Write-Host "Archivo descargado exitosamente en $msiPath."

    # Instalar el archivo MSI
    Write-Host "Instalando msdeploy..."
   Start-Process "msiexec.exe" -ArgumentList "/i `"$msiPath`" ADDLOCAL=ALL /quiet /norestart /L*v C:\install_log.txt" -Wait

    # Verificar si msdeploy se instaló correctamente
    $msdeployCheck = Test-Path "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    if ($msdeployCheck) {
        Write-Host "msdeploy se instaló correctamente."
    } else {
        Write-Host "La instalación de msdeploy falló."
    }
} else {
    Write-Host "Error: No se pudo descargar el archivo MSI."
}

# Nombre del servicio
$serviceName = "MsDepSvc"

# Verificar el estado del servicio
$service = Get-Service -Name $serviceName

if ($service.Status -ne 'Running') {
    Write-Host "El servicio $serviceName no está en ejecución. Iniciando..."
    Start-Service -Name $serviceName
} else {
    Write-Host "El servicio $serviceName está en ejecución."
}

# Verificar si el servicio está configurado en modo automático
$startupType = Get-WmiObject -Class Win32_Service | Where-Object { $_.Name -eq $serviceName }
if ($startupType.StartMode -ne 'Auto') {
    Write-Host "El servicio $serviceName no está configurado en modo automático. Cambiando..."
    Set-Service -Name $serviceName -StartupType Automatic
} else {
    Write-Host "El servicio $serviceName ya está configurado en modo automático."
}
