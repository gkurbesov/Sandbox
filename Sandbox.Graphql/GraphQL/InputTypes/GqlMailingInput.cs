namespace Sandbox.Graphql.GraphQL.OutputTypes;

public class GqlMailingInput
{
    public string? InternalId { get; set; }


    [GraphQLName("format")] public GqlMailingFormatInput? Format { get; set; }
}