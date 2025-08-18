namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiImageXmlItem
    {
        public IfiImageXmlItem(string file, string orientation)
        {
            this.File = file;
            this.Orientation = string.IsNullOrWhiteSpace(orientation) ? "landscape" : orientation;
        }
        public string File { get; }
        public string Orientation { get; }
    }
}