namespace LispGen.Test;

public class ExecutorTest
{
    private readonly Parser _parser = new();
    private readonly Executor _executor = new();

    private readonly Scope _rootScope = Scope.Root();

    [Fact]
    public void Test_BuiltInFn()
    {
        var parsed = _parser.Parse("""
            (add 40 (add 1 1))
        """);

        var result = _executor.Execute(_rootScope, parsed);
        Assert.Equal(new NumExpr(42), result);
    }

    [Fact]
    public void Test_Let()
    {
        var parsed = _parser.Parse("""
            (do (let ((var1 1) (var2 2))) (let ((var3 3))) var3)
        """);

        var result = _executor.Execute(_rootScope, parsed);
        Assert.Equal(new NumExpr(3), result);
    }

    [Fact]
    public void Test_DefinedFN()
    {
        var parsed = _parser.Parse("""
            (do (defn add2 (x y) (add x y)) (let ((res (add2 40 (add 1 1))))) res)
        """);

        var result = _executor.Execute(_rootScope, parsed);
        Assert.Equal(new NumExpr(42), result);
    }
}