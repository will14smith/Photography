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
    defaults = { }

    dev = {
      static-site-acm-certificate-arn = "arn:aws:acm:us-east-1:682179218046:certificate/0a059434-104e-49d0-a7b8-5ca2fcb951ab"
      static-site-aliases = "photography-dev.toxon.co.uk"
    }

    prod = {
      static-site-acm-certificate-arn = "arn:aws:acm:us-east-1:682179218046:certificate/6cc4623e-c061-41b8-85b3-ed0c4a67b39d"
      static-site-aliases = "toxon.co.uk;www.toxon.co.uk"
    }
  }

  workspace = "${merge(local.env["defaults"], local.env[terraform.workspace])}"
}

# application
module "app" {
  source = "terraform"

  stage = "${terraform.workspace}"  
  static-site-acm-certificate-arn = "${local.workspace["static-site-acm-certificate-arn"]}"
  static-site-aliases = "${split(";", local.workspace["static-site-aliases"])}"
}

# output
output "image-bucket-arn" {
  value = "${module.app.image-bucket-arn}"
}
output "site-bucket-arn" {
  value = "${module.app.site-bucket-arn}"
}
output "photograph-table-arn" {
  value = "${module.app.photograph-table-arn}"
}
output "image-processor-topic-arn" {
  value = "${module.app.image-processor-topic-arn}"
}
output "image-processor-topic-name" {
  value = "${module.app.image-processor-topic-name}"
}
output "state-site-domain-name" {
  value = "${module.app.state-site-domain-name}"
}