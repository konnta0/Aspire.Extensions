# AspireIntegration.Hosting.RustFs

Aspire hosting integration for [RustFs](https://github.com/s3rius/rustfs) - A lightweight S3-compatible object storage server written in Rust.

## Installation

```bash
dotnet add package AspireIntegration.Hosting.RustFs
```

## Usage

### Basic Setup

```csharp
using Aspire.Hosting.RustFs;

var builder = DistributedApplication.CreateBuilder(args);

var rustfs = builder.AddRustFs("rustfs")
    .WithDataVolume();

builder.Build().Run();
```

### With Custom Credentials

```csharp
var rustfs = builder.AddRustFs("rustfs",
    accessKey: builder.AddParameter("rustfs-access-key"),
    secretKey: builder.AddParameter("rustfs-secret-key"),
    port: 9110,
    consolePort: 9111)
    .WithDataVolume();
```

### Creating Buckets

```csharp
var rustfs = builder.AddRustFs("rustfs")
    .WithDataVolume()
    .AddBucket("my-bucket");

// Or create multiple buckets
rustfs.AddBucket(["bucket1", "bucket2", "bucket3"]);
```

## Features

- S3-compatible API
- Lightweight and fast (written in Rust)
- Easy bucket creation
- Data persistence with volumes
- Health check integration
- Console UI for management

## Configuration

The following environment variables are automatically configured:

- `STORAGE_TYPE`: Set to "rustfs"
- `RUSTFS_ADDRESS`: Primary endpoint address
- `RUSTFS_CONSOLE_ADDRESS`: Console UI endpoint address
- `RUSTFS_ACCESS_KEY`: S3 API access key
- `RUSTFS_SECRET_KEY`: S3 API secret key

## Endpoints

- **Primary Endpoint (9000)**: S3-compatible API
- **Console Endpoint (9001)**: Web-based management console

## License

MIT