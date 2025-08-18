using Patents.ArtRepoCloud.GraphService.Enums;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand
{
    public class EnqueueBatchCommandResult
    {
        public EnqueueBatchCommandResult(ReferenceEnqueueStatus[] statuses, bool isSuccess = true)
        {
            Statuses = statuses;
            IsSuccess = isSuccess;
        }

        public ReferenceEnqueueStatus[] Statuses { get; }
        public bool IsSuccess { get; }

        public class ReferenceEnqueueStatus
        {
            public ReferenceEnqueueStatus(string referenceNumber, EnqueueStatus status)
            {
                ReferenceNumber = referenceNumber;
                Status = status;
            }

            public string ReferenceNumber { get; }
            public EnqueueStatus Status { get; }
        }
    }
}