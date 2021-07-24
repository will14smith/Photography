resource "aws_vpc" "vpc" {
  cidr_block = "10.0.0.0/24"
}

resource "aws_subnet" "subnet" {
  vpc_id = aws_vpc.vpc.id
  cidr_block = "10.0.0.0/24"
  map_public_ip_on_launch = true
}

resource "aws_security_group" "security-group" {
  vpc_id = aws_vpc.vpc.id
  description = "Default security group"
  egress {
    from_port = 0
    to_port = 0
    protocol = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# routing
resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.vpc.id
}

resource "aws_route_table" "routes" {
    vpc_id = aws_vpc.vpc.id    
}

resource "aws_route" "route" {
  route_table_id = aws_route_table.routes.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id = aws_internet_gateway.igw.id  
}

resource "aws_route_table_association" "attach" {
  route_table_id = aws_route_table.routes.id
  subnet_id = aws_subnet.subnet.id
}

# outputs
output "subnet-id" {
  value = aws_subnet.subnet.id
}
output "security-group-id" {
  value = aws_security_group.security-group.id
}