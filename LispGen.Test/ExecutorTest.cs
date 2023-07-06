namespace LispGen.Test;

public class ExecutorTest
{
    private readonly Parser _parser = new();
    private readonly Executor _executor = new();

    private readonly Scope _rootScope = Scope.Root();
    private readonly ExecutionContext _rootContext;

    public ExecutorTest()
    {
        _rootContext = new(_rootScope);
    }

    [Fact]
    public void Test_BuiltInFn()
    {
        var parsed = _parser.Parse("""
            (add 40 (add 1 1))
        """);

        var result = _executor.Execute(_rootContext, parsed);
        Assert.Equal(new NumExpr(42), result.Result);
    }

    [Fact]
    public void Test_Let()
    {
        var parsed = _parser.Parse("""
            (do 
                ;(let ((var1 1) (var2 2))) 
                (let ((var3 3))) 
                var3)
        """);

        var result = _executor.Execute(_rootContext, parsed);
        Assert.Equal(new NumExpr(3), result.Result);
    }

    [Fact]
    public void Test_DefinedFn()
    {
        var parsed = _parser.Parse("""
            (do (defn add2 (x y) (add x y)) 
                (let ((res (add2 40 (add 1 1))))) 
                res)
        """);

        var result = _executor.Execute(_rootContext, parsed);
        Assert.Equal(new NumExpr(42), result.Result);
    }


    [Fact]
    public void Test_DefinedFnScope()
    {
        var parsed = _parser.Parse("""
            (do 
                ; set x and y
                (let ((x 42) 
                      (y 13))) 
                    
                ; create a fn that uses an x and y param.
                ; and tries to use z (which should be undefined).
                (defn add2 (x y) 
                    (add x (add y z))) 
                
                ; define z as a red herring that shouldn't be
                ; available in add2's scope.
                (let ((z 18))) 

                ; invoke add2 with x and z and make sure they
                ; bind correctly to the x and y params in add2.
                (add2 x z))
        """);

        var result = _executor.Execute(_rootContext, parsed);
        Assert.Equal(new NumExpr(60), result.Result);
    }
}