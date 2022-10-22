resource "aws_s3_bucket" "image-bucket" {
  bucket = "photography-${var.stage}-image"
}

resource "aws_s3_bucket_acl" "image-bucket" {
  bucket = aws_s3_bucket.image-bucket.id
  acl = "private"
}

resource "aws_s3_bucket_cors_configuration" "image-bucket" {
  bucket = aws_s3_bucket.image-bucket.id
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
}

resource "aws_s3_bucket_acl" "site-bucket" {
  bucket = aws_s3_bucket.site-bucket.id
  acl = "public-read"
}

resource "aws_s3_bucket_website_configuration" "site-bucket" {
  bucket = aws_s3_bucket.site-bucket.id

  index_document {
    suffix = "index.html"
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