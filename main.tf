terraform {
  backend "s3" {
    bucket = "toxon-terraform"
    key    = "photography"
    region = "eu-west-2"
  }
}

provider "aws" {
  region = "eu-west-2"
}

# variables

locals {
  env = {
    defaults = {}

    dev = {
      static-site-acm-certificate-arn = "arn:aws:acm:us-east-1:682179218046:certificate/0a059434-104e-49d0-a7b8-5ca2fcb951ab"
      static-site-aliases             = "photography-dev.toxon.co.uk"
    }

    prod = {
      static-site-acm-certificate-arn = "arn:aws:acm:us-east-1:682179218046:certificate/6cc4623e-c061-41b8-85b3-ed0c4a67b39d"
      static-site-aliases             = "toxon.co.uk;www.toxon.co.uk"
    }
  }

  workspace = merge(local.env["defaults"], local.env[terraform.workspace])
}

# application
module "app" {
  source = "./terraform"

  stage                           = terraform.workspace
  static-site-acm-certificate-arn = local.workspace["static-site-acm-certificate-arn"]
  static-site-aliases             = split(";", local.workspace["static-site-aliases"])
}

locals {
  parameters = {
    "image-bucket-arn"                      = module.app.image-bucket-arn
    "site-bucket-arn"                       = module.app.site-bucket-arn
    "photograph-table-arn"                  = module.app.photograph-table-arn
    "image-processor-topic-arn"             = module.app.image-processor-topic-arn
    "image-processor-topic-name"            = module.app.image-processor-topic-name
    "state-site-domain-name"                = module.app.state-site-domain-name
    "thumbnail-processor-cluster-arn"       = module.app.thumbnail-processor-cluster-arn
    "thumbnail-processor-task-arn"          = module.app.thumbnail-processor-task-arn
    "subnet-id"                             = module.app.subnet-id
    "security-group-id"                     = module.app.security-group-id
    "site-generator-accesskey-parameter"    = module.app.site-generator-accesskey-parameter
    "site-generator-secretkey-parameter"    = module.app.site-generator-secretkey-parameter
    "cognito-user-pool-id"                  = module.app.cognito-user-pool-id
    "cognito-user-pool-arn"                 = module.app.cognito-user-pool-arn
    "cognito-user-pool-web-client-id"       = module.app.cognito-user-pool-web-client-id
    "cognito-identity-pool-id"              = module.app.cognito-identity-pool-id
    "lambda-role-arn"                       = module.app.lambda-role-arn
    "api-gateway-rest-api-id"               = module.app.api-gateway-rest-api-id
    "api-gateway-rest-api-root-resource-id" = module.app.api-gateway-rest-api-root-resource-id
  }
}

resource "aws_ssm_parameter" "parameters" {
  for_each = local.parameters

  name  = "/photography/${terraform.workspace}/${each.key}"
  type  = "String"
  value = each.value
}
