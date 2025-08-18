using System.ComponentModel.DataAnnotations.Schema;
using Patents.ArtRepoCloud.Domain.Extensions;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class PairData
    {
        private List<ChildContinuity> _childContinuities;
        private List<ChildContinuity> _parentContinuities;
        private List<TransactionHistory> _transactionHistories;
        private List<CorrespondenceAddress> _correspondenceAddresses;
        private List<PairFileData> _pairFiles;

        public PairData(){}

        public PairData(
            string? titleOfInvention,
            DateTime changedDate,
            string? applicationType,
            string? examinerName,
            string? groupArtUnit,
            string? confirmationNumber,
            string? attorneyDocketNumber,
            string? classSubClass,
            string? firstNamedInventor,
            string? customerNumber,
            string? status,
            string? previousStatus,
            DateTime? statusDate,
            string? location,
            DateTime? locationDate,
            string? earliestPublicationNumber,
            DateTime? earliestPublicationDate)
        {
            TitleOfInvention = titleOfInvention;
            ChangedDate = changedDate;
            ApplicationType = applicationType;
            ExaminerName = examinerName;
            GroupArtUnit = groupArtUnit;
            ConfirmationNumber = confirmationNumber;
            AttorneyDocketNumber = attorneyDocketNumber;
            ClassSubClass = classSubClass;
            FirstNamedInventor = firstNamedInventor;
            CustomerNumber = customerNumber;
            Status = status;
            PreviousStatus = previousStatus;
            StatusDate = statusDate;
            Location = location;
            LocationDate = locationDate;
            EarliestPublicationNumber = earliestPublicationNumber;
            EarliestPublicationDate = earliestPublicationDate;
        }

        public string? TitleOfInvention { get; private set; }
        public DateTime ChangedDate { get; private set; }
        public string? ApplicationType { get; private set; }
        public string? ExaminerName { get; private set; }
        public string? GroupArtUnit { get; private set; }
        public string? ConfirmationNumber { get; private set; }
        public string? AttorneyDocketNumber { get; private set; }
        public string? ClassSubClass { get; private set; }
        public string? FirstNamedInventor { get; private set; }
        public string? CustomerNumber { get; private set; }
        public string? Status { get; private set; }
        public string? PreviousStatus { get; private set; }
        public DateTime? StatusDate { get; private set; }
        public string? Location { get; private set; }
        public DateTime? LocationDate { get; private set; }
        public string? EarliestPublicationNumber { get; private set; }
        public DateTime? EarliestPublicationDate { get; private set; }
        public List<ChildContinuity> ChildContinuities => _childContinuities;
        public List<ChildContinuity> ParentContinuities => _parentContinuities;
        public List<TransactionHistory> TransactionHistories => _transactionHistories;
        public List<CorrespondenceAddress> CorrespondenceAddresses => _correspondenceAddresses;
        public List<PairFileData> PairFiles => _pairFiles;

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

        public void SetApplicationType(string value)
        {
            if (ApplicationType != value)
            {
                ApplicationType = value;
                IsMutated = true;
            }
        }

        public void SetExaminerName(string? value)
        {
            if (!ExaminerName.Compare(value))
            {
                ExaminerName = value;
                IsMutated = true;
            }
        }

        public void SetGroupArtUnit(string? value)
        {
            if (GroupArtUnit != value)
            {
                GroupArtUnit = value;
                IsMutated = true;
            }
        }

        public void SetConfirmationNumber(string? value)
        {
            if (ConfirmationNumber != value)
            {
                ConfirmationNumber = value;
                IsMutated = true;
            }
        }

        public void SetAttorneyDocketNumber(string? value)
        {
            if (AttorneyDocketNumber != value)
            {
                AttorneyDocketNumber = value;
                IsMutated = true;
            }
        }

        public void SetClassSubClass(string? value)
        {
            if (ClassSubClass != value)
            {
                ClassSubClass = value;
                IsMutated = true;
            }
        }

        public void SetFirstNamedInventor(string? value)
        {
            if (!FirstNamedInventor.Compare(value))
            {
                FirstNamedInventor = value;
                IsMutated = true;
            }
        }

        public void SetStatus(string? value)
        {
            if (!Status.Compare(value))
            {
                Status = value;

                if (string.IsNullOrEmpty(PreviousStatus))
                {
                    PreviousStatus = value;
                }

                IsMutated = true;
            }
        }

        public void SetStatusDate(DateTime? value)
        {
            if (!StatusDate.Equals(value))
            {
                StatusDate = value;
                IsMutated = true;
            }
        }

        public void SetLocation(string? value)
        {
            if (!Location.Compare(value))
            {
                Location = value;
                IsMutated = true;
            }
        }

        public void SetLocationDate(DateTime? value)
        {
            if (!LocationDate.Equals(value))
            {
                LocationDate = value;
                IsMutated = true;
            }
        }

        public void SetEarliestPublicationNumber(string? value)
        {
            if (EarliestPublicationNumber != value)
            {
                EarliestPublicationNumber = value;
                IsMutated = true;
            }
        }

        public void SetEarliestPublicationDate(DateTime? value)
        {
            if (!EarliestPublicationDate.Equals(value))
            {
                EarliestPublicationDate = value;
                IsMutated = true;
            }
        }

        public void SetTitleOfInvention(string? value)
        {
            if (!TitleOfInvention.Compare(value))
            {
                TitleOfInvention = value;
                IsMutated = true;
            }
        }

        public void SetCustomerNumber(string customerNumber)
        {
            CustomerNumber = customerNumber;
        }

        public void AddPairFile(PairFileData file)
        {
            _pairFiles.Add(file);
        }
    }
}