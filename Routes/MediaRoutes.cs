// using link_up.Models;
// using link_up.Services;

// namespace link_up.Routes
// {
//     public static class MediasRoutes
//     {
//         public static void MapMediaRoutes(this IEndpointRouteBuilder app)
//         {
//             // Route GET pour récupérer tous les médias
//             app.MapGet("/", async (MediaCosmosService mediaCosmosService) =>
//             {
//                 var medias = await mediaCosmosService.GetAllMediasAsync();
//                 return medias;
//             })
//             .WithName("GetAllMedias")
//             .WithOpenApi();


//             // Route GET pour récupérer un médias par son ID
//             app.MapGet("/{id}", async (string id, MediaCosmosService mediaCosmosService) =>
//             {
//                 try
//                 {
//                     var content = await mediaCosmosService.GetMediaByIdAsync(id);

//                     if (content == null)
//                     {
//                         return Results.NotFound(new { message = $"Media with ID {id} not found." });
//                     }

//                     return Results.Ok(content);
//                 }
//                 catch (Exception ex)
//                 {
//                     return Results.Problem(ex.Message);
//                 }
//             })
//             .WithName("GetMedia")
//             .WithOpenApi();

//             // Route DELETE pour supprimer un Contenu
//             app.MapDelete("/{id}", async (string id, MediaCosmosService mediaCosmosService) =>
//             {
//                 try
//                 {
//                     await mediaCosmosService.DeleteMediaAsync(id);
//                     return Results.NoContent();
//                 }
//                 catch (Exception ex)
//                 {
//                     return Results.Problem(ex.Message);
//                 }
//             })
//             .WithName("DeleteMedia")
//             .WithOpenApi();
//         }
//     }
// }

using link_up.Models;
using link_up.Services;

namespace link_up.Routes
{
    public static class MediasRoutes
    {
        public static void MapMediaRoutes(this IEndpointRouteBuilder app)
        {
            // Route GET pour récupérer tous les médias
            app.MapGet("/", async (MediaCosmosService mediaCosmosService) =>
            {
                var medias = await mediaCosmosService.GetAllMediasAsync();
                return medias;
            })
            .WithName("GetAllMedias")
            .WithOpenApi();

            // Route GET pour récupérer un média par son ID
            app.MapGet("/{id}", async (string id, MediaCosmosService mediaCosmosService) =>
            {
                try
                {
                    var content = await mediaCosmosService.GetMediaByIdAsync(id);

                    if (content == null)
                    {
                        return Results.NotFound(new { message = $"Media with ID {id} not found." });
                    }

                    return Results.Ok(content);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetMedia")
            .WithOpenApi();

            // Route DELETE pour supprimer un média
            app.MapDelete("/{id}", async (string id, MediaCosmosService mediaCosmosService) =>
            {
                try
                {
                    await mediaCosmosService.DeleteMediaAsync(id);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DeleteMedia")
            .WithOpenApi();
        }
    }
}
