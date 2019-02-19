using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Models
{
    public class Module
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }
        public List<Operation> Operations { get; set; } = new List<Operation>();
    }
}
