# CompkillerNEW07

Учебный интерпретатор на C# 14 / .NET 10. Поддерживает целочисленные переменные, присваивание, блоки, пользовательские функции `int`/`void`, `return`, `print`, `readInt()`, `abs`, `min`, `max` и арифметику. Правила языка описаны в [спецификации](docs/specification.md).

## Сборка

Нужен [.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

```sh
git clone --branch iteration_3 https://github.com/krytoi812/CompkillerNEW07.git
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

Примеры с переменными и пользовательскими функциями:

```sh
dotnet run --project src/Compiler -- examples/rectangle.ck
dotnet run --project src/Compiler -- examples/order.ck
```

`rectangle.ck` читает координаты двух углов прямоугольника: для `1`, `2`, `7`, `9` результат равен `42`. `order.ck` читает количество и цену двух товаров, добавляет доставку 50: для `2`, `150`, `3`, `20` результат равен `410`. Все входные числа вводятся на отдельных строках.

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
- `src/Ast` — узлы синтаксического дерева и интерфейс посетителя.
- `src/Parsing` — рекурсивный спуск и построение AST.
- `src/Execution` — стековый обход AST и выполнение программы.
- `src/Compiler` — консольный запуск и обработка ошибок.
- `tests/Lexemes.UnitTests`, `tests/Parsing.UnitTests` — тесты лексера и парсера.
- `tests/Compiler.AcceptanceTests` — приёмочные тесты всего интерпретатора.

Статические анализаторы подключены в `Directory.Build.props`, их настройки находятся в `.editorconfig`.
