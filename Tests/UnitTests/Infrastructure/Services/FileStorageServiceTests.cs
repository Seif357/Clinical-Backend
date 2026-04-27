using FakeItEasy;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace UnitTests.Infrastructure.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly ILogger<FileStorageService> _loggerFake;
    private readonly FileStorageService _sut; // SUT = System Under Test
    private readonly string _testWwwrootPath;

    public FileStorageServiceTests()
    {
        // 1. Arrange - Setup Dependencies
        _loggerFake = A.Fake<ILogger<FileStorageService>>();

        // Set the wwwroot path inside the test execution directory (bin folder)
        _testWwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // Clean up any residual files from previous test runs before starting
        CleanUpDirectory();

        // Instantiating the SUT will automatically create the base directories
        _sut = new FileStorageService(_loggerFake);
    }

    // This method is called automatically after each test case to clean up files (IDisposable implementation)
    public void Dispose()
    {
        CleanUpDirectory();
    }

    private void CleanUpDirectory()
    {
        if (Directory.Exists(_testWwwrootPath))
        {
            Directory.Delete(_testWwwrootPath, true);
        }
    }

    #region SaveFileAsync Tests

    [Fact]
    public async Task SaveFileAsync_ValidFile_SavesFileAndReturnsRelativePath()
    {
        // Arrange
        var folderName = "test-folder";
        var fileName = "sample-image.jpg";
        var fileContent = "dummy file content";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);

        // Mocking IFormFile using FakeItEasy
        var fileFake = A.Fake<IFormFile>();
        A.CallTo(() => fileFake.FileName).Returns(fileName);
        A.CallTo(() => fileFake.Length).Returns(fileBytes.Length);

        // Simulate the file copy process
        A.CallTo(() => fileFake.CopyToAsync(A<Stream>._, A<CancellationToken>._))
            .Invokes((Stream stream, CancellationToken _) =>
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SaveFileAsync(fileFake, folderName);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().StartWith("uploads/histopathology/test-folder");
        result.Should().EndWith(".jpg");

        // Verify that the file was actually created on the disk in the test directory
        var fullPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result);
        File.Exists(fullPhysicalPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_NullFile_ThrowsNullReferenceException()
    {
        // Arrange
        IFormFile nullFile = null!;
        var folderName = "test-folder";

        // Act
        Func<Task> action = async () => await _sut.SaveFileAsync(nullFile, folderName);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    #endregion

    #region FileExistsAsync Tests

    [Fact]
    public async Task FileExistsAsync_FileExists_ReturnsTrue()
    {
        // Arrange
        var relativePath = "uploads/test-file.txt";
        var fullPhysicalPath = Path.Combine(_testWwwrootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPhysicalPath)!);
        await File.WriteAllTextAsync(fullPhysicalPath, "test content");

        // Act
        var result = await _sut.FileExistsAsync(relativePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var relativePath = "uploads/non-existing-file.txt";

        // Act
        var result = await _sut.FileExistsAsync(relativePath);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ReadFileAsync Tests

    [Fact]
    public async Task ReadFileAsync_FileExists_ReturnsByteArray()
    {
        // Arrange
        var relativePath = "uploads/test-read.txt";
        var fullPhysicalPath = Path.Combine(_testWwwrootPath, relativePath);
        var expectedContent = "hello world";

        Directory.CreateDirectory(Path.GetDirectoryName(fullPhysicalPath)!);
        await File.WriteAllTextAsync(fullPhysicalPath, expectedContent);

        // Act
        var resultBytes = await _sut.ReadFileAsync(relativePath);

        // Assert
        resultBytes.Should().NotBeNull();
        var resultString = Encoding.UTF8.GetString(resultBytes);
        resultString.Should().Be(expectedContent);
    }

    [Fact]
    public async Task ReadFileAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var relativePath = "uploads/missing-file.txt";

        // Act
        Func<Task> action = async () => await _sut.ReadFileAsync(relativePath);

        // Assert
        // Ensure the error message contains the specific file name
        await action.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"*{relativePath}*");
    }

    #endregion

    #region DeleteFileAsync Tests

    [Fact]
    public async Task DeleteFileAsync_FileExists_DeletesFileAndReturnsTrue()
    {
        // Arrange
        var relativePath = "uploads/test-delete.txt";
        var fullPhysicalPath = Path.Combine(_testWwwrootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPhysicalPath)!);
        await File.WriteAllTextAsync(fullPhysicalPath, "to be deleted");

        // Act
        var result = await _sut.DeleteFileAsync(relativePath);

        // Assert
        result.Should().BeTrue();

        // Ensure the file is successfully deleted from the disk
        File.Exists(fullPhysicalPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var relativePath = "uploads/already-missing.txt";

        // Act
        var result = await _sut.DeleteFileAsync(relativePath);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}