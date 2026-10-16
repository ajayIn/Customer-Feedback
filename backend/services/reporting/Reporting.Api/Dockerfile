FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/shared/CustomerFeedback.BuildingBlocks/ backend/shared/CustomerFeedback.BuildingBlocks/
COPY backend/services/reporting/Reporting.Api/ backend/services/reporting/Reporting.Api/
RUN dotnet restore backend/services/reporting/Reporting.Api/Reporting.Api.csproj
RUN dotnet publish backend/services/reporting/Reporting.Api/Reporting.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Reporting.Api.dll"]
