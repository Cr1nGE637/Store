FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Store.API/Store.API.csproj", "Store.API/"]
COPY ["Store.Infrastructure/Store.Infrastructure.csproj", "Store.Infrastructure/"]
COPY ["Store.Domain/Store.Domain.csproj", "Store.Domain/"]
COPY ["Store.Application/Store.Application.csproj", "Store.Application/"]
RUN dotnet restore "Store.API/Store.API.csproj"
COPY . .
WORKDIR "/src/Store.API"
RUN dotnet build "./Store.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Store.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.API.dll"]
