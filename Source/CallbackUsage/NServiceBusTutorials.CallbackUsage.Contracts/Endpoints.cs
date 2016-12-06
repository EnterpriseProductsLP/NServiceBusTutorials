namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public static class Endpoints
    {
        public const string AuditQueue = "NServiceBusTutorials.CallbackUsage.AuditQueue";

        public const string ErrorQueue = "NServiceBusTutorials.CallbackUsage.ErrorQueue";

        public const string Receiver = "NServiceBusTutorials.CallbackUsage.Receiver";

        public const string Sender = "NServiceBusTutorials.CallbackUsage.Sender";
    }
}