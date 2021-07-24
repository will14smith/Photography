#!/bin/bash

#install zip on debian OS, since microsoft/dotnet container doesn't have zip by default
if [ -f /etc/debian_version ]
then
  apt -qq update
  apt -qq -y install zip
fi

cd src/Toxon.Photography

dotnet restore
dotnet lambda package --configuration release --framework netcoreapp3.1 --output-package ../../deploy/deploy-package.zip

cd ../Toxon.Photography.ThumbnailProcessor

dotnet restore
dotnet publish -c Release -o out
docker build -t toxon/photography-thumbnailprocessor .

aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin 682179218046.dkr.ecr.eu-west-2.amazonaws.com

docker tag toxon/photography-thumbnailprocessor:latest 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-dev-thumbnailprocessor:latest
docker push 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-dev-thumbnailprocessor:latest
docker tag toxon/photography-thumbnailprocessor:latest 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-prod-thumbnailprocessor:latest
docker push 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-prod-thumbnailprocessor:latest