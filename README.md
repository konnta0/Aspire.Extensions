# Aspire.Extensions

A collection of community extensions for .NET Aspire.

## Packages

### AspireIntegration.Hosting.RustFs

[![NuGet](https://img.shields.io/nuget/v/AspireIntegration.Hosting.RustFs.svg)](https://www.nuget.org/packages/AspireIntegration.Hosting.RustFs)

Aspire hosting integration for RustFs - A lightweight S3-compatible object storage server written in Rust.

[View Package Documentation](./src/Aspire.Hosting.RustFs/README.md)

## Getting Started

### Installation

```bash
dotnet add package AspireIntegration.Hosting.RustFs
```

### Usage

```csharp
using Aspire.Hosting.RustFs;

var builder = DistributedApplication.CreateBuilder(args);

var rustfs = builder.AddRustFs("rustfs")
    .WithDataVolume()
    .AddBucket("my-bucket");

builder.Build().Run();
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/konnta0/Aspire.Extensions.git
cd Aspire.Extensions

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

## License

This project is licensed under the MIT License.