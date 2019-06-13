FROM microsoft/dotnet:2.2.0-runtime-deps

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

EXPOSE 7006/tcp
ENV ASPNETCORE_URLS http://*:7006

ENTRYPOINT ["./BuildingRegistry.Projector"]
