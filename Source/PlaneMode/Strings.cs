using System;
using System.Collections.Generic;
using System.Threading;

namespace PlaneMode
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
        }

        public static string Plane
        {
            get { return Lookup["Plane"]; }
        }

        public static string PlaneMode
        {
            get { return Lookup["Plane Mode"]; }
        }

        public static string RocketMode
        {
            get { return Lookup["Rocket Mode"]; }
        }

        public static string SwitchToAirplaneMode
        {
            get { return Lookup["Switch to Plane Mode"]; }
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

            // ReSharper disable once UnusedMember.Local
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
