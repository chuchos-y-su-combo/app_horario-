# publish-backend.ps1
# ----------------------------------------------------------------------------
# PASO 1 de 2 del flujo de empaquetado Electron:
#
#   Publica el backend .NET como ejecutable self-contained win-x64 y lo
#   coloca en frontend\resources\backend\ para que electron-builder lo
#   incluya en el instalador.
#
# Uso (desde la raíz del repositorio):
#   .\publish-backend.ps1
#
# PASO 2 (después de este script):
#   cd frontend
#   pnpm run electron:build
# ----------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$repoRoot    = $PSScriptRoot
$projectPath = Join-Path $repoRoot 'backend\src\ApplicationSchedule.Api'
$outputPath  = Join-Path $repoRoot 'frontend\resources\backend'

Write-Host ''
Write-Host '==========================================' -ForegroundColor Cyan
Write-Host '  AppHorario – Backend Publish (Paso 1/2)' -ForegroundColor Cyan
Write-Host '==========================================' -ForegroundColor Cyan
Write-Host ''
Write-Host "Proyecto : $projectPath"
Write-Host "Destino  : $outputPath"
Write-Host ''

# ── 1. Limpiar output anterior (conservar .gitkeep) ─────────────────────────
if (Test-Path $outputPath) {
    Get-ChildItem $outputPath -Exclude '.gitkeep' | Remove-Item -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

# ── 2. Publicar ejecutable self-contained ───────────────────────────────────
Write-Host 'Compilando backend self-contained win-x64...' -ForegroundColor Yellow

dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    --output $outputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host ''
    Write-Host "ERROR: dotnet publish falló (código $LASTEXITCODE)." -ForegroundColor Red
    exit $LASTEXITCODE
}

# ── 3. Asegurar caché de Electron para electron-builder ─────────────────────
# Si el caché ya está relleno (builds anteriores) se omite la copia.
$electronVersion = "34.5.8"
$cacheTarget = "$env:LOCALAPPDATA\electron-builder\Cache\electron\electron-v$electronVersion-win32-x64"

if (-not (Test-Path "$cacheTarget\electron.exe")) {
    Write-Host ''
    Write-Host 'Precargando binario de Electron en caché de electron-builder...' -ForegroundColor Yellow

    # Buscar electron dist en el store de pnpm
    $electronDist = Join-Path $repoRoot "frontend\node_modules\.pnpm\electron@$electronVersion\node_modules\electron\dist"

    if (Test-Path "$electronDist\electron.exe") {
        New-Item -ItemType Directory -Force -Path $cacheTarget | Out-Null
        Copy-Item "$electronDist\*" -Destination $cacheTarget -Recurse -Force
        Write-Host "Caché rellenado ($cacheTarget)." -ForegroundColor Green
    } else {
        Write-Host 'AVISO: No se encontró electron dist local.' -ForegroundColor Yellow
        Write-Host "  electron-builder descargará el binario la primera vez (~116 MB)."
    }
}

Write-Host ''
Write-Host '========================================' -ForegroundColor Green
Write-Host '  Backend publicado correctamente!      ' -ForegroundColor Green
Write-Host '========================================' -ForegroundColor Green
Write-Host ''
Write-Host 'PASO 2 – Crear el instalador .exe:' -ForegroundColor Cyan
Write-Host '  cd frontend'
Write-Host '  pnpm install --ignore-scripts   # si aun no instalaste Electron'
Write-Host '  pnpm run electron:build         # build del frontend + instalador NSIS'
Write-Host ''
Write-Host 'El instalador se genera en: frontend\dist-electron\AppHorario Setup x.x.x.exe'
Write-Host ''
