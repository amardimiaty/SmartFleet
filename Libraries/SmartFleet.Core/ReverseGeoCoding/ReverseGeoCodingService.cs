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
        private string[] user_agents = { "Mozilla/4.0 (Mozilla/4.0; MSIE 7.0; Windows NT 5.1; FDM; SV1)"
            , "Mozilla/4.0 (Mozilla/4.0; MSIE 7.0; Windows NT 5.1; FDM; SV1; .NET CLR 3.0.04506.30)",
            "Mozilla/4.0 (Windows; MSIE 7.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)",
            "Mozilla/4.0 (Windows; U; Windows NT 5.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.33 Safari/532.0",
            "Mozilla/4.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/525.19 (KHTML, like Gecko) Chrome/1.0.154.59 Safari/525.19",
            "Mozilla/4.0 (compatible; MSIE 6.0; Linux i686 ; en) Opera 9.70",
            "Mozilla/4.0 (compatible; MSIE 6.0; Mac_PowerPC; en) Opera 9.24",
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; de) Opera 9.50",
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en) Opera 9.24",
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en) Opera 9.26",
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; es-la) Opera 9.27",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; YPC 3.2.0; SLCC1; .NET CLR 2.0.50727; .NET CLR 3.0.04506)"
        };
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
        public async Task<string> ReverseGeoCoding(double Lat, double Long)
        {
             var lat = Lat.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            var lon = Long.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
            var client = new HttpClient();
            var url = $"https://us1.locationiq.com/v1/reverse.php?key={KEY}&lat={lat}&lon={lon}&format=json";
            var rd = new Random();
     
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var r = await response.Content.ReadAsStringAsync();
                var ressult= JsonConvert.DeserializeObject<LocationiqResponse>(r);
                return ressult.display_name;
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
            var rd = new Random();
            client.UserAgent = user_agents[rd.Next(0, user_agents.Length)]; 
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
