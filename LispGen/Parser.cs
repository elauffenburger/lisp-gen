using System.Text;

namespace LispGen;

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
            var c =>
                char.IsAsciiDigit(c)
                    ? ParseNumberExpr()
                    : char.IsAsciiLetter(ch.Value)
                        ? ParseAtom()
                        : throw new Exception(),
        };

        ChompWhitespace();

        return result;
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

    private NumExpr ParseNumberExpr()
    {
        var numStr = new StringBuilder();
        for (var ch = PeekNext(); ch != null; ch = PeekNext())
        {
            if (char.IsAsciiDigit(ch.Value) || ch == '.')
            {
                Next();
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

        ChompUntil('\n');
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

    private string? ChompUntil(char needle)
    {
        var result = new StringBuilder();
        for (var ch = PeekNext(); ch != needle; ch = Next())
        {
            result.Append(ch);
        }

        return result.Length == 0 ? null : result.ToString();
    }
}
