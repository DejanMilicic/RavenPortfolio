namespace RavenPortfolio
{
    // portfolio_id_1, 1.1.2023, GOOG, 12, 12, 215, factors...

    public class Portfolio
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }

        // load testing: 3.000 symbols for a specific date
        public List<Entry> Entries = new List<Entry>();
        
        public class Entry
        {
            public string Symbol { get; set; }

            public decimal Price { get; set; }

            public int Quantity { get; set; }
            
            public int Factor1 { get; set; }
            
            public int Factor2 { get; set; }
            
            public int Factor3 { get; set; }
        }
    }
}
