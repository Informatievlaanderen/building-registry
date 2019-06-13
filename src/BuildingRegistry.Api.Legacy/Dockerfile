FROM microsoft/dotnet:2.2.0-runtime-deps

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

EXPOSE 6002/tcp
ENV ASPNETCORE_URLS http://*:6002

ENTRYPOINT ["./BuildingRegistry.Api.Legacy"]
