# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Manga.Domain/Manga.Domain.csproj src/Manga.Domain/
COPY src/Manga.Application/Manga.Application.csproj src/Manga.Application/
COPY src/Manga.Infrastructure/Manga.Infrastructure.csproj src/Manga.Infrastructure/
COPY src/Manga.Api/Manga.Api.csproj src/Manga.Api/

RUN dotnet restore src/Manga.Api/Manga.Api.csproj

COPY . .
RUN dotnet publish src/Manga.Api/Manga.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "Manga.Api.dll"]
