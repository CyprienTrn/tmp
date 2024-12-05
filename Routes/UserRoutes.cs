using link_up.Services;
using LinkUpUser = link_up.Models.User;

namespace link_up.Routes
{
    public static class UserRoutes
    {
        public static void MapUserRoutes(this IEndpointRouteBuilder app)
        {
            // Route POST pour créer un utilisateur
            app.MapPost("/", async (LinkUpUser user, UserCosmosService userCosmosService) =>
            {
                var createdUser = await userCosmosService.CreateUserAsync(user);
                return Results.Created($"/{createdUser.id}", createdUser);
            })
            .WithName("CreateUser")
            .WithOpenApi();

            // Route GET pour récupérer tous les utilisateurs
            app.MapGet("/", async (UserCosmosService userCosmosService) =>
            {
                var utilisateurs = await userCosmosService.GetAllUtilisateursAsync();
                return utilisateurs;
            })
            .WithName("GetAllUsers")
            .WithOpenApi();

            // Route GET pour récupérer un utilisateur par ID
            app.MapGet("/{id}", async (string id, UserCosmosService userCosmosService) =>
            {
                try
                {
                    var user = await userCosmosService.GetUserByIdAsync(id);

                    if (user == null)
                    {
                        return Results.NotFound(new { message = $"User with ID {id} not found." });
                    }

                    return Results.Ok(user);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetUser")
            .WithOpenApi();

            // Route DELETE pour supprimer un utilisateur
            app.MapDelete("/{id}", async (string id, UserCosmosService userCosmosService) =>
            {
                try
                {
                    await userCosmosService.DeleteUserAsync(id);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DeleteUser")
            .WithOpenApi();

            // Route PUT pour mettre à jour un utilisateur
            app.MapPut("/{id}", async (string id, LinkUpUser updatedUser, UserCosmosService userCosmosService) =>
            {
                try
                {
                    var updatedUserData = await userCosmosService.UpdateUserAsync(id, updatedUser);
                    return Results.Ok(updatedUserData);
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
            .WithName("UpdateUser")
            .WithOpenApi();
        }
    }
}
