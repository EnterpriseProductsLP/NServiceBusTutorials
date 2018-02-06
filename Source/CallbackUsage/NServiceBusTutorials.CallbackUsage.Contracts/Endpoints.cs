namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public static class Endpoints
    {
        public const string ErrorQueue = "errors";

        public const string Receiver = "NServiceBusTutorials.CallbackUsage.Receiver";

        public const string Sender = "NServiceBusTutorials.CallbackUsage.Sender";
    }
}