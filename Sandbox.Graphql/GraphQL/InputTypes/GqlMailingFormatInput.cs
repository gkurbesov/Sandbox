namespace Sandbox.Graphql.GraphQL.OutputTypes;

[UnionType("MailingFormatInput")]
[GraphQLName("MailingFormatInput")]
public abstract class GqlMailingFormatInput
{
    public string? InternalId { get; set; }
    public string? Name { get; set; }
    public abstract string? Vendor { get; }
}

[GraphQLName("RawMailingFormatInput")]
public sealed class GqlRawMailingFormatInput : GqlMailingFormatInput
{
    public override string? Vendor => null;
}

[GraphQLName("SpecialMailingFormatInput")]
public sealed class GqlSpecialMailingFormatInput : GqlMailingFormatInput
{
    public override string? Vendor => "CustomVendor";
    public string? Description { get; set; }
}