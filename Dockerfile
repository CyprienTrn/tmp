# Étape 1 : Utiliser l'image .NET SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Définir le répertoire de travail dans le conteneur
WORKDIR /app

# Copier les fichiers projet dans le conteneur
COPY *.csproj ./

# Restaurer les dépendances du projet
RUN dotnet restore

# Copier tout le reste du code source dans le conteneur
COPY . ./

# Construire l'application en mode Release
RUN dotnet publish -c Release -o /out

# Étape 2 : Utiliser l'image Runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Définir le répertoire de travail dans l'image finale
WORKDIR /app

# Copier les fichiers publiés depuis l'étape précédente
COPY --from=build-env /out .

# Exposer le port sur lequel l'application écoute
EXPOSE 5053

# Commande pour démarrer l'application
ENTRYPOINT ["dotnet", "link-up.dll"]

# CMD ["dotnet", "watch", "run"]
