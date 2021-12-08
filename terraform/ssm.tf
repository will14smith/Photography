locals {
  parameters = {
    "image-bucket-arn"                      = aws_s3_bucket.image-bucket.arn
    "site-bucket-arn"                       = aws_s3_bucket.site-bucket.arn
    "photograph-table-arn"                  = aws_dynamodb_table.photograph.arn
    "image-processor-topic-arn"             = aws_sns_topic.image-processor.arn
    "image-processor-topic-name"            = aws_sns_topic.image-processor.name
    "state-site-domain-name"                = aws_cloudfront_distribution.static-site.domain_name
    "thumbnail-processor-cluster-arn"       = aws_ecs_cluster.thumbnail-cluster.arn
    "thumbnail-processor-task-arn"          = aws_ecs_task_definition.thumbnail-task.arn
    "subnet-id"                             = aws_subnet.subnet.id
    "security-group-id"                     = aws_security_group.security-group.id
    "site-generator-accesskey-parameter"    = aws_ssm_parameter.site-generator-signing-accesskey.name
    "site-generator-secretkey-parameter"    = aws_ssm_parameter.site-generator-signing-secretkey.name
    "cognito-user-pool-id"                  = aws_cognito_user_pool.web-auth.id
    "cognito-user-pool-arn"                 = aws_cognito_user_pool.web-auth.arn
    "cognito-user-pool-web-client-id"       = aws_cognito_user_pool_client.web-auth.id
    "cognito-identity-pool-id"              = aws_cognito_identity_pool.web-auth.id
    "lambda-role-arn"                       = aws_iam_role.lambda.arn
    "api-gateway-rest-api-id"               = aws_api_gateway_rest_api.api.id
    "api-gateway-rest-api-root-resource-id" = aws_api_gateway_rest_api.api.root_resource_id
  }
}

resource "aws_ssm_parameter" "parameters" {
  for_each = local.parameters

  name  = "/photography/${terraform.workspace}/${each.key}"
  type  = "String"
  value = each.value
}
