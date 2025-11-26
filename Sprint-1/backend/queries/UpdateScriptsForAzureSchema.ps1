# Script to update SQL files for Azure Schema
# Updates all SQL scripts to use G02-2025-II-DB database and Fiesta_Fries_DB schema

$queriesPath = "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\queries"
$backupPath = "$queriesPath\backup_original_scripts"

# Create backup folder
if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath | Out-Null
    Write-Host "Backup folder created: $backupPath" -ForegroundColor Green
}

# Get all SQL files except the schema creation script
$sqlFiles = Get-ChildItem -Path $queriesPath -Filter "*.sql" | Where-Object { 
    $_.Name -ne "000_CreateSchema_Fiesta_Fries_DB.sql" -and 
    $_.Name -ne "UpdateScriptsForAzureSchema.ps1"
}

Write-Host "`nStarting SQL script update process" -ForegroundColor Cyan
Write-Host "Total files to process: $($sqlFiles.Count)" -ForegroundColor Yellow
Write-Host ""

$updatedCount = 0
$errorCount = 0

foreach ($file in $sqlFiles) {
    try {
        Write-Host "Procesando: $($file.Name)..." -NoNewline
        
        # Crear backup del archivo original
        Copy-Item -Path $file.FullName -Destination "$backupPath\$($file.Name)" -Force
        
        # Leer contenido del archivo
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        
        # 1. Reemplazar CREATE DATABASE y USE por G02-2025-II-DB
        $content = $content -replace "CREATE DATABASE Fiesta_Fries_DB;", "-- CREATE DATABASE ya existe: G02-2025-II-DB`n-- Schema: Fiesta_Fries_DB"
        $content = $content -replace "USE Fiesta_Fries_DB;", "USE [G02-2025-II-DB];`nGO`n-- Todas las tablas se crearán bajo el schema Fiesta_Fries_DB"
        
        # 2. Agregar prefijo de schema a CREATE TABLE (sin dbo)
        $content = $content -replace "CREATE TABLE ([A-Za-z_][A-Za-z0-9_]*)\s*\(", "CREATE TABLE [Fiesta_Fries_DB].[$1]("
        
        # 3. Corregir si ya tenía [dbo]
        $content = $content -replace "CREATE TABLE \[dbo\]\.", "CREATE TABLE [Fiesta_Fries_DB]."
        $content = $content -replace "CREATE TABLE dbo\.", "CREATE TABLE [Fiesta_Fries_DB]."
        
        # 4. Agregar prefijo a ALTER TABLE
        $content = $content -replace "ALTER TABLE ([A-Za-z_][A-Za-z0-9_]*)", "ALTER TABLE [Fiesta_Fries_DB].[$1]"
        $content = $content -replace "ALTER TABLE \[([A-Za-z_][A-Za-z0-9_]*)\]", "ALTER TABLE [Fiesta_Fries_DB].[$1]"
        
        # 5. Corregir ALTER TABLE que ya tenían dbo
        $content = $content -replace "ALTER TABLE \[dbo\]\.", "ALTER TABLE [Fiesta_Fries_DB]."
        $content = $content -replace "ALTER TABLE dbo\.", "ALTER TABLE [Fiesta_Fries_DB]."
        
        # 6. Agregar prefijo a DROP TABLE
        $content = $content -replace "DROP TABLE ([A-Za-z_][A-Za-z0-9_]*)", "DROP TABLE [Fiesta_Fries_DB].[$1]"
        $content = $content -replace "DROP TABLE \[([A-Za-z_][A-Za-z0-9_]*)\]", "DROP TABLE [Fiesta_Fries_DB].[$1]"
        
        # 7. Agregar prefijo a TRUNCATE TABLE
        $content = $content -replace "TRUNCATE TABLE ([A-Za-z_][A-Za-z0-9_]*)", "TRUNCATE TABLE [Fiesta_Fries_DB].[$1]"
        
        # 8. Agregar prefijo a INSERT INTO
        $content = $content -replace "INSERT INTO ([A-Za-z_][A-Za-z0-9_]*)", "INSERT INTO [Fiesta_Fries_DB].[$1]"
        $content = $content -replace "INSERT INTO \[([A-Za-z_][A-Za-z0-9_]*)\]", "INSERT INTO [Fiesta_Fries_DB].[$1]"
        
        # 9. Corregir INSERT que ya tenían dbo
        $content = $content -replace "INSERT INTO \[dbo\]\.", "INSERT INTO [Fiesta_Fries_DB]."
        $content = $content -replace "INSERT INTO dbo\.", "INSERT INTO [Fiesta_Fries_DB]."
        
        # 10. Agregar prefijo a UPDATE
        $content = $content -replace "UPDATE ([A-Za-z_][A-Za-z0-9_]*)\s+SET", "UPDATE [Fiesta_Fries_DB].[$1] SET"
        $content = $content -replace "UPDATE \[([A-Za-z_][A-Za-z0-9_]*)\]\s+SET", "UPDATE [Fiesta_Fries_DB].[$1] SET"
        
        # 11. Agregar prefijo a DELETE FROM
        $content = $content -replace "DELETE FROM ([A-Za-z_][A-Za-z0-9_]*)", "DELETE FROM [Fiesta_Fries_DB].[$1]"
        $content = $content -replace "DELETE FROM \[([A-Za-z_][A-Za-z0-9_]*)\]", "DELETE FROM [Fiesta_Fries_DB].[$1]"
        
        # 12. Agregar prefijo a SELECT * FROM (básico)
        $content = $content -replace "FROM ([A-Za-z_][A-Za-z0-9_]*)\s*;", "FROM [Fiesta_Fries_DB].[$1];"
        $content = $content -replace "FROM \[([A-Za-z_][A-Za-z0-9_]*)\]\s*;", "FROM [Fiesta_Fries_DB].[$1];"
        
        # 13. Corregir referencias que ya tenían dbo
        $content = $content -replace "FROM \[dbo\]\.", "FROM [Fiesta_Fries_DB]."
        $content = $content -replace "FROM dbo\.", "FROM [Fiesta_Fries_DB]."
        $content = $content -replace "JOIN \[dbo\]\.", "JOIN [Fiesta_Fries_DB]."
        $content = $content -replace "JOIN dbo\.", "JOIN [Fiesta_Fries_DB]."
        
        # 14. Agregar prefijo a CREATE INDEX
        $content = $content -replace "CREATE INDEX ([A-Za-z_][A-Za-z0-9_]*)\s+ON\s+([A-Za-z_][A-Za-z0-9_]*)", "CREATE INDEX [$1] ON [Fiesta_Fries_DB].[$2]"
        $content = $content -replace "CREATE UNIQUE INDEX ([A-Za-z_][A-Za-z0-9_]*)\s+ON\s+([A-Za-z_][A-Za-z0-9_]*)", "CREATE UNIQUE INDEX [$1] ON [Fiesta_Fries_DB].[$2]"
        
        # 15. Agregar prefijo a CREATE TRIGGER
        $content = $content -replace "CREATE TRIGGER ([A-Za-z_][A-Za-z0-9_]*)\s+ON\s+([A-Za-z_][A-Za-z0-9_]*)", "CREATE TRIGGER [Fiesta_Fries_DB].[$1] ON [Fiesta_Fries_DB].[$2]"
        
        # 16. Corregir duplicados que puedan haberse creado
        $content = $content -replace "\[Fiesta_Fries_DB\]\.\[Fiesta_Fries_DB\]\.", "[Fiesta_Fries_DB]."
        
        # Save only if changes were made
        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
            Write-Host " [UPDATED]" -ForegroundColor Green
            $updatedCount++
        } else {
            Write-Host " [NO CHANGES]" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host " [ERROR]: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Files processed: $($sqlFiles.Count)" -ForegroundColor White
Write-Host "Files updated: $updatedCount" -ForegroundColor Green
Write-Host "Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host "Backup created at: $backupPath" -ForegroundColor Yellow
Write-Host "`nProcess completed successfully" -ForegroundColor Green

# Important note
Write-Host "`nIMPORTANT NOTE:" -ForegroundColor Yellow
Write-Host "Stored procedures, functions and views within the scripts" -ForegroundColor White
Write-Host "may need manual adjustments in their JOINs and internal references." -ForegroundColor White
Write-Host "Review especially files containing SP_ and CREATE FUNCTION/PROCEDURE." -ForegroundColor White
