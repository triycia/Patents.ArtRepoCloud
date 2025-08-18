namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class CorrespondenceAddress
    {
        public CorrespondenceAddress(){}

        public CorrespondenceAddress(
            DateTime changedDate, 
            string correspondenceName, 
            string correspondenceAddress, 
            string customerNumber)
        {
            ChangedDate = changedDate;
            CorrespondenceName = correspondenceName;
            Address = correspondenceAddress;
            CustomerNumber = customerNumber;
        }

        public DateTime ChangedDate { get; private set; }
        public string CorrespondenceName { get; private set; }
        public string Address { get; private set; }
        public string CustomerNumber { get; private set; }
    }
}