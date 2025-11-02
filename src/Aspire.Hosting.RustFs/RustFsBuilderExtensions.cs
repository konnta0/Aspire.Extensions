using System.Text;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.RustFs;

/// <summary>
/// Extension methods for adding and configuring RustFs resources in a distributed application.
/// </summary>
public static class RustFsBuilderExtensions
{
    /// <summary>
    /// Adds a RustFs resource to the distributed application builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="accessKey"></param>
    /// <param name="secretKey"></param>
    /// <param name="port"></param>
    /// <param name="consolePort"></param>
    /// <returns></returns>
    public static IResourceBuilder<RustFsResource> AddRustFs(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? accessKey = null,
        IResourceBuilder<ParameterResource>? secretKey = null,
        int? port = null,
        int? consolePort = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var accessKeyParameter = accessKey?.Resource ??
                                 ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder,
                                     $"{name}-accessKey");
        var secretKeyParameter = secretKey?.Resource ??
                                 ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-secretKey");

        var resource = new RustFsResource(name, accessKeyParameter, secretKeyParameter);

        var b = builder.AddResource(resource)
            .WithImage("rustfs/rustfs")
            .WithImageRegistry("docker.io")
            .WithHttpEndpoint(name: RustFsResource.PrimaryEndpointName, port: port,
                targetPort: RustFsResource.PrimaryTargetPort)
            .WithHttpEndpoint(name: RustFsResource.ConsoleEndpointName, port: consolePort,
                targetPort: RustFsResource.ConsoleTargetPort)
            .WithUrlForEndpoint(RustFsResource.PrimaryEndpointName, annot =>
            {
                annot.DisplayText = "Primary";
            })
            .WithUrlForEndpoint(RustFsResource.ConsoleEndpointName, annot =>
            {
                annot.DisplayText = "Console";
            })
            .WithEnvironment("STORAGE_TYPE", "rustfs")
            .WithEnvironment("RUSTFS_ADDRESS", $":{RustFsResource.PrimaryTargetPort.ToString()}")
            .WithEnvironment("RUSTFS_CONSOLE_ADDRESS", $":{RustFsResource.ConsoleTargetPort.ToString()}")
            .WithEnvironment("RUSTFS_ACCESS_KEY", resource.AccessKey)
            .WithEnvironment("RUSTFS_SECRET_KEY", resource.SecretKey)
            .WithHttpHealthCheck("/health", 200, RustFsResource.PrimaryEndpointName);

        return b;
    }

    /// <summary>
    /// Adds a data volume to the RustFs resource.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IResourceBuilder<RustFsResource> WithDataVolume(this IResourceBuilder<RustFsResource> builder, string? name = null)
    {
        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/data");
    }

    /// <summary>
    /// Adds a bucket to the RustFs resource.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    public static IResourceBuilder<ContainerResource> AddBucket(this IResourceBuilder<RustFsResource> builder, string bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
        }

        return builder.AddBucket(
            name: $"{builder.Resource.Name}-create-bucket-{bucketName}",
            bucketNames: [bucketName]
        );
    }

    /// <summary>
    /// Adds a bucket to the RustFs resource.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="bucketNames"></param>
    /// <returns></returns>
    public static IResourceBuilder<ContainerResource> AddBucket(this IResourceBuilder<RustFsResource> builder, IReadOnlyList<string> bucketNames)
    {
        if (bucketNames is null || bucketNames.Count is 0)
        {
            throw new ArgumentException("Bucket names cannot be null or empty.", nameof(bucketNames));
        }

        return builder.AddBucket(
            name: $"{builder.Resource.Name}-create-buckets-{bucketNames[0]}",
            bucketNames: bucketNames
        );
    }

    private static IResourceBuilder<ContainerResource> AddBucket(
        this IResourceBuilder<RustFsResource> builder,
        [ResourceName] string name,
        IReadOnlyList<string> bucketNames)
    {
        return builder.ApplicationBuilder
            .AddContainer(name, "minio/mc")
            .WithImageRegistry("docker.io")
            .WithParentRelationship(builder)
            .WaitFor(builder)
            .WithEntrypoint("/bin/sh")
            .WithArgs(async ctx =>
            {
                var rustFsResource = builder.Resource;

                var accessKey = await rustFsResource.AccessKey.GetValueAsync(ctx.CancellationToken);
                var secretKey = await rustFsResource.SecretKey.GetValueAsync(ctx.CancellationToken);

                var sb = new StringBuilder();

                sb.Append($"mc alias set rustfs {GetRustFsPrimaryUri(rustFsResource)} '{accessKey}' '{secretKey}';");

                foreach (var bucket in bucketNames)
                {
                    if (string.IsNullOrWhiteSpace(bucket))
                    {
                        continue;
                    }

                    sb.Append($"mc mb rustfs/{bucket} --ignore-existing;");
                }

                ctx.Args.Add("-c");
                ctx.Args.Add(sb.ToString());
            });

        static string GetRustFsPrimaryUri(RustFsResource rustFs)
        {
            var endpoint = rustFs.GetEndpoint(RustFsResource.PrimaryEndpointName);
            return $"{endpoint.Scheme}://{rustFs.Name}:{endpoint.TargetPort}";
        }
    }
}