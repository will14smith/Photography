resource "aws_iam_user" "site-generator-signing" {
  name = "photography-${var.stage}-site-generator-signing"
}

data "aws_iam_policy_document" "site-generator-signing-policy" {
  statement {
    actions = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.image-bucket.arn}/*"]
  }
}

resource "aws_iam_user_policy" "site-generator-signing" {
  user = aws_iam_user.site-generator-signing.name
  policy = data.aws_iam_policy_document.site-generator-signing-policy.json
}

resource "aws_iam_access_key" "site-generator-signing" {
  user    = aws_iam_user.site-generator-signing.name
}

resource "aws_ssm_parameter" "site-generator-signing-accesskey" {
  name = "photography-${var.stage}-SiteGeneratorSigningUser-AccessKey"
  type = "SecureString"
  value = aws_iam_access_key.site-generator-signing.id
  overwrite = true
}
resource "aws_ssm_parameter" "site-generator-signing-secretkey" {
  name = "photography-${var.stage}-SiteGeneratorSigningUser-SecretKey"
  type = "SecureString"
  value = aws_iam_access_key.site-generator-signing.secret
  overwrite = true
}

output "site-generator-accesskey-parameter" {
  value = aws_ssm_parameter.site-generator-signing-accesskey.name
}
output "site-generator-secretkey-parameter" {
  value = aws_ssm_parameter.site-generator-signing-secretkey.name
}