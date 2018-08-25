# Photography

A serverless personal photography website.

Playing around with the [serverless](https://github.com/serverless/serverless) framework for hosting my photography website at very minimal cost.

## Installation

```
npm install -g serverless
cd src
dotnet restore
```

## Deployment

```
./build.sh
sls deploy
```

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
