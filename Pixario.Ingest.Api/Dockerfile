FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish Pixario.Ingest.Api/Pixario.Ingest.Api.csproj \
    -c Release \
    -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /out .

ENTRYPOINT ["dotnet", "Pixario.Ingest.Api.dll"]
