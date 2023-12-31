﻿using heatquizapp_api.Models.BaseModels;

namespace HeatQuizAPI.Models.BaseModels
{
    public class DataPool
    {
        public int Id { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public string Name { get; set; }
        public string NickName { get; set; }

        public bool IsHidden { get; set; }

        public List<DataPoolAccess> PoolAccesses { get; set; } = new List<DataPoolAccess>();

    }
}
