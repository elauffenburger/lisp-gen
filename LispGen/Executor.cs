namespace LispGen;

public record ExecutionContext(Scope Scope)
{
    public ExecutionContext WithScope(Scope scope) => this with { Scope = scope };
}

public class Executor
{
    public InvokeResult Execute(ExecutionContext ctx, IExpression expr, bool ExpandAtoms = false)
    {
        var result = expr switch
        {
            QuotedExpr(var inner) => new(inner, ctx),
            ListExpr list => ExecuteListExpr(ctx, list),
            AtomExpr atom => new InvokeResult(ctx.Scope.TryGetValueRecursively(atom.Name, out var val, ExpandAtoms) ? val! : NullExpr.Instance, ctx),
            _ => new(expr, ctx)
        };

        return result.Result switch
        {
            ListExpr list => Execute(ctx, list),
            _ => result
        };
    }

    private InvokeResult ExecuteListExpr(ExecutionContext ctx, ListExpr expr)
    {
        var exprs = expr.Expressions;
        if (!exprs.Any())
        {
            throw new Exception();
        }

        return Invoke(ctx, exprs.First(), exprs.Skip(1).ToList());
    }

    private InvokeResult Invoke(ExecutionContext ctx, IExpression head, IList<IExpression> rest)
    {
        if (head is not AtomExpr atom)
        {
            throw new Exception();
        }

        if (!ctx.Scope.TryGetValueRecursively(atom.Name, out var atomExpr))
        {
            throw new Exception();
        }

        if (atomExpr is not FnExpr fn)
        {
            throw new Exception();
        }

        return fn.Body.Invoke(this, ctx, fn.DeclContext, rest);
    }
}