cd src/Toxon.Photography

dotnet restore
dotnet lambda package --configuration release --framework netcoreapp2.1 --output-package ../../deploy/deploy-package.zip
