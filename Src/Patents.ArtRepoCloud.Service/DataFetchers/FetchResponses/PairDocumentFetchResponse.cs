using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.ValueObjects.PairData;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public class PairDocumentFetchResponse : IFetchResponse
    {
        public PairDocumentFetchResponse(
            ApplicationDataDto? applicationData, 
            ContinuityDto? continuity, 
            TransactionsDto? transactions, 
            AttorneyAgentsDto? attorneyAgents, 
            ReferenceNumber referenceNumber, 
            HttpReasonCode httpReasonCode, ImportSource source)
        {
            ApplicationData = applicationData;
            Continuity = continuity;
            Transactions = transactions;
            AttorneyAgents = attorneyAgents;
            ReferenceNumber = referenceNumber;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public PairDocumentFetchResponse(
            ReferenceNumber referenceNumber, 
            HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public ApplicationDataDto? ApplicationData { get; }
        public ContinuityDto? Continuity { get; }
        public TransactionsDto? Transactions { get; }
        public AttorneyAgentsDto? AttorneyAgents { get; }
        public ReferenceNumber ReferenceNumber { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}