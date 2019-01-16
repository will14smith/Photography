# execution role
data "aws_iam_policy_document" "thumbnail-execution-role" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}
data "aws_iam_policy_document" "thumbnail-execution-policy" {
  statement {
    actions = [
      "ecr:GetAuthorizationToken",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage",
      "logs:CreateLogStream",
      "logs:PutLogEvent",
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents",
      "logs:DescribeLogStreams"
    ]
    resources = ["*"]
  }
}
resource "aws_iam_role" "thumbnail-execution" {
  name_prefix = "${var.stage}-thumbnail-execution"
  assume_role_policy = "${data.aws_iam_policy_document.thumbnail-execution-role.json}"
}
resource "aws_iam_role_policy" "thumbnail-execution-policy" {
  role = "${aws_iam_role.thumbnail-execution.id}"
  policy = "${data.aws_iam_policy_document.thumbnail-execution-policy.json}"
}

# task role
data "aws_iam_policy_document" "thumbnail-task-role" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}
data "aws_iam_policy_document" "thumbnail-task-policy" {
  statement {
    actions = ["s3:GetObject", "s3:PutObject"]
    resources = ["${aws_s3_bucket.image-bucket.arn}/*"]
  }
  statement {
    actions = ["dynamodb:UpdateItem"]
    resources = ["${aws_dynamodb_table.photograph.arn}"]
  }
}
resource "aws_iam_role" "thumbnail-task" {
  name_prefix = "${var.stage}-thumbnail-task"
  assume_role_policy = "${data.aws_iam_policy_document.thumbnail-task-role.json}"
}
resource "aws_iam_role_policy" "thumbnail-task-policy" {
  role = "${aws_iam_role.thumbnail-execution.id}"
  policy = "${data.aws_iam_policy_document.thumbnail-task-policy.json}"
}