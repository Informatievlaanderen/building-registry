FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="building-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 6000/tcp
ENV ASPNETCORE_URLS http://*:6000

ENTRYPOINT ["./init.sh"]
