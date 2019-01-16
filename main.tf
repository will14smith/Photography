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

module "app" {
  source = "terraform"

  stage = "${terraform.workspace}"  
}

output "image-bucket-arn" {
  value = "${module.app.image-bucket-arn}"
}
output "site-bucket-arn" {
  value = "${module.app.site-bucket-arn}"
}
output "photograph-table-arn" {
  value = "${module.app.photograph-table-arn}"
}