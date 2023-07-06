namespace LispGen.Lib;

public record Context(Scope Scope)
{
    public Context WithScope(Scope scope) => this with { Scope = scope };
}

public class Executor
{
    public InvokeResult Execute(Context ctx, IExpression expr, bool expandAtoms = false)
    {
        var result = expr switch
        {
            QuotedExpr(var inner) => new(inner, ctx),
            ListExpr list => ExecuteListExpr(ctx, list),
            AtomExpr atom => new InvokeResult(ctx.Scope.TryGetValueRecursively(atom.Name, out var val, expandAtoms) ? val! : NullExpr.Instance, ctx),
            _ => new(expr, ctx)
        };

        return result.Result switch
        {
            ListExpr list => Execute(ctx, list),
            _ => result
        };
    }

    private InvokeResult ExecuteListExpr(Context ctx, ListExpr expr)
    {
        var exprs = expr.Expressions;
        if (!exprs.Any())
        {
            throw new Exception();
        }

        return Invoke(ctx, exprs.First(), exprs.Skip(1).ToList());
    }

    private InvokeResult Invoke(Context ctx, IExpression head, IList<IExpression> rest)
    {
        if (head is not AtomExpr atom)
        {
            throw new Exception($"head of invocation expression is not an atom ({head})");
        }

        if (!ctx.Scope.TryGetValueRecursively(atom.Name, out var atomExpr))
        {
            throw new Exception($"lookup for '{atom.Name}' failed");
        }

        if (atomExpr is not FnExpr fn)
        {
            throw new Exception($"{atom.Name} is not a function");
        }

        return fn.Body.Invoke(this, ctx, fn.DeclContext, rest);
    }
}
