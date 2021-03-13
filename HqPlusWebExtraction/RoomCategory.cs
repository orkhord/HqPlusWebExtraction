using System;

namespace HqPlusWebExtraction
{
    public class RoomCategory
    {
        public int? MaxAdults { get; set; }
        public int? MaxKids { get; set; }
        public string Type { get; set; }
        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                RoomCategory rc = (RoomCategory)obj;
                return MaxAdults == rc.MaxAdults && MaxKids == rc.MaxKids && Type == rc.Type;
            }
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(MaxAdults, MaxKids, Type);
        }

    }
}
