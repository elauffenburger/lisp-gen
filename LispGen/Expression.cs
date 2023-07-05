using System.Text;

namespace LispGen;

public interface IExpression { }

public record AtomExpr(string Name) : IExpression { }

public record StringExpr(string Value) : IExpression { }

public record NumExpr(float Value) : IExpression { }

public record QuotedExpr(IExpression Expression) : IExpression { }

public record ListExpr(IEnumerable<IExpression> Expressions) : IExpression
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

public record CommentExpr(string Comment) : IExpression { }

public interface IFnExprBody
{
    IExpression Invoke(Executor executor, Scope scope, IEnumerable<IExpression> args);
}

public record NativeFnExprBody(Func<Executor, Scope, IEnumerable<IExpression>, IExpression> Body) : IFnExprBody
{
    public IExpression Invoke(Executor executor, Scope scope, IEnumerable<IExpression> args)
    {
        return Body(executor, scope.CreateChildScope(), args);
    }
}

public record DefnFnExprBody(IEnumerable<AtomExpr> ArgNames, ListExpr Body) : IFnExprBody
{
    public IExpression Invoke(Executor executor, Scope scope, IEnumerable<IExpression> args)
    {
        // Create a new scope with the args bound to the appropriate names.
        var fnScope = scope.CreateChildScope();
        for (var i = 0; i < ArgNames.Count(); i++)
        {
            fnScope.Data.Add(ArgNames.ElementAt(i).Name, args.ElementAt(i));
        }

        return executor.Execute(fnScope, Body);
    }
}

public record FnExpr(Scope Scope, IFnExprBody Body) : IExpression { }

public record NullExpr : IExpression
{
    public readonly static NullExpr Instance = new();
}