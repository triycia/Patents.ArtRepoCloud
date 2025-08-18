using Patents.ArtRepoCloud.Domain.Enums;
using MediatR;
using static Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand.EnqueueBatchCommand;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand
{
    public class EnqueueBatchCommand : IRequest<EnqueueBatchCommandResult>
    {
        public EnqueueBatchCommand(IEnumerable<BatchItem> enqueueBatches, ImportSource? source, bool? useSourceStrictly, int? userId)
        {
            EnqueueBatches = enqueueBatches;
            Source = source ?? ImportSource.All;
            UseSourceStrictly = useSourceStrictly ?? false;
            UserId = userId;
        }

        public IEnumerable<BatchItem> EnqueueBatches { get; }
        public ImportSource? Source { get; }
        public bool? UseSourceStrictly { get; }
        public int? UserId { get; }

        public class BatchItem
        {
            public BatchItem(string referenceNumber, ImportPriority? priority, bool? rescrape)
            {
                ReferenceNumber = referenceNumber;
                Priority = priority ?? ImportPriority.Normal;
                Rescrape = rescrape ?? false;
            }

            public string ReferenceNumber { get; }
            public ImportPriority? Priority { get; }
            public bool? Rescrape { get; }
        }
    }
}