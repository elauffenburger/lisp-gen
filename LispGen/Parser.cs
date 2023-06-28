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

    private IExpression? ParseExpression()
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

    private NumExpr ParseNumberExpr()
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

        return new NumExpr(float.Parse(numStr.ToString()));
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