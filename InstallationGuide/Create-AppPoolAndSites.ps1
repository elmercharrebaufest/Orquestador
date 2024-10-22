# Parámetros generales
$siteName = "SCATO"
$appPoolName = "Orquest.web"
$physicalPath = "G:\SCATO"
$appPoolUser = "molinosagro\UsrSvcAccesosOrqPRD"
$virtualAppName = "Orquest.Web"
$virtualAppPath = "G:\SCATO\Orquest.Web"

# Función para mostrar mensajes con formato
function Write-Message {
    param([string]$message, [string]$color = "White")
    Write-Host $message -ForegroundColor $color
}

# Solicitar la contraseña de forma interactiva
$appPoolPassword = Read-Host -Prompt "Ingresa la contraseña del usuario del Application Pool" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($appPoolPassword))

# Creación del Application Pool
$appPoolPath = "IIS:\AppPools\$appPoolName"
if (-not (Test-Path $appPoolPath)) {
    Write-Message "Creando Application Pool: $appPoolName" "Yellow"
    New-WebAppPool -Name $appPoolName

    # Asignar usuario y contraseña
    Set-ItemProperty IIS:\AppPools\$appPoolName -Name processModel.identityType -Value SpecificUser
    Set-ItemProperty IIS:\AppPools\$appPoolName -Name processModel.userName -Value $appPoolUser
    Set-ItemProperty IIS:\AppPools\$appPoolName -Name processModel.password -Value $plainPassword

    Write-Message "Application Pool '$appPoolName' configurada con el usuario $appPoolUser" "Green"
} else {
    Write-Message "El Application Pool '$appPoolName' ya existe." "Cyan"
}

# Creación del sitio web
if (-not (Get-Website -Name $siteName -ErrorAction SilentlyContinue)) {
    Write-Message "Creando el sitio web: $siteName" "Yellow"

    # Crear directorio si no existe
    if (-not (Test-Path $physicalPath)) {
        New-Item -Path $physicalPath -ItemType Directory
        Write-Message "Directorio creado: $physicalPath" "Green"
    }

    # Crear sitio web y asignar el Application Pool
    New-Website -Name $siteName -PhysicalPath $physicalPath -ApplicationPool $appPoolName
    Write-Message "Sitio web '$siteName' creado usando el Application Pool '$appPoolName'." "Green"
} else {
    Write-Message "El sitio web '$siteName' ya existe." "Cyan"
}

# Creación del directorio para la aplicación virtual si no existe
if (-not (Test-Path $virtualAppPath)) {
    New-Item -Path $virtualAppPath -ItemType Directory
    Write-Message "Directorio para la aplicación virtual '$virtualAppPath' creado." "Green"
}

# Verificar si la aplicación virtual ya existe
if (-not (Get-WebApplication -Site $siteName -Name $virtualAppName -ErrorAction SilentlyContinue)) {
    Write-Message "Creando la aplicación virtual '$virtualAppName'..." "Yellow"
    New-WebApplication -Name $virtualAppName -Site $siteName -PhysicalPath $virtualAppPath -ApplicationPool $appPoolName
    Write-Message "Aplicación virtual '$virtualAppName' creada dentro del sitio '$siteName'." "Green"
} else {
    Write-Message "La aplicación virtual '$virtualAppName' ya existe." "Cyan"
}

# Desbloquear y habilitar autenticación de Windows en el sitio web
Write-Message "Desbloqueando la sección de Windows Authentication..." "Yellow"
Start-Process "C:\Windows\System32\inetsrv\appcmd.exe" -ArgumentList "unlock config /section:windowsAuthentication" -Wait

# Habilitar autenticación de Windows en la aplicación virtual
Write-Message "Habilitando Windows Authentication en la aplicación virtual '$virtualAppName'..." "Yellow"
Set-WebConfigurationProperty -Filter system.webServer/security/authentication/windowsAuthentication -PSPath "IIS:\Sites\$siteName\$virtualAppName" -Name enabled -Value $true
Write-Message "Autenticación de Windows habilitada en la aplicación virtual '$virtualAppName'." "Green"
