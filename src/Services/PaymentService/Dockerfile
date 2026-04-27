FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/PaymentService/PaymentService.csproj", "src/Services/PaymentService/"]
RUN dotnet restore "src/Services/PaymentService/PaymentService.csproj"
COPY . .
WORKDIR "/src/src/Services/PaymentService"
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
