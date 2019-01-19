data "aws_iam_policy_document" "cognito-user-assume" {
  statement {
    actions = ["sts:AssumeRoleWithWebIdentity"]
    principals {
      type = "Federated"
      identifiers = ["cognito-identity.amazonaws.com"]
    }
    condition {
      test = "StringEquals"
      variable = "cognito-identity.amazonaws.com:aud"
      values = ["${aws_cognito_identity_pool.web-auth.id}"]
    }
    condition {
      test = "ForAnyValue:StringLike"
      variable = "cognito-identity.amazonaws.com:amr"
      values = ["authenticated"]
    }
  }
}
data "aws_iam_policy_document" "cognito-user-policy" {
  statement {
    actions = [
      "s3:GetObject",
      "s3:PutObject",
      "s3:AbortMultipartUpload",
      "s3:ListMultipartUploadParts"
    ]
    resources = ["${aws_s3_bucket.image-bucket.arn}/*"]
  }
}
resource "aws_iam_role" "cognito-user" {
  name_prefix = "photography-${var.stage}-cognito-user"
  assume_role_policy = "${data.aws_iam_policy_document.cognito-user-assume.json}"
}
resource "aws_iam_role_policy" "cognito-user-policy" {
  role = "${aws_iam_role.cognito-user.id}"
  policy = "${data.aws_iam_policy_document.cognito-user-policy.json}"
}

