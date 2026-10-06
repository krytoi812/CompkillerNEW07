# Проект compilerNEW на C#

Проект предназначен для .NET 10 (C# 14): https://dotnet.microsoft.com/en-us/download/dotnet/10.0

## Клонирование проекта

Клонирование без авторизации в SourceCraft:

```bash
# Клонируем репозиторий в каталог compilerNEW/
git clone https://github.com/krytoi812/compilerNEW.git compilerNEW
```

Если у вас есть аккаунт SourceCraft и вы настроили SSH-ключи, то можно клонировать по SSH:

```bash
# Клонируем репозиторий в каталог compiler/
git clone git@github.com:krytoi812/compilerNEW.git compilerNEW
```

## Сборка

Сборка консольной утилитой dotnet из .NET SDK:

```bash
# Сборка
dotnet build

# Запуск тестов
dotnet test
```

## Анализ покрытия кода тестами

Запустите скрипт и HTML-отчёт о покрытии появится в tests/coverage-report/index.html

```bash
scripts/run-tests-with-coverage
```

## Структура проекта на C#

Ликбез по типам файлов:

1. Файл `*.slnx` (Solution) — основной файл «решения» (то есть проекта), обычно один на репозиторий с кодом
2. Файл `*.csproj` (C# Project) — файл с описанием одного Assembly (_рус._ Сборка), компилируется в один DLL
3. Файл `*.dll` — содержит машинный код для виртуальной машины .NET
    - Расширение `*.dll` также используется для разделяемых библиотек в ОС Windows; в Linux аналогом являются файлы `*.so` (shared library)

Код на C# компилируется в MSIL — код виртуальной машины платформы .NET. Такой код сохраняется в файлах `.dll` для всех платформ, где работает .NET.

Структура проекта была создана такими командами:

```bash
# Создаём Solution (основной проект).
dotnet new sln

# Создаём библиотеку ExampleLib в каталоге src/ и добавляем её в Solution
dotnet new classlib -o src/ExampleLib
dotnet sln add src/ExampleLib/

# Создаём проект модульных тестов на тестовом фреймворке XUnit
dotnet new xunit -o tests/ExampleLib.UnitTests
dotnet sln add tests/ExampleLib.UnitTests/

# Добавляем проекту тестов ссылку на проект
dotnet add tests/ExampleLib.UnitTests/ reference src/ExampleLib/
```

Как только библиотека ExampleLib станет вам не нужна, вы должны удалить её из проекта командами:

```bash
# Удаляем ExampleLib и тесты из файла решения
dotnet sln remove src/ExampleLib/
dotnet sln remove tests/ExampleLib.UnitTests/

# Удаляем ExampleLib и тесты с диска и из системы контроля версий
git rm -r src/ExampleLib/
git rm -r tests/ExampleLib.UnitTests/
```

## Статический анализ

Проект содержит подключённые статические анализаторы:

* Статические анализаторы подключены в файле `Directory.Build.props` — система сборки MSBuild воспринимает этот файл как общие параметры сборки всего проекта;
* В корне проекта есть файл `.editorconfig` с настройками статических анализаторов

Вы можете менять файл `.editorconfig` — но только при условии, что это не снизит качество кода.
