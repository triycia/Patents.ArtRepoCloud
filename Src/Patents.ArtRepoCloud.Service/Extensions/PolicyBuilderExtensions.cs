using System.Net;
using Patents.ArtRepoCloud.Service.Code.ArtRepoRetryPolicy;
using Polly;
using Polly.Retry;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class PolicyBuilderExtensions
    {
        public static AsyncArtRepoRetryPolicy<TResult> CipWaitAndRetryAsync<TResult>(
            this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount),
                    "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync = (_, __, ___, ____) =>
                Task.CompletedTask;

            return new AsyncArtRepoRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, outcome, ctx)
            );
        }

        public static AsyncArtRepoRetryPolicy<TResult> CipWaitAndRetryAsync<TResult>(
            this PolicyBuilder<TResult> policyBuilder, int retryCount,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider,
            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount),
                    "Value must be greater than or equal to zero.");
            if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
            if (onRetryAsync == null) onRetryAsync = (_, __, ___, ____) => Task.CompletedTask;

            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> doNothing = (_, __, ___, ____) =>
                Task.CompletedTask;

            //IEnumerable<TimeSpan> sleepDurations = Enumerable.Range(1, retryCount)
            //    .Select(sleepDurationProvider);

            return new AsyncArtRepoRetryPolicy<TResult>(
                policyBuilder,
                onRetryAsync,
                retryCount,
                sleepDurationProvider: (i, outcome, ctx) => sleepDurationProvider(i, outcome, ctx)
            );
        }
    }


    public class UsptoPolicy<TResult> : AsyncPolicy<TResult>, IRetryPolicy<TResult>
    {
        private readonly int _timeoutInMilliSecs;

        private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

        internal UsptoPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        }

        protected override async Task<TResult> ImplementationAsync(
            Func<Context, CancellationToken, Task<TResult>> action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var delegateTask = action(context, cancellationToken);
            var timeoutTask = Task.Delay(_timeoutInMilliSecs);

            await Task.WhenAny(delegateTask, timeoutTask).ConfigureAwait(continueOnCapturedContext);

            if (timeoutTask.IsCompleted)
            {
                //throw new DuongTimeoutException(
                //    $"{context.OperationKey}: Task did not complete within: {_timeoutInMilliSecs}ms");
            }

            return await delegateTask;
        }
    }
}