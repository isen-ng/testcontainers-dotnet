using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Listeners;
using uhttpsharp.Logging;
using uhttpsharp.RequestProviders;
using Xunit;

namespace Containers.Integration.Tests.Fixtures
{
    public class HttpServerFixture : IAsyncLifetime
    {
        static HttpServerFixture()
        {
            LogProvider.LogProviderResolvers.Clear();
            LogProvider.LogProviderResolvers.Add(
                new Tuple<LogProvider.IsLoggerAvailable, LogProvider.CreateLogProvider>(() => true,
                    () => NullLoggerProvider.Instance));
        }

        private readonly HttpServer _server;
        private readonly TcpListener _tcpListener;

        public string DefaultResponse { get; } = "hello world";

        public int Port => ((IPEndPoint) _tcpListener.LocalEndpoint).Port;

        public HttpServerFixture()
        {
            _tcpListener = new TcpListener(IPAddress.Loopback, 0);

            _server = new HttpServer(new HttpRequestProvider());
            _server.Use(new TcpListenerAdapter(_tcpListener));
            _server.Use((context, next) =>
            {
                context.Response = new HttpResponse(HttpResponseCode.Ok, DefaultResponse, false);
                return Task.Factory.GetCompleted();
            });
        }

        public Task InitializeAsync()
        {
            return Task.Run(() => _server.Start());
        }

        public Task DisposeAsync()
        {
            return Task.Run(() => _server.Dispose());
        }

        public class NullLoggerProvider : ILogProvider
        {
            public static readonly NullLoggerProvider Instance = new NullLoggerProvider();

            private static readonly ILog NullLogInstance = new NullLog();

            public ILog GetLogger(string name)
            {
                return NullLogInstance;
            }

            public IDisposable OpenNestedContext(string message)
            {
                return null;
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                return null;
            }

            private class NullLog : ILog
            {
                public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null,
                    params object[] formatParameters)
                {
                    return true;
                }
            }
        }
    }
}