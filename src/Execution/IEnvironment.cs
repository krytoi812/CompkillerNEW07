namespace Execution;

/// <summary>
/// Зависимости ввода-вывода интерпретатора.
/// </summary>
public interface IEnvironment
{
    /// <summary>
    /// Поток ввода программы (stdin).
    /// </summary>
    TextReader Input { get; }

    /// <summary>
    /// Поток вывода программы (stdout).
    /// </summary>
    TextWriter Output { get; }
}