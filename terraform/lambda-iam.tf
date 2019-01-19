data "aws_iam_policy_document" "lambda-assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}
# This could be split into per-function policies
data "aws_iam_policy_document" "lambda-policy" {
  statement {
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream"
    ]
    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/aws/lambda/*:*"
    ]
  }
  statement {
    actions = [
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/aws/lambda/*:*:*"
    ]
  }

  statement {
    actions = [
      "dynamodb:DescribeTable",
      "dynamodb:Query",
      "dynamodb:Scan",
      "dynamodb:GetItem",
      "dynamodb:PutItem",
      "dynamodb:UpdateItem",
      "dynamodb:DeleteItem"
    ]
    resources = ["${aws_dynamodb_table.photograph.arn}"]
  }
  statement {
    actions = [
      "s3:GetObject",
      "s3:PutObject",
      "s3:DeleteObject"
    ]
    resources = ["${aws_s3_bucket.image-bucket.arn}/*"]
  }
  statement {
    actions = [
      "s3:PutObject",
      "s3:DeleteObject"
    ]
    resources = ["${aws_s3_bucket.site-bucket.arn}/*"]
  }
  statement {
    actions = ["sns:Publish"]
    resources = ["${aws_sns_topic.image-processor.arn}"]
  }
  statement {
    actions = [
      "ssm:GetParameter",
      "ssm:GetParameters"
    ]
    resources = [
      "${aws_ssm_parameter.site-generator-signing-accesskey.arn}",
      "${aws_ssm_parameter.site-generator-signing-secretkey.arn}"
    ]
  }
  statement {
    actions = ["ecs:RunTask"]
    resources = ["${aws_ecs_task_definition.thumbnail-task.arn}"]
  }
  statement {
    actions = ["iam:PassRole"]
    resources = [
      "${aws_iam_role.thumbnail-execution.arn}",
      "${aws_iam_role.thumbnail-task.arn}"
    ]
  }
}
resource "aws_iam_role" "lambda" {
  name_prefix = "photography-${var.stage}-lambda"
  assume_role_policy = "${data.aws_iam_policy_document.lambda-assume.json}"
}
resource "aws_iam_role_policy" "lambda" {
  role = "${aws_iam_role.lambda.id}"
  policy = "${data.aws_iam_policy_document.lambda-policy.json}"
}

output "lambda-role-arn" {
  value = "${aws_iam_role.lambda.arn}"
}