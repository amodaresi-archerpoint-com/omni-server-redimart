namespace LSRetail.Omni.Domain.DataModel.Loyalty.Notifications
{
    public class TopicSubscriptionError
    {
        public int Index { get; set; }
        public string Token { get; set; }
        public string Reason { get; set; }
    }
}
