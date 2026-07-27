# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet publish 241RunnersAPI.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DATABASE_PROVIDER=Postgres
ENV STORAGE_PROVIDER=Supabase

EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "241RunnersAPI.dll"]
