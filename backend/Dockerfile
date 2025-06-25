# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["241RunnersAwareness.BackendAPI.csproj", "./"]
RUN dotnet restore "241RunnersAwareness.BackendAPI.csproj"

# Copy the rest of the source code
COPY . .
RUN dotnet build "241RunnersAwareness.BackendAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "241RunnersAwareness.BackendAPI.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "241RunnersAwareness.BackendAPI.dll"] 