using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace link_up.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _container;

        public BlobService()
        {
            const string blobServiceEndpoint = "https://mediaslinkupsupinfo.blob.core.windows.net/";
            const string storageAccountName = "mediaslinkupsupinfo";
            const string storageAccountKey = "M4exfSezsXy7cp460zmTtG9MtWp3+IqQZlJzDfjtlMr5Y1A3stcTlkD3jkD+bDNwO522QmRdM+VW+AStCHyWzQ==";
            const string containerName = "medias";
            // Initialisation du client BlobServiceClient
            StorageSharedKeyCredential accountCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            _blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), accountCredentials);

            _container = _blobServiceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        public async Task<string> GetBlobUriAsync(string blobName)
        {
            BlobClient blob = _container.GetBlobClient(blobName);
            bool exists = await blob.ExistsAsync();
            if (!exists)
            {
                Console.WriteLine($"Blob {blobName} not found!");
                return null; // Ou lancez une exception si nécessaire
            }
            else
            {
                Console.WriteLine($"Blob Found, URI:\t{blob.Uri}");
            }
            return blob.Uri.ToString();
        }


        public async Task<string> UploadFileAsync(string localFilePath)
        {
            // on vérifie l'existence du fichier en local
            if (!File.Exists(localFilePath))
            {
                Console.WriteLine($"Le fichier local '{localFilePath}' n'existe pas.");
                return null;
            }

            // on récupère le nom du fichier
            string fileName = Path.GetFileName(localFilePath);

            // on crée le fichier dans le blob azure
            BlobClient blobClient = _container.GetBlobClient(fileName);

            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);

            // Attendre et retourner l'URI du blob
            return await this.GetBlobUriAsync(fileName);
        }

        public async Task DeleteFileAsync(string blobName)
        {
            // Obtenir le client du blob
            BlobClient blobClient = _container.GetBlobClient(blobName);

            // Vérifier si le blob existe
            bool exists = await blobClient.ExistsAsync();
            if (!exists)
            {
                Console.WriteLine($"Blob '{blobName}' not found. Suppression impossible.");
            }

            // Supprimer le blob
            await blobClient.DeleteAsync();
            Console.WriteLine($"Blob '{blobName}' supprimé avec succès.");
        }


    }
}
