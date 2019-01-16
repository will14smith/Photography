resource "aws_dynamodb_table" "photograph" {
  name = "photography-${var.stage}-photograph"
  read_capacity = 1
  write_capacity = 1

  hash_key = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

# outputs
output "photograph-table-arn" {
  value = "${aws_dynamodb_table.photograph.arn}"
}