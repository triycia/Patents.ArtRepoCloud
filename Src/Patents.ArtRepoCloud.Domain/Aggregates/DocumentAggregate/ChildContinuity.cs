using System.ComponentModel.DataAnnotations.Schema;
using Patents.ArtRepoCloud.Domain.Extensions;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class ChildContinuity
    {
        public ChildContinuity(){}

        public ChildContinuity(
            string filedApplicationNumber, 
            DateTime? filedDate, 
            string? status, 
            string benefitApplicationNumber,
            int parentApplicationId,
            DateTime? parentFiledDate, 
            string? parentStatus, 
            string? parentDescription,
            DateTime changedDate,
            int? sequence)
        {
            FiledApplicationNumber = filedApplicationNumber;
            FiledDate = filedDate;
            Status = status;
            BenefitApplicationNumber = benefitApplicationNumber;
            ParentApplicationId = parentApplicationId;
            ParentFiledDate = parentFiledDate;
            ParentStatus = parentStatus;
            ParentDescription = parentDescription;
            ChangedDate = changedDate;
            Sequence = sequence;
        }

        public int? Sequence { get; private set; }
        public string FiledApplicationNumber { get; private set; }
        public DateTime? FiledDate { get; private set; }
        public string? Status { get; private set; }
        public string BenefitApplicationNumber { get; private set; }
        public DateTime ChangedDate { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public int ParentApplicationId { get; private set; }
        public DateTime? ParentFiledDate { get; private set; }
        public string? ParentStatus { get; private set; }
        public string? ParentDescription { get; private set; }

        [NotMapped]
        private bool _isMutated;

        [NotMapped]
        public bool IsMutated
        {
            get => _isMutated;
            private set
            {
                _isMutated = value;
                ChangedDate = DateTime.Now;
            }
        }

        public void SetStatus(string? value)
        {
            if (!Status.Compare(value))
            {
                Status = value;
            }
        }

        public void SetParentStatus(string? value)
        {
            if (!ParentStatus.Compare(value))
            {
                ParentStatus = value;
            }
        }

        public void SetFiledDate(DateTime? value)
        {
            if (!FiledDate.Equals(value))
            {
                FiledDate = value;
                IsMutated = true;
            }
        }

        public void SetParentDescription(string? value)
        {
            if (!ParentDescription.Compare(value))
            {
                ParentDescription = value;
            }
        }
    }
}