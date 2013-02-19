using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HierarchicalFacet2Find
{
    public class Hierarchy : List<string>
    {
        public static implicit operator Hierarchy(string hierarchy)
        {
            var property = new Hierarchy();
            var sections = hierarchy.Split('/');
            var concatenatedSections = sections[0];
            for(int i = 1; i < sections.Length; i++)
            {
                property.Add(concatenatedSections);
                concatenatedSections += "/" + sections[i];
            }

            // add full path last
            property.Add(hierarchy);

            return property;
        }

        public static implicit operator string(Hierarchy property)
        {
            if (property == null)
            {
                return null;
            }

            return property.Last(); // this is the full path
        }
    }
}
