using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.ValueObjects.PairData;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocument
{
    public class SavePairDocumentCommand : IRequest<SavePairDocumentCommandResult>
    {
        public SavePairDocumentCommand(
            ReferenceNumber referenceNumber, 
            ApplicationDataDto? applicationData, 
            ContinuityDto? continuity, 
            TransactionsDto? transactions, 
            AttorneyAgentsDto? attorneyAgents)
        {
            ReferenceNumber = referenceNumber;
            ApplicationData = applicationData;
            Continuity = continuity;
            Transactions = transactions;
            AttorneyAgents = attorneyAgents;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ApplicationDataDto? ApplicationData { get; }
        public ContinuityDto? Continuity { get; }
        public TransactionsDto? Transactions { get; }
        public AttorneyAgentsDto? AttorneyAgents { get; }
    }
}