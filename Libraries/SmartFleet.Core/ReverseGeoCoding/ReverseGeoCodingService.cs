using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.ReverseGeoCoding.Dtos;

namespace SmartFleet.Core.ReverseGeoCoding
{
    public class ReverseGeoCodingService
    {
        private const string KEY = "pk.cc7d7c232c3b43aa3a87127b93b22339";
        private int count = 0;
        public async Task ReverseGeoCoding(CreateTk103Gps gpsStatement)
        {
            var lat = gpsStatement.Latitude.ToString().Replace(",", ".");
            var lon = gpsStatement.Longitude.ToString().Replace(",", ".");
            var client = new HttpClient();
            var url = $"https://us1.locationiq.com/v1/reverse.php?key={KEY}&lat={lat}&lon={lon}&format=json";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var r = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LocationiqResponse>(r);
                gpsStatement.Address = result.display_name;
                gpsStatement.Region = result.address.region;
                gpsStatement.State = result.address.state;
            }

        }
        public async Task ReverseGeoCoding(Position gpsStatement)
        {
            var lat = gpsStatement.Lat.ToString().Replace(",", ".");
            var lon = gpsStatement.Long.ToString().Replace(",", ".");
            var client = new HttpClient();
            var url = $"https://us1.locationiq.com/v1/reverse.php?key={KEY}&lat={lat}&lon={lon}&format=json";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var r = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LocationiqResponse>(r);
                gpsStatement.Address = result.display_name;
                gpsStatement.Region = result.address.region;
                gpsStatement.State = result.address.state;
            }

        }
        public async Task<dynamic> ReverseGeoCoding(double Lat, double Long)
        {
            var key =
                "_Txch3gEncyKpVVy7Nal709W29yUXqgDZR72wlclSY93W49CnVDekJZtlA2VKw2cFvspcSbq94sRDD3Kpv6nYs3mXYNhnymakBp2czyjm8OVZWKfF3O8lQfemtG6t6Lnco1whptJcVmiwF6KDCOLjw..";
            var lat = Lat.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            var lon = Long.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            var client = new HttpClient();
            var url = $"http://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lon}&zoom=18&addressdetails=1";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var r = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LocationiqResponse>(r);
            }

            return null;

        }
        public  async Task<NominatimResult> ExecuteQuery(double lat, double lng)
        {
            var client = new RestClient();
            var _lat =lat. ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            var _lon = lng.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            count++;
            Debug.WriteLine(count);
            var url = $"https://nominatim.openstreetmap.org/reverse.php?format=json&lat={_lat}&lon={_lon}";
            var request = new RestRequest(Method.GET);
            //request.Resource = "wsRest/wsServerArticle/getArticle";
            client.BaseUrl = new System.Uri(url);
            try
            {
                
                var response = await client.ExecutePostTaskAsync<NominatimResult>(request);
                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response.  Check inner details for more info.";
                    throw new ApplicationException(message, response.ErrorException);
                }
                else return (NominatimResult)response.Data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

    }
}
