using System.Globalization;
using System.Text.RegularExpressions;

namespace Api.Comercial.Services;

public interface IBenefitFormulaEvaluator
{
    bool TryEvaluate(string formula, IReadOnlyDictionary<string, decimal> variables, out decimal value, out string? errorMessage);
}

public sealed class BenefitFormulaEvaluator : IBenefitFormulaEvaluator
{
    public bool TryEvaluate(string formula, IReadOnlyDictionary<string, decimal> variables, out decimal value, out string? errorMessage)
    {
        value = 0m;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(formula))
        {
            errorMessage = "Formula is required when IsFormula is true.";
            return false;
        }

        var normalized = NormalizeFormula(formula);
        if (!TryToRpn(normalized, out var rpn, out errorMessage))
        {
            return false;
        }

        if (!TryEvaluateRpn(rpn, variables, out value, out errorMessage))
        {
            return false;
        }

        value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        return true;
    }

    private static string NormalizeFormula(string formula)
    {
        var normalized = formula.Trim();
        if (normalized.Length >= 2
            && normalized[0] == '['
            && normalized[^1] == ']'
            && !IsVariableToken(normalized))
        {
            normalized = normalized[1..^1].Trim();
        }

        return normalized;
    }

    private static bool TryToRpn(string expression, out List<string> output, out string? errorMessage)
    {
        output = new List<string>();
        errorMessage = null;
        var operators = new Stack<string>();

        var index = 0;
        var expectOperand = true;
        while (index < expression.Length)
        {
            var ch = expression[index];
            if (char.IsWhiteSpace(ch))
            {
                index++;
                continue;
            }

            if (ch == '[')
            {
                var end = expression.IndexOf(']', index + 1);
                if (end < 0)
                {
                    errorMessage = "Formula has invalid variable token.";
                    return false;
                }

                var variableToken = expression[index..(end + 1)];
                if (!IsVariableToken(variableToken))
                {
                    errorMessage = $"Formula contains invalid variable token '{variableToken}'.";
                    return false;
                }

                output.Add(variableToken);
                index = end + 1;
                expectOperand = false;
                continue;
            }

            if (char.IsDigit(ch) || ch == '.')
            {
                var start = index;
                var hasDot = ch == '.';
                index++;
                while (index < expression.Length)
                {
                    var c = expression[index];
                    if (char.IsDigit(c))
                    {
                        index++;
                        continue;
                    }

                    if (c == '.' && !hasDot)
                    {
                        hasDot = true;
                        index++;
                        continue;
                    }

                    break;
                }

                var numberToken = expression[start..index];
                if (!decimal.TryParse(numberToken, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    errorMessage = $"Invalid number token '{numberToken}'.";
                    return false;
                }

                output.Add(numberToken);
                expectOperand = false;
                continue;
            }

            if (ch == '(')
            {
                operators.Push("(");
                index++;
                expectOperand = true;
                continue;
            }

            if (ch == ')')
            {
                var foundOpen = false;
                while (operators.Count > 0)
                {
                    var op = operators.Pop();
                    if (op == "(")
                    {
                        foundOpen = true;
                        break;
                    }

                    output.Add(op);
                }

                if (!foundOpen)
                {
                    errorMessage = "Formula has unbalanced parentheses.";
                    return false;
                }

                index++;
                expectOperand = false;
                continue;
            }

            if (IsOperator(ch))
            {
                if (expectOperand)
                {
                    if (ch == '-')
                    {
                        output.Add("0");
                    }
                    else
                    {
                        errorMessage = $"Unexpected operator '{ch}' in formula.";
                        return false;
                    }
                }

                var op1 = ch.ToString();
                while (operators.Count > 0 && operators.Peek() != "(" && Precedence(operators.Peek()) >= Precedence(op1))
                {
                    output.Add(operators.Pop());
                }

                operators.Push(op1);
                index++;
                expectOperand = true;
                continue;
            }

            errorMessage = $"Formula contains unsupported token '{ch}'.";
            return false;
        }

        while (operators.Count > 0)
        {
            var op = operators.Pop();
            if (op == "(")
            {
                errorMessage = "Formula has unbalanced parentheses.";
                return false;
            }

            output.Add(op);
        }

        if (output.Count == 0)
        {
            errorMessage = "Formula cannot be empty.";
            return false;
        }

        return true;
    }

    private static bool TryEvaluateRpn(List<string> rpn, IReadOnlyDictionary<string, decimal> variables, out decimal value, out string? errorMessage)
    {
        value = 0m;
        errorMessage = null;

        var stack = new Stack<decimal>();
        foreach (var token in rpn)
        {
            if (IsVariableToken(token))
            {
                var variableKey = token[1..^1];
                if (!variables.TryGetValue(variableKey, out var variableValue))
                {
                    errorMessage = $"Unknown formula variable '{token}'.";
                    return false;
                }

                stack.Push(variableValue);
                continue;
            }

            if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                stack.Push(number);
                continue;
            }

            if (!IsOperatorToken(token))
            {
                errorMessage = $"Unsupported token '{token}' in formula.";
                return false;
            }

            if (stack.Count < 2)
            {
                errorMessage = "Formula is malformed.";
                return false;
            }

            var right = stack.Pop();
            var left = stack.Pop();

            decimal result;
            switch (token)
            {
                case "+":
                    result = left + right;
                    break;
                case "-":
                    result = left - right;
                    break;
                case "*":
                    result = left * right;
                    break;
                case "/":
                    if (right == 0m)
                    {
                        errorMessage = "Division by zero is not allowed in formula.";
                        return false;
                    }

                    result = left / right;
                    break;
                default:
                    errorMessage = $"Unsupported operator '{token}'.";
                    return false;
            }

            stack.Push(result);
        }

        if (stack.Count != 1)
        {
            errorMessage = "Formula is malformed.";
            return false;
        }

        value = stack.Pop();
        return true;
    }

    private static bool IsOperator(char ch) => ch is '+' or '-' or '*' or '/';

    private static bool IsOperatorToken(string token) => token is "+" or "-" or "*" or "/";

    private static int Precedence(string token)
        => token is "*" or "/" ? 2 : 1;

    private static bool IsVariableToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length < 3)
        {
            return false;
        }

        if (token[0] != '[' || token[^1] != ']')
        {
            return false;
        }

        var key = token[1..^1];
        return Regex.IsMatch(key, "^[A-Za-z0-9_.]+$");
    }
}
