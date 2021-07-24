# ecs
resource "aws_ecr_repository" "thumbnail-repository" {
  name = "toxon/photography-${var.stage}-thumbnailprocessor"
}

resource "aws_ecs_cluster" "thumbnail-cluster" {
  name = "photography-${var.stage}-thumbnailprocessor"
}

resource "aws_cloudwatch_log_group" "thumbnail-log-group" {
  name = "/ecs/photography-${var.stage}/ThumbnailProcessorTask"
  retention_in_days = 7
}

data "template_file" "thumbnail-containers" {
  template = file("${path.module}/thumbnail-processor-containers.json")

  vars = {
    image = "${aws_ecr_repository.thumbnail-repository.repository_url}:latest"
    photograph_table = aws_dynamodb_table.photograph.name
    image_bucket = aws_s3_bucket.image-bucket.id
    log_group_arn = aws_cloudwatch_log_group.thumbnail-log-group.name
    region = data.aws_region.current.name
  }
}

resource "aws_ecs_task_definition" "thumbnail-task" {
  family = "photography-${var.stage}-thumbnailprocessor"
  container_definitions = data.template_file.thumbnail-containers.rendered

  task_role_arn = aws_iam_role.thumbnail-task.arn
  execution_role_arn = aws_iam_role.thumbnail-execution.arn

  requires_compatibilities = ["FARGATE"]
  network_mode = "awsvpc"
  cpu = 512
  memory = 4096
}

# outputs
output "thumbnail-processor-cluster-arn" {
  value = aws_ecs_cluster.thumbnail-cluster.arn
}
output "thumbnail-processor-task-arn" {
  value = aws_ecs_task_definition.thumbnail-task.arn
}