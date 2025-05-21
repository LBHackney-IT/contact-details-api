resource "aws_dynamodb_table" "contactdetailsapi_dynamodb_table" {
  name         = "ContactDetails"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "targetId"
  range_key    = "id"

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
    { BackupPolicy = "Dev", Backup = false, Confidentiality = "Internal" }
  )

  point_in_time_recovery {
    enabled = false
  }
}
