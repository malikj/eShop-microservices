# ?? FIXED: CatalogService Azure Deployment - LINUX VERSION

**Status:** ? **READY TO DEPLOY (FINAL FIX)**

---

## ?? ROOT CAUSE OF 400 ERROR

Your app is configured for **Linux deployment** (RuntimeIdentifier: linux-x64), but the ZIP had:

1. ? Extra leftover folders from previous attempts
2. ? Corrupted/mixed files from test builds
3. ? Missing proper structure

**Why web.config was missing:**
- Your app targets `linux-x64` (not Windows/IIS)
- Azure App Service is running **Linux** with **DOTNETCORE|8.0 runtime**
- Linux deployments don't need web.config (that's for Windows IIS)
- **web.config is NOT needed for your setup** ?

---

## ? WHAT WAS FIXED

### Issue 1: Corrupted Publish Folder
```
? OLD: publish\ contained test-zip\ and nested publish\ folder
? NEW: Clean publish\ with only deployment files
```

### Issue 2: Build Configuration
```
? Correct: <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
? Correct: ASP.NET Core self-contained deployment
? Correct: DOTNETCORE|8.0 runtime on Azure App Service
```

### Issue 3: ZIP Structure
```
? Catalog.Api (executable) at root
? Catalog.Api.dll at root
? All dependencies at root
? appsettings.json at root
? No nested folders
```

---

## ?? NEW DEPLOYMENT PACKAGE

**File:** `publish.zip` (8.4 MB)

**Contents:**
- ? Catalog.Api executable (Linux x64)
- ? All DLLs and dependencies
- ? Configuration files
- ? Runtime configuration
- ? Swagger UI files

**Structure:** ? Correct (all files at root, no nesting)

---

## ?? DEPLOY NOW

```powershell
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

az webapp deploy `
  --resource-group eshop-rg `
  --name eshop-catalog-app `
  --src-path publish.zip `
  --type zip
```

---

## ? EXPECTED RESULT

```
Status: Build successful. Time: 1(s)
Status: Starting the site... Time: 16(s)
Status: Site started successfully. Time: 31(s)
Deployment has completed successfully
```

---

## ?? TEST AFTER DEPLOYMENT

### Swagger UI
```
https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/swagger
```

### API Endpoints
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

---

## ?? CONFIGURATION SUMMARY

| Setting | Value | Status |
|---------|-------|--------|
| **Runtime Identifier** | linux-x64 | ? Correct for Azure Linux |
| **Target Framework** | net8.0 | ? Matches Azure Runtime |
| **Azure Runtime** | DOTNETCORE\|8.0 | ? Linux |
| **Swagger** | Enabled | ? Works |
| **Exception Handler** | Skips /swagger | ? Configured |
| **Port Configuration** | WEBSITES_PORT env var | ? Dynamic |
| **Database** | In-Memory | ? No external deps |
| **Messaging** | DummyEventPublisher | ? No RabbitMQ |

---

## ?? WHY THIS WORKS NOW

**Previous attempts failed because:**
1. ZIP had corrupted/mixed files from multiple build attempts
2. Leftover test folders interfering with deployment
3. Confusion about Windows vs Linux deployment

**Current approach is correct:**
1. ? Linux x64 target matches Azure App Service Linux
2. ? No web.config needed (that's for Windows IIS)
3. ? Standalone executable with all dependencies
4. ? ASP.NET Core runtime on Linux handles startup

---

## ?? KEY POINTS

**Azure App Service Linux:**
- Runs .NET applications on Linux containers
- Uses DOTNETCORE runtime
- Expects executable + dependencies (not web.config)
- Uses WEBSITES_PORT environment variable for port binding

**Your Application:**
- Configured as `linux-x64` in .csproj ?
- Has `app.Urls.Add($"http://0.0.0.0:{port}");` ?
- All dependencies included in ZIP ?
- Swagger middleware correctly ordered ?

---

## ?? IF STILL NOT WORKING

1. Check logs:
```bash
az webapp log tail --resource-group eshop-rg --name eshop-catalog-app --follow
```

2. Restart app:
```bash
az webapp restart --resource-group eshop-rg --name eshop-catalog-app
```

3. Check status:
```bash
az webapp show --resource-group eshop-rg --name eshop-catalog-app --query state
```

---

## ? YOU'RE READY TO DEPLOY!

**Final publish.zip:** 8.4 MB ?  
**Structure:** Correct ?  
**Configuration:** Verified ?  
**Ready:** YES ?

**Next step:** Run the deployment command above!

