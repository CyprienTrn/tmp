using Microsoft.Azure.Cosmos;
using System.Net;
using UserApp = link_up.Models.User;
using link_up.DTO;

namespace link_up.Services
{
    public class UserCosmosService
    {
        public CosmosClient _cosmosClient;
        private readonly Database _database;
        public Container _container;
        private string _userPartitionKey;


        public UserCosmosService(IConfiguration configuration)
        {
            // on charge la configuration depuis le appsettings.json
            // var cosmosSettings = configuration.GetSection("CosmosDb");
            var cosmosSettings = configuration.GetSection("CosmosDbCyp");
            string endpointUri = cosmosSettings["EndpointUri"];
            string primaryKey = cosmosSettings["PrimaryKey"];
            string databaseId = cosmosSettings["DatabaseId"];
            string containerId = cosmosSettings["ContainerUserId"];
            _userPartitionKey = cosmosSettings["UserPartitionKey"];

            _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions { ApplicationName = "LinkUpApp" });
            _database = _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).Result;
            ContainerProperties containerProperties = new ContainerProperties()
            {
                Id = containerId,
                PartitionKeyPath = _userPartitionKey,
            };
            _container = _database.CreateContainerIfNotExistsAsync(containerProperties).Result;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUtilisateursAsync()
        {
            var query = "SELECT c.id, c.Email, c.Name, c.IsPrivate, c.CreatedAt FROM c";
            var queryDefinition = new QueryDefinition(query);
            var queryResultSetIterator = _container.GetItemQueryIterator<UserDTO>(queryDefinition);

            var users = new List<UserDTO>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                users.AddRange(currentResultSet);
            }
            return users;
        }

        public async Task<bool> IsEmailValidAndUniqueAsync(string email)
        {
            // 1. Vérifier le format de l'email
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                throw new ArgumentException("L'email fourni n'est pas valide.");
            }

            var query = "SELECT COUNT(1) FROM c WHERE c.Email = @Email";
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@Email", email);

            var queryIterator = _container.GetItemQueryIterator<int>(queryDefinition);

            while (queryIterator.HasMoreResults)
            {
                var resultSet = await queryIterator.ReadNextAsync();
                if (resultSet.FirstOrDefault() > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    return false;

                // Validation avec expression régulière pour s'assurer d'un domaine valide
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
                return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }


        public async Task<UserApp> CreateUserAsync(UserApp user)
        {
            try
            {
                // on génère un id unique à chaque utilisateur
                user.id = Guid.NewGuid().ToString();

                // on génère la clef de partition
                user.user_id = this._userPartitionKey;

                if (!this.IsValidEmail(user.Email))
                {
                    throw new InvalidOperationException("L'email saisi n'est pas valide");
                }

                user.CreatedAt = DateTime.UtcNow;
                var response = await _container.CreateItemAsync(user, new PartitionKey(user.user_id));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new Exception($"User with ID {user.id} already exists.", ex);
            }
        }

        public async Task<UserApp?> GetUserByIdAsync(string userId)
        {
            try
            {
                // Lit l'élément de la base Cosmos DB
                var response = await _container.ReadItemAsync<UserApp>(userId, new PartitionKey(this._userPartitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Retourne null si l'utilisateur n'est pas trouvé
                return null;
            }
            catch (CosmosException ex)
            {
                // Log et relance toute autre exception Cosmos
                Console.WriteLine($"An error occurred while retrieving the user: {ex.Message}");
                throw;
            }
        }


        public async Task<UserApp> UpdateUserAsync(string userId, UserApp updatedUser)
        {
            try
            {
                // Appelle la méthode pour récupérer l'utilisateur existant
                var user = await GetUserByIdAsync(userId);
                if (user == null) throw new Exception($"User with ID {userId} not found.");

                // Met à jour les propriétés de l'utilisateur
                user.Email = updatedUser.Email ?? user.Email;
                user.Name = updatedUser.Name ?? user.Name;
                user.IsPrivate = updatedUser.IsPrivate;

                // Remplace l'élément dans la base de données
                var response = await _container.ReplaceItemAsync(user, user.id.ToString(), new PartitionKey(this._userPartitionKey));
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                // Log et relance toute exception Cosmos
                Console.WriteLine($"An error occurred while updating the user: {ex.Message}");
                throw;
            }
        }


        public async Task DeleteUserAsync(string userId)
        {
            try
            {
                await _container.DeleteItemAsync<UserApp>(userId.ToString(), new PartitionKey(this._userPartitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception($"User with ID {userId} not found.", ex);
            }
        }

        // Méthode pour vérifier l'existence de l'utilisateur

        public async Task<bool> CheckUserExistsAsync(string userId)
        {
            try
            {
                // Exemple de recherche de l'utilisateur dans la base Cosmos DB
                var response = await _container.ReadItemAsync<UserApp>(userId, new PartitionKey("/user_id"));
                return response.Resource != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
