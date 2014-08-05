using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Helpers;
using EPiServer.Find.Helpers.Reflection;
using EPiServer.ServiceLocation;
using HierarchicalFacet2Find;
using HierarchicalFacet2Find.Api.Facets;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Find.HierarchicalFacet2Find
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
                var facetRequest = new ExtendedTermsFacetRequest(facetName)
                {
                    Field = fieldName,
                    Order = "term",
                    Size = 1000
                };
                context.RequestBody.Facets.Add(facetRequest);
            });
        }

        public static HierarchicalFacet HierarchicalFacetFor<TResult>(
            this IHasFacetResults<TResult> facetsResultsContainer,
            Expression<Func<PageData, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument("fieldSelector");

            var facetName = fieldSelector.GetFieldPath();
            var facet = facetsResultsContainer.Facets[facetName] as TermsFacet;

            var resultFacet = new HierarchicalFacet();
            if (facet == null) return resultFacet;

            foreach (var termCount in facet)
            {
                if (!termCount.Term.Contains('/'))
                {
                    // create top path
                    resultFacet.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count, Name = termCount.GetTopicName() });
                }
                else
                {
                    // traversing paths
                    var sections = termCount.Term.Split('/');
                    var hierarchyPath = resultFacet.Single(x => x.Path.Equals(sections[0]));
                    for (int i = 2; i < sections.Length; i++)
                    {
                        hierarchyPath = hierarchyPath.Single(x => x.Path.Equals(string.Join("/", sections.Take(i))));
                    }

                    hierarchyPath.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count, Name = termCount.GetTopicName() });
                }
            }

            return resultFacet;
        }

        public static HierarchicalFacet HierarchicalFacetFor<TResult>(this IHasFacetResults<TResult> facetsResultsContainer,
            Expression<Func<TResult, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument("fieldSelector");

            var facetName = fieldSelector.GetFieldPath();
            var facet = facetsResultsContainer.Facets[facetName] as TermsFacet;

            var resultFacet = new HierarchicalFacet();
            if (facet == null) return resultFacet;

            foreach (var termCount in facet)
            {
                if (!termCount.Term.Contains('/'))
                {
                    // create top path
                    resultFacet.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count, Name = termCount.GetTopicName() });
                }
                else
                {
                    // traversing paths
                    var sections = termCount.Term.Split('/');
                    var hierarchyPath = resultFacet.Single(x => x.Path.Equals(sections[0]));
                    for (int i = 2; i < sections.Length; i++)
                    {
                        hierarchyPath = hierarchyPath.Single(x => x.Path.Equals(string.Join("/", sections.Take(i))));
                    }

                    hierarchyPath.Add(new HierarchyPath { Path = termCount.Term, Count = termCount.Count, Name = termCount.GetTopicName() });
                }
            }

            return resultFacet;
        }

        public static string GetTopicName(this TermCount termCount)
        {
            if (string.IsNullOrWhiteSpace(termCount.Term))
                return string.Empty;
            var term = termCount.Term.Split('/').Last();

            var pageRef = new PageReference(term);
            var pageData = ServiceLocator.Current.GetInstance<IContentLoader>().Get<PageData>(pageRef);
            if (pageData == null) return string.Empty;

            return pageData.PageName;
        }
    }
}
