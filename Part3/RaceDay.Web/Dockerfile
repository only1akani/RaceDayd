# stage 1: build and publish the website
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy the project file first so the package restore is cached between builds
COPY RaceDay.Web.csproj ./
RUN dotnet restore RaceDay.Web.csproj

# copy the rest of the source and publish a release build
COPY . ./
RUN dotnet publish RaceDay.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# stage 2: a small runtime image that only holds the published website
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# the website listens on port 8080 and finds the API on the host computer by default
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ApiSettings__BaseUrl=http://host.docker.internal:5022/

EXPOSE 8080

COPY --from=build /app/publish ./

# run as the built-in non-root user, not as root
USER app

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]