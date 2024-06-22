# Photography

A serverless personal photography website.

Playing around with the [AWS SAM](https://aws.amazon.com/serverless/sam/) tool for hosting my photography website at very minimal cost.

## Deployment

```
sam build
sam validate
sam deploy 
```

once confirmed in dev, promote to production using:

```
sam deploy --config-env prod 
```

## Architecture

TODO update for new architecture

![Architecture diagram](docs/arch.png)

The original (and probably most up to date) diagram is [here](https://docs.google.com/drawings/d/1bWO_n-EJH5N4NxZV0H3L5mgzImvndEI_i47nRyrZGTA/edit?usp=sharing)
