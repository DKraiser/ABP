FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx                                     .
COPY src/domain/domain.csproj                   domain/
COPY src/application/application.csproj         application/
COPY src/infrastructure/infrastructure.csproj   infrastructure/
COPY src/api/api.csproj                         api/
RUN dotnet restore /src/api/api.csproj

COPY src .
RUN dotnet publish api/api.csproj \ 
                            --no-restore \
                            --output /app/publish \
                            --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER app

COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS 8080
EXPOSE 8080
ENTRYPOINT dotnet api.dll