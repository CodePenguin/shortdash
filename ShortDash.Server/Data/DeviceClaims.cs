using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DeviceClaims : List<DeviceClaim>
    {
        public void Add(string type, string value)
        {
            Add(new DeviceClaim { Type = type, Value = value });
        }

        public bool Equals(DeviceClaims compare)
        {
            var sortedCompareList = compare.OrderBy(c => c.Type).ThenBy(c => c.Value);
            var sortedList = this.OrderBy(c => c.Type).ThenBy(c => c.Value);
            return Enumerable.SequenceEqual(sortedCompareList, sortedList);
        }
    }
}
