namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public static class ErrorCodes
    {
        public const string InvalidEmail = "W40";
        public const string InvalidPhoneNumber = "W41";
        public const string XssCheckFailure = "W42";
    }
}
