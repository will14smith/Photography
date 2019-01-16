# variables
variable "static-site-acm-certificate-arn" {
  type = "string"
}
variable "static-site-aliases" {
  type = "list"
}

# resources
resource "aws_cloudfront_distribution" "static-site" {
  enabled = true
  is_ipv6_enabled = true
  default_root_object = "index.html"
  price_class = "PriceClass_100"

  aliases = "${var.static-site-aliases}"

  default_cache_behavior {
    target_origin_id = "s3"
    viewer_protocol_policy = "redirect-to-https"

    allowed_methods = ["GET", "HEAD"]
    cached_methods = ["GET", "HEAD"]

    forwarded_values {
      query_string = false

      cookies {
        forward = "none"
      }
    }
  }

  viewer_certificate {
    acm_certificate_arn = "${var.static-site-acm-certificate-arn}"
    minimum_protocol_version = "TLSv1.1_2016"
    ssl_support_method = "sni-only"
  }

  origin {
    domain_name = "${aws_s3_bucket.site-bucket.bucket_regional_domain_name}"
    origin_id = "s3"
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }
}

# outputs
output "state-site-domain-name" {
  value = "${aws_cloudfront_distribution.static-site.domain_name}"
}