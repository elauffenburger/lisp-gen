namespace LispGen.Test;

using LispGen;

[CollectionDefinition(Collection)]
public class ParserTestCollectionDefinition : ICollectionFixture<ParserTestCollectionDefinition>
{
    public const string Collection = "Parser";

}

[Collection(ParserTestCollectionDefinition.Collection)]
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
        """
        );

        Assert.Equal(
            new ListExpr(new Expression[] {
                new AtomExpr("foo"),
                new ListExpr(new Expression[] {
                    new AtomExpr("bar"),
                    new NumberExpr(1),
                    new ListExpr(new Expression[] {
                        new AtomExpr("baz"),
                        new NumberExpr(2.3f)
                    })
                })
            }),
            parsed,
            new ExpressionEqualityComparer()
        );
    }
}