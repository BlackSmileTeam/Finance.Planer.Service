FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FinancialPlanner.sln", "."]
COPY ["FinancialPlanner.Api/FinancialPlanner.Api.csproj", "FinancialPlanner.Api/"]
COPY ["FinancialPlanner.Application/FinancialPlanner.Application.csproj", "FinancialPlanner.Application/"]
COPY ["FinancialPlanner.Infrastructure/FinancialPlanner.Infrastructure.csproj", "FinancialPlanner.Infrastructure/"]
COPY ["FinancialPlanner.Domain/FinancialPlanner.Domain.csproj", "FinancialPlanner.Domain/"]

RUN dotnet restore "FinancialPlanner.Api/FinancialPlanner.Api.csproj"

COPY . .
RUN dotnet publish "FinancialPlanner.Api/FinancialPlanner.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "FinancialPlanner.Api.dll"]
