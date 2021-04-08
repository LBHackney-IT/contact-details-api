resource "aws_dynamodb_table" "contactdetailsapi_dynamodb_table" {
    name                  = "ContactDetails"
    billing_mode          = "PROVISIONED"
    read_capacity         = 10
    write_capacity        = 10
    hash_key              = "targetId"
    range_key             = "id"

    attribute {
        name              = "id"
        type              = "S"
    }

    
    attribute {
        name              = "targetId"
        type              = "S"
    }

    tags = {
        Name              = "contact-details-api-${var.environment_name}"
        Environment       = var.environment_name
        terraform-managed = true
        project_name      = var.project_name
    }
}

resource "aws_iam_policy" "contactdetailsapi_dynamodb_table_policy" {
    name                  = "lambda-dynamodb-contact-details-api"
    description           = "A policy allowing read/write operations on contact details dynamoDB for the contact details API"
    path                  = "/contact-details-api/"

    policy                = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                        "dynamodb:BatchGetItem",
                        "dynamodb:GetItem",
                        "dynamodb:Query",
                        "dynamodb:Scan",
                        "dynamodb:BatchWriteItem",
                        "dynamodb:PutItem",
                        "dynamodb:UpdateItem"
                     ],
            "Resource": "${aws_dynamodb_table.contactdetailsapi_dynamodb_table.arn}"
        }
    ]
}
EOF
}