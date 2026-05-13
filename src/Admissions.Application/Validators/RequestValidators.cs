using Admissions.Application.Features.Compare;
using Admissions.Application.Features.Fields;
using Admissions.Application.Features.Programs;
using Admissions.Application.Features.Universities;
using Admissions.Application.Imports;
using FluentValidation;

namespace Admissions.Application.Validators;

public sealed class CompareProgramsValidator : AbstractValidator<CompareProgramsQuery>
{
    public CompareProgramsValidator()
    {
        RuleFor(x => x.Ids)
            .Must(ids => CompareProgramsQuery.ParseIds(ids).Count <= CompareProgramsQuery.MaxIds)
            .WithMessage($"შედარება შესაძლებელია მაქსიმუმ {CompareProgramsQuery.MaxIds} პროგრამისთვის.");
    }
}

public sealed class CreateUniversityValidator : AbstractValidator<CreateUniversityCommand>
{
    public CreateUniversityValidator()
    {
        RuleFor(x => x.Body.Name)
            .NotEmpty().WithMessage("სახელი სავალდებულოა.")
            .MaximumLength(500);
        RuleFor(x => x.Body.ShortName).MaximumLength(100);
        RuleFor(x => x.Body.Code).MaximumLength(10);
    }
}

public sealed class CreateFieldValidator : AbstractValidator<CreateFieldCommand>
{
    public CreateFieldValidator()
    {
        RuleFor(x => x.Body.Name)
            .NotEmpty().WithMessage("სახელი სავალდებულოა.")
            .MaximumLength(300);
        RuleFor(x => x.Body.Code).MaximumLength(20);
    }
}

public sealed class UpdateFieldValidator : AbstractValidator<UpdateFieldCommand>
{
    public UpdateFieldValidator()
    {
        RuleFor(x => x.Body.Name)
            .NotEmpty().WithMessage("სახელი სავალდებულოა.")
            .MaximumLength(300);
        RuleFor(x => x.Body.Code).MaximumLength(20);
    }
}

public sealed class UpdateProgramYearStatsValidator : AbstractValidator<UpdateProgramYearStatsCommand>
{
    public UpdateProgramYearStatsValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Body.AnnouncedPlaces).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.EnrolledCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.FirstPriorityCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.TotalPriorityCount).GreaterThanOrEqualTo(0)
            .When(x => x.Body.TotalPriorityCount.HasValue);
        RuleFor(x => x.Body.AnnualFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.GrantFullCount).GreaterThanOrEqualTo(0)
            .When(x => x.Body.GrantFullCount.HasValue);
        RuleFor(x => x.Body.GrantPartialCount).GreaterThanOrEqualTo(0)
            .When(x => x.Body.GrantPartialCount.HasValue);
    }
}

public sealed class ImportEnrollmentsValidator : AbstractValidator<ImportEnrollmentsCommand>
{
    public ImportEnrollmentsValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("მიუთითეთ კორექტული წელი (?year=YYYY).");
    }
}

public sealed class ImportPrioritiesValidator : AbstractValidator<ImportPrioritiesCommand>
{
    public ImportPrioritiesValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("მიუთითეთ კორექტული წელი (?year=YYYY).");
    }
}

public sealed class ImportHandbookValidator : AbstractValidator<ImportHandbookCommand>
{
    public ImportHandbookValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("მიუთითეთ კორექტული წელი (?year=YYYY).");
    }
}
