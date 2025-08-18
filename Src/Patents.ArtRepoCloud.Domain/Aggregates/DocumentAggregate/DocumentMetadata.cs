using Patents.ArtRepoCloud.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentMetadata
    {
        public DocumentMetadata() {}
        public DocumentMetadata(DataSource? documentDataSource, string? dataSourceReferenceNumber, DateTime dateCreated)
        {
            DateCreated = dateCreated;
            DateModified = dateCreated;
            DocumentDataSource = documentDataSource ?? DataSource.Unknown;
            DataSourceReferenceNumber = dataSourceReferenceNumber;
        }

        [JsonConstructor]
        public DocumentMetadata(
            string? dataSourceReferenceNumber, 
            DateTime dateModified, 
            DateTime dateCreated, 
            DataSource documentDataSource)
        {
            DataSourceReferenceNumber = dataSourceReferenceNumber;
            DateModified = dateModified;
            DateCreated = dateCreated;
            DocumentDataSource = documentDataSource;
        }

        public string? DataSourceReferenceNumber { get; private set; }
        public DateTime DateModified { get; private set; }
        public DateTime DateCreated { get; private set; }
        public DataSource DocumentDataSource { get; private set; }

        public void SetDataSourceReferenceNumber(string value)
        {
            DataSourceReferenceNumber = value;
        }

        public void SetModified()
        {
            DateModified = DateTime.Now;
        }

        public void SetDataSource(DataSource source)
        {
            DocumentDataSource = source;
        }
    }
}