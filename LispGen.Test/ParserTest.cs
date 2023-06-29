namespace LispGen.Test;

using LispGen;

public class ParserTest
{
    private readonly Parser _parser;

    public ParserTest()
    {
        _parser = new Parser();
    }

    [Fact]
    public void Test_Simple()
    {
        var parsed = _parser.Parse("""
            (foo (bar 1 (baz 2.3)))
        """);

        Assert.Equal(
            new ListExpr(new IExpression[] {
                new AtomExpr("foo"),
                new ListExpr(new IExpression[] {
                    new AtomExpr("bar"),
                    new NumExpr(1),
                    new ListExpr(new IExpression[] {
                        new AtomExpr("baz"),
                        new NumExpr(2.3f)
                    })
                })
            }),
            parsed,
            new ExpressionEqualityComparer()
        );
    }

    [Fact]
    public void Test_Let()
    {
        var parsed = _parser.Parse("""
            (do (let ((var1 1) (var2 2))) (let ((var3 3))) var3)
        """);

        Assert.Equal(
            new ListExpr(new IExpression[] {
                new AtomExpr("do"),
                new ListExpr(new IExpression[] {
                    new AtomExpr("let"),
                    new ListExpr(new IExpression[] {
                        new ListExpr(new IExpression[] {
                            new AtomExpr("var1"),
                            new NumExpr(1),
                        }),
                        new ListExpr(new IExpression[] {
                            new AtomExpr("var2"),
                            new NumExpr(2),
                        })
                    })
                }),
                new ListExpr(new IExpression[] {
                    new AtomExpr("let"),
                    new ListExpr(new IExpression[] {
                        new ListExpr(new IExpression[] {
                            new AtomExpr("var3"),
                            new NumExpr(3),
                        }),
                    })
                }),
                new AtomExpr("var3"),
            }),
            parsed,
            new ExpressionEqualityComparer()
        );
    }

}