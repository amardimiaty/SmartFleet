using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SmartFLEET.Web.Areas.Administrator.Models
{
    public class AddCustomerViewModel
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Tel { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        [Required]
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public int CustomerStatus { get; set; }
        public List<string> Countries => CountriesList();
        public static List<string> CountriesList()
        {

            List<string> CountryList = new List<string>();
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (!(CountryList.Contains(R.EnglishName)))
                {
                    CountryList.Add(R.EnglishName);
                }
            }

            CountryList.Sort();
            return CountryList;
        }
    }
}