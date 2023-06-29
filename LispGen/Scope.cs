namespace LispGen;

public record Scope(Scope? Parent, Dictionary<string, IExpression> Data)
{
    public static Scope Root()
    {
        var scope = new Scope(null, new());

        /*
         * (do (println "hello world!") 42)       
         *
         */
        scope.Data["do"] = new FnExpr(
            scope,
            new NativeFnExprBody(
                (executor, scope, args) =>
                {
                    IExpression? result = null;
                    foreach (var expr in args)
                    {
                        result = executor.Execute(scope, expr);
                    }

                    return result ?? NullExpr.Instance;
                }
            )
        );

        /*
         * (defn hello (name)
             (println (str "hello, " name)))       
         *
         */
        scope.Data["defn"] = new FnExpr(
            scope,
            new NativeFnExprBody(
                (executor, scope, args) =>
                {
                    if (args.ToList() is not [AtomExpr fnName, ListExpr fnArgs, ListExpr fnBody])
                    {
                        throw new Exception();
                    }

                    if (!fnArgs.Expressions.All(arg => arg is AtomExpr))
                    {
                        throw new Exception();
                    }

                    var fn = new FnExpr(scope, new DefnFnExprBody(fnArgs.Expressions.Cast<AtomExpr>(), fnBody));
                    scope.Parent!.Data.Add(fnName.Name, fn);

                    return fn;
                }
            )
        );

        /*
         * (let ((var1 1) 
         *       (var2 2)))
         */
        scope.Data["let"] = new FnExpr(
            scope,
            new NativeFnExprBody(
                (executor, scope, args) =>
                {
                    if (args.ToList() is not [ListExpr lets])
                    {
                        throw new Exception();
                    }

                    foreach (var expr in lets.Expressions)
                    {
                        if (expr is not ListExpr list)
                        {
                            throw new Exception();
                        }

                        if (list.Expressions.ToList() is not [AtomExpr atom, IExpression value])
                        {
                            throw new Exception();
                        }

                        scope.Parent!.Data.Add(atom.Name, executor.Execute(scope, value));
                    }

                    return NullExpr.Instance;
                }
            )
        );

        /*
         * (add x y)
         */
        scope.Data["add"] = new FnExpr(
            scope,
            new NativeFnExprBody(
                (executor, scope, args) =>
                {
                    var sum = 0f;
                    foreach (var arg in args)
                    {
                        var unwrapped = executor.Execute(scope, arg);
                        if (unwrapped is not NumExpr num)
                        {
                            throw new Exception();
                        }

                        sum += num.Value;
                    }

                    return new NumExpr(sum);
                }
            )
        );

        return scope;
    }

    public bool TryGetValueRecursively(string name, out IExpression expr)
    {
        var scope = this;
        while (scope != null)
        {
            if (scope.Data.TryGetValue(name, out expr!))
            {
                return true;
            }

            scope = scope.Parent;
        }

        expr = NullExpr.Instance;
        return false;
    }

    public Scope CreateChildScope() => new(this, new());
}