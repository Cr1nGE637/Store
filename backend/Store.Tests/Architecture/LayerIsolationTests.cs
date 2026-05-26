using System.Xml.Linq;

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

    private static readonly string[] ContractsModules =
    [
        "Store.Catalog.Contracts",
        "Store.Carts.Contracts",
        "Store.Ordering.Contracts",
        "Store.Inventory.Contracts",
        "Store.Identity.Contracts"
    ];

    [Fact]
    public void DomainLayer_DoesNotReferenceInfrastructureOrApiDetails()
    {
        var forbiddenReferences = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "Npgsql",
            ".Infrastructure",
            ".Application",
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
            .Concat([
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Npgsql"
            ])
            .ToArray();

        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module, "Application")))
            .SelectMany(file => FindForbiddenReferences(file, forbiddenReferences))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void BusinessModules_UseOtherModulesOnlyThroughContracts()
    {
        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module))
                .SelectMany(file => FindForbiddenReferences(
                    file,
                    BusinessModules
                        .Where(otherModule => otherModule != module)
                        .SelectMany(ForbiddenNonContractNamespacesFor))))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void ApiLayer_DoesNotReferenceInfrastructureOrDomainImplementation()
    {
        var violations = BusinessModules
            .SelectMany(module => SourceFilesIn(Path.Combine(RepositoryRoot(), module, "API"))
                .SelectMany(file => FindForbiddenReferences(
                    file,
                    [
                        $"{module}.Infrastructure",
                        $"{module}.Domain"
                    ])))
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void BusinessModuleProjects_ReferenceOnlySharedBuildingBlocksAndContracts()
    {
        var allowedProjectReferences = BusinessModules
            .ToDictionary(
                module => module,
                module => new HashSet<string>(
                    [
                        "Store.SharedKernel",
                        "Store.EventOutbox",
                        $"{module}.Contracts",
                        .. ContractsModules
                    ],
                    StringComparer.OrdinalIgnoreCase));

        var violations = BusinessModules
            .SelectMany(module =>
            {
                var projectName = $"{module}.csproj";
                var projectPath = Path.Combine(RepositoryRoot(), module, projectName);
                var references = ProjectReferences(projectPath);

                return references
                    .Where(reference => !allowedProjectReferences[module].Contains(reference))
                    .Select(reference => $"{module}/{projectName} references disallowed project {reference}");
            })
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void ContractProjects_ReferenceOnlySharedKernel()
    {
        var violations = ContractsModules
            .SelectMany(module =>
            {
                var projectName = $"{module}.csproj";
                var projectPath = Path.Combine(RepositoryRoot(), module, projectName);
                var references = ProjectReferences(projectPath);

                return references
                    .Where(reference => !string.Equals(reference, "Store.SharedKernel", StringComparison.OrdinalIgnoreCase))
                    .Select(reference => $"{module}/{projectName} references disallowed project {reference}");
            })
            .ToArray();

        AssertNoViolations(violations);
    }

    [Fact]
    public void SharedKernel_DoesNotReferenceFeatureModulesOrInfrastructurePackages()
    {
        var projectPath = Path.Combine(RepositoryRoot(), "Store.SharedKernel", "Store.SharedKernel.csproj");
        var projectReferenceViolations = ProjectReferences(projectPath)
            .Select(reference => $"Store.SharedKernel/Store.SharedKernel.csproj references disallowed project {reference}");

        var forbiddenPackages = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "Microsoft.AspNetCore"
        };

        var packageViolations = PackageReferences(projectPath)
            .Where(package => forbiddenPackages.Any(forbidden =>
                package.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase)))
            .Select(package => $"Store.SharedKernel/Store.SharedKernel.csproj references disallowed package {package}");

        var sourceViolations = SourceFilesIn(Path.Combine(RepositoryRoot(), "Store.SharedKernel"))
            .SelectMany(file => FindForbiddenReferences(
                file,
                BusinessModules.Concat(ContractsModules)))
            .ToArray();

        var violations = projectReferenceViolations
            .Concat(packageViolations)
            .Concat(sourceViolations)
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

    private static IEnumerable<string> ForbiddenNonContractNamespacesFor(string module) =>
    [
        $"{module}.Domain",
        $"{module}.Application",
        $"{module}.Infrastructure",
        $"{module}.API"
    ];

    private static IEnumerable<string> FindForbiddenReferences(string file, IEnumerable<string> forbiddenReferences)
    {
        var text = File.ReadAllText(file);
        var relativePath = Path.GetRelativePath(RepositoryRoot(), file);

        return forbiddenReferences
            .Where(reference => text.Contains(reference, StringComparison.Ordinal))
            .Select(reference => $"{relativePath} references {reference}");
    }

    private static IReadOnlyList<string> ProjectReferences(string projectPath) =>
        ProjectItemIncludes(projectPath, "ProjectReference")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Cast<string>()
            .ToArray();

    private static IReadOnlyList<string> PackageReferences(string projectPath) =>
        ProjectItemIncludes(projectPath, "PackageReference").ToArray();

    private static IEnumerable<string> ProjectItemIncludes(string projectPath, string itemName)
    {
        var document = XDocument.Load(projectPath);
        return document
            .Descendants(itemName)
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Cast<string>();
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
