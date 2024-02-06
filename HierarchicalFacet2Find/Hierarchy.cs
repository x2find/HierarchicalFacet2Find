using System.Collections.Generic;
using System.Linq;

namespace HierarchicalFacet2Find
{
    public class Hierarchy : List<string>
    {

        public static implicit operator Hierarchy(string h1)
        {
            var property = new Hierarchy();

            var section1 = h1.Split('#');

            foreach (var h2 in section1)
            {
                var sections = h2.Split('/');
                var concatenatedSections = sections[0];
                for (int i = 1; i < sections.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(concatenatedSections))
                        property.Add(concatenatedSections);
                    concatenatedSections += "/" + sections[i];
                }

                // add full path last
                if (!string.IsNullOrWhiteSpace(h2))
                    property.Add(h2);
            }

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
