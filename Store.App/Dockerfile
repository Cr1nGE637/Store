FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Store.App/Store.App.csproj", "Store.App/"]
COPY ["Store.SharedKernel/Store.SharedKernel.csproj", "Store.SharedKernel/"]
COPY ["Store.Identity/Store.Identity.csproj", "Store.Identity/"]
COPY ["Store.Catalog/Store.Catalog.csproj", "Store.Catalog/"]
COPY ["Store.Catalog.Contracts/Store.Catalog.Contracts.csproj", "Store.Catalog.Contracts/"]
COPY ["Store.Carts/Store.Carts.csproj", "Store.Carts/"]
COPY ["Store.Carts.Contracts/Store.Carts.Contracts.csproj", "Store.Carts.Contracts/"]
COPY ["Store.Ordering/Store.Ordering.csproj", "Store.Ordering/"]
RUN dotnet restore "Store.App/Store.App.csproj"
COPY . .
WORKDIR "/src/Store.App"
RUN dotnet build "./Store.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Store.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.App.dll"]
