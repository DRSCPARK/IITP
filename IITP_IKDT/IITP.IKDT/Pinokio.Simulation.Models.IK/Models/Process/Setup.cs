using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;

namespace Pinokio.Simulation.Models.IK
{
    public class Setup
    {
        public string Name { get; set; }
        public string ProductName { get; set; }

        public Distribution Time { get; set; }

        public void SetSetupData(string name, string productName, double setUpTime, string distribution, double offset)
        {
            if (Enum.TryParse(distribution, out DistributionType distributionType))
            {
                this.Name = name;
                this.ProductName = productName;
                this.Time = new Distribution(distributionType);
                if (distributionType is DistributionType.Const)
                {
                    this.Time.Mean = 60 * setUpTime;
                }
                else if (distributionType is DistributionType.Uniform)
                {
                    this.Time.Min = 60 * (setUpTime - offset);
                    this.Time.Max = 60 * (setUpTime + offset);
                }
            }
        }
    }
}
