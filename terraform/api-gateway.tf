resource "aws_api_gateway_rest_api" "api" {
  name = "photography-${var.stage}"

  endpoint_configuration {
    types = ["EDGE"]
  }
}

output "api-gateway-rest-api-id" {
  value = aws_api_gateway_rest_api.api.id
}
output "api-gateway-rest-api-root-resource-id" {
  value = aws_api_gateway_rest_api.api.root_resource_id
}