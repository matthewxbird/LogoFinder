namespace LogoFinderConsole
{
    public class Merchant
    {
        public Merchant()
        {

        }

        public Merchant(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }

        public string Name { get; set; }
        public string Uri { get; set; }
    }
}
