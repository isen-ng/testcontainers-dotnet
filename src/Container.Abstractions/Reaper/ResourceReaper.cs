using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.Container.Abstractions.Reaper.Filters;

namespace TestContainers.Container.Abstractions.Reaper
{
    /// <summary>
    /// Starts a Ryuk container to kill containers started in this session.
    /// </summary>
    public static class ResourceReaper
    {
        private const string DefaultRyukImage = "quay.io/testcontainers/ryuk:0.2.3";

        /// <summary>
        /// Class label name applied to containers created by this library
        /// </summary>
        public static readonly string TestContainerLabelName = typeof(IContainer).FullName;

        /// <summary>
        /// Session label added to containers created by this library in this particular run
        /// </summary>
        public static readonly string TestContainerSessionLabelName = TestContainerLabelName + ".SessionId";

        /// <summary>
        /// Session id for this particular run
        /// </summary>
        public static readonly string SessionId = Guid.NewGuid().ToString();

        /// <summary>
        /// Labels that needs to be applied to containers for Ryuk to run properly
        /// </summary>
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { TestContainerLabelName, "true" },
            { TestContainerSessionLabelName, SessionId }
        };

        private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);

        private static RyukContainer _ryukContainer;

        private static TaskCompletionSource<bool> _ryukStartupTaskCompletionSource;

        /// <summary>
        /// Starts the resource reaper if it is enabled
        /// </summary>
        /// <param name="dockerClient">Docker client to use</param>
        /// <returns>Task that completes when reaper starts successfully</returns>
        public static async Task StartAsync(IDockerClient dockerClient)
        {
            var disabled = Environment.GetEnvironmentVariable("REAPER_DISABLED");
            if (!string.IsNullOrWhiteSpace(disabled) &&
                (disabled.Equals("1") || disabled.ToLower().Equals("true")))
            {
                return;
            }

            var ryukImage = Environment.GetEnvironmentVariable("REAPER_IMAGE");
            if (string.IsNullOrWhiteSpace(ryukImage))
            {
                ryukImage = DefaultRyukImage;
            }

            if (_ryukStartupTaskCompletionSource == null)
            {
                await InitLock.WaitAsync();

                try
                {
                    if (_ryukStartupTaskCompletionSource == null)
                    {
                        _ryukStartupTaskCompletionSource = new TaskCompletionSource<bool>();
                        _ryukContainer = new RyukContainer(ryukImage, dockerClient, NullLoggerFactory.Instance);

                        var ryukStartupTask = _ryukContainer.StartAsync();
                        await ryukStartupTask.ContinueWith(_ =>
                        {
                            _ryukContainer.AddToDeathNote(new LabelsFilter(Labels));
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

        /// <summary>
        /// Registers a filter to be cleaned up after this process exits
        /// </summary>
        /// <param name="filter">filter</param>
        public static void RegisterFilterForCleanup(IFilter filter)
        {
            _ryukContainer.AddToDeathNote(filter);
        }

        internal static void KillTcpConnection()
        {
            _ryukContainer?.KillTcpConnection();
        }

        internal static void Dispose()
        {
            _ryukContainer?.Dispose();
        }

        internal static Task<bool?> IsConnected()
        {
            return _ryukContainer?.IsConnected();
        }

        internal static string GetRyukContainerId()
        {
            return _ryukContainer?.ContainerId;
        }
    }
}
