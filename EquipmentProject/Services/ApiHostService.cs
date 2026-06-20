using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EquipmentProject.Services
{
    public sealed class ApiHostService : IDisposable
    {
        private static readonly Uri HealthUri = new("http://127.0.0.1:5028/health");

        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly ILogger<ApiHostService> _logger;

        private Process? _ownedProcess;
        private bool _disposed;

        public ApiHostService(ILogger<ApiHostService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> EnsureApiReadyAsync(CancellationToken cancellationToken = default)
        {
            if (await IsApiHealthyAsync(cancellationToken))
            {
                return true;
            }

            await _gate.WaitAsync(cancellationToken);

            try
            {
                if (await IsApiHealthyAsync(cancellationToken))
                {
                    return true;
                }

                if (!OperatingSystem.IsWindows())
                {
                    _logger.LogInformation("Skipping local API auto-start because the current OS is not Windows.");
                    return false;
                }

                var launchCommand = GetLaunchCommand();
                if (launchCommand == null)
                {
                    _logger.LogWarning("Could not find the copied API host files next to the desktop app.");
                    return false;
                }

                if (_ownedProcess == null || _ownedProcess.HasExited)
                {
                    _ownedProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = launchCommand.FileName,
                        Arguments = launchCommand.Arguments,
                        WorkingDirectory = launchCommand.WorkingDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }

                return await WaitForApiAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while trying to start or reach the local API host.");
                return false;
            }
            finally
            {
                _gate.Release();
            }
        }

        public void StopOwnedProcess()
        {
            try
            {
                if (_ownedProcess is { HasExited: false })
                {
                    _ownedProcess.Kill(entireProcessTree: true);
                    _ownedProcess.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not stop the local API host cleanly.");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopOwnedProcess();
            _gate.Dispose();
            _disposed = true;
        }

        private async Task<bool> WaitForApiAsync(CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < 15; attempt++)
            {
                if (await IsApiHealthyAsync(cancellationToken))
                {
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            _logger.LogWarning("The local API host did not become healthy within the expected startup time.");
            return false;
        }

        private static async Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            try
            {
                using var response = await client.GetAsync(HealthUri, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static LaunchCommand? GetLaunchCommand()
        {
            var apiHostDirectory = Path.Combine(AppContext.BaseDirectory, "ApiHost");
            var apiExecutable = Path.Combine(apiHostDirectory, "EquipmentAPI.exe");
            if (File.Exists(apiExecutable))
            {
                return new LaunchCommand(apiExecutable, "--urls http://127.0.0.1:5028", apiHostDirectory);
            }

            var apiAssembly = Path.Combine(apiHostDirectory, "EquipmentAPI.dll");
            if (File.Exists(apiAssembly))
            {
                return new LaunchCommand("dotnet", $"\"{apiAssembly}\" --urls http://127.0.0.1:5028", apiHostDirectory);
            }

            return null;
        }

        private sealed record LaunchCommand(string FileName, string Arguments, string WorkingDirectory);
    }
}
