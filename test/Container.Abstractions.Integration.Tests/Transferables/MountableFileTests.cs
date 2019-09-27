using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Transferables.Fixtures;
using Container.Test.Utility;
using ICSharpCode.SharpZipLib.Tar;
using TestContainers.Container.Abstractions.Transferables;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Transferables
{
    [Collection(MountableFileTestCollection.CollectionName)]
    public class MountableFileTests
    {
        private readonly MountableFileFixture _fixture;

        public MountableFileTests(MountableFileFixture fixture)
        {
            _fixture = fixture;
        }

        public class ConstructorTests : MountableFileTests
        {
            public ConstructorTests(MountableFileFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public void ShouldThrowArgumentNullExceptionWhenPathIsNull()
            {
                // act
                var ex = Record.Exception(() => new MountableFile(null));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class GetSizeTests : MountableFileTests
        {
            public GetSizeTests(MountableFileFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public void ShouldReturnSizeOfFileIfItExists()
            {
                // arrange
                var mountableFile = new MountableFile(_fixture.TempFilePath);

                // act
                var actual = mountableFile.GetSize();

                // assert
                Assert.Equal(_fixture.TempFileLengthInBytes, actual);
            }

            [Fact]
            public void ShouldThrowFileNotFoundExceptionIfFileDoesNotExist()
            {
                // arrange
                var mountableFile = new MountableFile("/does/not/exist/path");

                // act
                var ex = Record.Exception(() => mountableFile.GetSize());

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }
        }

        public class TransferToTests : MountableFileTests
        {
            private readonly MountableFile _mountableFile;

            public TransferToTests(MountableFileFixture fixture) : base(fixture)
            {
                _mountableFile = new MountableFile(fixture.TempFilePath);
            }

            [Fact]
            public async Task ShouldThrowArgumentNullExceptionIfTarArchiveIsNull()
            {
                // act
                var ex = await Record.ExceptionAsync(async () => await _mountableFile.TransferTo(null, "my_file"));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }

            [Fact]
            public async Task ShouldThrowArgumentNullExceptionIfDestinationIsNull()
            {
                // arrange
                var memoryStream = new MemoryStream();
                var tarArchive = TarArchive.CreateOutputTarArchive(memoryStream);

                // act
                var ex = await Record.ExceptionAsync(async () => await _mountableFile.TransferTo(tarArchive, null));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class TransferFromFileTests : MountableFileTests
        {
            private readonly string _tarFilePath;
            private readonly FileStream _tarFileStream;
            private readonly TarArchive _tarWriteStream;

            public TransferFromFileTests(MountableFileFixture fixture) : base(fixture)
            {
                _tarFilePath = Path.GetTempFileName();
                _tarFileStream = new FileStream(_tarFilePath, FileMode.Create);
                _tarWriteStream = TarArchive.CreateOutputTarArchive(_tarFileStream);
            }

            ~TransferFromFileTests()
            {
                _tarFileStream.Dispose();
                File.Delete(_tarFilePath);
            }

            [Fact]
            public async Task ShouldThrowFileNotFoundExceptionIfFileDoesNotExist()
            {
                // arrange
                var mountableFile = new MountableFile("/does/not/exist/path");

                // act
                var ex = await Record.ExceptionAsync(async () =>
                    await mountableFile.TransferTo(_tarWriteStream, "no"));

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }

            [Fact]
            public async Task ShouldTransferRecursivelyToArchiveIfFolderExists()
            {
                // arrange
                var mountableFile = new MountableFile(_fixture.TempFilePath);
                var destinationFileName = Path.GetFileName(_fixture.TempFilePath);

                // act
                await mountableFile.TransferTo(_tarWriteStream, destinationFileName);
                _tarWriteStream.Close();

                // assert
                using (var tarFileStream = new FileStream(_tarFilePath, FileMode.Open))
                using (var tarReadStream = TarArchive.CreateInputTarArchive(tarFileStream))
                {
                    var extractionPath = Path.GetTempPath() + "/extracted";
                    Directory.CreateDirectory(extractionPath);
                    tarReadStream.ExtractContents(extractionPath);

                    var expected = new FileInfo(_fixture.TempFilePath);
                    var actual = new FileInfo(extractionPath + "/" + destinationFileName);

                    Assert.Equal(expected, actual, new FileComparer());

                    Directory.Delete(extractionPath, true);
                }
            }
        }

        public class TransferFromFolderTests : MountableFileTests
        {
            private readonly string _tarFilePath;
            private readonly FileStream _tarFileStream;
            private readonly TarArchive _tarWriteStream;

            public TransferFromFolderTests(MountableFileFixture fixture) : base(fixture)
            {
                _tarFilePath = Path.GetTempFileName();
                _tarFileStream = new FileStream(_tarFilePath, FileMode.Create);
                _tarWriteStream = TarArchive.CreateOutputTarArchive(_tarFileStream);
            }

            ~TransferFromFolderTests()
            {
                _tarFileStream.Dispose();
                File.Delete(_tarFilePath);
            }

            [Fact]
            public async Task ShouldThrowFileNotFoundExceptionIfFolderDoesNotExist()
            {
                // arrange
                var mountableFile = new MountableFile("/does/not/exist/path");

                // act
                var ex = await Record.ExceptionAsync(async () =>
                    await mountableFile.TransferTo(_tarWriteStream, "."));

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }

            [Fact]
            public async Task ShouldTransferRecursivelyToArchiveIfFolderExists()
            {
                // arrange
                var mountableFile = new MountableFile(_fixture.TempFolderPath);

                // act
                await mountableFile.TransferTo(_tarWriteStream, ".");
                _tarWriteStream.Close();

                // assert
                using (var tarFileStream = new FileStream(_tarFilePath, FileMode.Open))
                using (var tarReadStream = TarArchive.CreateInputTarArchive(tarFileStream))
                {
                    var extractionPath = Path.GetTempPath() + "/extracted";
                    Directory.CreateDirectory(extractionPath);
                    tarReadStream.ExtractContents(extractionPath);

                    AssertDirectoryEquals(_fixture.TempFolderPath, extractionPath);

                    Directory.Delete(extractionPath, true);
                }
            }

            private static void AssertDirectoryEquals(string expectedPath, string actualPath)
            {
                var expectedDirectory = new DirectoryInfo(expectedPath);
                var actualDirectory = new DirectoryInfo(actualPath);

                var expectedFiles = expectedDirectory.GetFiles("*", SearchOption.AllDirectories);
                var actualFiles = actualDirectory.GetFiles("*", SearchOption.AllDirectories);

                var areIdentical = expectedFiles.SequenceEqual(actualFiles, new FileComparer());

                Assert.True(areIdentical);
            }
        }
    }
}
