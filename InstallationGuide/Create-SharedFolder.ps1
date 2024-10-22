# Define el directorio que deseas compartir
$sharedFolderPath = "G:\Orquestador"
$shareName = "Orquestador"
$user = "molinosagro\tfs_servicio"  # Cambia esto por el nombre del usuario o grupo al que quieres dar acceso
$logFolderPath = "G:\OrquestadorLogs"

# Crea el directorio si no existe
if (-not (Test-Path $sharedFolderPath)) {
    New-Item -Path $sharedFolderPath -ItemType Directory
    Write-Host "Directorio creado: $sharedFolderPath"
} else {
    Write-Host "El directorio ya existe: $sharedFolderPath"
}

# Crea el directorio de logs si no existe
if (-not (Test-Path $logFolderPath)) {
    New-Item -Path $logFolderPath -ItemType Directory
    Write-Host "Directorio de logs creado: $logFolderPath"
} else {
    Write-Host "El directorio de logs ya existe: $logFolderPath"
}

# Crear el recurso compartido con acceso completo al usuario
New-SmbShare -Name $shareName -Path $sharedFolderPath -FullAccess $user
Write-Host "Carpeta compartida '$shareName' creada en '$sharedFolderPath' con acceso completo para '$user'."

# Asignar permisos a nivel de directorio
$acl = Get-Acl $sharedFolderPath
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($user, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl $sharedFolderPath $acl
Write-Host "Permisos de 'FullControl' asignados al usuario '$user' en el directorio '$sharedFolderPath'."
