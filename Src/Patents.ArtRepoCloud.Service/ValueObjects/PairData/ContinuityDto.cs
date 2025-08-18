namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class ContinuityDto
    {
        public ContinuityDto(IEnumerable<Continuity> parentContinuities, IEnumerable<Continuity> childContinuities)
        {
            ParentContinuities = parentContinuities;
            ChildContinuities = childContinuities;
        }

        public IEnumerable<Continuity> ParentContinuities { get; }
        public IEnumerable<Continuity> ChildContinuities { get; }

        public class Continuity
        {
            public Continuity(
                string parentApplicationNumberText, 
                string childApplicationNumberText, 
                string? status, 
                string? parentDescription,
                DateTime? filingDate, 
                int sequence)
            {
                ParentApplicationNumberText = parentApplicationNumberText;
                ChildApplicationNumberText = childApplicationNumberText;
                Status = status;
                ParentDescription = parentDescription;
                FilingDate = filingDate;
                Sequence = sequence;
            }

            public string ParentApplicationNumberText { get; }
            public string ChildApplicationNumberText { get; }
            public string? Status { get; }
            public string? ParentDescription { get; }
            public DateTime? FilingDate { get; }
            public int Sequence { get; }
        }
    }
}