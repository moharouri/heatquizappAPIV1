﻿using heatquizapp_api.Models.BaseModels;
using Microsoft.AspNetCore.Identity;

namespace HeatQuizAPI.Models.BaseModels
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public DateTime RegisteredOn { get; set; }

        public bool Active { get; set; }

        public string? ProfilePicture { get; set; }

        public List<DataPoolAccess> PoolAccesses { get; set; } = new List<DataPoolAccess>();

        public DateTime? StatisticsStartDate { get; set; }
        public bool SaveStatistics{ get; set; }

        //Linked keys
        public List<UserLinkedPlayerKey> LinkedKeys = new List<UserLinkedPlayerKey>();

        //Notification subsription
        public List<DatapoolNotificationSubscription> notificationSubscriptions = new List<DatapoolNotificationSubscription>();

    }
}
