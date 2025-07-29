# Snipe-IT Automated UI Testing

This project provides automated end-to-end tests for the Snipe-IT asset management system using [Microsoft Playwright for .NET](https://playwright.dev/dotnet/) and [NUnit](https://nunit.org/).

## 📁 Project Structure

```
SnipeItTests/
│
├── SnipeItTests.cs         # Main NUnit test class
├── SnipeItHelper.cs        # Helper methods using Playwright to interact with the UI
├── playwright.config.ts    # (Optional) Playwright configuration
├── bin/                    # Compiled output
├── obj/                    # Temporary object files
├── SnipeItTests.csproj     # .NET project file
└── README.md               # This file
```

## ✅ Features Tested

1. Login to the snipeit demo at https://demo.snipeitapp.com/login
2. Create a new Macbook Pro 13" asset with the ready to deploy status and checked out to a random user
3. Find the asset you just created in the assets list to verify it was created successfully
4. Navigate to the asset page from the list and validate relevant details from the asset creation
5. Validate the details in the "History" tab on the asset page

## 🔧 Prerequisites

- .NET SDK 9.0(lastest version)
- Playwright

## 🚀 Setup

### 1. Clone the Repository

```bash
git clone https://your-repo-url/SnipeItTests.git
cd SnipeItTests
```

### 2. Install Dependencies

```bash
dotnet add package Microsoft.Playwright.NUnit
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
```

### 3. Install Playwright Browsers

```bash
npx playwright install
```

## 🧪 Run Tests

```bash
dotnet test
```

You should see output like:

```
NUnit Adapter 5.0.0.0: Test execution complete
  SnipeItTests test succeeded (19.2s)

Test summary: total: 1, failed: 0, succeeded: 1, skipped: 0, duration: 19.1s
Build succeeded with 4 warning(s) in 20.6s
```
