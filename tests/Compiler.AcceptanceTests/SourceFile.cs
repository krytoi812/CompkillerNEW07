using System.IO;
using System.Text;

namespace Compiler.AcceptanceTests;

/// <summary>
/// Временный файл исходной программы, удаляемый после теста.
/// </summary>
internal sealed class SourceFile : IDisposable
{
    public SourceFile(string source)
    {
        FilePath = Path.Combine(Path.GetTempPath(), $"compkiller-{Guid.NewGuid():N}.ck");
        File.WriteAllText(FilePath, source, new UTF8Encoding(false));
    }

    public string FilePath { get; }

    public void Dispose()
    {
        File.Delete(FilePath);
    }
}