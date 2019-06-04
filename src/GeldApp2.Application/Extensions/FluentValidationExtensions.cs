using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Extensions
{
    public static class FluentValidationExtensions
    {
        public static void ThrowOnError(this ValidationResult result, string title)
        {
            if (result.IsValid)
                return;

            var sb = new StringBuilder();
            sb.AppendLine(title);
            foreach (var err in result.Errors)
            {
                sb.AppendLine($"- {err.ErrorMessage}");
            }

            throw new ValidationException(sb.ToString());
        }
    }
}
