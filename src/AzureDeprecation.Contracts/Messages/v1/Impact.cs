﻿using System.Collections.Generic;
using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class Impact
    {
        public string Description { get; set; }
        public ImpactType Type { get; set; }
        public ImpactArea Area { get; set; }
        public AzureCloud Cloud { get; set; }
        public List<AzureService> Services { get; set; } = new List<AzureService>();
    }
}
