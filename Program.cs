using link_up.Services;
using link_up.Routes;

public class Program
{
    public static void Main(string[] args)
    {


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<UserCosmosService>();
builder.Services.AddSingleton<MediaCosmosService>();
builder.Services.AddSingleton<ContentCosmosService>();
builder.Services.AddSingleton<BlobService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    // Configure Swagger pour associer des tags aux groupes
    c.TagActionsBy(api => new[] { api.GroupName });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);  // Pour ne pas afficher les modèles
    });
}

app.UseHttpsRedirection();

app.MapGroup("/utilisateurs")
   .WithTags("Utilisateurs")
   .MapUserRoutes();

app.MapGroup("/contenus")
   .WithTags("Contenus")
   .MapContentRoutes();

app.MapGroup("/medias")
   .WithTags("Médias")
   .MapMediaRoutes();

app.Run();

  }
}

