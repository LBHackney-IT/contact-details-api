resource "aws_dynamodb_table" "contactdetailsapi_dynamodb_table" {
  name           = "ContactDetails"
  billing_mode   = "PROVISIONED"
  read_capacity  = 10
  write_capacity = 10
  hash_key       = "targetId"
  range_key      = "id"

  attribute {
    name = "id"
    type = "S"
  }

  attribute {
    name = "targetId"
    type = "S"
  }

  tags = merge(
    local.default_tags,
    { BackupPolicy = "Stg" }
  )

  point_in_time_recovery {
    enabled = true
  }
}
