using System;
using FluentValidation;

namespace Dnsy.Api.Types
{
    public class DnsQuery
    {
        public string Query { get; init; } = default;
    }

    public class DnsQueryValidator : AbstractValidator<DnsQuery>
    {
        public DnsQueryValidator()
        {
            RuleFor(_ => _.Query)
                .NotEmpty()
                .MaximumLength(63)
                .MinimumLength(4);
        }
    }
}