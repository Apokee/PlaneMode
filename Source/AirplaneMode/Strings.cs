using System;
using System.Collections.Generic;
using System.Threading;

namespace AirplaneMode
{
    internal static class Strings
    {
        private static readonly StringLookup Lookup = new StringLookup();

        public static string Culture
        {
            get
            {
                return Lookup.LookupCulture;
            }

            set
            {
                Lookup.LookupCulture = value;
            }
        }

        static Strings()
        {
            Lookup.Add("Airplane", "en-AU", "Aeroplane");
            Lookup.Add("Airplane", "en-NZ", "Aeroplane");
            Lookup.Add("Airplane", "en-UK", "Aeroplane");
            Lookup.Add("Airplane", "en-ZA", "Aeroplane");

            Lookup.Add("Airplane Mode", "en-AU", "Aeroplane Mode");
            Lookup.Add("Airplane Mode", "en-NZ", "Aeroplane Mode");
            Lookup.Add("Airplane Mode", "en-UK", "Aeroplane Mode");
            Lookup.Add("Airplane Mode", "en-ZA", "Aeroplane Mode");

            Lookup.Add("Switch to Airplane Mode", "en-AU", "Switch to Aeroplane Mode");
            Lookup.Add("Switch to Airplane Mode", "en-NZ", "Switch to Aeroplane Mode");
            Lookup.Add("Switch to Airplane Mode", "en-UK", "Switch to Aeroplane Mode");
            Lookup.Add("Switch to Airplane Mode", "en-ZA", "Switch to Aeroplane Mode");
        }

        public static string Airplane
        {
            get { return Lookup["Airplane"]; }
        }

        public static string AirplaneMode
        {
            get { return Lookup["Airplane Mode"]; }
        }

        public static string RocketMode
        {
            get { return Lookup["Rocket Mode"]; }
        }

        public static string SwitchToAirplaneMode
        {
            get { return Lookup["Switch to Airplane Mode"]; }
        }

        public static string SwitchToRocketMode
        {
            get { return Lookup["Switch to Rocket Mode"]; }
        }

        private sealed class StringLookup
        {
            private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

            public string this[string phrase]
            {
                get
                {
                    string value;
                    return _store.TryGetValue(Key(phrase, LookupCulture), out value) ? value : phrase;
                }
            }

            public string LookupCulture { get; set; }

            public StringLookup()
            {
                LookupCulture = Thread.CurrentThread.CurrentUICulture.Name;
            }

            public void Add(string phrase, string culture, string culturePhrase)
            {
                _store.Add(Key(phrase, culture), culturePhrase);
            }

            private static string Key(string phrase, string culture)
            {
                return String.Format("{0}:{1}", culture, phrase);
            }
        }
    }
}
