using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetType
    {
        Person,
        Organisation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum ContactType
    {
        Phone,
        EmailAddress,
        Address
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum SubType
    {
        Mobile,
        CorrespondenceAddress
    }
}
