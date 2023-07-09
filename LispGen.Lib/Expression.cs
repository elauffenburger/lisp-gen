using System.Text;

namespace LispGen.Lib;

public interface IExpression
{
    public static bool IsTruthy(IExpression expr) => !expr.Equals(AtomExpr.False);
}

public record AtomExpr(string Name) : IExpression
{
    public readonly static AtomExpr True = new("T");
    public readonly static NullExpr False = NullExpr.Instance;
}

public record StringExpr(string Value) : IExpression { }

public record NumExpr(float Value) : IExpression { }

public record QuotedExpr(IExpression Expression) : IExpression { }

public record ListExpr(IList<IExpression> Expressions) : IExpression
{
    public override string ToString()
    {
        var str = new StringBuilder();
        str.Append("ListExpr { ");

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
    InvokeResult Invoke(Executor executor, Context ctx, Scope fnDeclScope, IList<IExpression> args);
}

public record NativeFnExprBody(Func<Executor, Context, IList<IExpression>, InvokeResult> Body) : IFnExprBody
{
    public InvokeResult Invoke(Executor executor, Context ctx, Scope _, IList<IExpression> args)
    {
        return Body(executor, ctx, args);
    }
}

public record DefnFnExprBody(IList<AtomExpr> ArgNames, ListExpr Body) : IFnExprBody
{
    public InvokeResult Invoke(Executor executor, Context ctx, Scope fnDeclScope, IList<IExpression> args)
    {
        // Create a new scope with the args bound to the appropriate names.
        var fnScope = fnDeclScope.CreateChildScope();
        for (var i = 0; i < ArgNames.Count; i++)
        {
            fnScope.Data.Add(ArgNames[i].Name, executor.Execute(ctx, args[i]).Result);
        }

        return executor.Execute(ctx.WithScope(fnScope), Body);
    }
}

public record FnExpr(Scope DeclScope, IFnExprBody Body) : IExpression { }

public record NullExpr : IExpression
{
    public readonly static NullExpr Instance = new();
}
