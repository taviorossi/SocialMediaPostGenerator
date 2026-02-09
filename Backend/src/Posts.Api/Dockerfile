# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Posts.sln", "./"]
COPY ["src/Posts.Domain/Posts.Domain.csproj", "src/Posts.Domain/"]
COPY ["src/Posts.Application/Posts.Application.csproj", "src/Posts.Application/"]
COPY ["src/Posts.Infrastructure/Posts.Infrastructure.csproj", "src/Posts.Infrastructure/"]
COPY ["src/Posts.Api/Posts.Api.csproj", "src/Posts.Api/"]
RUN dotnet restore "src/Posts.Api/Posts.Api.csproj"
COPY src/ src/
RUN dotnet publish "src/Posts.Api/Posts.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage (Google Cloud Run: PORT env is set automatically)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Posts.Api.dll"]
