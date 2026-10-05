FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BluecoreApi.csproj ./
RUN dotnet restore BluecoreApi.csproj
COPY . ./
RUN dotnet publish BluecoreApi.csproj --configuration Release --no-restore --output /app/publish

FROM build AS migrations
RUN dotnet tool install dotnet-ef --tool-path /tools --version 10.0.12
ENTRYPOINT ["/tools/dotnet-ef"]
CMD ["database", "update", "--project", "BluecoreApi.csproj", "--configuration", "Release", "--no-build"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "BluecoreApi.dll"]
