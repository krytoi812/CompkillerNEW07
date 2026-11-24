namespace Execution;

/// <summary>
/// Ошибка времени выполнения программы: арифметика или ввод.
/// </summary>
public sealed class ExecutionException : Exception
{
    /// <summary>
    /// Создаёт исключение без сообщения.
    /// </summary>
    public ExecutionException()
    {
    }

    /// <summary>
    /// Создаёт исключение с сообщением об ошибке.
    /// </summary>
    /// <param name="message">Описание ошибки.</param>
    public ExecutionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт исключение с сообщением и внутренней причиной.
    /// </summary>
    /// <param name="message">Описание ошибки.</param>
    /// <param name="innerException">Исходная причина ошибки.</param>
    public ExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}