using System.Collections.Generic;

namespace HierarchicalFacet2Find
{
    public class HierarchicalFacet : List<HierarchyPath>
    {

    }

    public class HierarchyPath : List<HierarchyPath>
    {
        public string Path { get; set; }
        public new int Count { get; set; }
    }
}
