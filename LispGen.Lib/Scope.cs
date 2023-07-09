namespace LispGen.Lib;

public record Scope(Scope? Parent, Dictionary<string, IExpression> Data)
{
    public static Scope Root()
    {
        var rootScope = new Scope(null, new());

        AddPrimitives(rootScope);
        AddCore(rootScope);
        AddMath(rootScope);

        return rootScope;
    }

    private static void AddPrimitives(Scope rootScope)
    {
        rootScope.Data["t"] = AtomExpr.True;
        rootScope.Data["T"] = AtomExpr.True;
        rootScope.Data["NIL"] = NullExpr.Instance;
    }

    private static void AddCore(Scope rootScope)
    {
        /*
         * (assert (= 1 1) t "1 should equal 1")
         */
        rootScope.Data["assert"] = new FnExpr(
            rootScope,
            new NativeFnExprBody((executor, ctx, args) =>
            {
                var testResult = executor.Execute(ctx, args[0]);
                if (!IExpression.IsTruthy(testResult.Result))
                {
                    // If we don't have an error message, just throw an exception.
                    if (args.Count < 2)
                    {
                        throw new Exception();
                    }

                    // If we shouldn't _use_ the error message (if it's even there), just throw an exception.
                    var useMessage = executor.Execute(ctx, args[1]);
                    if (!IExpression.IsTruthy(useMessage.Result))
                    {
                        throw new Exception();
                    }

                    // If the arg in the error message position isn't a string, bail.
                    if (args[2] is not StringExpr msg)
                    {
                        throw new Exception("argument in message position was not a string");
                    }

                    if (args.Count > 3)
                    {
                        msg = executor.Interpolate(ctx, msg.Value, args.Skip(3).ToList());
                    }

                    throw new Exception(msg.Value);
                }

                return new(AtomExpr.True, ctx);
            })
        );

        /*
         * (do (println "hello world!") 42)
         */
        rootScope.Data["do"] = new FnExpr(
            rootScope,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    InvokeResult? result = null;
                    Context currCtx = ctx;
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
         *   (println (str "hello, " name)))       
         */
        rootScope.Data["defn"] = new FnExpr(
            rootScope,
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
                    var fn = new FnExpr(ctx.Scope, new DefnFnExprBody(fnArgs.Expressions.Cast<AtomExpr>().ToList(), fnBody));
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
            rootScope,
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
    }

    private static void AddMath(Scope rootScope)
    {
        Func<Executor, Context, IList<IExpression>, InvokeResult> DoArithmetic(Func<float, float, float> op)
        {
            return (executor, ctx, args) =>
            {
                float? total = null;
                foreach (var arg in args)
                {
                    var unwrapped = executor.Execute(ctx, arg);
                    if (unwrapped.Result is not NumExpr)
                    {
                        if (unwrapped.Result is NullExpr)
                        {
                            continue;
                        }

                        throw new Exception();
                    }

                    var val = ((NumExpr)unwrapped.Result).Value;
                    if (total == null)
                    {
                        total = val;
                        continue;
                    }

                    total = op(total!.Value, val);
                }

                return new(new NumExpr(total!.Value), ctx);
            };
        }

        /*
         * (+ x y)
         */
        rootScope.Data["+"] = new FnExpr(rootScope, new NativeFnExprBody(DoArithmetic((total, val) => total + val)));

        /*
         * (- x y)
         */
        rootScope.Data["-"] = new FnExpr(rootScope, new NativeFnExprBody(DoArithmetic((total, val) => total - val)));

        /*
         * (* x y)
         */
        rootScope.Data["*"] = new FnExpr(rootScope, new NativeFnExprBody(DoArithmetic((total, val) => total * val)));

        /*
         * (/ x y)
         */
        rootScope.Data["/"] = new FnExpr(rootScope, new NativeFnExprBody(DoArithmetic((total, val) => total / val)));

        Func<Executor, Context, IList<IExpression>, InvokeResult> DoComparison(Func<float, float, bool> op)
        {
            return (executor, ctx, args) =>
            {
                float? compareTo = null;
                foreach (var arg in args)
                {
                    var unwrapped = executor.Execute(ctx, arg);
                    if (unwrapped.Result is not NumExpr)
                    {
                        throw new Exception();
                    }

                    var val = ((NumExpr)unwrapped.Result).Value;
                    if (compareTo == null)
                    {
                        compareTo = val;
                        continue;
                    }

                    if (!op(compareTo.Value, val))
                    {
                        return new(AtomExpr.False, ctx);
                    }
                }

                return new(AtomExpr.True, ctx);
            };
        }

        /*
         * (= x y)
         */
        rootScope.Data["="] = new FnExpr(rootScope, new NativeFnExprBody(DoComparison((a, b) => a == b)));

        /*
         * (< x y)
         */
        rootScope.Data["<"] = new FnExpr(rootScope, new NativeFnExprBody(DoComparison((a, b) => a < b)));

        /*
         * (<= x y)
         */
        rootScope.Data["<="] = new FnExpr(rootScope, new NativeFnExprBody(DoComparison((a, b) => a <= b)));

        /*
         * (> x y)
         */
        rootScope.Data[">"] = new FnExpr(rootScope, new NativeFnExprBody(DoComparison((a, b) => a > b)));

        /*
         * (>= x y)
         */
        rootScope.Data[">="] = new FnExpr(rootScope, new NativeFnExprBody(DoComparison((a, b) => a >= b)));

        Func<Executor, Context, IList<IExpression>, InvokeResult> DoIncDec(Func<float, float> op)
        {
            return (executor, ctx, args) =>
            {
                if (args.Count != 1)
                {
                    throw new Exception();
                }

                var unwrapped = executor.Execute(ctx, args[0], false);
                if (unwrapped.Result is not NumExpr numExpr)
                {
                    throw new Exception();
                }

                return new(new NumExpr(op(numExpr.Value)), ctx);
            };
        }

        /*
         * (1+ x)
         */
        rootScope.Data["1+"] = new FnExpr(rootScope, new NativeFnExprBody(DoIncDec(val => val + 1)));

        /*
         * (1- x)
         */
        rootScope.Data["1-"] = new FnExpr(rootScope, new NativeFnExprBody(DoIncDec(val => val - 1)));

        /*
         * (and T)
         */
        rootScope.Data["and"] = new FnExpr(
            rootScope,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    if (args.Count == 0)
                    {
                        return new(AtomExpr.False, ctx);
                    }

                    foreach (var arg in args)
                    {
                        var unwrapped = executor.Execute(ctx, arg, false);
                        var val = unwrapped.Result switch
                        {
                            AtomExpr atomExpr => atomExpr == AtomExpr.True ? true : throw new Exception(),
                            NullExpr nullExpr => false,
                            _ => throw new Exception(),
                        };

                        if (!val)
                        {
                            return new(AtomExpr.False, ctx);
                        }
                    }

                    return new(AtomExpr.True, ctx);
                }
            )
        );

        /*
         * (not T)
         */
        rootScope.Data["not"] = new FnExpr(
            rootScope,
            new NativeFnExprBody(
                (executor, ctx, args) =>
                {
                    if (args.Count != 1)
                    {
                        throw new Exception();
                    }

                    var unwrapped = executor.Execute(ctx, args[0], false);
                    IExpression result = unwrapped.Result switch
                    {
                        AtomExpr atomExpr => atomExpr == AtomExpr.True ? AtomExpr.False : throw new Exception(),
                        NullExpr nullExpr => AtomExpr.True,
                        _ => throw new Exception(),
                    };

                    return new(result, ctx);
                }
            )
        );
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
