using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.RustFs;

/// <summary>
/// Represents a RustFs resource in a distributed application.
/// </summary>
/// <param name="name"></param>
/// <param name="accessKey"></param>
/// <param name="secretKey"></param>
public sealed class RustFsResource(string name, ParameterResource accessKey, ParameterResource secretKey)
    : ContainerResource(name)
{
    /// <summary>
    /// The name of the primary endpoint.
    /// </summary>
    public const string PrimaryEndpointName = "http";
    /// <summary>
    /// The name of the console endpoint.
    /// </summary>
    public const string ConsoleEndpointName = "console";

    internal const int PrimaryTargetPort = 9000;
    internal const int ConsoleTargetPort = 9001;

    /// <summary>
    /// Gets the access key parameter resource for RustFs.
    /// </summary>
    public ParameterResource AccessKey { get; } = accessKey;

    /// <summary>
    /// Gets the secret key parameter resource for RustFs.
    /// </summary>
    public ParameterResource SecretKey { get; } = secretKey;
}