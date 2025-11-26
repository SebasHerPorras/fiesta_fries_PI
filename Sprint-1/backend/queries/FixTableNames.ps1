# Script para restaurar los nombres de tablas que fueron borrados por error
# Restaura desde los archivos de backup originales

$backupFolder = "backup_original_scripts"
$queriesFolder = "."

Write-Host "=== Restaurando nombres de tablas desde backup ===" -ForegroundColor Cyan

# Obtener todos los archivos SQL (excepto el script de fix)
$sqlFiles = Get-ChildItem -Path $backupFolder -Filter "*.sql" -File | Where-Object { $_.Name -notlike "*Fix*" }

$totalFiles = $sqlFiles.Count
$currentFile = 0
$fixedFiles = 0
$errorFiles = 0

foreach ($backupFile in $sqlFiles) {
    $currentFile++
    $targetFile = Join-Path $queriesFolder $backupFile.Name
    
    # Verificar si el archivo existe en la carpeta queries
    if (Test-Path $targetFile) {
        Write-Host "[$currentFile/$totalFiles] Procesando: $($backupFile.Name)" -ForegroundColor Yellow
        
        try {
            # Leer contenido del archivo backup
            $backupContent = Get-Content $backupFile.FullName -Raw -Encoding UTF8
            
            # Leer contenido del archivo actual
            $currentContent = Get-Content $targetFile -Raw -Encoding UTF8
            
            # Reemplazar USE statement
            $newContent = $currentContent -replace 'USE Fiesta_Fries_DB;', 'USE [G02-2025-II-DB];'
            
            # Extraer nombres de tablas del backup y aplicarlos al archivo actual
            # Patrón para encontrar CREATE TABLE nombre_tabla
            $createTablePattern = 'CREATE TABLE (\w+)\('
            $backupMatches = [regex]::Matches($backupContent, $createTablePattern)
            
            foreach ($match in $backupMatches) {
                $tableName = $match.Groups[1].Value
                # Reemplazar [Fiesta_Fries_DB].[] con [Fiesta_Fries_DB].[TableName]
                $newContent = $newContent -replace '\[Fiesta_Fries_DB\]\.\[\]', "[Fiesta_Fries_DB].[$tableName]"
            }
            
            # Patrón para INSERT INTO nombre_tabla
            $insertPattern = 'INSERT INTO (\w+)'
            $backupInsertMatches = [regex]::Matches($backupContent, $insertPattern)
            
            foreach ($match in $backupInsertMatches) {
                $tableName = $match.Groups[1].Value
                # Reemplazar INSERT INTO [Fiesta_Fries_DB].[].[] con el nombre correcto
                $newContent = $newContent -replace 'INSERT INTO \[Fiesta_Fries_DB\]\.\[\]\.\[\]', "INSERT INTO [Fiesta_Fries_DB].[$tableName]"
            }
            
            # Patrón para ALTER TABLE nombre_tabla
            $alterPattern = 'ALTER TABLE (\w+)'
            $backupAlterMatches = [regex]::Matches($backupContent, $alterPattern)
            
            foreach ($match in $backupAlterMatches) {
                $tableName = $match.Groups[1].Value
                $newContent = $newContent -replace 'ALTER TABLE \[Fiesta_Fries_DB\]\.\[\]', "ALTER TABLE [Fiesta_Fries_DB].[$tableName]"
            }
            
            # Guardar el archivo corregido
            Set-Content -Path $targetFile -Value $newContent -Encoding UTF8 -NoNewline
            $fixedFiles++
            Write-Host "  ✓ Corregido exitosamente" -ForegroundColor Green
        }
        catch {
            $errorFiles++
            Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "Total archivos procesados: $currentFile" -ForegroundColor White
Write-Host "Archivos corregidos: $fixedFiles" -ForegroundColor Green
Write-Host "Archivos con errores: $errorFiles" -ForegroundColor Red
Write-Host "`nNOTA: Algunos archivos pueden requerir corrección manual." -ForegroundColor Yellow
