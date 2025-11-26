# Script to update Repository files for Azure Schema
# Updates all C# repository files to use Fiesta_Fries_DB schema prefix

$repositoriesPath = "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\Repositories"
$backupPath = "$repositoriesPath\backup_original_files"

# Create backup folder
if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath | Out-Null
    Write-Host "Backup folder created: $backupPath" -ForegroundColor Green
}

# Get all C# repository files
$csFiles = Get-ChildItem -Path $repositoriesPath -Filter "*.cs"

Write-Host "`nStarting C# repository update process" -ForegroundColor Cyan
Write-Host "Total files to process: $($csFiles.Count)" -ForegroundColor Yellow
Write-Host ""

$updatedCount = 0
$errorCount = 0

foreach ($file in $csFiles) {
    try {
        Write-Host "Processing: $($file.Name)..." -NoNewline
        
        # Create backup of original file
        Copy-Item -Path $file.FullName -Destination "$backupPath\$($file.Name)" -Force
        
        # Read file content
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        
        # 1. Update SELECT queries - FROM clause
        $content = $content -replace '(FROM|JOIN)\s+dbo\.([A-Za-z_][A-Za-z0-9_]*)', '$1 [Fiesta_Fries_DB].[$2]'
        $content = $content -replace '(FROM|JOIN)\s+\[dbo\]\.', '$1 [Fiesta_Fries_DB].'
        
        # 2. Update SELECT queries - tables without schema
        $content = $content -replace 'FROM\s+([A-Za-z_][A-Za-z0-9_]*)\s+', 'FROM [Fiesta_Fries_DB].[$1] '
        $content = $content -replace 'FROM\s+\[([A-Za-z_][A-Za-z0-9_]*)\]\s+', 'FROM [Fiesta_Fries_DB].[$1] '
        $content = $content -replace 'JOIN\s+([A-Za-z_][A-Za-z0-9_]*)\s+', 'JOIN [Fiesta_Fries_DB].[$1] '
        $content = $content -replace 'JOIN\s+\[([A-Za-z_][A-Za-z0-9_]*)\]\s+', 'JOIN [Fiesta_Fries_DB].[$1] '
        
        # 3. Update INSERT INTO statements
        $content = $content -replace 'INSERT INTO\s+dbo\.', 'INSERT INTO [Fiesta_Fries_DB].'
        $content = $content -replace 'INSERT INTO\s+\[dbo\]\.', 'INSERT INTO [Fiesta_Fries_DB].'
        $content = $content -replace 'INSERT INTO\s+([A-Za-z_][A-Za-z0-9_]*)\s+', 'INSERT INTO [Fiesta_Fries_DB].[$1] '
        $content = $content -replace 'INSERT INTO\s+\[([A-Za-z_][A-Za-z0-9_]*)\]\s+', 'INSERT INTO [Fiesta_Fries_DB].[$1] '
        
        # 4. Update UPDATE statements
        $content = $content -replace 'UPDATE\s+dbo\.', 'UPDATE [Fiesta_Fries_DB].'
        $content = $content -replace 'UPDATE\s+\[dbo\]\.', 'UPDATE [Fiesta_Fries_DB].'
        $content = $content -replace 'UPDATE\s+([A-Za-z_][A-Za-z0-9_]*)\s+SET', 'UPDATE [Fiesta_Fries_DB].[$1] SET'
        $content = $content -replace 'UPDATE\s+\[([A-Za-z_][A-Za-z0-9_]*)\]\s+SET', 'UPDATE [Fiesta_Fries_DB].[$1] SET'
        
        # 5. Update DELETE FROM statements
        $content = $content -replace 'DELETE FROM\s+dbo\.', 'DELETE FROM [Fiesta_Fries_DB].'
        $content = $content -replace 'DELETE FROM\s+\[dbo\]\.', 'DELETE FROM [Fiesta_Fries_DB].'
        $content = $content -replace 'DELETE FROM\s+([A-Za-z_][A-Za-z0-9_]*)', 'DELETE FROM [Fiesta_Fries_DB].[$1]'
        $content = $content -replace 'DELETE FROM\s+\[([A-Za-z_][A-Za-z0-9_]*)\]', 'DELETE FROM [Fiesta_Fries_DB].[$1]'
        
        # 6. Fix any duplicates
        $content = $content -replace '\[Fiesta_Fries_DB\]\.\[Fiesta_Fries_DB\]\.', '[Fiesta_Fries_DB].'
        
        # 7. Handle EXEC stored procedures - keep them as is since SP already has schema in SQL
        # No changes needed for EXEC statements
        
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
Write-Host "Files processed: $($csFiles.Count)" -ForegroundColor White
Write-Host "Files updated: $updatedCount" -ForegroundColor Green
Write-Host "Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host "Backup created at: $backupPath" -ForegroundColor Yellow
Write-Host "`nProcess completed successfully" -ForegroundColor Green

Write-Host "`nIMPORTANT NOTE:" -ForegroundColor Yellow
Write-Host "Please review the updated files to ensure SQL queries are correct." -ForegroundColor White
Write-Host "Test the application thoroughly before deploying to production." -ForegroundColor White
