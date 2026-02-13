using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;
using Api.Comercial.Models.Responses;

namespace Api.Comercial.Services;

public interface IBenefitFormulaVariableResolver
{
    Task<OperationResult<IReadOnlyDictionary<string, decimal>>> ResolveAsync(int contractId, string formula, CancellationToken cancellationToken);
}

public sealed class BenefitFormulaVariableResolver : IBenefitFormulaVariableResolver
{
    private static readonly Regex VariableTokenRegex = new(@"\[(?<key>[A-Za-z0-9_.]+)\]", RegexOptions.Compiled);

    private readonly ApeironDbContext _context;

    public BenefitFormulaVariableResolver(ApeironDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<IReadOnlyDictionary<string, decimal>>> ResolveAsync(int contractId, string formula, CancellationToken cancellationToken)
    {
        var keys = ExtractVariableKeys(formula);
        if (keys.Count == 0)
        {
            return OperationResult<IReadOnlyDictionary<string, decimal>>.Ok(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase));
        }

        var variables = await _context.BenefitFormulaVariables
            .AsNoTracking()
            .Where(v => v.Active && keys.Contains(v.VariableKey))
            .ToListAsync(cancellationToken);

        var missing = keys.Except(variables.Select(v => v.VariableKey), StringComparer.OrdinalIgnoreCase).ToList();
        if (missing.Count > 0)
        {
            return OperationResult<IReadOnlyDictionary<string, decimal>>.Fail(
                "validation",
                $"Unknown formula variable(s): {string.Join(", ", missing.Select(k => $"[{k}]"))}.");
        }

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var variable in variables)
        {
            var readResult = await ReadVariableValueAsync(contractId, variable.SourceScope, variable.SourceSchema, variable.SourceTable, variable.SourceColumn, cancellationToken);
            if (!readResult.Success)
            {
                return OperationResult<IReadOnlyDictionary<string, decimal>>.Fail(readResult.ErrorCode!, $"Variable [{variable.VariableKey}] failed: {readResult.ErrorMessage}");
            }

            result[variable.VariableKey] = readResult.Data;
        }

        return OperationResult<IReadOnlyDictionary<string, decimal>>.Ok(result);
    }

    private async Task<OperationResult<decimal>> ReadVariableValueAsync(
        int contractId,
        string sourceScope,
        string sourceSchema,
        string sourceTable,
        string sourceColumn,
        CancellationToken cancellationToken)
    {
        if (!IsSqlIdentifier(sourceSchema) || !IsSqlIdentifier(sourceTable) || !IsSqlIdentifier(sourceColumn))
        {
            return OperationResult<decimal>.Fail("validation", "Invalid catalog SQL identifiers.");
        }

        string sql;
        if (sourceScope.Equals("Contract", StringComparison.OrdinalIgnoreCase))
        {
            sql = $@"SELECT CAST(t.[{sourceColumn}] AS decimal(28,10))
FROM [{sourceSchema}].[{sourceTable}] t
WHERE t.[ContractId] = @contractId;";
        }
        else if (sourceScope.Equals("EmployeeFromContract", StringComparison.OrdinalIgnoreCase))
        {
            sql = $@"SELECT CAST(t.[{sourceColumn}] AS decimal(28,10))
FROM [{sourceSchema}].[{sourceTable}] t
INNER JOIN hr.EmployeeContract c ON c.EmployeeId = t.EmployeeId
WHERE c.ContractId = @contractId;";
        }
        else
        {
            return OperationResult<decimal>.Fail("validation", "Unsupported SourceScope.");
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        var parameter = new SqlParameter("@contractId", SqlDbType.Int) { Value = contractId };
        command.Parameters.Add(parameter);

        object? scalar;
        try
        {
            scalar = await command.ExecuteScalarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return OperationResult<decimal>.Fail("validation", $"Unable to read source value ({ex.GetBaseException().Message}).");
        }

        if (scalar is null || scalar == DBNull.Value)
        {
            return OperationResult<decimal>.Fail("validation", "Source value is null.");
        }

        if (!decimal.TryParse(scalar.ToString(), out var decimalValue))
        {
            return OperationResult<decimal>.Fail("validation", "Source value is not numeric.");
        }

        return OperationResult<decimal>.Ok(decimalValue);
    }

    private static HashSet<string> ExtractVariableKeys(string formula)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in VariableTokenRegex.Matches(formula ?? string.Empty))
        {
            var key = match.Groups["key"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(key))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static bool IsSqlIdentifier(string value)
        => !string.IsNullOrWhiteSpace(value)
           && Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_]*$");
}
