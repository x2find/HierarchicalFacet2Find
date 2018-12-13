using System.Collections.Generic;
using System.Linq;
using Xunit;
using EPiServer.Find;
using System.Threading;
using FluentAssertions;
using StoryQ;

namespace HierarchicalFacet2Find.Tests.Stories
{
    public class NestedHierarchicalFacetStory
    {
        [Fact]
        public void NestedHierarchicalFacet()
        {
            new Story("Getting a hierarchical facet")
                .InOrderTo("be able to get a hierarchical facet in the result")
                .AsA("developer")
                .IWant("to be able to use a hierarchical property and get a hierarchical facet")
                .WithScenario("use a hierarchical property and create a hierarchical facet")
                .Given(IHaveAClient)
                    .And(TheClientHasANestedConvention)
                    .And(IHaveADocument)
                    .And(TheDocumentHasANestedDocument)
                    .And(TheDocumentNestedDocumentHasAFourLevelHierarchyABCD)
                    .And(IHaveASecondDocument)
                    .And(TheSecondDocumentHasANestedDocument)
                    .And(TheSecondDocumentNestedDocumentHasAFourLevelHierarchyABCE)
                    .And(TheSecondDocumentHasASecondNestedDocument)
                    .And(TheSecondDocumentSecondNestedDocumentHasAThreeLevelHierarchyKLM)
                    .And(IHaveIndexedTheDocuments)
                    .And(IHaveWaitedForASecond)
                .When(ISearchForAllDocumentsAndCreateANestedHierarchicalFacet)
                .Then(IShouldGetAHierarchicalFacet)
                .And(IShouldGetAHierarchicalFacetWithPathAAndCount2)
                .And(IShouldGetAHierarchicalFacetWithPathABAndCount2)
                .And(IShouldGetAHierarchicalFacetWithPathABCAndCount2)
                .And(IShouldGetAHierarchicalFacetWithPathABCDAndCount1)
                .And(IShouldGetAHierarchicalFacetWithPathABCEAndCount1)
                .And(IShouldGetAHierarchicalFacetWithPathKAndCount1)
                .And(IShouldGetAHierarchicalFacetWithPathKLAndCount1)
                .And(IShouldGetAHierarchicalFacetWithPathKLMAndCount1)
                .Execute();
        }

        protected IClient client;
        void IHaveAClient()
        {
            client = Client.CreateFromConfig();
            client.Conventions.NestedConventions.ForType<Document>().Add(x => x.Documents);
        }

        void TheClientHasANestedConvention()
        {
            client.Conventions.NestedConventions.ForType<Document>().Add(x => x.Documents);
        }

        private Document document;
        void IHaveADocument()
        {
            document = new Document { Id = "1" };
        }

        void TheDocumentHasANestedDocument()
        {
            document.Documents.Add(new NestedDocument { Id = "2" });
        }

        void TheDocumentNestedDocumentHasAFourLevelHierarchyABCD()
        {
            document.Documents.First().Hierarchy = "A/B/C/D";
        }

        private Document secondDocument;
        void IHaveASecondDocument()
        {
            secondDocument = new Document { Id = "3" };
        }

        void TheSecondDocumentHasANestedDocument()
        {
            secondDocument.Documents.Add(new NestedDocument { Id = "4" });
        }

        void TheSecondDocumentNestedDocumentHasAFourLevelHierarchyABCE()
        {
            secondDocument.Documents.First().Hierarchy = "A/B/C/E";
        }

        void TheSecondDocumentHasASecondNestedDocument()
        {
            secondDocument.Documents.Add(new NestedDocument { Id = "5" });
        }

        void TheSecondDocumentSecondNestedDocumentHasAThreeLevelHierarchyKLM()
        {
            secondDocument.Documents.Last().Hierarchy = "K/L/M";
        }

        void IHaveIndexedTheDocuments()
        {
            client.Index(document, secondDocument);
        }

        void IHaveWaitedForASecond()
        {
            Thread.Sleep(1000);
        }

        SearchResults<Document> result;
        void ISearchForAllDocumentsAndCreateANestedHierarchicalFacet()
        {
            result = client.Search<Document>()
                        .HierarchicalFacetFor(x => x.Documents, x => x.Hierarchy)
                        .GetResult();
        }

        HierarchicalFacet facet;
        void IShouldGetAHierarchicalFacet()
        {
            facet = result.HierarchicalFacetFor(x => x.Documents, x => x.Hierarchy);
            facet.Should().NotBeEmpty();
        }

        void IShouldGetAHierarchicalFacetWithPathAAndCount2()
        {
            facet.Single(x => x.Path.Equals("A")).Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABAndCount2()
        {
            facet.Single(x => x.Path.Equals("A"))
                 .Single(x => x.Path.Equals("A/B")).Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABCAndCount2()
        {
            facet.Single(x => x.Path.Equals("A"))
                 .Single(x => x.Path.Equals("A/B"))
                 .Single(x => x.Path.Equals("A/B/C")).Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABCDAndCount1()
        {
            facet.Single(x => x.Path.Equals("A"))
                 .Single(x => x.Path.Equals("A/B"))
                 .Single(x => x.Path.Equals("A/B/C"))
                 .Single(x => x.Path.Equals("A/B/C/D")).Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathABCEAndCount1()
        {
            facet.Single(x => x.Path.Equals("A"))
                 .Single(x => x.Path.Equals("A/B"))
                 .Single(x => x.Path.Equals("A/B/C"))
                 .Single(x => x.Path.Equals("A/B/C/E")).Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKAndCount1()
        {
            facet.Single(x => x.Path.Equals("K")).Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKLAndCount1()
        {
            facet.Single(x => x.Path.Equals("K"))
                 .Single(x => x.Path.Equals("K/L")).Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKLMAndCount1()
        {
            facet.Single(x => x.Path.Equals("K"))
                 .Single(x => x.Path.Equals("K/L"))
                 .Single(x => x.Path.Equals("K/L/M")).Count.Should().Be(1);
        }

        public class Document
        {
            public Document()
            {
                Documents = new List<NestedDocument>();
            }

            [Id]
            public string Id { get; set; }

            public ICollection<NestedDocument> Documents { get; set; }
        }

        public class NestedDocument
        {
            [Id]
            public string Id { get; set; }

            public Hierarchy Hierarchy { get; set; }
        }
    }
}
