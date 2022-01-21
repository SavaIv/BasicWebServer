using System.Collections;

namespace BasicWebServer.Server.HTTP
{
    public class HeaderCollection : IEnumerable<Header>
    {
        private readonly Dictionary<string, Header> headers;

        public HeaderCollection()
            => this.headers = new Dictionary<string, Header>();

        // Indexers allow instances of a class to be indexed just like arrays and the indexed value can be set
        // or retrieved without explicitly specifying a type or instance member
        public string this[string name]
            => this.headers[name].Value;

        // това е същото като горното
        //public string this[string name]
        //{
        //    get
        //    {
        //        return headers[name].Value;
        //    }
        //}

        public int Count => this.headers.Count;

        // the Contains(string name) method will return whether there is a header with the given name.
        public bool Contains(string name) => this.headers.ContainsKey(name);

        // Add(string name, string value) метода ще ползва indexer – this way if the header already exists it won't
        // be added to the collection, which prevents you from duplicating headers.
        public void Add(string name, string value)
            => this.headers[name] = new Header(name, value);

        public IEnumerator<Header> GetEnumerator()
            =>this.headers.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
