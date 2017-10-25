using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProvider_IN : AddressLookupProviderBase
    {
        private static readonly Dictionary<string, List<string>> MajorCityDict = new Dictionary<string, List<string>>
        {
            {"GJ", new List<string> { "Major City:Ahmedabad" }},
            {"OR", new List<string> { "Major City:Bhubaneshwar", "Major City:Cuttack" }},
            {"TN", new List<string> { "Major City:Chennai" }},
            {"KA", new List<string> { "Major City:Bangalore City", "Major City:Bangalore Rural" }},
            {"DL", new List<string> { "Major City:Delhi" }},
            {"HR", new List<string> { "Major City:Gurgaon" }},
            {"AS", new List<string> { "Major City:Guwahati" }},
            {"MN", new List<string> { "Major City:Imphal" }},
            {"BR", new List<string> { "Major City:Patna" }},
            {"MP", new List<string> { "Major City:Indore" }},
            {"RJ", new List<string> { "Major City:Jaipur" }},
            {"WB", new List<string> { "Major City:Kolkata" }},
            {"UP", new List<string> { "Major City:Lucknow" }},
            {"PJ", new List<string> { "Major City:Ludhiana" }},
            {"MH", new List<string> { "Major City:Mumbai", "Major City:Pune" }},
            {"CG", new List<string> { "Major City:Bhilai", "Major City:Durg", "Major City:Raipur" }},
            {"AP", new List<string> { "Major City:Hyderabad", "Major City:Vijayawada" }},
            {"UC", new List<string> { "Major City:Dehradun" }},
        };
        public override List<string> GetCitiesForState(string country, string state)
        {
            //1) get actual list from DB
            List<string> actualCityList = base.GetCitiesForState(country, state);

            if (MajorCityDict.ContainsKey(state))
            {
                //2) special-sort them with major-city on top of the list
                var finalList = new List<string>();
                finalList.AddRange(MajorCityDict[state]);

                var withoutMajorList =
                    (from a in actualCityList where !finalList.Contains(a) select a).ToList();
                finalList.AddRange(withoutMajorList);
                return finalList;
            }
            return actualCityList;
        }

        public Dictionary<string, List<string>> MajorCities
        {
            get { return MajorCityDict; }
        }
    }
}
