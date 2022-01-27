
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" build.proj /t:Build /p:Configuration=Jenkins
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" deploy.proj /t:Deploy-Local /p:Environment="local"
PAUSE