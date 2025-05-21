resource "aws_ssm_parameter" "allowed_service_soft_groups" {
  name  = "/housing/pre-production/auth-allowed-groups-service-soft"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "whitelist_service_soft_ips" {
  name  = "/housing/pre-production/whitelist-ip-address-service-soft"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "batch_get_size" {
  name  = "/housing/pre-production/contact-details-api/batch-get-size"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}
