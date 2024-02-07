resource "aws_dynamodb_table" "contactdetailsapi_dynamodb_table" {
  name           = "ContactDetails"
  billing_mode   = "PAY_PER_REQUEST"
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
