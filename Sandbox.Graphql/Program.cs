using Sandbox.Graphql.GraphQL;
using Sandbox.Graphql.GraphQL.OutputTypes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGraphQLServer().AddMutationType<GqlRootMutation>()
    .AddType<GqlMailingInput>()
    .AddType<GqlMailingFormatInput>()
    .AddType<GqlRawMailingFormatInput>()
    .AddType<GqlSpecialMailingFormatInput>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

//app.MapControllers();
app.MapGraphQL();

app.Run();