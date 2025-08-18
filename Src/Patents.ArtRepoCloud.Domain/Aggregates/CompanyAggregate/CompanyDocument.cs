namespace Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate
{
    public class CompanyDocument
    {
        public CompanyDocument() { }

        public CompanyDocument(string referenceNumber, int companyId)
        {
            ReferenceNumber = referenceNumber;
            CompanyId = companyId;
        }

        public string ReferenceNumber { get; private set; }
        public int CompanyId { get; private set; }

        public virtual Company Company { get; private set; }
    }
}
