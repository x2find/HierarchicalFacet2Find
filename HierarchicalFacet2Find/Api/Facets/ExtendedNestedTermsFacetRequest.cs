using EPiServer.Find.Api.Facets;
using Newtonsoft.Json;

namespace HierarchicalFacet2Find.Api.Facets
{
    internal class ExtendedNestedTermsFacetRequest : NestedTermsFacetRequest
    {
        public ExtendedNestedTermsFacetRequest(string name)
            : base(name)
        {
        }

        [JsonProperty("regex", NullValueHandling = NullValueHandling.Ignore)]
        public string Regex { get; set; }

        [JsonProperty("regex_flags", NullValueHandling = NullValueHandling.Ignore)]
        public string RegexFlags { get; set; }

        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public string Order { get; set; }
    }
}