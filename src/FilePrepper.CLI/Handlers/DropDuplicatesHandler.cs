﻿using FilePrepper.Tasks.DropDuplicates;
using FilePrepper.Tasks;
using Microsoft.Extensions.Logging;
using FilePrepper.CLI.Parameters;

namespace FilePrepper.CLI.Handlers;

public class DropDuplicatesHandler : ICommandHandler
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DropDuplicatesHandler> _logger;

    public DropDuplicatesHandler(
        ILoggerFactory loggerFactory,
        ILogger<DropDuplicatesHandler> logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(ICommandParameters parameters)
    {
        var opts = (DropDuplicatesParameters)parameters;

        try
        {
            // Validate when using subset columns
            if (opts.SubsetColumnsOnly && !opts.TargetColumns.Any())
            {
                _logger.LogError("Target columns must be specified when using subset-only");
                return 1;
            }

            var options = new DropDuplicatesOption
            {
                KeepFirst = opts.KeepFirst,
                SubsetColumnsOnly = opts.SubsetColumnsOnly,
                TargetColumns = opts.TargetColumns.ToArray(),
                Common = opts.GetCommonOptions()
            };

            var taskLogger = _loggerFactory.CreateLogger<DropDuplicatesTask>();
            var task = new DropDuplicatesTask(options, taskLogger);
            var context = new TaskContext
            {
                InputPath = opts.InputPath,
                OutputPath = opts.OutputPath
            };

            return await task.ExecuteAsync(context) ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing drop-duplicates command");
            return 1;
        }
    }
}