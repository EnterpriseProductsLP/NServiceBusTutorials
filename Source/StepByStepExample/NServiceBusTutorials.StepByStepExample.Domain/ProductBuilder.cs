using System;
using System.Collections.Generic;

namespace NServiceBusTutorials.StepByStepExample.Domain
{
    public static class ProductBuilder
    {
        private static readonly object NextProductLock;
        private static readonly IList<Product> Products;
        private static readonly Random Random;

        static ProductBuilder()
        {
            NextProductLock = new object();
            Products = new List<Product>();
            Random = new Random(DateTime.Now.Millisecond);
            foreach (var productName in GetProductNames())
            {
                Products.Add(new Product(Guid.NewGuid(), productName));
            }
        }

        public static Product NextProduct()
        {
            lock (NextProductLock)
            {
                var productIndex = Random.Next(0, Products.Count - 1);
                return Products[productIndex];
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