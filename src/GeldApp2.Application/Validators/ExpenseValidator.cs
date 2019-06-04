using FluentValidation;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Application.Validators
{
    public class ExpenseValidator : AbstractValidator<Expense>
    {
        public static readonly DateTimeOffset MinDate = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset EndOfLife = new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public ExpenseValidator()
        {
            this.When(e => e.Type == ExpenseType.Expense || e.Type == ExpenseType.RegularExpense, () =>
            {
                this.RuleFor(m => m.Amount)
                    .LessThanOrEqualTo(0)
                    .WithMessage("Expense must be negative");
            });

            this.When(e => e.Type == ExpenseType.Revenue || e.Type == ExpenseType.RegularRevenue, () =>
            {
                this.RuleFor(m => m.Amount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Revenue must be positive");
            });

            this.RuleFor(m => m.Id)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Invalid id");

            this.RuleFor(m => m.AccountId)
                .GreaterThan(0)
                .WithMessage("No account set");

            this.RuleFor(m => m.Category)
                .NotEmpty();

            this.RuleFor(m => m.Details)
                .NotNull();

            this.RuleFor(m => m.Subcategory)
                .NotEmpty();

            this.RuleFor(m => m.Type)
                .IsInEnum();

            this.RuleFor(m => m.Created)
                .InclusiveBetween(MinDate, EndOfLife);

            this.RuleFor(m => m.LastModified)
                .InclusiveBetween(MinDate, EndOfLife);

            this.RuleFor(m => m.Date)
                .InclusiveBetween(MinDate.LocalDateTime, EndOfLife.LocalDateTime)
                .Must(m => m.Hour == 0 && m.Minute == 0 && m.Second == 0 && m.Millisecond == 0);

            this.RuleFor(m => m.CreatedBy)
                .NotEmpty();

            this.RuleFor(m => m.LastModifiedBy)
                .NotEmpty();
        }
    }
}
