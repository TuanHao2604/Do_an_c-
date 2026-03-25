using SmartTourApp.Application.Services;

namespace SmartTourApp.API;

/// <summary>
/// Bridges IWebHostEnvironment.WebRootPath to the Application layer's IWebHostEnvironmentAccessor.
/// </summary>
public class WebHostEnvAccessor : IWebHostEnvironmentAccessor
{
    public string WebRootPath { get; }
    public WebHostEnvAccessor(string webRootPath) => WebRootPath = webRootPath;
}
