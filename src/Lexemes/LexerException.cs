namespace Lexemes;

/// <summary>
/// Ошибка лексического анализа: неизвестный символ, незакрытая строка
/// или незакрытый комментарий.
/// </summary>
public sealed class LexerException : Exception
{
    /// <summary>
    /// Создаёт исключение без сообщения.
    /// </summary>
    public LexerException()
        : base()
    {
    }

    /// <summary>
    /// Создаёт исключение с сообщением об ошибке.
    /// </summary>
    /// <param name="message">Описание ошибки на русском языке.</param>
    public LexerException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт исключение с сообщением и внутренней причиной.
    /// </summary>
    /// <param name="message">Описание ошибки на русском языке.</param>
    /// <param name="innerException">Исходная причина ошибки.</param>
    public LexerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}