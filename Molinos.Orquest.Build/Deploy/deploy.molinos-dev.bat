cd /d %~dp0
START cmd.exe /k "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %~dp0deploy.proj /t:Deploy-Local /p:Environment=molinos-dev"