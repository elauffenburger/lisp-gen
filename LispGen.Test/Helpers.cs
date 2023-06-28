using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;

namespace LispGen.Test;

public class ExpressionEqualityComparer : IEqualityComparer<IExpression>
{
    public bool Equals(IExpression? x, IExpression? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if ((x != null && y == null) || (x == null && y != null))
        {
            return false;
        }

        if (x!.GetType() != y!.GetType())
        {
            return false;
        }

        return (x, y) switch
        {
            (ListExpr(var xs), ListExpr(var ys)) => xs.Zip(ys).All((el) => Equals(el.First, el.Second)),
            _ => x.Equals(y)
        };
    }

    public int GetHashCode([DisallowNull] IExpression obj) => obj.GetHashCode();
}