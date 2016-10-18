﻿using System;
using System.Net;
using EPiServer.Personalization;
using MaxMind.GeoIP2;
using System.Net.Sockets;
using Pixie.Extensions.Maxmind.GeoIp.Models;
using System.Collections.Specialized;
using EPiServer.Web;

namespace Pixie.Extensions.Maxmind.GeoIp.Services
{
    public class GeolocationMaxmindService : IGeolocationService
    {
        private string maxMindDatabaseFileName = "GeoLite2-City.mmdb";

        public GeoLocationResult GetGeoLocation(IPAddress address, NameValueCollection config)
        {
            string text = config["databaseFileName"];

            if (!string.IsNullOrEmpty(text))
            {
                maxMindDatabaseFileName = VirtualPathUtilityEx.RebasePhysicalPath(text);
                config.Remove("databaseFileName");
            }

            if (string.IsNullOrWhiteSpace(maxMindDatabaseFileName))
            {
                throw new ArgumentException("db name is not provided");
            }

            if (!System.IO.File.Exists(maxMindDatabaseFileName))
            {
                throw new ArgumentException(string.Format("db does not exist at location {0}", maxMindDatabaseFileName));
            }

            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                return null;
            }

            try
            {
                using (var reader = new DatabaseReader(maxMindDatabaseFileName))
                {
                    var dbResult = reader.City(address);

                    if (dbResult == null)
                    {
                        return null;
                    }

                    GeoLocationResult result = new GeoLocationResult();
                    result.CountryCode = dbResult.Country.IsoCode;
                    result.CountryName = dbResult.Country.Name;
                    result.Latitude = (dbResult.Location.Latitude != null) ? dbResult.Location.Latitude.Value : 0;
                    result.Longitude = (dbResult.Location.Longitude != null) ? dbResult.Location.Longitude.Value : 0;
                    result.MetroCode = (dbResult.Location.MetroCode != null) ? dbResult.Location.MetroCode.Value : 0;
                    result.City = dbResult.City.Name;
                    result.PostalCode = dbResult.Postal.Code;
                    result.CountinentCode = dbResult.Continent.Code;

                    return result;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
