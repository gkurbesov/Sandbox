using Sandbox.Graphql.GraphQL.OutputTypes;

namespace Sandbox.Graphql.GraphQL;

public class GqlRootMutation
{
    public string Save([GraphQLName("input"), GraphQLNonNullType] GqlMailingInput input)
    {
        var formatType = input.Format?.GetType().Name ?? "value is null";
        return formatType;
    }
}