FROM microsoft/dotnet:runtime

WORKDIR /app
COPY ./out ./
ENTRYPOINT [ "dotnet", "Toxon.Photography.ThumbnailProcessor.dll" ]