using link_up.Models;
using link_up.Services;

namespace link_up.Routes
{
    public static class ContentRoutes
    {
        public static void MapContentRoutes(this IEndpointRouteBuilder app)
        {
            // Route POST pour créer un contenu
            app.MapPost("/", async (Content content, ContentCosmosService contentCosmosService) =>
            {
                var createdContent = await contentCosmosService.CreateContentAsync(content);
                return Results.Created($"/{createdContent.id}", createdContent);
            })
            .WithName("CreateContent")
            .WithOpenApi();

            // Route GET pour récupérer tous les contenus
            app.MapGet("/", async (ContentCosmosService contentCosmosService) =>
            {
                var contents = await contentCosmosService.GetAllContentsAsync();
                return contents;
            })
            .WithName("GetAllContents")
            .WithOpenApi();

            // Route GET pour récupérer un contenu par son ID
            app.MapGet("/{id}", async (string id, ContentCosmosService contentCosmosService) =>
            {
                try
                {
                    var content = await contentCosmosService.GetContentByIdAsync(id);

                    if (content == null)
                    {
                        return Results.NotFound(new { message = $"Content with ID {id} not found." });
                    }

                    return Results.Ok(content);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetContent")
            .WithOpenApi();

            // Route DELETE pour supprimer un Contenu
            app.MapDelete("/{id}", async (string id, ContentCosmosService contentCosmosService) =>
            {
                try
                {
                    await contentCosmosService.DeleteContentAsync(id);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DeleteContent")
            .WithOpenApi();

            // Route PUT pour mettre à jour un contenu
            app.MapPut("/{id}", async (string id, Content updatedContent, ContentCosmosService contentCosmosService) =>
            {
                try
                {
                    var updatedContentData = await contentCosmosService.UpdateContentAsync(id, updatedContent);
                    return Results.Ok(updatedContentData);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("not found"))
                    {
                        return Results.NotFound(new { message = ex.Message });
                    }
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("UpdateContent")
            .WithOpenApi();
        }
    }
}