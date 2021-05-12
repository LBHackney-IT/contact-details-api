namespace ContactDetailsApi.V1.Domain
{
    public class AddressExtended
    {
        public long UPRN { get; set; }

        public bool IsOverseasAddress { get; set; }

        public string OverseasAddress { get; set; }
    }
}
