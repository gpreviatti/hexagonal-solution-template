using System.Reflection;
using System.Xml.Linq;

namespace UnitTests.Architecture;

internal static class ArchitectureTestHelper
{
    private static readonly string _repositoryRoot = FindRepositoryRoot();

    public static void AssertClassNamesDoNotEndWith(Assembly assembly, params string[] forbiddenSuffixes)
    {
        var invalidTypes = assembly
            .GetTypes()
            .Where(static type => type.IsClass && !type.IsAbstract)
            .Where(type => forbiddenSuffixes.Any(suffix => type.Name.EndsWith(suffix, StringComparison.Ordinal)))
            .Select(static type => type.FullName ?? type.Name)
            .OrderBy(static typeName => typeName, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            invalidTypes.Length == 0,
            $"The assembly '{assembly.GetName().Name}' contains classes with forbidden suffixes:{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", invalidTypes)}"
        );
    }

    public static void AssertNamespacesStartWith(
        Assembly assembly,
        string expectedRootNamespace,
        params string[] additionalAllowedRootNamespaces)
    {
        var allowedRootNamespaces = new[] { expectedRootNamespace }
            .Concat(additionalAllowedRootNamespaces)
            .ToArray();

        var invalidTypes = assembly
            .GetTypes()
            .Where(static type => !string.IsNullOrWhiteSpace(type.Namespace))
            .Where(type => type.Namespace is not null
                && !allowedRootNamespaces.Any(allowedRootNamespace =>
                    type.Namespace.Equals(allowedRootNamespace, StringComparison.Ordinal)
                    || type.Namespace.StartsWith($"{allowedRootNamespace}.", StringComparison.Ordinal)))
            .Select(static type => type.FullName ?? type.Name)
            .OrderBy(static typeName => typeName, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            invalidTypes.Length == 0,
            $"The assembly '{assembly.GetName().Name}' contains namespaces outside '{expectedRootNamespace}':{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", invalidTypes)}"
        );
    }

    public static void AssertAssemblyDoesNotReference(Assembly assembly, params string[] forbiddenAssemblyNames)
    {
        var references = assembly
            .GetReferencedAssemblies()
            .Select(static referencedAssembly => referencedAssembly.Name)
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var invalidReferences = forbiddenAssemblyNames
            .Where(references.Contains)
            .OrderBy(static assemblyName => assemblyName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.True(
            invalidReferences.Length == 0,
            $"The assembly '{assembly.GetName().Name}' must not reference: {string.Join(", ", invalidReferences)}."
        );
    }

    public static void AssertProjectReferences(string projectRelativePath, params string[] expectedProjectRelativePaths)
    {
        var projectFilePath = Path.Combine(_repositoryRoot, projectRelativePath);

        Assert.True(File.Exists(projectFilePath), $"The project file '{projectFilePath}' was not found.");

        var projectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException($"Could not determine the directory for '{projectFilePath}'.");

        var actualReferences = XDocument
            .Load(projectFilePath)
            .Descendants("ProjectReference")
            .Select(static node => (string?)node.Attribute("Include"))
            .OfType<string>()
            .Select(reference => NormalizePath(Path.GetFullPath(Path.Combine(projectDirectory, reference))))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var expectedReferences = expectedProjectRelativePaths
            .Select(relativePath => NormalizePath(Path.GetFullPath(Path.Combine(_repositoryRoot, relativePath))))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(expectedReferences, actualReferences);
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var readmeFilePath = Path.Combine(currentDirectory.FullName, "Readme.md");
            var sourceDirectoryPath = Path.Combine(currentDirectory.FullName, "src");

            if (File.Exists(readmeFilePath) && Directory.Exists(sourceDirectoryPath))
                return currentDirectory.FullName;

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Could not find the repository root for the architecture tests.");
    }

    private static string NormalizePath(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
