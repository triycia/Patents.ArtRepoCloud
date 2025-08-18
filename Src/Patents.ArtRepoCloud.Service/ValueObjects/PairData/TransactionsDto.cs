namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class TransactionsDto
    {
        public TransactionsDto(IEnumerable<TransactionHistoryDto> transactions)
        {
            Transactions = transactions;
        }

        public IEnumerable<TransactionHistoryDto> Transactions { get; }
    }
}