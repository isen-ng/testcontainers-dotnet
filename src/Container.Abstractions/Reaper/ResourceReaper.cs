using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Container.Abstractions.Utilities.Platform;

namespace TestContainers.Container.Abstractions.Reaper
{
    public static class ResourceReaper
    {
        public static readonly string TestContainerLabelName = typeof(IContainer).FullName;
        public static readonly string TestContainerSessionLabelName = TestContainerLabelName + ".SessionId";
        public static readonly string SessionId = Guid.NewGuid().ToString();

        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            {TestContainerLabelName, "true"},
            {TestContainerSessionLabelName, SessionId}
        };

        private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);

        private static RyukContainer _ryukContainer;

        private static TaskCompletionSource<bool> _ryukStartupTaskCompletionSource;

        public static async Task StartAsync(IDockerClient dockerClient)
        {
            var disabled = Environment.GetEnvironmentVariable("REAPER_DISABLED");
            if (!string.IsNullOrWhiteSpace(disabled) &&
                (disabled.Equals("1") || disabled.ToLower().Equals("true")))
            {
                return;
            }

            // todo: Remove this when ryuk for windows is complete
            var platformSpecificFactory = new PlatformSpecificFactory();
            if (platformSpecificFactory.IsWindows())
            {
                // don't start resource reaper for windows
                // because ryuk for windows is not developed yet
                return;
            }
            
            if (_ryukStartupTaskCompletionSource == null)
            {
                await InitLock.WaitAsync();

                try
                {
                    if (_ryukStartupTaskCompletionSource == null)
                    {
                        _ryukStartupTaskCompletionSource = new TaskCompletionSource<bool>();
                        _ryukContainer = new RyukContainer(dockerClient, platformSpecificFactory.Create());

                        var ryukStartupTask = _ryukContainer.StartAsync();
                        await ryukStartupTask.ContinueWith(_ =>
                        {
                            _ryukContainer.AddToDeathNote(Labels);
                            _ryukStartupTaskCompletionSource.SetResult(true);
                        });
                    }
                }
                finally
                {
                    InitLock.Release();
                }
            }

            await _ryukStartupTaskCompletionSource.Task;
        }

        internal static void KillTcpConnectionAsync()
        {
            _ryukContainer?.KillTcpConnection();
        }

        internal static string GetRyukContainerId()
        {
            return _ryukContainer?.ContainerId;
        }
    }
}