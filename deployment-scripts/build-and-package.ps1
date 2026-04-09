$projectPath = "c:\Users\arman\source\repos\TeknoSOS.WebApp\TeknoSOS.WebApp"
$publishPath = "$projectPath\bin\Release\publish"

Write-Host "🔨 Building project..." -ForegroundColor Green
dotnet build -c Release

Write-Host "📦 Publishing project..." -ForegroundColor Green
dotnet publish -c Release -o $publishPath

Write-Host "🗜️  Creating deployment package..." -ForegroundColor Green
Compress-Archive -Path "$publishPath\*" -DestinationPath "$projectPath\deploy.zip" -Force

Write-Host "✅ Deployment package created: deploy.zip" -ForegroundColor Green
Write-Host "📊 Package size: $(((Get-Item "$projectPath\deploy.zip").Length / 1MB).ToString('F2')) MB" -ForegroundColor Cyan

Write-Host "`n🚀 Next steps:" -ForegroundColor Yellow
Write-Host "1. Upload deploy.zip to your hosting provider"
Write-Host "2. Extract and run the application"
Write-Host "3. Test the application at https://your-domain.com"
