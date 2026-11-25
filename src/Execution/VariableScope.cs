namespace Execution;

/// <summary>Переменные блока и ссылка на внешнюю лексическую область.</summary>
internal sealed class VariableScope
{
    private readonly Dictionary<string, long> _variables = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    private readonly VariableScope? _parent;

    public VariableScope(VariableScope? parent)
    {
        _parent = parent;
    }

    public void Declare(string name, long value)
    {
        if (!_variables.TryAdd(name, value))
        {
            throw new ExecutionException($"Переменная '{name}' уже объявлена в этой области.");
        }
    }

    public long Read(string name)
    {
        if (_variables.TryGetValue(name, out long value))
        {
            return value;
        }

        if (_parent is not null)
        {
            return _parent.Read(name);
        }

        throw new ExecutionException($"Неизвестная переменная '{name}'.");
    }

    public void Assign(string name, long value)
    {
        if (_variables.ContainsKey(name))
        {
            _variables[name] = value;
            return;
        }

        if (_parent is not null)
        {
            _parent.Assign(name, value);
            return;
        }

        throw new ExecutionException($"Неизвестная переменная '{name}'.");
    }
}