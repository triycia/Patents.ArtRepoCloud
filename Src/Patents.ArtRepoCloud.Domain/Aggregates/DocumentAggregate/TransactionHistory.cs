namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class TransactionHistory
    {
        public TransactionHistory(){}

        public TransactionHistory(
            DateTime transactionDate, 
            string transactionDescription)
        {
            TransactionDate = transactionDate;
            TransactionDescription = transactionDescription;
        }

        public TransactionHistory(
            DateTime changedDate, 
            int sequence, 
            DateTime transactionDate, 
            string transactionDescription,
            DateTime createdDate)
        {
            ChangedDate = changedDate;
            Sequence = sequence;
            TransactionDate = transactionDate;
            TransactionDescription = transactionDescription;
            CreatedDate = createdDate;
        }

        public DateTime ChangedDate { get; private set; }
        public int Sequence { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public string TransactionDescription { get; private set; }
        public DateTime CreatedDate { get; private set; }
    }
}