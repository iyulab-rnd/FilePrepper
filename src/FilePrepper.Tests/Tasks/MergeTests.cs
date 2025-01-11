using FilePrepper.Tasks;
using FilePrepper.Tasks.Merge;
using Xunit.Abstractions;

namespace FilePrepper.Tests.Tasks;

public class MergeTests : TaskBaseTest<MergeTask, MergeValidator>
{
    public MergeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Validate_WithFewerThanTwoPaths_ShouldReturnError()
    {
        // Arrange
        var option = new MergeOption
        {
            MergeType = MergeType.Vertical,
            InputPaths = new List<string> { "OnlyOneFile.csv" }
        };

        // Act
        var errors = option.Validate();

        // Assert
        Assert.Contains(errors, e => e.Contains("At least two input files must be specified"));
    }

    [Fact]
    public void Validate_WithEmptyFilePath_ShouldReturnError()
    {
        // Arrange
        var option = new MergeOption
        {
            MergeType = MergeType.Vertical,
            InputPaths = new List<string> { "File1.csv", "" }
        };

        // Act
        var errors = option.Validate();

        // Assert
        Assert.Contains(errors, e => e.Contains("cannot be empty or whitespace"));
    }

    [Fact]
    public void Validate_HorizontalNoJoinKey_ShouldReturnError()
    {
        // Arrange
        var option = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            InputPaths = new List<string> { "File1.csv", "File2.csv" },
            JoinKeyColumns = new() // 비어 있음
        };

        // Act
        var errors = option.Validate();

        // Assert
        Assert.Contains(errors, e => e.Contains("At least one join key column must be specified"));
    }

    [Fact]
    public void Validate_NoErrors_ShouldSucceed()
    {
        // Arrange
        var option = new MergeOption
        {
            MergeType = MergeType.Vertical,
            InputPaths = new List<string> { "File1.csv", "File2.csv" }
        };

        // Act
        var errors = option.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Execute_VerticalMerge_WithMismatchColumns_ShouldSucceed()
    {
        // Arrange
        // 첫 번째 CSV: Id,Name,Score
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Id,Name,Score",
            "1,John,85",
            "2,Jane,90"
        });

        // 두 번째 CSV: Id,Name,Age => 컬럼 다름
        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Id,Name,Age",
            "3,Mary,25",
            "4,Tom,30"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Vertical,
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);
        var context = new TaskContext
        {
            InputPath = file1,  // 실제로는 사용되지 않음
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // 헤더 + 4행 = 5줄
        Assert.Equal(5, lines.Length);

        // 첫 번째 파일 (Score 컬럼), 두 번째 파일 (Age 컬럼)이 각각 존재해야 함
        // 1번째 CSV 행: Age 컬럼은 빈 문자열, 2번째 CSV 행: Score 컬럼은 빈 문자열
        Assert.Contains("1,John,85,", lines[1]);
        Assert.Contains("3,Mary,,25", lines[3]);  // Score 자리에 공백
    }

    [Fact]
    public void Execute_HorizontalMerge_InnerJoin_ShouldSucceed()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Id,Feature1",
            "1,A",
            "2,B",
            "3,C"
        });

        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Id,Feature2",
            "2,X",
            "3,Y",
            "4,Z"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            JoinType = JoinType.Inner,
            JoinKeyColumns = new List<string> { "Id" },
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);

        var context = new TaskContext
        {
            InputPath = file1,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // Inner Join => Id=2, Id=3
        Assert.Equal(3, lines.Length); // 헤더 + 2행
        Assert.Contains("2,B,X", lines[1]);
        Assert.Contains("3,C,Y", lines[2]);
    }

    [Fact]
    public void Execute_HorizontalMerge_LeftJoin_ShouldSucceed()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Id,Feature1",
            "1,A",
            "2,B",
            "3,C"
        });

        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Id,Feature2",
            "2,X",
            "3,Y",
            "4,Z"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            JoinType = JoinType.Left,
            JoinKeyColumns = new List<string> { "Id" },
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);
        var context = new TaskContext
        {
            InputPath = file1,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // Left Join => Id=1,2,3
        // Id=1은 우측에 매칭 레코드가 없으므로 Feature2는 빈 문자열
        Assert.Equal(4, lines.Length); // 헤더 + 3행
        Assert.Contains("1,A,", lines[1]);
        Assert.Contains("2,B,X", lines[2]);
        Assert.Contains("3,C,Y", lines[3]);
    }

    [Fact]
    public void Execute_HorizontalMerge_RightJoin_ShouldSucceed()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Id,Feature1",
            "1,A",
            "2,B",
            "3,C"
        });

        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Id,Feature2",
            "2,X",
            "3,Y",
            "4,Z"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            JoinType = JoinType.Right,
            JoinKeyColumns = new List<string> { "Id" },
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);
        var context = new TaskContext
        {
            InputPath = file1,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // Right Join => Id=2,3,4
        // Id=4는 왼쪽에 없으므로 Feature1 빈 문자열
        Assert.Equal(4, lines.Length); // 헤더 + 3행
        Assert.Contains("2,B,X", lines[1]);
        Assert.Contains("3,C,Y", lines[2]);
        Assert.Contains("4,,Z", lines[3]);
    }

    [Fact]
    public void Execute_HorizontalMerge_FullJoin_ShouldSucceed()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Id,Feature1",
            "1,A",
            "2,B",
            "3,C"
        });

        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Id,Feature2",
            "2,X",
            "3,Y",
            "4,Z"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            JoinType = JoinType.Full,
            JoinKeyColumns = new List<string> { "Id" },
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);
        var context = new TaskContext
        {
            InputPath = file1,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // Full Join => Id=1,2,3,4 모두 포함
        Assert.Equal(5, lines.Length); // 헤더 + 4행
        Assert.Contains("1,A,", lines[1]);   // Id=1 -> Feature2 없음
        Assert.Contains("2,B,X", lines[2]); // Id=2 -> 매칭
        Assert.Contains("3,C,Y", lines[3]); // Id=3 -> 매칭
        Assert.Contains("4,,Z", lines[4]);  // Id=4 -> Feature1 없음
    }

    [Fact]
    public void Execute_NoErrorsButNoOverlap_InnerJoin_ShouldYieldHeaderOnly()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        File.WriteAllLines(file1, new[]
        {
            "Key,Val1",
            "1,A"
        });

        var file2 = Path.GetTempFileName();
        File.WriteAllLines(file2, new[]
        {
            "Key,Val2",
            "2,B"
        });

        var options = new MergeOption
        {
            MergeType = MergeType.Horizontal,
            JoinType = JoinType.Inner,
            JoinKeyColumns = new List<string> { "Key" },
            InputPaths = new() { file1, file2 }
        };

        var task = new MergeTask(options, _mockLogger.Object, _mockValidatorLogger.Object);
        var context = new TaskContext
        {
            InputPath = file1,
            OutputPath = _testOutputPath
        };

        // Act
        bool result = task.Execute(context);

        // Assert
        Assert.True(result);
        var lines = ReadOutputFileLines();
        // InnerJoin 이므로, Key 매칭이 하나도 없으면 헤더만 나온다
        Assert.Single(lines); // 헤더 1줄
        Assert.Contains("Key,Val1,Val2", lines[0]);
    }
}
