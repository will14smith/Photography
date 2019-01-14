cd src/Toxon.Photography

dotnet restore
dotnet lambda package -c Release --framework netcoreapp2.1 --output-package ../../deploy/deploy-package.zip

cd ../Toxon.Photography.ThumbnailProcessor

dotnet restore
dotnet publish -c Release -o out
docker build -t toxon/photography-thumbnailprocessor .

docker tag toxon/photography-thumbnailprocessor:latest 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-dev-thumbnailprocessor:latest
docker push 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-dev-thumbnailprocessor:latest
docker tag toxon/photography-thumbnailprocessor:latest 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-prod-thumbnailprocessor:latest
docker push 682179218046.dkr.ecr.eu-west-2.amazonaws.com/toxon/photography-prod-thumbnailprocessor:latest