using Aspire.Hosting.RustFs;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddRustFs("rustfs",
    port: 9110,
    consolePort: 9111
    )
    .WithDataVolume()
    .AddBucket("resumebucket");
builder.Build().Run();