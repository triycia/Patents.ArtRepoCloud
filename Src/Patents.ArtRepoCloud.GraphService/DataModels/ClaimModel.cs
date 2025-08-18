namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class ClaimModel
    {
        public ClaimModel(int number, string text, bool isIndependent, bool isCanceled)
        {
            Number = number;
            Text = text;
            IsIndependent = isIndependent;
            IsCanceled = isCanceled;
        }

        public int Number { get; }
        public string Text { get; }
        public bool IsIndependent { get; }
        public bool IsCanceled { get; }
    }
}