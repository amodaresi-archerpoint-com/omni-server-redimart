using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Notifications
{
    public class TopicSubscriptionResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<TopicSubscriptionError> Errors { get; set; }
    }
}
