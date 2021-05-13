using System.Text.Json.Serialization;

namespace ContactDetailsApi.V1.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetType
    {
        person,
        organisation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum ContactType
    {
        phone,
        emailAddress,
        address
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum SubType
    {
        mobile,
        correspondenceAddress,
        landline
    }
}
