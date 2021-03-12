using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HqPlusWebExtraction
{
    public class BookingDataExtractor : IDataExtractor
    {
        public async Task<HotelInfo> ExtractFromFile(string path)
        {
            using FileStream fs = File.OpenRead(path);
            return await Extract(fs);
        }

        public async Task<HotelInfo> Extract(FileStream fs)
        {
            using var sr = new StreamReader(fs, Encoding.UTF8);
            var content = await sr.ReadToEndAsync();
            return Extract(content);
        }

        public HotelInfo Extract(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var docNode = htmlDoc.DocumentNode;

            var hotelInfo = new HotelInfo();

            hotelInfo.Name = docNode
                .SelectSingleNode("//span[@id=\"hp_hotel_name\"]")
                ?.InnerText
                ?.Trim();

            hotelInfo.Address = docNode
                .SelectSingleNode("//span[@id=\"hp_address_subtitle\"]")
                ?.InnerText
                ?.Trim();

            hotelInfo.Stars = GetStars(docNode);

            hotelInfo.ReviewPoints = GetReviewPoints(docNode);

            hotelInfo.NumberOfReviews = GetNumberOfReviews(docNode);

            hotelInfo.Description = GetDescription(docNode);

            hotelInfo.RoomCategories = GetRoomCategories(docNode);

            hotelInfo.AlternativeHotels = GetAlternativeHotels(docNode);

            return hotelInfo;
        }

        private static double? GetReviewPoints(HtmlNode docNode)
        {
            var reviewPointsString = docNode
                .SelectSingleNode("//div[@id=\"js--hp-gallery-scorecard\"]//span[contains(@class, 'js--hp-scorecard-scoreval')]")
                ?.InnerText
                ?.Trim();

            if (!string.IsNullOrWhiteSpace(reviewPointsString))
            {
                if (double.TryParse(reviewPointsString, out var reviewPoints))
                {
                    return reviewPoints;
                }
                return null;
            }
            return null;
        }

        private static int? GetNumberOfReviews(HtmlNode docNode)
        {
            var numberOfReviewsString = docNode
                .SelectSingleNode("//div[@id=\"js--hp-gallery-scorecard\"]//span[contains(@class, 'score_from_number_of_reviews')]/strong[@class='count']")
                ?.InnerText
                ?.Trim();

            if (!string.IsNullOrWhiteSpace(numberOfReviewsString))
            {
                if (int.TryParse(numberOfReviewsString, out var numberOfReviews))
                {
                    return numberOfReviews;
                }
                return null;
            }
            return null;
        }

        private static string GetDescription(HtmlNode docNode)
        {
            var mainDescNodes = docNode
                .SelectNodes("//div[@id=\"summary\"]/p")
                ?.Select(n => n.InnerText?.Trim()) ?? new List<string>();

            var otherDescNodes = docNode
                .SelectNodes("//div[contains(@class, 'hotel_description_wrapper_exp')]/p")
                ?.Select(n => n.InnerText?.Trim()) ?? new List<string>();

            var descNodes = mainDescNodes.Concat(otherDescNodes);

            return string.Join('\n', descNodes);
        }

        private static int? GetStars(HtmlNode docNode)
        {
            var starsClass = docNode
                .SelectSingleNode("//div[@id=\"wrap-hotelpage-top\"]//i[contains(@class, 'star_track')]")
                ?.Attributes["class"]
                ?.Value;

            if (!string.IsNullOrWhiteSpace(starsClass))
            {
                const string word = "ratings_stars_";
                var regex = new Regex(word + "([0-9]{1})");
                var match = regex.Match(starsClass);

                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    var starsString = match.Value[word.Length..];

                    if (int.TryParse(starsString, out var result)) {
                        return result;
                    }

                    return null;
                }
                return null;
            }
            return null;
        }

        private static int? GetMaxOccupancy(HtmlNode maxOccupancyNode, string className)
        {
            var nodeToLookUp = maxOccupancyNode;
            var childSpanNode = nodeToLookUp.SelectSingleNode("span");
            if (childSpanNode != null) nodeToLookUp = childSpanNode;

            var maxOccupancyNodeClass = nodeToLookUp
                .SelectSingleNode($"i[contains(@class, '{className}')]")
                ?.Attributes["class"]
                ?.Value;

            if (!string.IsNullOrWhiteSpace(maxOccupancyNodeClass))
            {
                var regex = new Regex(className + "([0-9]+)");
                var match = regex.Match(maxOccupancyNodeClass);

                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    var maxOccupancyString = match.Value[className.Length..];

                    if (int.TryParse(maxOccupancyString, out var result))
                    {
                        return result;
                    }

                    return null;
                }
                return null;
            }
            return null;
        }

        private static int? GetMaxAdults(HtmlNode maxOccupancyNode) => GetMaxOccupancy(maxOccupancyNode, "occupancy_max");

        private static int? GetMaxKids(HtmlNode maxOccupancyNode) => GetMaxOccupancy(maxOccupancyNode, "occupancy_kid");

        private static ICollection<RoomCategory> GetRoomCategories(HtmlNode docNode)
        {
            var tableRowNodes = docNode
                .SelectNodes("//table[@id=\"maxotel_rooms\"]/tbody/tr");

            return tableRowNodes
                ?.Select(tableRowNode =>
                {
                    var maxOccupancyNode = tableRowNode
                        .SelectSingleNode("td[@class='occ_no_dates']");

                    var maxAdults = GetMaxAdults(maxOccupancyNode);
                    var maxKids = GetMaxKids(maxOccupancyNode);

                    var type = tableRowNode
                        .SelectSingleNode("td[@class='ftd']")
                        ?.InnerText
                        ?.Trim();

                    return new RoomCategory
                    {
                        MaxAdults = maxAdults,
                        MaxKids = maxKids,
                        Type = type
                    };
                })
                ?.ToList() ?? new List<RoomCategory>();
        }

        private static ICollection<HotelInfo> GetAlternativeHotels(HtmlNode docNode)
        {

            var tableCellNodes = docNode
                .SelectNodes("//table[@id=\"althotelsTable\"]/tbody/tr/td");

            return tableCellNodes
                ?.Select(tableCellNode =>
                {
                    var hotelInfo = new HotelInfo();

                    hotelInfo.Name = tableCellNode
                        .SelectSingleNode("p[@class='althotels-name']/a[@class='althotel_link']")
                        ?.InnerText
                        ?.Trim();

                    hotelInfo.Description = tableCellNode
                        .SelectSingleNode("span[@class='hp_compset_description']")
                        ?.InnerText
                        ?.Trim();

                    hotelInfo.NumberOfReviews = GetAlternativeHotelNumberOfReviews(tableCellNode);

                    hotelInfo.ReviewPoints = GetAlternativeHotelReviewPoints(tableCellNode);

                    return hotelInfo;
                })?.ToList() ?? new List<HotelInfo>();
        }

        private static int? GetAlternativeHotelNumberOfReviews(HtmlNode tableCellNode)
        {
            var numberOfReviewsString = tableCellNode
                        .SelectSingleNode("div[contains(@class, 'alt_hotels_info_row')]/span[contains(@class, 'score_from_number_of_reviews')]/strong[@class='count']")
                        ?.InnerText
                        ?.Trim();

            if (!string.IsNullOrWhiteSpace(numberOfReviewsString))
            {
                if (int.TryParse(numberOfReviewsString, out var numberOfReviews))
                {
                    return numberOfReviews;
                }
                return null;
            }
            return null;
        }

        private static double? GetAlternativeHotelReviewPoints(HtmlNode tableCellNode)
        {
            var reviewPointsString = tableCellNode
                .SelectSingleNode("div[contains(@class, 'alt_hotels_info_row')]/a[contains(@class, 'hp_review_score')]/span[contains(@class, 'rating')]/span[contains(@class, 'js--hp-scorecard-scoreval')]")
                ?.InnerText
                ?.Trim();

            if (!string.IsNullOrWhiteSpace(reviewPointsString))
            {
                if (double.TryParse(reviewPointsString, out var reviewPoints))
                {
                    return reviewPoints;
                }
                return null;
            }
            return null;
        }
    }
}
