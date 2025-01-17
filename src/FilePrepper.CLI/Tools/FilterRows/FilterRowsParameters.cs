using CommandLine;
using FilePrepper.Tasks.FilterRows;
using Microsoft.Extensions.Logging;

namespace FilePrepper.CLI.Tools.FilterRows;

[Verb("filter-rows", HelpText = "Filter rows based on conditions")]
public class FilterRowsParameters : SingleInputParameters
{
    [Option('c', "conditions", Required = true, Separator = ',',
        HelpText = "Filter conditions in format column:operator:value (e.g. Age:GreaterThan:30)")]
    public IEnumerable<string> Conditions { get; set; } = Array.Empty<string>();

    public override Type GetHandlerType() => typeof(FilterRowsHandler);

    protected override bool ValidateInternal(ILogger logger)
    {
        if (!base.ValidateInternal(logger))
            return false;

        if (!Conditions.Any())
        {
            logger.LogError("At least one filter condition must be specified");
            return false;
        }

        foreach (var condition in Conditions)
        {
            var parts = condition.Split(':');
            if (parts.Length != 3)
            {
                logger.LogError("Invalid condition format: {Condition}. Expected format: column:operator:value", condition);
                return false;
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                logger.LogError("Column name cannot be empty in condition: {Condition}", condition);
                return false;
            }

            if (!Enum.TryParse<FilterOperator>(parts[1], true, out _))
            {
                logger.LogError("Invalid filter operator: {Operator}. Valid values are: {ValidValues}",
                    parts[1], string.Join(", ", Enum.GetNames<FilterOperator>()));
                return false;
            }
        }

        return true;
    }
}