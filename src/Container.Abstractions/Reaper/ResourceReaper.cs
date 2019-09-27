using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
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
            {TestContainerLabelName, "true"}, {TestContainerSessionLabelName, SessionId}
        };

        private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);

        private static readonly HashSet<string> ImagesToDelete = new HashSet<string>();

        private static readonly object ShutdownHookRegisterLock = new object();

        private static RyukContainer _ryukContainer;

        private static TaskCompletionSource<bool> _ryukStartupTaskCompletionSource;

        private static volatile bool _shutdownHookRegistered;

        /// <summary>
        /// Starts the resource reaper if it is enabled
        /// </summary>
        /// <param name="dockerClient">Docker client to use</param>
        /// <param name="logger">Optional logger to log progress</param>
        /// <returns>Task that completes when reaper starts successfully</returns>
        public static async Task StartAsync(IDockerClient dockerClient, ILogger logger = null)
        {
            var disabled = Environment.GetEnvironmentVariable("REAPER_DISABLED");
            if (!string.IsNullOrWhiteSpace(disabled) &&
                (disabled.Equals("1") || disabled.ToLower().Equals("true")))
            {
                logger?.LogInformation("Reaper is disabled via $REAPER_DISABLED environment variable");
                return;
            }

            var ryukImage = Environment.GetEnvironmentVariable("REAPER_IMAGE");
            if (string.IsNullOrWhiteSpace(ryukImage))
            {
                ryukImage = DefaultRyukImage;
            }

            if (_ryukStartupTaskCompletionSource == null)
            {
                logger?.LogTrace("Entering reaper init lock ...");

                await InitLock.WaitAsync();

                logger?.LogTrace("Entered reaper init lock");

                try
                {
                    if (_ryukStartupTaskCompletionSource == null)
                    {
                        logger?.LogDebug("Starting ryuk container ...");

                        _ryukStartupTaskCompletionSource = new TaskCompletionSource<bool>();
                        _ryukContainer = new RyukContainer(ryukImage, dockerClient, NullLoggerFactory.Instance);

                        var ryukStartupTask = _ryukContainer.StartAsync();
                        await ryukStartupTask.ContinueWith(_ =>
                        {
                            _ryukContainer.AddToDeathNote(new LabelsFilter(Labels));
                            _ryukStartupTaskCompletionSource.SetResult(true);

                            logger?.LogDebug("Started ryuk container");
                        });
                    }
                    else
                    {
                        logger?.LogDebug("Reaper is already started");
                    }
                }
                finally
                {
                    logger?.LogTrace("Releasing reaper init lock ...");

                    InitLock.Release();

                    logger?.LogTrace("Released reaper init lock");
                }
            }
            else
            {
                logger?.LogDebug("Reaper is already started");
            }

            SetupShutdownHook(dockerClient);

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

        /// <summary>
        /// Registers an image name to be cleaned up when this process exits
        /// </summary>
        /// <param name="imageName">image name to be deleted</param>
        /// <param name="dockerClient">docker client to be used for running the commands in the shutdown hook</param>
        public static void RegisterImageForCleanup(string imageName, IDockerClient dockerClient)
        {
            SetupShutdownHook(dockerClient);

            // todo: update ryuk to support image clean up
            // issue: https://github.com/testcontainers/moby-ryuk/issues/6
            ImagesToDelete.Add(imageName);
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

        private static void SetupShutdownHook(IDockerClient dockerClient)
        {
            if (_shutdownHookRegistered)
            {
                return;
            }

            lock (ShutdownHookRegisterLock)
            {
                if (_shutdownHookRegistered)
                {
                    return;
                }

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => PerformCleanup(dockerClient).Wait();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    PerformCleanup(dockerClient).Wait();

                    // don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                _shutdownHookRegistered = true;
            }
        }

        private static async Task PerformCleanup(IDockerClient dockerClient)
        {
            var imageDeleteParameters = new ImageDeleteParameters
            {
                Force = true,
                // this is actually a badly named variable, it means `noprune` instead of `pleaseprune`
                // this is fixed in https://github.com/microsoft/Docker.DotNet/pull/316 but there hasn't
                // been a release for a very long time (issue still exists in 3.125.2).
                PruneChildren = false
            };

            await Task.WhenAll(
                ImagesToDelete.Select(i => dockerClient.Images.DeleteImageAsync(i, imageDeleteParameters)));
        }
    }
}
