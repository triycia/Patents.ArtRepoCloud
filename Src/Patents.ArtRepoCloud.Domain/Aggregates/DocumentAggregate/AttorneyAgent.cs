namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class AttorneyAgent
    {
        public AttorneyAgent(){}

        public AttorneyAgent(
            DateTime changedDate, 
            int? sequence, 
            string regNumber, 
            string attorneyName, 
            string phone)
        {
            ChangedDate = changedDate;
            Sequence = sequence;
            RegNumber = regNumber;
            AttorneyName = attorneyName;
            Phone = phone;
        }

        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public DateTime ChangedDate { get; private set; }
        public int? Sequence { get; private set; }
        public string RegNumber { get; private set; }
        public string AttorneyName { get; private set; }
        public string Phone { get; private set; }
    }
}