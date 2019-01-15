# Photography

A serverless personal photography website.

Playing around with the [serverless](https://github.com/serverless/serverless) framework for hosting my photography website at very minimal cost.

## Installation

```
npm install -g serverless
yarn
cd src
dotnet restore
```

Install terraform

## Deployment

```
./build.sh
tf workspace select dev
tf apply
sls deploy
```

```
./build.sh
tf workspace select prod
tf apply
sls deploy --stage prod
```

Manual steps:

(required for first deploy)

- Create access key for IAM User `SiteGeneratorSigningUser`
- Create SSM Parameter:
  - Name: `photography-${stage}-SiteGeneratorSigningUser-AccessKey`
  - Type: SecureString
  - Value: AccessKey for IAM User `SiteGeneratorSigningUser`
- Create SSM Parameter:
  - Name: `photography-${stage}-SiteGeneratorSigningUser-SecretKey`
  - Type: SecureString
  - Value: SecretKey for IAM User `SiteGeneratorSigningUser`

## Running admin client

Local:

Ensure `src/Toxon.Photography.Admin/.env` is present and up to date.

```
cd src/Toxon.Photography.Admin
yarn start
```

Production: Just push to master, it is deployed automatically by Netlify

## Architecture

![Architecture diagram](docs/arch.png)

The original (and probably most up to date) diagram is [here](https://docs.google.com/drawings/d/1bWO_n-EJH5N4NxZV0H3L5mgzImvndEI_i47nRyrZGTA/edit?usp=sharing)
