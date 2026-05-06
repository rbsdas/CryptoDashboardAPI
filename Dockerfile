FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/CryptoDashboardAPI/CryptoDashboardAPI.csproj ./CryptoDashboardAPI/
RUN dotnet restore ./CryptoDashboardAPI/CryptoDashboardAPI.csproj
COPY src/CryptoDashboardAPI/ ./CryptoDashboardAPI/
RUN dotnet publish ./CryptoDashboardAPI/CryptoDashboardAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CryptoDashboardAPI.dll"]
