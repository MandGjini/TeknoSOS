az login

$resourceGroup = "TeknoSOSResourceGroup"
$appServicePlan = "TeknoSOSPlan"
$webAppName = "teknosos-webapp"
$location = "eastus"

Write-Host "🔧 Creating resource group..." -ForegroundColor Green
az group create --name $resourceGroup --location $location

Write-Host "📋 Creating app service plan..." -ForegroundColor Green
az appservice plan create `
  --name $appServicePlan `
  --resource-group $resourceGroup `
  --sku B2 `
  --is-linux

Write-Host "🌐 Creating web app..." -ForegroundColor Green
az webapp create `
  --resource-group $resourceGroup `
  --plan $appServicePlan `
  --name $webAppName `
  --runtime "DOTNETCORE|8.0"

Write-Host "⚙️  Configuring app settings..." -ForegroundColor Green
az webapp config appsettings set `
  --resource-group $resourceGroup `
  --name $webAppName `
  --settings `
    WEBSITES_ENABLE_APP_SERVICE_STORAGE=true `
    ConnectionString=$env:CONNECTION_STRING `
    JwtSecret=$env:JWT_SECRET

Write-Host "✅ Azure resources created successfully!" -ForegroundColor Green
Write-Host "Web app URL: https://$webAppName.azurewebsites.net" -ForegroundColor Cyan
