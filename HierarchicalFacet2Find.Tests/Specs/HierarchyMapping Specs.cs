﻿using EPiServer.Find.ClientConventions;
using EPiServer.Find.Json;
using Machine.Specifications;
using Newtonsoft.Json;

namespace HierarchicalFacet2Find.Tests.Specs
{
    public class when_serializing_a_Hierarchy
    {
        static JsonSerializer serializer;
        static string serializedString;
        static Hierarchy originalProperty;
        static Hierarchy deserializedProperty;

        Establish context = () =>
        {
            serializer = Serializer.CreateDefault();
            var conventions = new DefaultConventions();
            serializer.ContractResolver = conventions.ContractResolver;

            originalProperty = "a/b/c";
        };

        Because of = () =>
        {
            var serializedString = serializer.Serialize(originalProperty);
            deserializedProperty = serializer.Deserialize<Hierarchy>(serializedString);
        };

        It should_equal_the_original_object = () =>
        {
            ((string) deserializedProperty).ShouldEqual((string) originalProperty);
        };
    }
}
