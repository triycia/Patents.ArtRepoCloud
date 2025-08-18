using System.Diagnostics;
using Polly;
using Polly.Retry;

namespace Patents.ArtRepoCloud.Service.Code.ArtRepoRetryPolicy
{
    public class AsyncArtRepoRetryPolicy<TResult> : AsyncPolicy<TResult>, IRetryPolicy<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> _onRetryAsync;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

        internal AsyncArtRepoRetryPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync ?? throw new ArgumentNullException(nameof(onRetryAsync));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncArtRepoRetryEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _onRetryAsync,
                _permittedRetryCount,
                _sleepDurationsEnumerable,
                _sleepDurationProvider,
                continueOnCapturedContext
            );
    }
}