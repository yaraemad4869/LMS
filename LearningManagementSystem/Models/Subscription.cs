﻿using LearningManagementSystem.Data;

namespace LearningManagementSystem.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual User Student { get; set; }
        public virtual SubscriptionPlan Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string TransactionId { get; set; }
    }
}
