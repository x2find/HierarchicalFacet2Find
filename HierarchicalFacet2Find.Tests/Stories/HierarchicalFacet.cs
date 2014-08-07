using EPiServer.Find;
using FluentAssertions;
using StoryQ;
using System.Linq;
using System.Threading;
using Xunit;

namespace HierarchicalFacet2Find.Tests.Stories
{
    public class HierarchicalFacetStory
    {
        [Fact]
        public void HierarchicalFacet()
        {
            new Story("Getting a hierarchical facet")
                .InOrderTo("be able to get a hierarchical facet in the result")
                .AsA("developer")
                .IWant("to be able to use a hierarchical property and get a hierarchical facet")
                .WithScenario("use a hierarchical property and create a hierarchical facet")
                .Given(IHaveAClient)
                    .And(IHaveADocument)
                    .And(TheDocumentHaveAFourLevelHierarchyABCD)
                    .And(IHaveASecondDocument)
                    .And(TheSecondDocumentHaveAFourLevelHierarchyABCE)
                    .And(IHaveAThirdDocument)
                    .And(TheThirdDocumentHaveAThreeLevelHierarchyKLM)
                    .And(IHaveIndexedTheDocuments)
                    .And(IHaveWaitedForASecond)
                .When(ISearchForAllDocumentsAndCreateAHierarchicalFacet)
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
        }

        private Document document;
        void IHaveADocument()
        {
            document = new Document { Id = "1" };
        }

        void TheDocumentHaveAFourLevelHierarchyABCD()
        {
            document.Hierarchy = "A/B/C/D";
        }

        private Document secondDocument;
        void IHaveASecondDocument()
        {
            secondDocument = new Document { Id = "2" };
        }

        void TheSecondDocumentHaveAFourLevelHierarchyABCE()
        {
            secondDocument.Hierarchy = "A/B/C/E";
        }

        private Document thirdDocument;
        void IHaveAThirdDocument()
        {
            thirdDocument = new Document { Id = "3" };
        }

        void TheThirdDocumentHaveAThreeLevelHierarchyKLM()
        {
            thirdDocument.Hierarchy = "K/L/M";
        }

        void IHaveIndexedTheDocuments()
        {
            client.Index(document, secondDocument, thirdDocument);
        }

        void IHaveWaitedForASecond()
        {
            Thread.Sleep(1000);
        }

        SearchResults<Document> result;
        void ISearchForAllDocumentsAndCreateAHierarchicalFacet()
        {
            result = client.Search<Document>()
                        .HierarchicalFacetFor(x => x.Hierarchy)
                        .GetResult();
        }

        HierarchicalFacet facet;
        void IShouldGetAHierarchicalFacet()
        {
            facet = result.HierarchicalFacetFor(x => x.Hierarchy);
            facet.Should().NotBeEmpty();
        }

        void IShouldGetAHierarchicalFacetWithPathAAndCount2()
        {
            facet.Where(x => x.Path.Equals("A")).Single().Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABAndCount2()
        {
            facet.Where(x => x.Path.Equals("A")).Single()
                .Where(x => x.Path.Equals("A/B")).Single().Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABCAndCount2()
        {
            facet.Where(x => x.Path.Equals("A")).Single()
                .Where(x => x.Path.Equals("A/B")).Single()
                .Where(x => x.Path.Equals("A/B/C")).Single().Count.Should().Be(2);
        }

        void IShouldGetAHierarchicalFacetWithPathABCDAndCount1()
        {
            facet.Where(x => x.Path.Equals("A")).Single()
                .Where(x => x.Path.Equals("A/B")).Single()
                .Where(x => x.Path.Equals("A/B/C")).Single()
                .Where(x => x.Path.Equals("A/B/C/D")).Single().Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathABCEAndCount1()
        {
            facet.Where(x => x.Path.Equals("A")).Single()
                .Where(x => x.Path.Equals("A/B")).Single()
                .Where(x => x.Path.Equals("A/B/C")).Single()
                .Where(x => x.Path.Equals("A/B/C/E")).Single().Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKAndCount1()
        {
            facet.Where(x => x.Path.Equals("K")).Single().Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKLAndCount1()
        {
            facet.Where(x => x.Path.Equals("K")).Single()
                .Where(x => x.Path.Equals("K/L")).Single().Count.Should().Be(1);
        }

        void IShouldGetAHierarchicalFacetWithPathKLMAndCount1()
        {
            facet.Where(x => x.Path.Equals("K")).Single()
                .Where(x => x.Path.Equals("K/L")).Single()
                .Where(x => x.Path.Equals("K/L/M")).Single().Count.Should().Be(1);
        }

        public class Document
        {
            [Id]
            public string Id { get; set; }

            public Hierarchy Hierarchy { get; set; }
        }
    }
}
