using FilePrepper.Tasks.AddColumns;
using FilePrepper.Tasks;
using Xunit.Abstractions;

namespace FilePrepper.Tests.Tasks;

public class AddColumnsTests : TaskBaseTest<AddColumnsTask, AddColumnsValidator>
{
    public AddColumnsTests(ITestOutputHelper output) : base(output)
    {
        // 테스트 입력 파일 생성
        File.WriteAllText(_testInputPath, "Id,Name\n1,John\n2,Jane");
    }

    [Fact]
    public void Execute_WithValidOptions_ShouldSucceed()
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = new Dictionary<string, string> { { "Age", "30" } }
        };

        var task = new AddColumnsTask(options, _mockLogger.Object);
        var context = new TaskContext
        {
            InputPath = _testInputPath,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_testOutputPath));
        string[] lines = File.ReadAllLines(_testOutputPath);
        Assert.Equal("Id,Name,Age", lines[0]);
        Assert.Equal("1,John,30", lines[1]);
        Assert.Equal("2,Jane,30", lines[2]);
    }

    [Fact]
    public void Validate_WithEmptyColumns_ShouldReturnError()
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = new Dictionary<string, string>()
        };

        // Act
        string[] errors = options.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Equal("At least one new column must be specified", errors[0]);
    }

    [Fact]
    public void Validate_WithNullColumns_ShouldReturnError()
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = null
        };

        // Act
        string[] errors = options.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Equal("At least one new column must be specified", errors[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidColumnName_ShouldReturnError(string? columnName)
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = columnName is not null
                ? new Dictionary<string, string> { { columnName, "value" } }
                : null
        };

        // Act
        string[] errors = options.Validate();

        // Assert
        Assert.Contains(errors, e => e.Contains("Column name cannot be empty or whitespace") ||
                                   e.Contains("At least one new column must be specified"));
    }

    [Fact]
    public void Execute_WithDuplicateColumnName_ShouldReturnError()
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = new Dictionary<string, string> { { "Name", "Test" } }  // Name already exists in input file
        };

        var task = new AddColumnsTask(options, _mockLogger.Object);
        var context = new TaskContext
        {
            InputPath = _testInputPath,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Execute_WithMultipleNewColumns_ShouldSucceed()
    {
        // Arrange
        var options = new AddColumnsOption
        {
            NewColumns = new Dictionary<string, string>
            {
                { "Age", "30" },
                { "City", "Seoul" },
                { "Country", "Korea" }
            }
        };

        var task = new AddColumnsTask(options, _mockLogger.Object);
        var context = new TaskContext
        {
            InputPath = _testInputPath,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        string[] lines = File.ReadAllLines(_testOutputPath);
        Assert.Equal("Id,Name,Age,City,Country", lines[0]);
        Assert.Equal("1,John,30,Seoul,Korea", lines[1]);
        Assert.Equal("2,Jane,30,Seoul,Korea", lines[2]);
    }
}