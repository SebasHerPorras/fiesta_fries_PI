# Script para restaurar y corregir CORRECTAMENTE los archivos SQL
# Este script copia desde backup y aplica el prefijo de schema SIN borrar nombres de tablas

$backupFolder = "backup_original_scripts"
$queriesFolder = "."

Write-Host "=== Restaurando archivos SQL desde backup ===" -ForegroundColor Cyan
Write-Host "Este script restaurará los archivos originales y aplicará el schema correctamente" -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "¿Deseas continuar? (S/N)"
if ($confirmation -ne 'S' -and $confirmation -ne 's') {
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    exit
}

# Obtener todos los archivos SQL del backup
$sqlFiles = Get-ChildItem -Path $backupFolder -Filter "*.sql" -File

$totalFiles = $sqlFiles.Count
$currentFile = 0
$processedFiles = 0
$errorFiles = 0

foreach ($backupFile in $sqlFiles) {
    $currentFile++
    $targetFile = Join-Path $queriesFolder $backupFile.Name
    
    Write-Host "[$currentFile/$totalFiles] Procesando: $($backupFile.Name)" -ForegroundColor Yellow
    
    try {
        # Leer contenido del archivo backup
        $content = Get-Content $backupFile.FullName -Raw -Encoding UTF8
        
        # 1. Reemplazar USE statement
        $content = $content -replace 'USE Fiesta_Fries_DB;', 'USE [G02-2025-II-DB];'
        
        # 2. Agregar schema a CREATE TABLE (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)CREATE TABLE (\w+)\(', '$1CREATE TABLE [Fiesta_Fries_DB].[$2]('
        
        # 3. Agregar schema a INSERT INTO (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)INSERT INTO (\w+)', '$1INSERT INTO [Fiesta_Fries_DB].[$2]'
        
        # 4. Agregar schema a ALTER TABLE (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)ALTER TABLE (\w+)', '$1ALTER TABLE [Fiesta_Fries_DB].[$2]'
        
        # 5. Agregar schema a UPDATE (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)UPDATE (\w+)', '$1UPDATE [Fiesta_Fries_DB].[$2]'
        
        # 6. Agregar schema a DELETE FROM (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)DELETE FROM (\w+)', '$1DELETE FROM [Fiesta_Fries_DB].[$2]'
        
        # 7. Agregar schema a FROM en consultas SELECT (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)FROM (\w+)(\s+|\)|;)', '$1FROM [Fiesta_Fries_DB].[$2]$3'
        
        # 8. Agregar schema a JOIN (preservando el nombre de tabla)
        $content = $content -creplace '(\s+)JOIN (\w+)(\s+)', '$1JOIN [Fiesta_Fries_DB].[$2]$3'
        
        # 9. Corregir doble schema si se aplicó dos veces
        $content = $content -replace '\[Fiesta_Fries_DB\]\.\[Fiesta_Fries_DB\]\.', '[Fiesta_Fries_DB].'
        
        # Guardar el archivo corregido
        Set-Content -Path $targetFile -Value $content -Encoding UTF8 -NoNewline
        $processedFiles++
        Write-Host "  ✓ Restaurado y corregido exitosamente" -ForegroundColor Green
    }
    catch {
        $errorFiles++
        Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "Total archivos: $totalFiles" -ForegroundColor White
Write-Host "Archivos procesados: $processedFiles" -ForegroundColor Green
Write-Host "Archivos con errores: $errorFiles" -ForegroundColor Red

if ($processedFiles -gt 0) {
    Write-Host "`n✓ Los archivos SQL han sido restaurados y corregidos" -ForegroundColor Green
    Write-Host "  - USE statement actualizado a G02-2025-II-DB" -ForegroundColor White
    Write-Host "  - Schema [Fiesta_Fries_DB] agregado a todas las tablas" -ForegroundColor White
    Write-Host "  - Nombres de tablas preservados correctamente" -ForegroundColor White
}
