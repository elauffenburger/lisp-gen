namespace LispGen;

public class Executor
{
    public IExpression Execute(Dictionary<string, IExpression> scope, IExpression expr)
    {
        return expr switch
        {
            QuotedExpr(var inner) => inner,
            ListExpr list => ExecuteListExpr(scope, list),
            _ => expr
        };
    }

    private IExpression ExecuteListExpr(Dictionary<string, IExpression> scope, ListExpr expr)
    {
        var exprs = expr.Expressions;
        if (!exprs.Any())
        {
            throw new Exception();
        }

        return Invoke(scope, exprs.First(), exprs.Skip(1));
    }

    private IExpression Invoke(Dictionary<string, IExpression> scope, IExpression head, IEnumerable<IExpression> rest)
    {
        if (head is not AtomExpr atom)
        {
            throw new Exception();
        }

        if (!scope.TryGetValue(atom.Name, out var atomExpr))
        {
            throw new Exception();
        }

        if (atomExpr is not FnExpr fn)
        {
            throw new Exception();
        }

        return fn.Body.Invoke(this, scope, rest);
    }
}