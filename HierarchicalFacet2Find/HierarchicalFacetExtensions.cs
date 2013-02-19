using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer.Find;
using EPiServer.Find.Helpers;
using EPiServer.Find.Helpers.Reflection;
using System.Linq.Expressions;
using EPiServer.Find.Api.Querying;
using HierarchicalFacet2Find.Api.Facets;
using EPiServer.Find.Api.Facets;

namespace HierarchicalFacet2Find
{
    public static class HierarchicalFacetExtensions
    {
        public static ITypeSearch<TSource> HierarchicalFacetFor<TSource>(
           this ITypeSearch<TSource> search,
           Expression<Func<TSource, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument("fieldSelector");

            var facetName = fieldSelector.GetFieldPath();
            var fieldName = search.Client.Conventions.FieldNameConvention.GetFieldName(fieldSelector);
            return new Search<TSource, IQuery>(search, context =>
            {
                var facetRequest = new ExtendedTermsFacetRequest(facetName);
                facetRequest.Field = fieldName;
                facetRequest.Order = "term";
                facetRequest.Size = 1000;
                context.RequestBody.Facets.Add(facetRequest);
            });
        }

        public static HierarchicalFacet HierarchicalFacetFor<TResult>(this IHasFacetResults<TResult> facetsResultsContainer, Expression<Func<TResult, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument("fieldSelector");

            var facetName = fieldSelector.GetFieldPath();
            var facet = facetsResultsContainer.Facets[facetName] as TermsFacet;

            var resultFacet = new HierarchicalFacet();
            foreach (var termCount in facet)
            {
                if (!termCount.Term.Contains('/'))
                {
                    // create top path
                    resultFacet.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count });
                }
                else
                {
                    // traversing paths
                    var sections = termCount.Term.Split('/');
                    var hierarchyPath = resultFacet.Where(x => x.Path.Equals(sections[0])).Single();
                    for (int i = 2; i < sections.Length; i++)
                    {
                        hierarchyPath = hierarchyPath.Where(x => x.Path.Equals(string.Join("/", sections.Take(i)))).Single();
                    }

                    hierarchyPath.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count });
                }
            }

            return resultFacet;
        }
    }
}
