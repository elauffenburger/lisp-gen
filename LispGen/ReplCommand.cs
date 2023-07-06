using System.Diagnostics;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

using LispGen.Lib;

namespace LispGen;

[Command("repl")]
public class ReplCommand : ICommand
{
    private readonly Parser _parser;

    [CommandOption("debug")]
    public bool Debug { get; init; }

    public ReplCommand(Parser parser)
    {
        _parser = parser;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        if (Debug)
        {
            while (!Debugger.IsAttached)
            {
                await console.Output.WriteLineAsync("waiting for debugger...");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        while (true)
        {
            var line = await console.Input.ReadLineAsync();
            if (line == null)
            {
                break;
            }

            if (line.Length == 0)
            {
                continue;
            }

            if (line[0] == '^')
            {
                switch (line[1..])
                {
                    case "exit":
                        break;

                    default:
                        throw new Exception();
                }
            }

            var parsed = _parser.Parse(line);
            await console.Output.WriteLineAsync(parsed.ToString());
        }
    }
}
