using System.Text;

namespace LispGen.Lib;

public interface IExpression { }

public record AtomExpr(string Name) : IExpression { }

public record StringExpr(string Value) : IExpression { }

public record NumExpr(float Value) : IExpression { }

public record QuotedExpr(IExpression Expression) : IExpression { }

public record ListExpr(IList<IExpression> Expressions) : IExpression
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

public record InvokeResult(IExpression Result, Context NewContext) { }

public interface IFnExprBody
{
    InvokeResult Invoke(Executor executor, Context ctx, Context fnDeclCtx, IList<IExpression> args);
}

public record NativeFnExprBody(Func<Executor, Context, IList<IExpression>, InvokeResult> Body) : IFnExprBody
{
    public InvokeResult Invoke(Executor executor, Context ctx, Context _, IList<IExpression> args)
    {
        return Body(executor, ctx, args);
    }
}

public record DefnFnExprBody(IList<AtomExpr> ArgNames, ListExpr Body) : IFnExprBody
{
    public InvokeResult Invoke(Executor executor, Context ctx, Context fnDeclCtx, IList<IExpression> args)
    {
        // Create a new scope with the args bound to the appropriate names.
        var fnScope = fnDeclCtx.Scope.CreateChildScope();
        for (var i = 0; i < ArgNames.Count; i++)
        {
            fnScope.Data.Add(ArgNames[i].Name, executor.Execute(ctx, args[i]).Result);
        }

        return executor.Execute(fnDeclCtx.WithScope(fnScope), Body);
    }
}

public record FnExpr(Context DeclContext, IFnExprBody Body) : IExpression { }

public record NullExpr : IExpression
{
    public readonly static NullExpr Instance = new();
}
