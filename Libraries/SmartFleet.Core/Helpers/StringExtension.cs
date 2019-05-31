using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFleet.Core.Helpers
{
    public static class StringExtension
    {
        public static DateTime ParseToDate(this String date)
        {
            try
            {
                var array = date.Split('-');
                return new DateTime(Convert.ToInt32(array[0]) , Convert.ToInt32(array[1]), Convert.ToInt32(array.Last().Split(':')[0])/100);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
