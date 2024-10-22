function Install-FeaturesFromCsv {
    param (
        [string]$importPath
    )

    # Verificar si el archivo CSV existe
    if (-Not (Test-Path $importPath)) {
        Write-Host "El archivo CSV $importPath no se encuentra."
        return
    }

    # Leer el archivo CSV e instalar las features
    $featuresToInstall = Import-Csv -Path $importPath | Select-Object -ExpandProperty Name

    foreach ($feature in $featuresToInstall) {
        # Verificar si la feature ya está instalada
        $installedFeature = Get-WindowsFeature -Name $feature
        if ($installedFeature.Installed -eq $false) {
            try {
                Install-WindowsFeature -Name $feature -ErrorAction Stop
                Write-Host "Feature $feature instalada correctamente."
            }
            catch {
                Write-Host "Error al instalar la feature $feature : $_"
            }
        }
        else {
            Write-Host "Feature $feature ya está instalada."
        }
    }
}
Install-FeaturesFromCsv -importPath "C:\InstalledFeatures.csv"