using Patents.ArtRepoCloud.Domain.Enums;
using FluentValidation;
using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand;

namespace Patents.ArtRepoCloud.GraphService.Application.Validations
{
    public class EnqueueBatchCommandValidator : AbstractValidator<EnqueueBatchCommand>
    {
        public EnqueueBatchCommandValidator()
        {
            RuleForEach(x => x.EnqueueBatches).NotEmpty().WithMessage("No references provided for enqueue.");
            RuleForEach(x => x.EnqueueBatches).SetValidator(new BatchItemValidator());

            When(r => !r.Source.Equals(ImportSource.Uspto), () =>
            {
                RuleForEach(x => x.EnqueueBatches).SetValidator(new BatchItemValidator());
            });

            When(r => r.Source.Equals(ImportSource.Uspto), () =>
            {
                RuleForEach(x => x.EnqueueBatches).SetValidator(new UsptoBatchItemValidator());
            });
        }
    }

    public class BatchItemValidator : AbstractValidator<EnqueueBatchCommand.BatchItem>
    {
        public BatchItemValidator()
        {
            RuleFor(r => r.ReferenceNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage("Reference Number must have a value.");
            RuleFor(r => r.ReferenceNumber)
                .Matches(@"^[a-zA-Z]{2}[A-Z0-9]{1,12}")
                .MinimumLength(3)
                .MaximumLength(16)
                .WithMessage("Reference Number must start with two letters country code followed by 1 - 12 digits.");
        }
    }

    public class UsptoBatchItemValidator : AbstractValidator<EnqueueBatchCommand.BatchItem>
    {
        public UsptoBatchItemValidator()
        {
            RuleFor(r => r.ReferenceNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage("Reference Number must have a value.");
            RuleFor(r => r.ReferenceNumber)
                .Matches(
                    Regex.Replace(@"^[0-9]{2}[/]{1}[0-9]{3}[,]{1}[0-9]{3}$|
                                            ^[0-9]{2}[0-9]{3}[0-9]{3}$|
                                            ^[0-9]{1}[,]{1}[0-9]{3}[,]{1}[0-9]{3}$|
                                            ^[0-9]{2}[,]{1}[0-9]{3}[,]{1}[0-9]{3}$|
                                            ^[0-9]{2}[0-9]{3}[0-9]{3}$|
                                            ^[0-9]{1}[0-9]{3}[0-9]{3}$|
                                            ^PCT[/]{1}[A-Za-z]{2}[0-9]{2}[/]{1}[0-9]{5}$|
                                            ^PCT[/]{1}[A-Za-z]{2}[0-9]{4}[/]{1}[0-9]{6}$|
                                            ^PCT[A-Za-z]{2}[0-9]{2}[0-9]{5}$|
                                            ^PCT[A-Za-z]{2}[0-9]{4}[0-9]{6}$|
                                            ^([US|us]+\s)[0-9]{4}[-]{1}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|
                                            ^[0-9]{4}[-]{1}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|
                                            ^[0-9]{4}[-]{1}[0-9]{7}$|
                                            ^([US|us]+\s)[0-9]{4}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|
                                            ^([0-9]{11})+\s[A-Za-z]{1}[0-9]|^[0-9]{11}"
                        , @"\t|\n|\r\s+", "")
                )
                .WithMessage("Reference Number does not match USPTO format.");
        }
    }
} 