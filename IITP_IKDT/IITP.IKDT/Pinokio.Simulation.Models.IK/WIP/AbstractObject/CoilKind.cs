using Pinokio.Core;
using System;

namespace Pinokio.Simulation.Models.IK
{
    public class CoilKind : Core.AbstractObject
    {
        private string _productName;
        private string _toolGroupName;
        private uint _maxThroughtput;
        private Setup _setup;
        private Distribution _processingTime;
        private Distribution _packagingTime;

        public string ProductName { get => _productName; }
        public string ToolGroupName { get => _toolGroupName; set => _toolGroupName = value; }
        public uint MaxThroughtput { get => _maxThroughtput; set => _maxThroughtput = value; }
        public Setup Setup { get => _setup; set => _setup = value; }
        public Distribution ProcessingTime { get => _processingTime; set => _processingTime = value; }
        public Distribution PackagingTime { get => _packagingTime; set => _packagingTime = value; }

        public CoilKind(string name, string productName) : base(0, name)
        {
            _productName = productName;
        }

        public void SetToolGroupName(string toolGroupName)
        {
            _toolGroupName = toolGroupName;
        }
        public void SetMaxThroughtput(object maxThroughput)
        {
            _maxThroughtput = Convert.ToUInt16(maxThroughput);
        }
        public void SetSetupData(Setup setup)
        {
            this.Setup = setup;
        }

        public void SetProcessingTime(string distribution, double mean, double offset)
        {
            if (Enum.TryParse(distribution, out DistributionType distributionType))
            {

                this.ProcessingTime = new Distribution(distributionType);
                if (distributionType is DistributionType.Uniform)
                {
                    this.ProcessingTime.Min = 60 * (mean - offset);
                    this.ProcessingTime.Max = 60 * (mean + offset);
                    this.ProcessingTime.Mean = 60 * (mean);
                }
            }
            else
            {
                throw new Exception($"Cannot Parse {distribution} to Distribution");
            }
        }
        public void SetPackagingTime(string distribution, double mean, double offset)
        {
            if (Enum.TryParse(distribution, out DistributionType distributionType))
            {

                this.PackagingTime = new Distribution(distributionType);
                if (distributionType is DistributionType.Uniform)
                {
                    this.PackagingTime.Min = 60 * (mean - offset);
                    this.PackagingTime.Max = 60 * (mean + offset);
                    this.PackagingTime.Mean = 60 * (mean);
                }
            }
            else
            {
                throw new Exception($"Cannot Parse {distribution} to Distribution");
            }
        }
    }
}
