namespace ContactDetailsApi.V2.Domain
{
    public class AddressExtended
    {
        public string UPRN { get; set; }

        public bool IsOverseasAddress { get; set; }

        public string OverseasAddress { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string PostCode { get; set; }
    }
}
