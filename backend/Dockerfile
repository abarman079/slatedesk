FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY backend/ ./backend/

WORKDIR /src/backend

RUN dotnet restore \
    src/SlateDesk.Api/SlateDesk.Api.csproj

RUN dotnet publish \
    src/SlateDesk.Api/SlateDesk.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build \
    /app/publish .

ENTRYPOINT ["dotnet", "SlateDesk.Api.dll"]