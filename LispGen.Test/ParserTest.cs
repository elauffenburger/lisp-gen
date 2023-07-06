namespace LispGen.Test;
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
    public void Test_Newlines()
    {
        var parsed = _parser.Parse("""
            (do 
                (let ((var1 1)
                      (var2 2)))
                (let ((var3 3))))
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
                        }),
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

    [Fact]
    public void Test_Comments()
    {
        var parsed = _parser.Parse("""
            ; comment
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

        Assert.Equal(
            new ListExpr(new IExpression[] {
                new AtomExpr("do"),
                new ListExpr(new IExpression[] {
                    new AtomExpr("let"),
                    new ListExpr(new IExpression[] {
                        new ListExpr(new IExpression[] {
                            new AtomExpr("x"),
                            new NumExpr(42),
                        }),
                        new ListExpr(new IExpression[] {
                            new AtomExpr("y"),
                            new NumExpr(13),
                        })
                    })
                }),
                new ListExpr(new IExpression[] {
                    new AtomExpr("defn"),
                    new AtomExpr("add2"),
                    new ListExpr(new IExpression[] {
                        new AtomExpr("x"),
                        new AtomExpr("y"),
                    }),
                    new ListExpr(new IExpression[] {
                        new AtomExpr("add"),
                        new AtomExpr("x"),
                        new ListExpr(new IExpression[] {
                            new AtomExpr("add"),
                            new AtomExpr("y"),
                            new AtomExpr("z"),
                        }),
                    })
                }),
                new ListExpr(new IExpression[] {
                    new AtomExpr("let"),
                    new ListExpr(new IExpression[] {
                        new ListExpr(new IExpression[] {
                            new AtomExpr("z"),
                            new NumExpr(18),
                        }),
                    })
                }),
                new ListExpr(new IExpression[] {
                    new AtomExpr("add2"),
                    new AtomExpr("x"),
                    new AtomExpr("z"),
                }),
            }),
            parsed,
            new ExpressionEqualityComparer()
        );
    }
}
