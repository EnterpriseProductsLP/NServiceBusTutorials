using System;
using System.Collections.Generic;

namespace NServiceBusTutorials.StepByStepExample.Domain
{
    public static class ProductBuilder
    {
        private static readonly object _nextProductLock;
        private static readonly IList<Product> _products;
        private static readonly Random _random;

        static ProductBuilder()
        {
            _nextProductLock = new object();
            _products = new List<Product>();
            _random = new Random(DateTime.Now.Millisecond);
            foreach (var productName in GetProductNames())
            {
                _products.Add(new Product(Guid.NewGuid(), productName));
            }
        }

        public static Product NextProduct()
        {
            lock (_nextProductLock)
            {
                var productIndex = _random.Next(0, _products.Count - 1);
                return _products[productIndex];
            }
        }

        private static IEnumerable<string> GetProductNames()
        {
            return new List<string>
            {
                "Ap Sansing",
                "Apsunin",
                "Bigcore",
                "Biois",
                "Biotom",
                "Conlex",
                "Dentoozeplus",
                "Doublefind",
                "Fin-String",
                "Finwarm",
                "Funhome",
                "Geo Tip",
                "Grooveair",
                "Groovecom",
                "Holdeco",
                "Holdhome",
                "Hotsonair",
                "Ice Tough",
                "Inch Tone",
                "Is Lab",
                "It-Soft",
                "Jaytip",
                "K--Tex",
                "Konksing",
                "Lightlex",
                "Meddinis",
                "Namsing",
                "Onto Keynix",
                "Onto-Dax",
                "Ontohold",
                "Ope-Ex",
                "Opencom",
                "Opetech",
                "Opezenron",
                "Ozer La",
                "Pluslab",
                "Red Hotdex",
                "Saoin",
                "Scotstrong",
                "Silzap",
                "Singlab",
                "Singlelam",
                "Softflex",
                "Solootstrong",
                "Sonit",
                "Spanphase",
                "Strongcore",
                "Sublam",
                "Tinstock",
                "Top Sancof",
                "Touchkix",
                "Toughtouch",
                "Trans-Sing",
                "Tripplela",
                "Tris Ing",
                "Trisdanlex",
                "Trust Tam",
                "U-tone",
                "Una-La",
                "Vaia Antop",
                "Vaia Fax",
                "Ventogoex",
                "Viladax",
                "Villaquolam",
                "Volfix",
                "Voltdom",
                "Warmronron",
                "White Jaystock",
                "Y-hold",
                "Year Sing",
                "Zathlux",
                "Zen Kix",
                "Zerlight",
                "Zonetanhome",
                "Zootech",
                "Zotstring",
                "Zumma-Ity",
                "Zumsing",
                "Zunex"
            };
        }
    }
}