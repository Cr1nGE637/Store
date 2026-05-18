namespace Store.Tests.Architecture;

public class LayerIsolationTests
{
    private static readonly string[] BusinessModules =
    [
        "Store.Catalog",
        "Store.Carts",
        "Store.Ordering",
        "Store.Inventory",
        "Store.Identity",
        "Store.Notifications"
    ];

    [Fact]
    public void DomainLayer_DoesNotReferenceInfrastructureOrApiDetails()
    {
        var forbiddenReferences = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            ".Infrastructure",
            ".API"
        };

        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module, "Domain")))
            .SelectMany(file => FindForbiddenReferences(file, forbiddenReferences))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void ApplicationLayer_DoesNotReferenceModuleInfrastructure()
    {
        var forbiddenReferences = BusinessModules
            .Select(module => $"{module}.Infrastructure")
            .ToArray();

        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module, "Application")))
            .SelectMany(file => FindForbiddenReferences(file, forbiddenReferences))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void BusinessModules_DoNotReferenceOtherModulesInfrastructure()
    {
        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module))
                .SelectMany(file => FindForbiddenReferences(
                    file,
                    BusinessModules
                        .Where(otherModule => otherModule != module)
                        .Select(otherModule => $"{otherModule}.Infrastructure"))))
            .ToArray();

        AssertNoViolations(violations);
    }

    private static IEnumerable<string> SourceFilesIn(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> FindForbiddenReferences(string file, IEnumerable<string> forbiddenReferences)
    {
        var text = File.ReadAllText(file);
        var relativePath = Path.GetRelativePath(RepositoryRoot(), file);

        return forbiddenReferences
            .Where(reference => text.Contains(reference, StringComparison.Ordinal))
            .Select(reference => $"{relativePath} references {reference}");
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Store.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException("Could not locate repository root with Store.sln.");
        }

        return directory.FullName;
    }

    private static void AssertNoViolations(IReadOnlyCollection<string> violations)
    {
        Assert.True(
            violations.Count == 0,
            "Layer isolation violations:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }
}
