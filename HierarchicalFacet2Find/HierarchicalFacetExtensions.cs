using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Find;
using EPiServer.Find.Helpers;
using EPiServer.Find.Helpers.Reflection;
using System.Linq.Expressions;
using EPiServer.Find.Api.Querying;
using HierarchicalFacet2Find.Api.Facets;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Api.Querying.Filters;

namespace HierarchicalFacet2Find
{
    public static class HierarchicalFacetExtensions
    {
        public static ITypeSearch<TSource> HierarchicalFacetFor<TSource>(
           this ITypeSearch<TSource> search,
           Expression<Func<TSource, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument(nameof(fieldSelector));

            var facetName = fieldSelector.GetFieldPath();
            var fieldName = search.Client.Conventions.FieldNameConvention.GetFieldName(fieldSelector);

            return new Search<TSource, IQuery>(search, context =>
            {
                var facetRequest = new ExtendedTermsFacetRequest(facetName)
                {
                    Field = fieldName, Order = "term", Size = 1000
                };

                context.RequestBody.Facets.Add(facetRequest);
            });
        }

        public static HierarchicalFacet HierarchicalFacetFor<TResult>(
            this IHasFacetResults<TResult> facetsResultsContainer, Expression<Func<TResult, Hierarchy>> fieldSelector)
        {
            fieldSelector.ValidateNotNullArgument(nameof(fieldSelector));

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

        public static ITypeSearch<TSource> HierarchicalFacetFor<TSource, TListItem>(this ITypeSearch<TSource> search,
            Expression<Func<TSource, IEnumerable<TListItem>>> enumerableFieldSelector,
            Expression<Func<TListItem, string>> itemFieldSelector,
            Expression<Func<TListItem, Filter>> filterExpression = null,
            Action<NestedTermsFacetRequest> facetRequestAction = null)
        {
            var filter = NestedFilter.Create(search.Client.Conventions, enumerableFieldSelector, filterExpression);

            var action = !facetRequestAction.IsNotNull() ? x => x.FacetFilter = filter : new Action<NestedTermsFacetRequest>(x => {
                x.FacetFilter = filter;
                facetRequestAction(x);
            });

            return search.AddNestedHierarchicalFacetFor(enumerableFieldSelector, itemFieldSelector, action);
        }

        public static HierarchicalFacet HierarchicalFacetFor<TResult, TEnumerableItem>(
            this IHasFacetResults<TResult> facetsResultsContainer,
            Expression<Func<TResult, IEnumerable<TEnumerableItem>>> enumerableFieldSelector,
            Expression<Func<TEnumerableItem, string>> itemFieldSelector)
        {
            enumerableFieldSelector.ValidateNotNullArgument(nameof(enumerableFieldSelector));
            itemFieldSelector.ValidateNotNullArgument(nameof(itemFieldSelector));

            string fieldPath = string.Concat(enumerableFieldSelector.GetFieldPath(), enumerableFieldSelector.GetFieldPath().Length > 0 ? "." : "",
                itemFieldSelector.GetFieldPath());

            var facet = facetsResultsContainer.Facets[fieldPath] as TermsFacet;
            var hierarchicalFacet = new HierarchicalFacet();

            foreach (var termCount in facet.OrderBy(x => x.Term.Count(c => c == '/')))
            {
                if (string.IsNullOrEmpty(termCount.Term))
                {
                    continue;
                }

                if (!termCount.Term.Contains('/'))
                {
                    hierarchicalFacet.Add(new HierarchyPath
                    {
                        Path = termCount.Term,
                        Count = termCount.Count
                    });

                    continue;
                }

                string[] sections = termCount.Term.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (!sections.Any())
                {
                    continue;
                }

                var hierarchyPath = hierarchicalFacet.FirstOrDefault(x => x.Path.Equals(sections[0]));

                if (hierarchyPath == null)
                {
                    continue;
                }

                for (int i = 2; i < sections.Length; ++i)
                {
                    hierarchyPath = hierarchyPath.Single(x => x.Path.Equals(string.Join("/", sections.Take(i))));
                }

                hierarchyPath.Add(new HierarchyPath
                {
                    Path = termCount.Term,
                    Count = termCount.Count
                });
            }

            return hierarchicalFacet;
        }

        private static ITypeSearch<TSource> AddNestedHierarchicalFacetFor<TSource>(this ITypeSearch<TSource> search,
            Expression enumerableFieldSelector, Expression itemFieldSelector,
            Action<NestedTermsFacetRequest> facetRequestAction)
        {
            enumerableFieldSelector.ValidateNotNullArgument(nameof(enumerableFieldSelector));
            itemFieldSelector.ValidateNotNullArgument(nameof(itemFieldSelector));

            string fieldPath = string.Concat(enumerableFieldSelector.GetFieldPath(), ".", itemFieldSelector.GetFieldPath());

            return new Search<TSource, IQuery>(search, context =>
            {
                NestedTermsFacetRequest nestedTermsFacetRequest = new ExtendedNestedTermsFacetRequest(fieldPath)
                {
                    Field = string.Concat(search.Client.Conventions.FieldNameConvention.GetNestedFieldPath(enumerableFieldSelector), ".", search.Client.Conventions.FieldNameConvention.GetFieldName(itemFieldSelector)),
                    Nested = search.Client.Conventions.FieldNameConvention.GetNestedFieldPath(enumerableFieldSelector),
                    Order = "term",
                    Size = 1000
                };

                if (facetRequestAction.IsNotNull())
                {
                    facetRequestAction(nestedTermsFacetRequest);
                }

                context.RequestBody.Facets.Add(nestedTermsFacetRequest);
            });
        }
    }
}
