using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace HqPlusWebExtraction.Tests
{
    [TestFixture]
    public class BookingDataExtractorTest
    {
        private BookingDataExtractor _extractor;
        private string _filePath;
        [SetUp]
        public void Setup()
        {
            _extractor = new BookingDataExtractor();
            var currentDirectory = Directory.GetCurrentDirectory();
            var inputDataDirectory = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "input-data");
            _filePath = Path.Combine(inputDataDirectory, "Kempinski Hotel Bristol Berlin, Germany - Booking.com.html");
        }

        [Test]
        public void ExtractName()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var expected = "Kempinski Hotel Bristol Berlin";
            Assert.AreEqual(expected, result.Name, $"Name should be '{expected}'");
        }

        [Test]
        public void ExtractAddress()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var expected = "Kurfürstendamm 27, Charlottenburg-Wilmersdorf, 10719 Berlin, Germany";
            Assert.AreEqual(expected, result.Address, $"Address should be '{expected}'");
        }

        [Test]
        public void ExtractStars()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var expected = 5;
            Assert.AreEqual(expected, result.Stars, $"Stars should be '{expected}'");
        }

        [Test]
        public void ExtractReviewPoints()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var expected = 8.3;
            Assert.AreEqual(expected, result.ReviewPoints, $"Review points should be '{expected}'");
        }

        [Test]
        public void ExtractNumberOfReviews()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var expected = 1401;
            Assert.AreEqual(expected, result.NumberOfReviews, $"Number of reviews should be '{expected}'");
        }

        [Test]
        public void ExtractDescription()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;
            var startsWith = "This 5-star hotel on Berlin";
            var endsWith = "This property has been on Booking.com since 17 May 2010.\nHotel Rooms: 301, \nHotel Chain:\nKempinski";
            StringAssert.StartsWith(startsWith, result.Description, $"Description should start with '{startsWith}'");
            StringAssert.EndsWith(endsWith, result.Description, $"Description should end with '{endsWith}'");
        }

        [Test]
        public void ExtractRoomCategories()
        {
            var result = _extractor.ExtractFromFile(_filePath).Result;

            var expected = new List<RoomCategory> {
                new RoomCategory { MaxAdults = 2, MaxKids = 1, Type = "Suite with Balcony" },
                new RoomCategory { MaxAdults = 2, Type = "Classic Double or Twin Room" },
                new RoomCategory { MaxAdults = 3, Type = "Superior Double or Twin Room" },
                new RoomCategory { MaxAdults = 2, Type = "Deluxe Double Room" },
                new RoomCategory { MaxAdults = 2, Type = "Deluxe Business Suite" },
                new RoomCategory { MaxAdults = 3, Type = "Junior Suite" },
                new RoomCategory { MaxAdults = 3, Type = "Family Room" }
            };

            CollectionAssert.AreEqual(expected, result.RoomCategories);
        }
    }
}