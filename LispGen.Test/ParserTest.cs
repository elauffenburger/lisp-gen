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
        _parser.Parse("""
            (foo (bar 1 (baz 2.3)))
        """
        );
    }
}