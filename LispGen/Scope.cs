namespace LispGen;

public record Scope(Scope? Parent, Dictionary<string, IExpression> Data)
{
    public static Scope Root()
    {
        var rootScope = new Scope(null, new());
        var rootCtx = new ExecutionContext(rootScope);

        /*
         * (do (println "hello world!") 42)       
         *
         */
        rootScope.Data["do"] = new FnExpr(
            rootCtx,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    InvokeResult? result = null;
                    ExecutionContext currCtx = ctx;
                    foreach (var expr in args)
                    {
                        result = executor.Execute(currCtx, expr);
                        currCtx = result.NewContext;
                    }

                    return result ?? new(NullExpr.Instance, ctx);
                }
            )
        );

        /*
         * (defn hello (name)
             (println (str "hello, " name)))       
         *
         */
        rootScope.Data["defn"] = new FnExpr(
            rootCtx,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    if (args is not [AtomExpr fnName, ListExpr fnArgs, ListExpr fnBody])
                    {
                        throw new Exception();
                    }

                    if (!fnArgs.Expressions.All(arg => arg is AtomExpr))
                    {
                        throw new Exception();
                    }

                    // Create a new child scope that will contain the fn and update the context to use that going forward.
                    var scope = ctx.Scope.CreateChildScope();

                    // Define the fn.
                    var fn = new FnExpr(ctx, new DefnFnExprBody(fnArgs.Expressions.Cast<AtomExpr>().ToList(), fnBody));
                    scope.Data.Add(fnName.Name, fn);

                    return new(fn, ctx.WithScope(scope));
                }
            )
        );

        /*
         * (let ((var1 1) 
         *       (var2 2)))
         */
        rootScope.Data["let"] = new FnExpr(
            rootCtx,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    if (args is not [ListExpr lets])
                    {
                        throw new Exception();
                    }

                    // Create a new child scope that will contain the bindings.
                    var scope = ctx.Scope.CreateChildScope();
                    foreach (var expr in lets.Expressions)
                    {
                        if (expr is not ListExpr list)
                        {
                            throw new Exception();
                        }

                        if (list.Expressions is not [AtomExpr atom, IExpression value])
                        {
                            throw new Exception();
                        }

                        scope.Data.Add(atom.Name, executor.Execute(ctx, value).Result);
                    }

                    // Update the context to use this new scope going forward.
                    return new(NullExpr.Instance, ctx.WithScope(scope));
                }
            )
        );

        /*
         * (add x y)
         */
        rootScope.Data["add"] = new FnExpr(
            rootCtx,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    var sum = 0f;
                    foreach (var arg in args)
                    {
                        var unwrapped = executor.Execute(ctx, arg, expandAtoms: true);
                        if (unwrapped.Result is not NumExpr num)
                        {
                            if (unwrapped.Result is NullExpr)
                            {
                                continue;
                            }

                            throw new Exception(unwrapped.GetType().ToString());
                        }

                        sum += num.Value;
                    }

                    return new(new NumExpr(sum), ctx);
                }
            )
        );

        return rootScope;
    }

    public bool TryGetValueRecursively(string name, out IExpression expr, bool ExpandAtoms = false)
    {
        var scope = this;
        while (scope != null)
        {
            // If we got the expression value from this scope and we don't need to continue expanding it, we're done!
            if (scope.Data.TryGetValue(name, out expr!) && (!ExpandAtoms || expr is not AtomExpr))
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
