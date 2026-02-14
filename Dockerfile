# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render expone PORT como variable de entorno
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# opcional (no obligatorio en Render, pero útil)
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiNet.dll"]