using System.Text;

namespace LispGen.Lib;

public record Context(Scope Scope)
{
    public Context WithScope(Scope scope) => this with { Scope = scope };
}

public class Executor
{
    public InvokeResult Execute(Context ctx, IExpression expr, bool expandAtoms = false)
    {
        return expr switch
        {
            QuotedExpr(var inner) => new(inner, ctx),
            ListExpr list => ExecuteListExpr(ctx, list),
            AtomExpr atom => ExecuteAtomExpr(ctx, atom, expandAtoms),
            _ => new(expr, ctx)
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

    private InvokeResult ExecuteAtomExpr(Context ctx, AtomExpr atom, bool expand)
    {
        var result = new InvokeResult(ctx.Scope.TryGetValueRecursively(atom.Name, out var val, expand) ? val! : NullExpr.Instance, ctx);

        return result.Result switch
        {
            ListExpr list => ExecuteListExpr(result.NewContext, list),
            _ => result,
        };
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

        return fn.Body.Invoke(this, ctx, fn.DeclScope, rest);
    }

    public StringExpr Interpolate(Context ctx, ReadOnlySpan<char> str, IList<IExpression> args)
    {
        var numResolvedDirectives = 0;
        var result = new StringBuilder(str.Length);
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] != '%' || numResolvedDirectives == args.Count)
            {
                result.Append(str[i]);
                continue;
            }

            i++;

            var argResult = Execute(ctx, args[numResolvedDirectives]);
            switch (str[i])
            {
                case 'd':
                    if (argResult.Result is not NumExpr num)
                    {
                        throw new Exception($"arg {numResolvedDirectives} ({argResult.Result}) cannot be formatted as a number");
                    }

                    result.Append(num.Value);
                    break;

                default:
                    throw new Exception($"unknown formatting directive {str[i]}");
            }

            numResolvedDirectives++;
        }

        return new StringExpr(result.ToString());
    }
}
