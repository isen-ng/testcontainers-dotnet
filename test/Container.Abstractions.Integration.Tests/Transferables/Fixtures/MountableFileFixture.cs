using System;
using System.IO;
using System.Threading.Tasks;
using TestContainers.Container.Abstractions.Utilities;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Transferables.Fixtures
{
    public class MountableFileFixture : IAsyncLifetime
    {
        private static readonly Random Random = new Random();

        public string TempFolderPath { get; }

        public string TempFilePath { get; }

        public long TempFileLengthInBytes { get; }

        public MountableFileFixture()
        {
            TempFolderPath = Path.GetTempPath() + "/" + Random.NextAlphaNumeric(32);
            TempFilePath = Path.GetTempFileName();
            TempFileLengthInBytes = Random.Next(1, 9999);
        }

        public Task InitializeAsync()
        {
            var content = new byte[TempFileLengthInBytes];
            Random.NextBytes(content);

            File.WriteAllBytes(TempFilePath, content);

            var nestedTempDirectory = Directory.CreateDirectory(TempFolderPath + "/dummy");
            File.WriteAllBytes(nestedTempDirectory + "/temp.1", content);
            File.WriteAllBytes(TempFolderPath + "/temp.2", content);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            File.Delete(TempFilePath);
            Directory.Delete(TempFolderPath, true);
            return Task.CompletedTask;
        }
    }
}
