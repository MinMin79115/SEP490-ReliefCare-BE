using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class PriorityCriteria
    {
        public bool PeopleTrapped { get; set; }
        public bool VulnerablePeople { get; set; }
        public bool SevereInjury { get; set; }

        public bool CollapseRisk { get; set; }
        public bool ToxicSmoke { get; set; }
        public bool RisingWater { get; set; }
        public bool NightOrPowerOutage { get; set; }
    }

}
