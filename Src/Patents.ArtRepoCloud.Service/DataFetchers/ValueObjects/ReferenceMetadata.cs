namespace Patents.ArtRepoCloud.Service.DataFetchers.ValueObjects
{
    public interface IReferenceMetadata
    {
        public string? Country { get; }
        public DateTime? Date { get; }
        public string? Number { get; }
        public string? Kind { get; }
        public string? Language { get; }
    }

    public class ReferenceMetadata : IReferenceMetadata
    {
        public string? Country { get; private set; }
        public DateTime? Date { get; private set; }
        public string? Number { get; private set; }
        public string? Kind { get; private set; }
        public string? Language { get; private set; }
        public string? Series { get; private set; }

        public void SetCountry(string? country)
        {
            Country = country;
        }

        public void SetNumber(string? number)
        {
            Number = number;
        }

        public void SetKind(string? kind)
        {
            Kind = kind;
        }

        public void SetLanguage(string? language)
        {
            Language = language;
        }

        public void SetSeries(string? series)
        {
            Series = series;
        }

        public void SetDate(DateTime? date)
        {
            Date = date;
        }
    }
}