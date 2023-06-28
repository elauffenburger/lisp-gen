using System.Text;

namespace LispGen;

public interface IExpression
{
    IExpression Clone();

    public static Dictionary<string, IExpression> CloneScope(Dictionary<string, IExpression> scope)
    {
        var cloned = new Dictionary<string, IExpression>(scope.Count);
        foreach (var kv in scope)
        {
            cloned.Add(kv.Key, kv.Value.Clone());
        }

        return cloned;
    }
}

public record AtomExpr(string Name) : IExpression
{
    IExpression IExpression.Clone() => new AtomExpr(Name);
}

public record StringExpr(string Value) : IExpression
{
    IExpression IExpression.Clone() => new StringExpr(Value);
}

public record NumExpr(float Value) : IExpression
{
    IExpression IExpression.Clone() => new NumExpr(Value);
}

public record QuotedExpr(IExpression Expression) : IExpression
{
    IExpression IExpression.Clone() => new QuotedExpr(Expression.Clone());
}

public record ListExpr(IEnumerable<IExpression> Expressions) : IExpression
{
    IExpression IExpression.Clone() => new ListExpr(Expressions.Select(expr => expr.Clone()).ToList());

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

public interface IFnExprBody
{
    IExpression Invoke(Executor executor, Dictionary<string, IExpression> scope, IEnumerable<IExpression> args);

    IFnExprBody Clone();
}

public record NativeFnExprBody(Func<Executor, Dictionary<string, IExpression>, IEnumerable<IExpression>, IExpression> Body) : IFnExprBody
{
    public IExpression Invoke(Executor executor, Dictionary<string, IExpression> scope, IEnumerable<IExpression> args)
    {
        return Body(executor, scope, args);
    }

    IFnExprBody IFnExprBody.Clone() => new NativeFnExprBody(Body);
}

public record DefinedFnExprBody(IEnumerable<AtomExpr> ArgNames, ListExpr Body) : IFnExprBody
{
    public IExpression Invoke(Executor executor, Dictionary<string, IExpression> scope, IEnumerable<IExpression> args)
    {
        // Create a new scope with the args bound to the appropriate names.
        var fnScope = IExpression.CloneScope(scope);
        for (var i = 0; i < ArgNames.Count(); i++)
        {
            fnScope.Add(ArgNames.ElementAt(i).Name, args.ElementAt(i));
        }

        return executor.Execute(fnScope, Body);
    }

    IFnExprBody IFnExprBody.Clone() => new DefinedFnExprBody(ArgNames.Select(arg => (AtomExpr)((IExpression)arg).Clone()).ToList(), (ListExpr)((IExpression)Body).Clone());
}

public record FnExpr(Dictionary<string, IExpression> Scope, IFnExprBody Body) : IExpression
{
    IExpression IExpression.Clone() => new FnExpr(IExpression.CloneScope(Scope), Body.Clone());
}

public record NullExpr : IExpression
{
    public readonly static NullExpr Instance = new();

    IExpression IExpression.Clone() => this;
}