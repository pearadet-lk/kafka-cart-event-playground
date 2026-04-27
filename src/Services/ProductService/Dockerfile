FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/ProductService/ProductService.csproj", "src/Services/ProductService/"]
COPY ["src/BuildingBlocks/Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/BuildingBlocks.EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/Outbox/BuildingBlocks.Outbox.csproj", "src/BuildingBlocks/Outbox/"]
COPY ["src/BuildingBlocks/Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/Observability/"]
RUN dotnet restore "src/Services/ProductService/ProductService.csproj"
COPY . .
WORKDIR "/src/src/Services/ProductService"
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]
