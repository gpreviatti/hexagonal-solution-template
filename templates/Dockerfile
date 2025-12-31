FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . .

WORKDIR /app/src/WebApp
RUN dotnet restore \
    && dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

RUN apt-get update \
    && apt-get install -y curl

EXPOSE 5000

ENTRYPOINT ["dotnet", "WebApp.dll", "--urls", "http://+:5000"]
