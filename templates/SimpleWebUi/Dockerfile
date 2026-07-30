FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . .

RUN dotnet restore \
    && dotnet publish src/WebUI/WebUi.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5015

ENTRYPOINT ["dotnet", "WebUi.dll", "--urls", "http://+:5015"]
