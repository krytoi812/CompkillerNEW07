namespace Parsing;

/// <summary>
/// Ошибка синтаксического анализа либо ошибка времени выполнения программы.
/// </summary>
public sealed class ParserException : Exception
{
    /// <summary>
    /// Создаёт исключение без сообщения.
    /// </summary>
    public ParserException()
    {
    }

    /// <summary>
    /// Создаёт исключение с описанием ошибки разбора или выполнения.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    public ParserException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт исключение с описанием и внутренним исключением.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="innerException">Внутреннее исключение.</param>
    public ParserException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}