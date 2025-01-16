# FilePrepper

FilePrepper is a powerful C# library for processing and transforming CSV files. It provides a wide range of data manipulation tasks through a flexible and extensible architecture.

## Features

### Data Manipulation
- **Add Columns**: Add new columns with specified values
- **Remove Columns**: Remove specified columns from the dataset
- **Rename Columns**: Rename columns with a mapping dictionary
- **Reorder Columns**: Change the order of columns in the output
- **Column Interaction**: Perform operations between columns (add, subtract, multiply, divide, concatenate)

### Data Transformation
- **Data Type Convert**: Convert column data types (string, integer, decimal, datetime, boolean)
- **Date Extraction**: Extract components from date columns (year, month, day, etc.)
- **Fill Missing Values**: Handle missing values with various methods (mean, median, mode, forward fill, etc.)
- **Normalize Data**: Normalize numeric columns using Min-Max or Z-score methods
- **One Hot Encoding**: Convert categorical variables into binary columns
- **Scale Data**: Scale numeric columns using Min-Max or Standardization methods
- **Value Replace**: Replace specific values in columns with new values

### Data Analysis
- **Aggregate**: Perform grouping and aggregation operations (sum, average, count, min, max)
- **Basic Statistics**: Calculate various statistics (mean, standard deviation, percentiles, etc.)

### Data Organization
- **Drop Duplicates**: Remove duplicate rows based on specified columns
- **Filter Rows**: Filter data based on various conditions
- **Merge**: Combine multiple CSV files (vertical/horizontal merge with different join types)
- **Data Sampling**: Sample data using different methods (random, systematic, stratified)

### File Format Support
- **File Format Convert**: Convert between different file formats (CSV, TSV, PSV, JSON, XML)

## Usage

Each task in FilePrepper follows a consistent pattern:

1. Configure the task options
2. Create a task instance
3. Execute the task with a context

Here's a basic example:

```csharp
// Configure options
var options = new AddColumnsOption 
{
    NewColumns = new Dictionary<string, string>
    {
        { "NewColumn", "DefaultValue" }
    }
};

// Create task
var logger = LoggerFactory.Create(builder => builder.AddConsole())
                         .CreateLogger<AddColumnsTask>();
var task = new AddColumnsTask(logger);

// Execute
var context = new TaskContext(options)
{
    InputPath = "input.csv",
    OutputPath = "output.csv"
};
await task.ExecuteAsync(context);
```

## Error Handling

FilePrepper provides flexible error handling through common options:

```csharp
var options = new AddColumnsOption 
{
    Common = new CommonTaskOptions
    {
        ErrorHandling = new ErrorHandlingOptions
        {
            IgnoreErrors = true,
            DefaultValue = "0"
        }
    }
};
```

## Installation

Add the FilePrepper package to your project:

```bash
dotnet add package FilePrepper
```