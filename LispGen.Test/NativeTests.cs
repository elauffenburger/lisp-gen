using System.Text;

using Microsoft.Extensions.FileProviders;

namespace LispGen.Test;

public class NativeTests
{
    private static readonly HashSet<string> AllowlistedTests = new()
    {
        "cons.list.lisp",

        "math.and.lisp",
        "math.dec.lisp",
        "math.div.lisp",
        "math.eq.lisp",
        "math.equal.lisp",
        "math.gt.lisp",
        "math.gte.lisp",
        "math.inc.lisp",
        "math.lt.lisp",
        "math.lte.lisp",
        "math.or.lisp",
        "math.plus.lisp",
        "math.sub.lisp",
        "math.times.lisp"
    };

    [Fact]
    public async Task Test_All()
    {
        var fileProvider = new EmbeddedFileProvider(GetType().Assembly);
        var tests = fileProvider.GetDirectoryContents("")
            .Where(file => ShouldRunTest(file.Name));

        var parser = new Parser();
        var executor = new Executor();

        var runTests = new HashSet<string>();
        var failedTests = new Dictionary<string, Exception>();
        foreach (var test in tests)
        {
            runTests.Add(test.Name);

            try
            {
                var fileContents = await new StreamReader(test.CreateReadStream()).ReadToEndAsync();
                var parsed = parser.Parse(fileContents);

                var ctx = new Context(Scope.Root());
                var result = executor.Execute(ctx, parsed);

                Assert.Equal(AtomExpr.True, result.Result);
            }
            catch (Exception e)
            {
                failedTests.Add(test.Name, e);
            }
        }

        // Make sure we ran the number of tests we expected to.
        Assert.Equal(runTests.Count, AllowlistedTests.Count);

        // If there were any failed tests (oh no!)
        if (failedTests.Any())
        {
            var formattedErrors = failedTests.Aggregate(new StringBuilder(), (acc, kvp) =>
            {
                return acc.Append($"- {kvp.Key} failed:\n{kvp.Value}\n\n");
            });

            throw new Exception(formattedErrors.ToString());
        }
    }

    private static bool ShouldRunTest(string name)
    {
        foreach (var test in AllowlistedTests)
        {
            if (name.EndsWith(test))
            {
                return true;
            }
        }

        return false;
    }
}
