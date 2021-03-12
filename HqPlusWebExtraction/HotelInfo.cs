using System.Collections;
using System.Collections.Generic;

namespace HqPlusWebExtraction
{
    public class HotelInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int? Stars { get; set; }
        public double? ReviewPoints { get; set; }
        public int? NumberOfReviews { get; set; }
        public string Description { get; set; }
        public ICollection<RoomCategory> RoomCategories { get; set; }
        public ICollection<HotelInfo> AlternativeHotels { get; set; }
    }
}
