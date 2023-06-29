namespace LispGen;

public class Executor
{
    public IExpression Execute(Scope scope, IExpression expr)
    {
        return expr switch
        {
            QuotedExpr(var inner) => inner,
            ListExpr list => ExecuteListExpr(scope, list),
            AtomExpr atom => scope.TryGetValueRecursively(atom.Name, out var result) ? result! : NullExpr.Instance,
            _ => expr
        };
    }

    private IExpression ExecuteListExpr(Scope scope, ListExpr expr)
    {
        var exprs = expr.Expressions;
        if (!exprs.Any())
        {
            throw new Exception();
        }

        return Invoke(scope, exprs.First(), exprs.Skip(1).ToList());
    }

    private IExpression Invoke(Scope scope, IExpression head, IList<IExpression> rest)
    {
        if (head is not AtomExpr atom)
        {
            throw new Exception();
        }

        if (!scope.TryGetValueRecursively(atom.Name, out var atomExpr))
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