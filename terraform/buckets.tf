resource "aws_s3_bucket" "image-bucket" {
  bucket = "photography-${var.stage}-image"

  acl = "private"

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "PUT", "POST", "DELETE"]
    allowed_origins = ["*"]
    expose_headers  = ["ETag"]
    max_age_seconds = 0
  }
}

resource "aws_s3_bucket" "site-bucket" {
  bucket = "photography-${var.stage}-site"

  acl = "public-read"

  website {
    index_document = "index.html"
  }
}
resource "aws_s3_bucket_policy" "site-bucket" {
  bucket = aws_s3_bucket.site-bucket.id
  policy = data.aws_iam_policy_document.site-bucket-public.json
}

data "aws_iam_policy_document" "site-bucket-public" {
  version = "2008-10-17"
  
  statement {
    sid = "PublicReadGetObject"
    effect = "Allow"
    principals {
      identifiers = ["*"]
      type = "*"
    }
    actions = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.site-bucket.arn}/*"]
  }
}

# outputs
output "image-bucket-arn" {
  value = aws_s3_bucket.image-bucket.arn
}
output "site-bucket-arn" {
  value = aws_s3_bucket.site-bucket.arn
}