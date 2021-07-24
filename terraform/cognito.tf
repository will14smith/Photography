resource "aws_cognito_user_pool" "web-auth" { 
  name = "photography-${var.stage}"

  admin_create_user_config {
    allow_admin_create_user_only = true
  }
  alias_attributes = ["email"]
  auto_verified_attributes = ["email"]
}
resource "aws_cognito_user_pool_client" "web-auth" { 
  name = "photography-${var.stage}"
  user_pool_id = aws_cognito_user_pool.web-auth.id

  generate_secret = false
}

resource "aws_cognito_identity_pool" "web-auth" {
  identity_pool_name = "photography ${var.stage}"
  allow_unauthenticated_identities = false

  cognito_identity_providers {
    client_id = aws_cognito_user_pool_client.web-auth.id
    provider_name = aws_cognito_user_pool.web-auth.endpoint
  }
}
resource "aws_cognito_identity_pool_roles_attachment" "web-auth" {
  identity_pool_id = aws_cognito_identity_pool.web-auth.id
  roles = {
    authenticated = aws_iam_role.cognito-user.arn
  }
}

output "cognito-user-pool-id" { 
  value = aws_cognito_user_pool.web-auth.id
}
output "cognito-user-pool-arn" { 
  value = aws_cognito_user_pool.web-auth.arn
}
output "cognito-user-pool-web-client-id" { 
  value = aws_cognito_user_pool_client.web-auth.id
}
output "cognito-identity-pool-id" { 
  value = aws_cognito_identity_pool.web-auth.id
}