namespace NServiceBusTutorials.ActivePassive.Contracts
{
    public static class Endpoints
    {
        public const string AuditQueue = "Audit";

        public const string ErrorQueue = "Errors";

        public const string Publisher = "NServiceBusTutorials.ActivePassive.Publisher";

        public const string Consumer = "NServiceBusTutorials.ActivePassive.Consumer";
    }
}
