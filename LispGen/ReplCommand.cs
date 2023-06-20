using System.Text;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace LispGen;

[Command("repl")]
public class ReplCommand : ICommand
{
    private readonly Parser _parser;

    public ReplCommand(Parser parser)
    {
        _parser = parser;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
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

            _parser.Parse(line);
        }
    }
}

public interface Expression { }

public record AtomExpr(string Name) : Expression { }

public record StringExpr(string Value) : Expression { }

public record NumberExpr(float Value) : Expression { }

public record QuotedExpr(Expression Expression) : Expression { }

public record ListExpr(IEnumerable<Expression> Expressions) : Expression { }

public class Parser
{
    private string? _input;
    private int _index;

    public Expression Parse(string input)
    {
        _input = input;
        _index = 0;

        return ParseExpression();
    }

    private Expression ParseExpression()
    {
        var list = new List<Expression>();

        for (var ch = Next(); ch != null; ch = Next())
        {
            Expression? expr = null;
            switch (ch)
            {
                case '(':
                    expr = ParseExpression();
                    break;

                case ')':
                    return new ListExpr(list);

                case '\'':
                    expr = new QuotedExpr(ParseExpression());
                    break;

                case '"':
                    expr = ParseStringExpr();
                    break;

                default:
                    if (char.IsAsciiDigit(ch.Value))
                    {
                        expr = ParseNumberExpr();
                        break;
                    }

                    if (char.IsAsciiLetter(ch.Value))
                    {
                        expr = ParseAtom();
                        break;
                    }

                    throw new Exception();
            }

            if (expr == null)
            {
                throw new Exception();
            }

            list.Add(expr);
        }

        return new ListExpr(list);
    }

    private StringExpr ParseStringExpr()
    {
        var str = new StringBuilder();

        for (var ch = PeekNext(); ch != null; ch = PeekNext())
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
        for (var ch = PeekNext(); ch != null; ch = PeekNext())
        {
            if (char.IsAsciiDigit(ch.Value) || ch == '.')
            {
                numStr.Append(Next());
                continue;
            }

            if (ch == ' ')
            {
                break;
            }

            throw new Exception();
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

        return _input[_index + 1];
    }
}