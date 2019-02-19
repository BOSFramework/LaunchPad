using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Models
{
    public class PermissionsSet
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ReferenceId { get; set; }
        public SetType Type { get; set; }
        public string ReferenceName { get; set; }
        public string Code { get; set; }
        public List<Operation> Permissions { get; set; }
    }

    public enum SetType
    {
        Uknown = 0,
        Role = 1,
        User = 2
    }
}
