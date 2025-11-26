# Script simplificado para restaurar y corregir archivos SQL desde backup
$backupFolder = "backup_original_scripts"
$currentDir = Get-Location

Write-Host "=== Restaurando y corregiendo archivos SQL ===" -ForegroundColor Cyan

$sqlFiles = Get-ChildItem -Path $backupFolder -Filter "*.sql" -File
$processedFiles = 0

foreach ($backupFile in $sqlFiles) {
    $targetFile = Join-Path $currentDir $backupFile.Name
    Write-Host "Procesando: $($backupFile.Name)" -ForegroundColor Yellow
    
    $content = Get-Content $backupFile.FullName -Raw -Encoding UTF8
    
    # Aplicar correcciones
    $content = $content -replace 'USE Fiesta_Fries_DB;', 'USE [G02-2025-II-DB];'
    $content = $content -creplace '(\s+)CREATE TABLE (\w+)\(', '$1CREATE TABLE [Fiesta_Fries_DB].[$2]('
    $content = $content -creplace '(\s+)INSERT INTO (\w+)', '$1INSERT INTO [Fiesta_Fries_DB].[$2]'
    $content = $content -creplace '(\s+)ALTER TABLE (\w+)', '$1ALTER TABLE [Fiesta_Fries_DB].[$2]'
    $content = $content -creplace '(\s+)FROM (\w+)(\s+|\)|;)', '$1FROM [Fiesta_Fries_DB].[$2]$3'
    $content = $content -creplace '(\s+)JOIN (\w+)(\s+)', '$1JOIN [Fiesta_Fries_DB].[$2]$3'
    $content = $content -replace '\[Fiesta_Fries_DB\]\.\[Fiesta_Fries_DB\]\.', '[Fiesta_Fries_DB].'
    
    Set-Content -Path $targetFile -Value $content -Encoding UTF8 -NoNewline
    $processedFiles++
    Write-Host "  ✓ Completado" -ForegroundColor Green
}

Write-Host "`n✓ Procesados: $processedFiles archivos" -ForegroundColor Green
