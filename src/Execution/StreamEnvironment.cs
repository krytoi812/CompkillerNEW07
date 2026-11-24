namespace Execution;

/// <summary>
/// Реализация окружения на основе переданных потоков ввода и вывода.
/// </summary>
public sealed class StreamEnvironment : IEnvironment
{
    /// <summary>
    /// Создаёт окружение с указанными потоками.
    /// </summary>
    /// <param name="input">Поток ввода.</param>
    /// <param name="output">Поток вывода.</param>
    public StreamEnvironment(TextReader input, TextWriter output)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <inheritdoc/>
    public TextReader Input { get; }

    /// <inheritdoc/>
    public TextWriter Output { get; }
}