﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Serialization;
using MarketplaceWebServiceFeedsClasses;
using MrCMS.Web.Apps.Amazon.Settings;
using MrCMS.Website;
using StringCollection = System.Collections.Specialized.StringCollection;

namespace MrCMS.Web.Apps.Amazon.Helpers
{
    public static class AmazonAppHelper
    {
        public static T GetEnumByValue<T>(this string value) where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>().SingleOrDefault(x => x.ToString() == value);
        }
        public static T GetEnumByValue<T>(this Enum value) where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>().SingleOrDefault(x => x.ToString() == value.ToString());
        }
        public static string ToShortString(this string value, int numOfCharacters)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                if (value.Length > numOfCharacters)
                    return value.Substring(0, numOfCharacters) + "...";
                return value;
            }
            return String.Empty;
        }
        public static string ToTokenizedString(this StringCollection collection, string token)
        {
            return collection.Count > 1 ?
                collection.Cast<string>().Aggregate(String.Empty, (current, item) => current + (item + token)) :
                collection.Cast<string>().Aggregate(String.Empty, (current, item) => current + item);
        }
        public static string GenerateImageUrl(string imageUrl)
        {
            if (CurrentRequestData.CurrentSite.BaseUrl.Contains("http://") || CurrentRequestData.CurrentSite.BaseUrl.Contains("https://"))
                return CurrentRequestData.CurrentSite.BaseUrl + imageUrl;
            return "http://" + CurrentRequestData.CurrentSite.BaseUrl + imageUrl;
        }

        public static FileStream GetStreamFromAmazonEnvelope(AmazonEnvelope amazonEnvelope, AmazonEnvelopeMessageType amazonEnvelopeMessageType)
        {
            var xml = Serialize(amazonEnvelope);
            var fileLocation = GetAmazonApiFolderPath(amazonEnvelopeMessageType);
            File.WriteAllText(fileLocation, xml);
            return File.Open(fileLocation, FileMode.Open, FileAccess.Read);
        }

        private static string Serialize<T>(T item)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream))
                {
                    new XmlSerializer(typeof(T)).Serialize(writer, item);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        private static string GetAmazonApiFolderPath(AmazonEnvelopeMessageType amazonEnvelopeMessageType)
        {
            var fileLocation=string.Format("{0}/{1}/{2}",
                          CurrentRequestData.CurrentSite.Id, "amazon",
                          string.Format("Amazon{0}Feed", amazonEnvelopeMessageType)
                          + "-" + CurrentRequestData.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".xml");

            var relativeFilePath = Path.Combine("/content/upload/", fileLocation);
            var baseDirectory = HostingEnvironment.ApplicationPhysicalPath.Substring(0, HostingEnvironment.ApplicationPhysicalPath.Length - 1);
            var path = Path.Combine(baseDirectory, relativeFilePath.Substring(1));

            return path;
        }

        public static bool GetAmazonAppSettingsStatus(AmazonAppSettings settings)
        {
            return settings.GetType().GetProperties().Where(info => info.CanWrite && info.Name != "Site").All(property => !String.IsNullOrWhiteSpace(Convert.ToString(property.GetValue(settings, null))));
        }

        public static bool GetAmazonSellerSettingsStatus(AmazonSellerSettings settings)
        {
            return settings.GetType().GetProperties().Where(info => info.CanWrite && info.Name != "Site").All(property => !String.IsNullOrWhiteSpace(Convert.ToString(property.GetValue(settings, null))));
        }
    }
}