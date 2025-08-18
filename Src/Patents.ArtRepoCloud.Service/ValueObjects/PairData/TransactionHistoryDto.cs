namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class TransactionHistoryDto
    {
        public TransactionHistoryDto(
            DateTime transactionDate, 
            string transactionDescription, 
            int sequence)
        {
            TransactionDate = transactionDate;
            TransactionDescription = transactionDescription;
            Sequence = sequence;
        }

        public DateTime TransactionDate { get; }
        public string TransactionDescription { get; }
        public int Sequence { get; }
    }
}