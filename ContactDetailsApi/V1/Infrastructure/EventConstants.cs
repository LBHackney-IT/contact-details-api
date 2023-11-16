namespace ContactDetailsApi.V1.Infrastructure
{
    public static class EventConstants
    {
        public const string CREATED = "ContactDetailAddedEvent";
        public const string DELETED = "ContactDetailDeletedEvent";
        public const string EDITED = "ContactDetailEditedEvent";

        public const string V1VERSION = "v1";
        public const string SOURCEDOMAIN = "ContactDetails";
        public const string SOURCESYSTEM = "ContactDetailsAPI";
    }
}
