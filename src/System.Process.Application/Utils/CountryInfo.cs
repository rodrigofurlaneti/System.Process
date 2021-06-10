using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Process.Application.Utils
{
    public struct CountryInfo
    {
        public string ShortTwo { get; set; }
        public string ShortThree { get; set; }
        public string FullName { get; set; }

        private static Dictionary<string, CountryInfo> CountryDictionary = new Dictionary<string, CountryInfo>
        {
            { "BR", new CountryInfo() { ShortTwo = "BR", ShortThree = "BRL", FullName = "Brasil" } },
            { "USA", new CountryInfo() { ShortTwo = "US", ShortThree = "USA", FullName = "United States" } }
        };

        public static CountryInfo GetCountryInfo(string value)
        {
            var country = CountryDictionary.FirstOrDefault(x => x.Value.ShortTwo == value || x.Value.ShortThree == value || x.Value.FullName == value);
            if (!String.IsNullOrEmpty(country.Value.ShortTwo))
            {
                return country.Value;
            }

            return new CountryInfo();
        }
    }
}
