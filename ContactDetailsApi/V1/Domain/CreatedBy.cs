using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Domain
{
    public class CreatedBy
    {
        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
    }
}
