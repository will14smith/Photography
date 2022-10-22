terraform {
  backend "remote" {
    hostname     = "will14smith.scalr.io"
    # Development
    # organization = "env-ua0ao11lgojfqs0"
    # Production
    organization = "env-tq0mi1vq39nn76o"
    workspaces {
      name = "photography"
    }
  }
}

data "aws_region" "current" {}
data "aws_caller_identity" "current" {}

provider "aws" {
  region = var.region
}
