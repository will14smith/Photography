resource "aws_sns_topic" "image-processor" {
  name = "photography-${var.stage}-imageprocessor"
}

# outputs
output "image-processor-topic-arn" {
  value = "${aws_sns_topic.image-processor.arn}"
}
output "image-processor-topic-name" {
  value = "${aws_sns_topic.image-processor.name}"
}