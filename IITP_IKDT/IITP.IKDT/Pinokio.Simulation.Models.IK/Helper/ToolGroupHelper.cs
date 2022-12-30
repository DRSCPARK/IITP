using System;
using System.Collections.Generic;
using System.Text;

namespace Pinokio.Simulation.Models.IK
{
    public static class ToolGroupHelper
    {
        private static Dictionary<string, string> _toolGroupId = new Dictionary<string, string>();
        private static Dictionary<string, object> _toolGroup = new Dictionary<string, object>();

        public static Dictionary<string, string> ToolGroupId
        { get => _toolGroupId; set => _toolGroupId = value; }

        public static Dictionary<string, object> ToolGroup
        { get => _toolGroup; set => _toolGroup = value; }
    }
}
