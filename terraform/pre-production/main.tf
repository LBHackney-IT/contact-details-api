terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}

locals {
  default_tags = {
    Name              = "contact-details-api-${var.environment_name}"
    Environment       = var.environment_name
    terraform-managed = true
    project_name      = var.project_name
    Application       = "MTFH Housing Pre-Production"
    TeamEmail         = "developementteam@hackney.gov.uk"
  }
}

terraform {
  backend "s3" {
    bucket         = "housing-pre-production-terraform-state"
    encrypt        = true
    region         = "eu-west-2"
    key            = "services/contact-details-api/state"
    dynamodb_table = "housing-pre-production-terraform-state-lock"
  }
}

resource "aws_sns_topic" "contactdetails_topic" {
  name                        = "contactdetails.fifo"
  fifo_topic                  = true
  content_based_deduplication = true
  kms_master_key_id           = "alias/aws/sns"
}

resource "aws_ssm_parameter" "contact_details_sns_arn" {
  name  = "/sns-topic/pre-production/contact_details/arn"
  type  = "String"
  value = aws_sns_topic.contactdetails_topic.arn
}
