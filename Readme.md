HierarchicalFacet2Find
===============

Adds hierarchical faceting to EPiServer Find's .NET API

### Build

In order to build HierarchicalFacet2Find the NuGet packages that it depends on must be restored.
See http://docs.nuget.org/docs/workflows/using-nuget-without-committing-packages

### Usage

Add a Hierarchy property to the document:

```c#
public class Document
{
    [Id]
    public string Id { get; set; }

    public Hierarchy Hierarchy { get; set; }
}
```

set the hierarchy path (sections separated by '/'):

```c#
document.Hierarchy = "A/B/C/D";
```

index and request a HierarchicalFacet when searching:

```c#
result = client.Search<Document>()
            .HierarchicalFacetFor(x => x.Hierarchy)
            .GetResult();
```

fetch it from the result:

```c#
facet = result.HierarchicalFacetFor(x => x.Hierarchy)
```

and loop over the nested hierarchy paths

```c#
foreach(var hierarchyPath in facet)
{
    hierarchyPath.Path;
    hierarchyPath.Count;
                
    foreach (var subHierarchyPath in hierarchyPath)
    {
        subHierarchyPath.Path;
        subHierarchyPath.Count;
        ...
    }
}
```