# Start all development services

Write-Host "Starting FileService (7292)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\FileService.WebAPI'; dotnet run"
Start-Sleep -Seconds 5

Write-Host "Starting IdentityService (7116)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\IdentityService.WebAPI'; dotnet run"
Start-Sleep -Seconds 5

Write-Host "Starting Listening.Admin (7109)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Listening.Admin.WebAPI'; dotnet run"
Start-Sleep -Seconds 5

Write-Host "Starting Listening.Main (7191)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Listening.Main.WebAPI'; dotnet run"
Start-Sleep -Seconds 5

Write-Host "Starting Frontend (3000)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\listening-frontend'; npm run dev"

Write-Host "`nAll services started! Wait for 'Now listening on' in each window" -ForegroundColor Cyan
Write-Host "Then open http://localhost:3000" -ForegroundColor Cyan
Write-Host "Press any key to close..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

