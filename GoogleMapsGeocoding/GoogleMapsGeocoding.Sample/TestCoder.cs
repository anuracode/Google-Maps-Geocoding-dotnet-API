// <copyright file="TestCoder.cs" company="">
// All rights reserved.
// </copyright>
// <author>Alberto Puyana</author>

using GoogleMapsGeocoding.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleMapsGeocoding.Sample
{
    /// <summary>
    /// Class to test the coder.
    /// </summary>
    public static class TestCoder
    {
        /// <summary>
        /// Test all the methods of the coder.
        /// </summary>
        /// <returns>Task to await.</returns>
        public static async Task AllAsync()
        {
            string apiKey = string.Empty;

            Geocoder coder = new Geocoder()
            {
                ApiKey = apiKey
            };

            string address = "1600 Amphitheatre Parkway";
            Console.WriteLine($"Geocode address:'{address}' with language and region");
            PrintResponse(await coder.GeocodeAsync(address, language: "en", region: "us"));

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Geocode address:'{address}' with bounds filter");

            // Antioquia bounds 5.418365,-77.135572|8.8814071,-73.871107
            Bounds boundFilter = new Bounds()
            {
                Southwest = new Southwest()
                {
                    Lat = 5.418365,
                    Lng = -77.135572
                },
                Northeast = new Northeast()
                {
                    Lat = 8.8814071,
                    Lng = -73.871107
                }
            };
            PrintResponse(await coder.GeocodeAsync(address, language: "en", boundsBias: boundFilter));

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Geocode address:'{address}' with component filter country");
            PrintResponse(await coder.GeocodeAsync(address, component: new ComponentFilter() { Country = "us" }));

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Geocode address:'{address}' with component filter country and administrative area");
            PrintResponse(await coder.GeocodeAsync(address, component: new ComponentFilter() { Country = "us", AdministravieArea = "California" }));

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Geocode address:'{address}' with component filter country and administrative area, language and department bounds.");
            PrintResponse(await coder.GeocodeAsync(address, component: new ComponentFilter() { Country = "us", AdministravieArea = "California" }, language: "en", boundsBias: boundFilter));

            var gresponse = await coder.GeocodeAsync("cra 28 29 145", component: new ComponentFilter() { Country = "co", AdministravieArea = "Antioquia" }, language: "es");
            PrintResponse(gresponse);

            List<PositionTest> positions = new List<PositionTest>();

            positions.Add(new PositionTest() { Latitude = 6.235718, Longitude = -75.573979 });
            positions.Add(new PositionTest() { Latitude = 6.227856, Longitude = -75.558780 });
            positions.Add(new PositionTest() { Latitude = 6.170472, Longitude = -75.587401 });
            positions.Add(new PositionTest() { Latitude = 6.204488, Longitude = -75.571561 });
            positions.Add(new PositionTest() { Latitude = 6.203884, Longitude = -75.571394 });
            positions.Add(new PositionTest() { Latitude = 6.207308, Longitude = -75.568542 });
            positions.Add(new PositionTest() { Latitude = 6.228104, Longitude = -75.558861 });
            positions.Add(new PositionTest() { Latitude = 6.2281042313961663, Longitude = -75.558861551630173 });

            foreach (var pos in positions)
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine($"ReverseGeocode lat:{pos.Latitude};long:{pos.Longitude}");

                var response = await coder.ReverseGeocodeAsync(pos.Latitude, pos.Longitude);
                var addressPart = GetFirstAddress(response);
                Console.WriteLine(string.Format("{0} {1} # {2} - {3}", addressPart.AddressLiteral1, addressPart.AddressNumber1, addressPart.AddressNumber2, addressPart.AddressNumber3));
            }
        }

        /// <summary>
        /// Get first address.
        /// </summary>
        /// <param name="respose">Response to parse.</param>
        /// <returns>Parsed response.</returns>
        public static AddressParts GetFirstAddress(GeocodeResponse respose)
        {
            AddressParts result = null;

            if ((respose != null) && (respose.Status == "OK"))
            {
                var ra = (from r in respose.Results
                          where r.Types.Contains("street_address")
                          select r).FirstOrDefault();

                if ((ra != null) && (ra.AddressComponents != null) && (ra.AddressComponents.Length > 0))
                {
                    string streetNumber = GetValueFromComponentType(ra.AddressComponents, "street_number");
                    string route = GetValueFromComponentType(ra.AddressComponents, "route");
                    string city = GetValueFromComponentType(ra.AddressComponents, "locality");
                    string state = GetValueFromComponentType(ra.AddressComponents, "administrative_area_level_1");
                    string country = GetValueFromComponentType(ra.AddressComponents, "country");
                    string zipCode = GetValueFromComponentType(ra.AddressComponents, "postal_code");
                    string neighborhood = GetValueFromComponentType(ra.AddressComponents, "neighborhood");

                    string addressLiteral1 = string.Empty;
                    string addressNumber1 = string.Empty;
                    string addressNumber2 = string.Empty;
                    string addressNumber3 = string.Empty;

                    if (!string.IsNullOrWhiteSpace(route))
                    {
                        int indexSpace = route.IndexOf(" ");
                        if (indexSpace > 0)
                        {
                            addressLiteral1 = route.Substring(0, indexSpace).Trim();
                            addressNumber1 = route.Substring(indexSpace + 1, route.Length - (indexSpace + 1)).Trim();
                        }
                        else
                        {
                            addressLiteral1 = route.Trim();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(streetNumber))
                    {
                        int indexSpace = streetNumber.IndexOf("-");
                        if (indexSpace > 0)
                        {
                            addressNumber2 = streetNumber.Substring(0, indexSpace).Trim();
                            addressNumber3 = streetNumber.Substring(indexSpace + 1, streetNumber.Length - (indexSpace + 1)).Trim();
                        }
                        else
                        {
                            addressNumber2 = streetNumber.Trim();
                        }
                    }

                    // Find common values address number.
                    if (!string.IsNullOrWhiteSpace(addressNumber2) && !string.IsNullOrWhiteSpace(addressNumber3) && (addressNumber2.Length > 1) && (addressNumber2.Length <= addressNumber3.Length))
                    {
                        int commonCount = 0;

                        for (int i = 0; i < addressNumber2.Length; i++)
                        {
                            if (addressNumber2[i] == addressNumber3[i])
                            {
                                commonCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (commonCount > 0)
                        {
                            string range1 = addressNumber2.Substring(commonCount, addressNumber2.Length - (commonCount));
                            string range2 = addressNumber3.Substring(commonCount, addressNumber3.Length - (commonCount));

                            int intRange1;
                            int intRange2;

                            if (int.TryParse(range1, out intRange1) && int.TryParse(range2, out intRange2))
                            {
                                addressNumber3 = Math.Abs((intRange2 - intRange1) * 0.5).ToString();
                                addressNumber2 = addressNumber2.Substring(0, commonCount);
                            }
                        }
                    }

                    result = new AddressParts()
                    {
                        AddressLiteral1 = addressLiteral1,
                        AddressNumber1 = addressNumber1,
                        AddressNumber2 = addressNumber2,
                        AddressNumber3 = addressNumber3,
                        City = city,
                        State = state,
                        Country = country,
                        Neighbourhood = neighborhood,
                        ZipCode = zipCode
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Print a response.
        /// </summary>
        /// <param name="respose">Response to use.</param>
        public static void PrintResponse(GeocodeResponse respose)
        {
            if ((respose != null) && (respose.Status == GlobalConstants.OK_STATUS) && (respose.Results != null))
            {
                foreach (var result in respose.Results)
                {
                    Console.WriteLine($"PlaceId:{result.PlaceId}; Types:{string.Join(",", result.Types)}; FormattedAddress:{result.FormattedAddress}");

                    if ((result.Geometry != null) && (result.Geometry.Bounds != null) && result.Geometry.Bounds.HasBounds)
                    {
                        Console.WriteLine($"Bounds:{result.Geometry.Bounds.ToQueryString()}");
                    }
                }
            }
        }

        /// <summary>
        /// Get value from the component list.
        /// </summary>
        /// <param name="components">Components to use.</param>
        /// <param name="type">Type to use.</param>
        /// <returns></returns>
        private static string GetValueFromComponentType(IEnumerable<AddressComponent> components, string type)
        {
            string value = string.Empty;

            if (components != null)
            {
                var componentValue = (from component in components
                                      where component.Types.Contains(type)
                                      select component.LongName).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(componentValue))
                {
                    value = componentValue.Trim();
                }
            }

            return value;
        }
    }

    /// <summary>
    /// Address parts
    /// </summary>
    public class AddressParts
    {
        /// <summary>
        /// Literal part of the address.
        /// </summary>
        public string AddressLiteral1 { get; set; }

        /// <summary>
        /// Number part of the address.
        /// </summary>
        public string AddressNumber1 { get; set; }

        /// <summary>
        /// Number part of the address.
        /// </summary>
        public string AddressNumber2 { get; set; }

        /// <summary>
        /// Number part of the address.
        /// </summary>
        public string AddressNumber3 { get; set; }

        /// <summary>
        /// City part.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country part.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Neighbourhood part.
        /// </summary>
        public string Neighbourhood { get; set; }

        /// <summary>
        /// State part.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Zip code.
        /// </summary>
        public string ZipCode { get; set; }
    }

    /// <summary>
    /// Position test.
    /// </summary>
    public class PositionTest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}