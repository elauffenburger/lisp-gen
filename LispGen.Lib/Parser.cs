using System.Text;

namespace LispGen.Lib;

public class Parser
{
    private string? _input;
    private int _index;

    public IExpression Parse(string input)
    {
        _input = input;
        _index = 0;

        return ParseExpression()!;
    }

    private IExpression ParseListExpression()
    {
        var list = new List<IExpression>();

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

    private char? ChompWhitespace()
    {
        char? ch;
        for (ch = PeekNext(); ch != null && (ch.Value == ' ' || ch.Value == '\n'); ch = PeekNext())
        {
            Next();
        }

        return ch;
    }

    private IExpression? ParseExpression()
    {
        var ch = ChompWhitespace();
        if (ch == null)
        {
            return null;
        }

        var result = ch.Value switch
        {
            '(' => ParseListExpression(),
            '\'' => new QuotedExpr(ParseExpression()!),
            '"' => ParseStringExpr(),
            ';' => ChompCommentAndReparse(),
            _ => ParseAtomOrNumber(),
        };

        ChompWhitespace();

        return result;
    }

    private IExpression ParseAtomOrNumber()
    {
        var word = ChompUntil(new char[] { ' ', '(', ')', '"', '#', '\n', ';' });
        if (word == null || word.Length == 0)
        {
            throw new Exception();
        }

        return word.All(ch => char.IsAsciiDigit(ch) || ch == '.')
            ? ParseAsNumberExpr(word.AsSpan())
            : new AtomExpr(word);
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
                    Next();
                    return new StringExpr(str.ToString());

                default:
                    str.Append(Next());
                    break;
            }
        }

        return new StringExpr(str.ToString());
    }

    private static NumExpr ParseAsNumberExpr(ReadOnlySpan<char> word)
    {
        var numStr = new StringBuilder();
        foreach (var ch in word)
        {
            if (char.IsAsciiDigit(ch) || ch == '.')
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

        return new NumExpr(float.Parse(numStr.ToString()));
    }

    private IExpression? ChompCommentAndReparse()
    {
        ChompComment();
        return ParseExpression();
    }

    private void ChompComment()
    {
        if (Next() != ';')
        {
            throw new Exception();
        }

        ChompUntil(new char[] { '\n' });
    }

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

    private string? ChompUntil(char[] needles)
    {
        var needleSet = new HashSet<char>(needles);

        var result = new StringBuilder();
        for (var ch = PeekNext(); ch != null && !needleSet.Contains(ch.Value); ch = PeekNext())
        {
            Next();
            result.Append(ch);
        }

        return result.Length == 0 ? null : result.ToString();
    }
}
