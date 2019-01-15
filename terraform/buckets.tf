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
  bucket = "${aws_s3_bucket.site-bucket.id}"

  policy = <<EOF
{
    "Version": "2008-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "${aws_s3_bucket.site-bucket.arn}/*"
        }
    ]
}
EOF
}

# outputs
output "image-bucket-arn" {
  value = "${aws_s3_bucket.image-bucket.arn}"
}
output "site-bucket-arn" {
  value = "${aws_s3_bucket.site-bucket.arn}"
}