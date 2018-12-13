using System.Collections.Generic;

namespace HierarchicalFacet2Find
{
    public class HierarchicalFacet : List<HierarchyPath>
    {

    }

    public class HierarchyPath : List<HierarchyPath>
    {
        public HierarchyPath(string path, int count)
        {
            Path = path;
            Count = count;
        }

        public string Path { get; }

        public new int Count { get; }
    }
}
