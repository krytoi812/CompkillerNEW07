# CompkillerNEW07

Учебный интерпретатор на C# 14 / .NET 10. Текущая версия вычисляет целочисленные выражения, поддерживает `print`, `readInt()`, операции `+`, `-`, `*`, `/`, `%`, `^` и скобки. Правила языка описаны в [спецификации](docs/specification.md).

## Сборка

Нужен [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

```sh
git clone --branch iteration_2 https://github.com/krytoi812/CompkillerNEW07.git
cd CompkillerNEW07
dotnet build
```

## Запуск

Из корня репозитория:

```sh
dotnet run --project src/Compiler -- examples/arithmetic.ck
```

Программа из `examples/arithmetic.ck`:

```text
main {
    print 10 + 7 * (2 * 3 + 4) + 5 + -5;
}
```

Ожидаемый вывод — `80` и перевод строки.

```sh
dotnet run --project src/Compiler -- examples/input.ck
```

Второй пример читает длину и ширину прямоугольника на отдельных строках. При вводе `6` и `7` выводится `42`. Ввод идёт через stdin, результаты — через stdout. При ошибке сообщение записывается в stderr, выполнение останавливается с кодом 1.

## Тесты

```sh
dotnet test
```

Приёмочные тесты можно запустить отдельно:

```sh
dotnet test tests/Compiler.AcceptanceTests
```

Тесты запускают интерпретатор через `Program.Run` с временным исходным файлом и заданными потоками ввода и вывода. Проверяются результат, диагностика и код завершения. Сценарии перечислены в [списке тестов](tests/Compiler.AcceptanceTests/TESTLIST.md).

## Покрытие

В PowerShell:

```powershell
./scripts/run-tests-with-coverage.ps1
```

В Bash:

```sh
bash ./scripts/run-tests-with-coverage
```

Скрипты восстанавливают инструменты из `dotnet-tools.json`, запускают тесты со сбором покрытия и создают HTML-отчёт в `tests/coverage-report/index.html`. Для восстановления пакетов при первом запуске нужен доступ к NuGet. Требование к парсеру — не менее 80% line coverage; в отчёте нужно смотреть класс `Parsing.Parser`.

## Модули

- `src/Lexemes` — лексический анализатор.
- `src/Parsing` — рекурсивный спуск и вычисление выражений.
- `src/Compiler` — консольный запуск и обработка ошибок.
- `tests/Lexemes.UnitTests`, `tests/Parsing.UnitTests` — тесты лексера и парсера.
- `tests/Compiler.AcceptanceTests` — приёмочные тесты всего интерпретатора.

Статические анализаторы подключены в `Directory.Build.props`, их настройки находятся в `.editorconfig`.
