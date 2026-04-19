# ?? ROOT CAUSE FOUND: Windows vs Linux Mismatch

## **THE REAL PROBLEM**

Your Azure App Service is **Linux-based** but your deployment package is **Windows-based**.

### Evidence from Azure CLI Output:

```json
{
  "kind": "app,linux",                    ? Linux App Service
  "linuxFxVersion": "DOTNETCORE|8.0",     ? .NET 8 on Linux
  "reserved": true                        ? Reserved for Linux
}
```

### Your Deployment Package Contains:

```
publish.zip
??? web.config              ? WINDOWS/IIS ONLY!
??? Catalog.Api.exe         ? Windows executable
??? Catalog.Api.dll
??? ... (Windows artifacts)
```

### Why It Fails:

```
1. You publish on Windows ? Creates web.config (for IIS)
2. Azure Linux receives it
3. Linux doesn't use IIS or web.config
4. Kudu can't deploy Windows config to Linux
5. Status Code 400 ?
```

---

## ? THE SOLUTION

You have **TWO options**:

---

## **OPTION 1: Use Linux-Friendly Deployment** (Recommended for Free Tier)

Publish with **no web.config**. Linux will run the app directly.

### Step 1: Modify Catalog.Api.csproj

Add this to your `<PropertyGroup>`:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>     ? ADD THIS
</PropertyGroup>
```

### Step 2: Publish for Linux

```powershell
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

# Clean
dotnet clean

# Publish for Linux (no web.config generated)
dotnet publish -c Release -o publish --runtime linux-x64 --no-self-contained
```

### Step 3: Create ZIP (without web.config)

```powershell
# Verify web.config is NOT in publish folder
Test-Path .\publish\web.config
# Should return: False

# Create ZIP
Remove-Item publish.zip -Force -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory("$PWD\publish", "$PWD\publish.zip")

# Verify ZIP
ls publish.zip
```

### Step 4: Deploy

```powershell
az webapp deploy --resource-group eshop-rg --name eshop-catalog-app --src-path publish.zip --type zip
```

---

## **OPTION 2: Switch to Windows App Service** (More Expensive)

If you prefer Windows deployment, you'd need to:
1. Delete current Linux app service
2. Create new Windows-based App Service Plan
3. Redeploy

*(Not recommended for free tier)*

---

## ?? RECOMMENDED: Go with Option 1

**Why:**
- ? Works with your current Linux setup
- ? No additional cost
- ? Simpler for free tier
- ? Better for microservices

---

## ?? IMPLEMENTATION STEPS

### **Step 1A: Update Catalog.Api.csproj**

Find this in `CatalogService/Catalog.Api/Catalog.Api.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

**Change to:**

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

---

### **Step 1B: Clean & Publish**

```powershell
cd C:\Users\ADMIN\Documents\Dev\eShop\CatalogService\Catalog.Api

# Clean previous builds
dotnet clean

# Publish for Linux (Release mode)
dotnet publish -c Release -o publish --runtime linux-x64 --no-self-contained
```

**Expected output:**
```
CatalogService\Catalog.Api\publish\ (created without web.config)
```

---

### **Step 2: Verify No web.config**

```powershell
# This should return: False
Test-Path .\publish\web.config
```

---

### **Step 3: Create ZIP**

```powershell
# Remove old ZIP
Remove-Item publish.zip -Force -ErrorAction SilentlyContinue

# Create new ZIP
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory("$PWD\publish", "$PWD\publish.zip")

# Verify
ls publish.zip
```

---

### **Step 4: Deploy to Azure**

```powershell
az webapp deploy `
  --resource-group eshop-rg `
  --name eshop-catalog-app `
  --src-path publish.zip `
  --type zip
```

**Expected success:**
```
Deployment successful.
```

---

## ? VERIFICATION CHECKLIST

- [ ] Added `<RuntimeIdentifier>linux-x64</RuntimeIdentifier>` to Catalog.Api.csproj
- [ ] Ran `dotnet clean`
- [ ] Ran `dotnet publish -c Release -o publish --runtime linux-x64 --no-self-contained`
- [ ] Verified `web.config` does NOT exist in publish folder
- [ ] Created new ZIP with correct structure
- [ ] Deployed with `az webapp deploy` command
- [ ] Deployment shows "successful"
- [ ] Can access: `https://eshop-catalog-app-ctahcveqagf0bbgw.azurewebsites.net/swagger`

---

## ?? WHY THIS WORKS

**Linux .NET 8 App Service expects:**
- ? Self-contained or framework-dependent executable
- ? No web.config (Kestrel handles HTTP)
- ? app entrypoint (Catalog.Api)
- ? appsettings.json for config

**NOT:**
- ? web.config (Windows/IIS only)
- ? IIS configuration
- ? Windows-specific files

---

## ?? BEFORE vs AFTER

### BEFORE (Failed - Windows artifacts):
```
publish.zip
??? web.config              ? ? Not on Linux!
??? Catalog.Api.exe         ? ? Windows executable!
??? Catalog.Api.dll
??? appsettings.json
```

### AFTER (Success - Linux artifacts):
```
publish.zip
??? Catalog.Api             ? ? Linux executable
??? Catalog.Api.dll
??? appsettings.json
??? Catalog.Api.runtimeconfig.json
??? (no web.config!)
```

---

**Ready to implement? Start with Step 1A above! ??**

