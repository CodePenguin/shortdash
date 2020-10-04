using System;

namespace ShortDash.Server.Data
{
    public class DeviceClaim
    {
        public DeviceClaim()
        {
        }

        public DeviceClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var compare = (DeviceClaim)obj;
                return string.Equals(Type, compare.Type) && string.Equals(Value, compare.Value);
            }
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Type, Value).GetHashCode();
        }
    }
}
