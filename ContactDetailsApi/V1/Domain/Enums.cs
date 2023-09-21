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
        email,
        address
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum SubType
    {
        correspondenceAddress,
        mobile,
        home,
        work,
        other,
        landline,
        mainNumber,
        emergencyContact,
        carer,
        wife,
        husband,
        son,
        daughter,
        relative,
        neighbour,
        doctor,
        socialWorker
    }
}
