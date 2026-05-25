FROM mcr.microsoft.com/dotnet/sdk:10.0.300 AS build-env
WORKDIR /app

COPY . .

WORKDIR /app/src/WebUi
RUN dotnet restore \
    && dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0.8
WORKDIR /app
COPY --from=build-env /app/out .

RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5015

ENTRYPOINT ["dotnet", "WebUi.dll", "--urls", "http://+:5015"]
