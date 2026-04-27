FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/CartService/CartService.csproj", "src/Services/CartService/"]
RUN dotnet restore "src/Services/CartService/CartService.csproj"
COPY . .
WORKDIR "/src/src/Services/CartService"
RUN dotnet publish "CartService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CartService.dll"]
