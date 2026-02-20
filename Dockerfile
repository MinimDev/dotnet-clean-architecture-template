# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CleanArchitecture.slnx", "./"]
COPY ["src/Core/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj", "src/Core/CleanArchitecture.Domain/"]
COPY ["src/Core/CleanArchitecture.Application/CleanArchitecture.Application.csproj", "src/Core/CleanArchitecture.Application/"]
COPY ["src/Infrastructure/CleanArchitecture.Infrastructure.Persistence/CleanArchitecture.Infrastructure.Persistence.csproj", "src/Infrastructure/CleanArchitecture.Infrastructure.Persistence/"]
COPY ["src/Infrastructure/CleanArchitecture.Infrastructure.Identity/CleanArchitecture.Infrastructure.Identity.csproj", "src/Infrastructure/CleanArchitecture.Infrastructure.Identity/"]
COPY ["src/Infrastructure/CleanArchitecture.Infrastructure.Shared/CleanArchitecture.Infrastructure.Shared.csproj", "src/Infrastructure/CleanArchitecture.Infrastructure.Shared/"]
COPY ["src/Presentation/CleanArchitecture.WebAPI/CleanArchitecture.WebAPI.csproj", "src/Presentation/CleanArchitecture.WebAPI/"]

# Restore dependencies
RUN dotnet restore "src/Presentation/CleanArchitecture.WebAPI/CleanArchitecture.WebAPI.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Presentation/CleanArchitecture.WebAPI"
RUN dotnet build "CleanArchitecture.WebAPI.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "CleanArchitecture.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.WebAPI.dll"]
