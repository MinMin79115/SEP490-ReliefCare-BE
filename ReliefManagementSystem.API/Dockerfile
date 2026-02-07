FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReliefManagementSystem.API/ReliefManagementSystem.API.csproj", "ReliefManagementSystem.API/"]
COPY ["ReliefManagementSystem.Application/ReliefManagementSystem.Application.csproj", "ReliefManagementSystem.Application/"]
COPY ["ReliefManagementSystem.Domain/ReliefManagementSystem.Domain.csproj", "ReliefManagementSystem.Domain/"]
COPY ["ReliefManagementSystem.Infrastructure/ReliefManagementSystem.Infrastructure.csproj", "ReliefManagementSystem.Infrastructure/"]
RUN dotnet restore "ReliefManagementSystem.API/ReliefManagementSystem.API.csproj"
COPY . .
WORKDIR "/src/ReliefManagementSystem.API"
RUN dotnet build "./ReliefManagementSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ReliefManagementSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReliefManagementSystem.API.dll"]
