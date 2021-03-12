using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HqPlusWebExtraction
{
    public interface IDataExtractor
    {
        Task<HotelInfo> ExtractFromFile(string path);
        Task<HotelInfo> Extract(FileStream stream);
        HotelInfo Extract(string content);
    }
}
