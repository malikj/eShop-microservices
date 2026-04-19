# ?? FIXED: Deploy CatalogService to Azure - Complete Guide

**Status:** ? Ready to Deploy  
**Issue Fixed:** Incorrect ZIP file structure  
**New Approach:** Using ZipFile API for correct structure

---

## ?? WHAT WAS THE PROBLEM?

Your previous deployment failed because of **incorrect ZIP structure**:

```
? WRONG Structure (what you had):
publish.zip
??? publish/
    ??? web.config
    ??? Catalog.Api.dll
    ??? appsettings.json
    ??? ... (all files nested one level too deep)

? CORRECT Structure (what you need):
publish.zip
??? web.config
??? Catalog.Api.dll
??? appsettings.json
??? Catalog.Api.exe
??? ... (all files at root level)
```

Azure's Kudu deployment service expects files at the **root level** of the ZIP. When it finds them nested, it fails with **Status Code 400**.

---

## ? SOLUTION IMPLEMENTED

You now have a correctly structured `publish.zip` using the `ZipFile` API instead of `Compress-Archive`.

**What was fixed:**
- ? Removed hardcoded port `8080` from `Program.cs`
- ? Removed hardcoded Kestrel config from `appsettings.json`
- ? Created properly structured ZIP file at root level
- ? Verified web.config is at root

---

## ?? DEPLOYMENT STEPS

### **Step 1: Verify ZIP Structure** ?

```sh
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

# Should show web.config at root
Test-Path .\publish\web.config
# Result: True
```

### **Step 2: Deploy to Azure**

**Using Azure CLI:**
```sh
az webapp deploy `
  --resource-group eshop-rg `
  --name eshop-catalog-app `
  --src-path publish.zip `
  --type zip
```

**Expected Output:**
```
Initiating deployment
Deploying from local path: publish.zip
Warming up Kudu before deployment.
Warmed up Kudu instance successfully.
Deployment successful.
```

### **Step 3: Verify Deployment**

**Check status:**
```sh
az webapp show --resource-group eshop-rg --name eshop-catalog-app --query state
# Should return: "Running"
```

**View logs:**
```sh
az webapp log tail --resource-group eshop-rg --name eshop-catalog-app
```

**Test in browser:**
```
https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/swagger
```

---

## ?? WHEN DEPLOYMENT SUCCEEDS

You'll see:
```
Deployment successful.
Site started.
Application started. Press Ctrl+C to exit.
```

**Test endpoints:**
```
GET  https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/products
GET  https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/categories
POST https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/api/checkout
```

---

## ?? IF YOU NEED TO REDEPLOY

**Always use this command sequence:**

```sh
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

# 1. Clean
dotnet clean

# 2. Publish for Release
dotnet publish -c Release -o publish

# 3. Create ZIP correctly (using ZipFile API, not Compress-Archive)
Remove-Item publish.zip -Force -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory("$PWD\publish", "$PWD\publish.zip")

# 4. Deploy
az webapp deploy --resource-group eshop-rg --name eshop-catalog-app --src-path publish.zip --type zip
```

---

## ?? CONFIGURATION CHECKLIST

Your files are now configured correctly:

### `Program.cs` ?
```csharp
var port = Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");
```

### `appsettings.json` ?
```json
{
  "ConnectionStrings": {...},
  "RabbitMQ": {...},
  "Logging": {...},
  "AllowedHosts": "*"
}
```
*(No hardcoded Kestrel port)*

### `web.config` ?
```xml
<aspNetCore processPath="dotnet" arguments=".\Catalog.Api.dll" />
```

---

## ?? IF DEPLOYMENT STILL FAILS

**Check these things:**

### 1. Verify Azure CLI is authenticated
```sh
az account show
```

### 2. Check resource group exists
```sh
az group show --resource-group eshop-rg
```

### 3. Check app service exists
```sh
az webapp show --resource-group eshop-rg --name eshop-catalog-app
```

### 4. Check deployment history
```sh
az webapp deployment list --resource-group eshop-rg --name eshop-catalog-app
```

### 5. View real-time logs
```sh
az webapp log tail --resource-group eshop-rg --name eshop-catalog-app --follow
```

---

## ?? IMPORTANT NOTES

**Free Tier Limitations:**
- Data resets on app restart (in-memory DB)
- May have slow startup
- No auto-scaling

**Your Configuration:**
- ? In-Memory Database (no SQL Server cost)
- ? No RabbitMQ (using DummyEventPublisher)
- ? Free App Service tier ($0/month)
- ? Dynamic port assignment via WEBSITES_PORT

---

## ?? YOU'RE READY!

Your CatalogService is now properly configured for Azure deployment.

**Next steps:**
1. Deploy using the steps above
2. Test endpoints in Swagger
3. Verify logs in Azure
4. When ready, deploy OrdersService and PaymentsService

---

**Happy deploying! ??**

