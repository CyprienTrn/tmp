using Microsoft.Azure.Cosmos;
using System.Net;
using link_up.Models;
using link_up.Services;

namespace link_up.Services
{
    public class ContentCosmosService
    {
        public CosmosClient _cosmosClient;
        private readonly UserCosmosService _userService;
        public MediaCosmosService _mediaService;
        private readonly Database _database;
        public Container _container;
        private string _contentPartitionKey;


        public ContentCosmosService(IConfiguration configuration, UserCosmosService userService, MediaCosmosService mediaService)
        {
            // on charge la configuration depuis le appsettings.json
            // var cosmosSettings = configuration.GetSection("CosmosDb");
            _userService = userService;
            _mediaService = mediaService;
            var cosmosSettings = configuration.GetSection("CosmosDbCyp");
            string endpointUri = cosmosSettings["EndpointUri"];
            string primaryKey = cosmosSettings["PrimaryKey"];
            string databaseId = cosmosSettings["DatabaseId"];
            string containerId = cosmosSettings["ContainerContentId"];
            _contentPartitionKey = cosmosSettings["ContentPartitionKey"];

            _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions { ApplicationName = "LinkUpApp" });
            _database = _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).Result;
            ContainerProperties containerProperties = new ContainerProperties()
            {
                Id = containerId,
                PartitionKeyPath = "/content_id",
            };
            _container = _database.CreateContainerIfNotExistsAsync(containerProperties).Result;
        }

        public async Task<Content> CreateContentAsync(Content content)
        {
            try
            {
                // Génération d'un ID pour le contenu
                content.id = Guid.NewGuid().ToString();

                // Configuration de la clé de partition
                content.content_id = this._contentPartitionKey;

                // Renseignement des dates
                content.CreatedAt = DateTime.UtcNow;
                content.UpdatedAt = DateTime.UtcNow;

                // Vérification que l'utilisateur existe par son ID
                var userExists = await _userService.CheckUserExistsAsync(content.UserId);
                if (!userExists)
                {
                    throw new Exception($"L'utilisateur avec l'ID '{content.UserId}' n'existe pas.");
                }

                // Vérification que le titre n'est pas vide
                if (string.IsNullOrWhiteSpace(content.Title))
                {
                    throw new Exception("Le champ 'Title' ne doit pas être vide, il est requis.");
                }

                // Vérification des médias
                if (content.medias != null && content.medias.Count > 0)
                {
                    foreach (var media in content.medias)
                    {
                        await this._mediaService.CreateMediaAsync(media, content.id);
                    }
                }

                // Création du contenu après la gestion des médias
                var response = await _container.CreateItemAsync(content, new PartitionKey(content.content_id));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new Exception($"Le contenu avec l'ID '{content.id}' existe déjà.", ex);
            }
        }


        public async Task<IEnumerable<Content>> GetAllContentsAsync()
        {
            var query = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(query);
            var queryResultSetIterator = _container.GetItemQueryIterator<Content>(queryDefinition);

            var contents = new List<Content>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                contents.AddRange(currentResultSet);
            }
            return contents;
        }

        public async Task<Content?> GetContentByIdAsync(string contentId)
        {
            try
            {
                // Lit l'élément de la base Cosmos DB
                var response = await _container.ReadItemAsync<Content>(contentId, new PartitionKey(this._contentPartitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Retourne null si le contenu n'est pas trouvé
                return null;
            }
            catch (CosmosException ex)
            {
                // Log et relance toute autre exception Cosmos
                Console.WriteLine($"An error occurred while retrieving the content: {ex.Message}");
                throw;
            }
        }

        public async Task<Content> UpdateContentAsync(string contentId, Content updatedContent)
        {
            try
            {
                // Appelle la méthode pour récupérer l'utilisateur existant
                var content = await GetContentByIdAsync(contentId);
                if (content == null) throw new Exception($"Content with ID {contentId} not found.");

                // Met à jour les propriétés du contenu
                content.Title = updatedContent.Title ?? content.Title;
                content.Description = updatedContent.Description ?? content.Description;

                // on modifie automatiquement l'heure et la date de modification
                content.UpdatedAt = DateTime.UtcNow;

                // Remplace l'élément dans la base de données
                var response = await _container.ReplaceItemAsync(content, content.id.ToString(), new PartitionKey(this._contentPartitionKey));
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                // Log et relance toute exception Cosmos
                Console.WriteLine($"An error occurred while updating the content: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteContentAsync(string contentId)
        {
            try
            {
                // Récupérer le contenu à supprimer pour identifier les médias associés
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @contentId")
                    .WithParameter("@contentId", contentId);

                var iterator = _container.GetItemQueryIterator<Content>(query);
                Content contentToDelete = null;

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    contentToDelete = response.FirstOrDefault();
                }

                if (contentToDelete == null)
                {
                    throw new Exception($"Le contenu avec l'ID '{contentId}' n'existe pas.");
                }

                // Supprimer les médias associés
                if (contentToDelete.medias != null && contentToDelete.medias.Count > 0)
                {
                    foreach (var media in contentToDelete.medias)
                    {
                        await _mediaService.DeleteMediaAsync(media.id);
                    }
                }

                // Supprimer le contenu principal
                await _container.DeleteItemAsync<Content>(contentId, new PartitionKey(this._contentPartitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception($"Le contenu avec l'ID '{contentId}' n'existe pas.", ex);
            }
        }


    }
}
