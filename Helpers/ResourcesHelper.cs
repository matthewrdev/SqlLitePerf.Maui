using System.Diagnostics;
using System.Reflection;

namespace SqlUsage.Helpers;

/// <summary>
/// Helper class for locating and reading embedded resources in an assembly.
/// </summary>
public static class ResourcesHelper
{
    /// <summary>
    /// Locates a resource in the provided assembly that has a partial match with the provided name.
    /// </summary>
    /// <returns>The matching resource identifier.</returns>
    /// <param name="instance">The instance.</param>
    /// <param name="partialName">Partial name.</param>
    /// <param name="suffix">Suffix.</param>
    public static string LocateMatchingResourceId(object instance, string resourceName)
    {
        if (instance == null)
        {
            return string.Empty;
        }

        return LocateMatchingResourceId(instance.GetType().Assembly, resourceName);
    }

    /// <summary>
    /// Locates a resource in the provided assembly that has a partial match with the provided name.
    /// </summary>
    /// <returns>The matching resource identifier.</returns>
    /// <param name="assembly">Assembly.</param>
    /// <param name="partialName">Partial name.</param>
    /// <param name="suffix">Suffix.</param>
    public static string LocateMatchingResourceId(Assembly assembly, string resourceName)
    {
        var resources = assembly.GetManifestResourceNames();

        var match = resources.FirstOrDefault(s => s.Contains(resourceName));

        if (match == null)
        {
            Debugger.Break();
        }

        return match;
    }

    /// <summary>
    /// Reads the text content of the resource from the assembly that owns <paramref name="instance"/>.
    /// </summary>
    /// <returns>The resource content.</returns>
    /// <param name="instance">Assembly.</param>
    /// <param name="resourceName">Resource name.</param>
    public static string ReadResourceTextContent(object instance, string resourceName)
    {
        if (instance == null)
        {
            return string.Empty;
        }

        return ReadResourceTextContent(instance.GetType().Assembly, resourceName);
    }

    /// <summary>
    /// Reads the text content of the resource from the provided assembly.
    /// </summary>
    /// <returns>The resource content.</returns>
    /// <param name="assembly">Assembly.</param>
    /// <param name="resourceName">Resource name.</param>
    public static string ReadResourceTextContent(Assembly assembly, string resourceName)
    {
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Reads the binary content of the resource from the assembly that owns <paramref name="instance"/>.
    /// </summary>
    /// <returns>The resource content.</returns>
    /// <param name="instance">Assembly.</param>
    /// <param name="resourceName">Resource name.</param>
    public static byte[] ReadResourceBinaryContent(object instance, string resourceName)
    {
        if (instance == null)
        {
            return null;
        }

        return ReadResourceBinaryContent(instance.GetType().Assembly, resourceName);
    }

    /// <summary>
    /// Reads the binary content of the resource from the provided assembly.
    /// </summary>
    /// <returns>The resource content.</returns>
    /// <param name="assembly">Assembly.</param>
    /// <param name="resourceName">Resource name.</param>
    public static byte[] ReadResourceBinaryContent(Assembly assembly, string resourceName)
    {
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            byte[] content = new byte[stream.Length];
            stream.Read(content, 0, content.Length);
            return content;
        }
    }

    /// <summary>
    /// Extracts the embedded resource '<paramref name="resourceName"/>' in the <paramref name="assembly"/> onto a new file '<paramref name="fileName"/>'.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="resourceName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static bool ExtractResourceToFile(Assembly assembly, string resourceName, string fileName)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentException($"'{nameof(resourceName)}' cannot be null or empty.", nameof(resourceName));
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException($"'{nameof(fileName)}' cannot be null or empty.", nameof(fileName));
        }

        var fileInfo = new FileInfo(fileName);
        if (!Directory.Exists(fileInfo.DirectoryName))
        {
            Directory.CreateDirectory(fileInfo.DirectoryName);
        }

        using (var resource = assembly.GetManifestResourceStream(resourceName))
        {
            using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                resource.CopyTo(file);
            }
        }

        return File.Exists(fileName);
    }
}