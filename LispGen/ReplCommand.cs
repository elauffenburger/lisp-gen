using System.Diagnostics;
using System.Net;
using System.Text;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

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

public interface Expression { }

public record AtomExpr(string Name) : Expression { }

public record StringExpr(string Value) : Expression { }

public record NumberExpr(float Value) : Expression { }

public record QuotedExpr(Expression Expression) : Expression { }

public record ListExpr(IEnumerable<Expression> Expressions) : Expression
{
    public override string ToString()
    {
        var str = new StringBuilder();
        str.Append("{ ");

        foreach (var expr in Expressions)
        {
            str.Append($"{expr.ToString()} ");
        }

        str.Append('}');

        return str.ToString();
    }
}

public class Parser
{
    private string? _input;
    private int _index;

    public Expression Parse(string input)
    {
        _input = input;
        _index = 0;

        return ParseExpression()!;
    }

    private Expression ParseListExpression()
    {
        var list = new List<Expression>();

        var next = Next();
        if (next != '(')
        {
            throw new Exception();
        }

        for (var ch = PeekNext(); ch != null; ch = PeekNext())
        {
            if (ch == ')')
            {
                Next();
                break;
            }

            var expr = ParseExpression();
            if (expr == null)
            {
                break;
            }

            list.Add(expr);
        }

        return new ListExpr(list);
    }

    private Expression? ParseExpression()
    {
        char? ch;
        for (ch = PeekNext(); ch != null && ch.Value == ' '; ch = PeekNext())
        {
            Next();
        }

        if (ch == null)
        {
            return null;
        }

        switch (ch.Value)
        {
            case '(':
                return ParseListExpression();

            case '\'':
                return new QuotedExpr(ParseExpression()!);

            case '"':
                return ParseStringExpr();

            default:
                if (char.IsAsciiDigit(ch.Value))
                {
                    return ParseNumberExpr();
                }

                if (char.IsAsciiLetter(ch.Value))
                {
                    return ParseAtom();
                }

                throw new Exception();
        }
    }

    private StringExpr ParseStringExpr()
    {
        var str = new StringBuilder();

        var ch = Next()!;
        if (ch != '"')
        {
            throw new Exception();
        }

        for (ch = PeekNext(); ch != null; ch = PeekNext())
        {
            switch (ch)
            {
                case '\\':
                    Next();
                    ch = PeekNext();

                    switch (ch)
                    {
                        case '"':
                            str.Append(Next());
                            break;

                        default:
                            throw new Exception();
                    }

                    break;

                case '"':
                    return new StringExpr(str.ToString());

                default:
                    str.Append(Next());
                    break;
            }

        }

        return new StringExpr(str.ToString());
    }

    private AtomExpr ParseAtom()
    {
        var atomStr = new StringBuilder();
        for (var ch = PeekNext(); ch != null; ch = PeekNext())
        {
            if (!IsAtomCh(ch.Value))
            {
                break;
            }

            atomStr.Append(Next());
        }

        return new AtomExpr(atomStr.ToString());
    }

    private NumberExpr ParseNumberExpr()
    {
        var numStr = new StringBuilder();
        for (var ch = Next(); ch != null; ch = Next())
        {
            if (char.IsAsciiDigit(ch.Value) || ch == '.')
            {
                numStr.Append(ch);
                continue;
            }

            if (ch == ' ' || ch == ')')
            {
                break;
            }

            throw new Exception($"{ch}");
        }

        return new NumberExpr(float.Parse(numStr.ToString()));
    }

    private static bool IsAtomCh(char ch) => char.IsAsciiLetter(ch) || char.IsAsciiDigit(ch);

    private char? Next()
    {
        if (_index == _input!.Length)
        {
            return null;
        }

        var ch = _input[_index];
        _index++;

        return ch;
    }

    private char? PeekNext()
    {
        if (_index == _input!.Length)
        {
            return null;
        }

        return _input[_index];
    }
}