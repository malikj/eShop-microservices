# ?? QUICK REFERENCE - CATALOGSERVICE ON AZURE

## ?? LIVE SERVICE

| What | URL |
|------|-----|
| **API** | https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net |
| **Swagger** | https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/swagger |
| **Scm/Kudu** | https://eshop-catalog-app-ctahcveqagf0bbgw.scm.southindia-01.azurewebsites.net |

## ?? ENDPOINTS

```bash
# Get Products
curl https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/products

# Get Categories
curl https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/categories

# Checkout
curl -X POST https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/checkout \
  -H "Content-Type: application/json" \
  -d '{"customerId":"750e8400-e29b-41d4-a716-446655440000","items":[{"productId":"test","quantity":1}]}'
```

## ?? REDEPLOY (When you make changes)

```bash
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

# Build
dotnet clean
dotnet publish -c Release -o publish

# Create ZIP
Remove-Item publish.zip -Force -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory("$PWD\publish", "$PWD\publish.zip")

# Deploy
az webapp deploy --resource-group eshop-rg --name eshop-catalog-app --src-path publish.zip --type zip

# Watch logs
az webapp log tail --resource-group eshop-rg --name eshop-catalog-app --follow
```

## ?? STATUS

- **Deployment:** ? Successful
- **Build Time:** 1 second
- **Startup Time:** 31 seconds
- **Instances:** 1/1 successful
- **Runtime:** RuntimeSuccessful

## ?? CONFIGURATION

| Setting | Value |
|---------|-------|
| **Runtime** | .NET 8.0 |
| **OS** | Linux |
| **Tier** | Free |
| **Cost** | $0/month |
| **Database** | In-Memory |
| **Messaging** | Dummy (no RabbitMQ) |

## ?? KEY FILES

- `Program.cs` - Uses WEBSITES_PORT env var
- `appsettings.json` - No hardcoded port
- `publish.zip` - Correct root-level structure
- `web.config` - IIS configuration

---

**Service is LIVE! ??**

