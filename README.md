# Space Invaders

Shoot the descending alien fleet before it reaches your cannon.

> Part of **100 Terminal Games** - a collection of 100 playable games written in C# for the terminal.
> Repository: <https://github.com/undoaundo-ctrl/spaceinvaders>

## Features

Formation movement, three alien types and shields

## Controls

Left/Right to aim, Space to fire, Q to quit

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
- A terminal that supports ANSI escape sequences

## How to run

```bash
dotnet run --project src/SpaceInvaders
```

Build once and run the produced binary:

```bash
dotnet build src/SpaceInvaders -c Release
dotnet src/SpaceInvaders/bin/Release/net8.0/SpaceInvaders.dll
```

## Project layout

```text
src/SpaceInvaders/
  Program.cs      # the game itself
  TerminalKit.cs  # small ANSI colour / input helper library
```

There are no NuGet dependencies - only the .NET base class library is used.

## Category

Arcade

## License

MIT - see [LICENSE](LICENSE).
