namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class Claim
    {
        private Claim(){}

        public Claim(int number, string text, bool isIndependent, bool isCanceled)
        {
            Number = number;
            Text = text;
            IsIndependent = isIndependent;
            IsCanceled = isCanceled;
        }

        public int Number { get; private set; }
        public string Text { get; private set; }
        public bool IsIndependent { get; private set; }
        public bool IsCanceled { get; private set; }
    }
}