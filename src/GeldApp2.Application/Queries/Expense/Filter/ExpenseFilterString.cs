using GeldApp2.Application.Exceptions;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GeldApp2.Application.Queries.Expense.Filter
{
    /// <summary>
    /// Parser for a filtering DSL.
    /// See the unit test for examples :)
    /// </summary>
    public class ExpenseFilterString
    {
        private static readonly Regex StartTokenRx = new Regex("^[A-Za-z.]+");
        private static readonly Regex ValueRx = new Regex(@"^:(?<VALUE>\d{1,5})($|\s)");
        private static readonly Regex StringValueRx = new Regex(@"^:(?<VALUE>.*?)($|\s)");
        private static readonly Regex QuotedStringValueRx = new Regex(@"^:'(?<VALUE>.*?)'($|\s)");
        private static readonly Regex AndRx = new Regex(@"^\s*and\s+");
        private static readonly Regex CompareAmountRx = new Regex(@"^(?<COMPARE>[><:]{1})(?<VALUE>-?\d{1,5}(,?)\d{0,2})($|\s)");

        private string remainder;

        private ExpenseFilterString()
        {
        }

        public string FilterString { get; private set; }

        public int? Month { get; private set; }
        public int? Year { get; private set; }
        public string Category { get; private set; }
        public string Subcategory { get; private set; }
        public ExpenseType? Type { get; private set; }

        public AmountCompareType? AmountCompareType { get; private set; }
        public decimal Amount { get; set; }

        public static ExpenseFilterString Parse(string filterString)
        {
            var instance = new ExpenseFilterString();
            filterString = filterString.Trim();
            instance.FilterString = filterString;
            instance.remainder = filterString;
            instance.Parse();
            return instance;
        }

        private void Parse()
        {
            if (string.IsNullOrEmpty(this.remainder))
                return;

            var m = StartTokenRx.Match(this.remainder);
            if (!m.Success)
                throw new FilterParseException($"'{this.FilterString}': Invalid start token: '{this.remainder}'");

            var startToken = m.Value;
            this.remainder = this.remainder.Substring(m.Length);
            switch (startToken)
            {
                case "month":
                    if (this.Month.HasValue)
                        throw new FilterParseException($"'{this.FilterString}': Duplicate value for month");
                    this.ParseInt(out var month);
                    this.Month = month;
                    break;

                case "year":
                    if (this.Year.HasValue)
                        throw new FilterParseException($"'{this.FilterString}': Duplicate value for year");
                    this.ParseInt(out var year);
                    this.Year = year;
                    break;

                case "category":
                    if (this.Category != null)
                        throw new FilterParseException($"'{this.FilterString}': Duplicate value for category");
                    this.ParseString(out var str);
                    this.Category = str;
                    break;

                case "subcategory":
                    if (this.Subcategory != null)
                        throw new FilterParseException($"'{this.FilterString}': Duplicate value for subcategory");
                    this.ParseString(out var substr);
                    this.Subcategory = substr;
                    break;

                case "type":
                    if (this.Type.HasValue)
                        throw new FilterParseException($"'{this.Type}': Duplicate value for type");
                    this.ParseString(out var typeStr);

                    if (!Enum.TryParse<ExpenseType>(typeStr, ignoreCase: true, out var type))
                        throw new FilterParseException($"{typeStr}: Invalid expense type");

                    this.Type = type;
                    break;

                case "amount":
                    if (this.AmountCompareType.HasValue)
                        throw new FilterParseException($"'{this.AmountCompareType}': Duplicate value for amount");
                    this.ParseAmountCompare(out var compType, out var amt);
                    this.AmountCompareType = compType;
                    this.Amount = amt;
                    break;

                default:
                    throw new FilterParseException($"'{this.FilterString}': Unknown start token: '{startToken}'");
            }

            if (string.IsNullOrEmpty(this.remainder))
                return;

            this.ExpectAnd();

            this.Parse();
        }

        private void ParseString(out string value)
        {
            var m = QuotedStringValueRx.Match(this.remainder);
            if (!m.Success)
                m = StringValueRx.Match(this.remainder);
            if (!m.Success)
              throw new FilterParseException($"'{this.FilterString}': Invalid string-value: '{this.remainder}'");

            this.remainder = this.remainder.Substring(m.Length);
            value = m.Groups["VALUE"].Value;
        }

        private void ParseAmountCompare(out AmountCompareType compareType, out decimal amount)
        {
            var m = CompareAmountRx.Match(this.remainder);
            if (!m.Success)
              throw new FilterParseException($"'{this.FilterString}': Invalid amount-value: '{this.remainder}'");

            this.remainder = this.remainder.Substring(m.Length);

            switch (m.Groups["COMPARE"].Value)
            {
                case ">": compareType = Filter.AmountCompareType.GreaterThan; break;
                case "<": compareType = Filter.AmountCompareType.LowerThan; break;
                case ":": compareType = Filter.AmountCompareType.Equals; break;
                default: throw new FilterParseException($"'{this.FilterString}': Invalid amount compare sign.");
            }

            if (!decimal.TryParse(m.Groups["VALUE"].Value, out amount))
              throw new FilterParseException($"'{this.FilterString}': Non parseable amount-value: '{m.Groups["VALUE"].Value}'");
        }

        private void ParseInt(out int value)
        {
            var m = ValueRx.Match(this.remainder);
            if (!m.Success)
              throw new FilterParseException($"'{this.FilterString}': Invalid int-value: '{this.remainder}'");

            this.remainder = this.remainder.Substring(m.Length);
            if (!int.TryParse(m.Groups["VALUE"].Value, out value))
              throw new FilterParseException($"'{this.FilterString}': Non parseable int-value: '{m.Groups["VALUE"].Value}'");
        }

        private void ExpectAnd()
        {
            var m = AndRx.Match(this.remainder);
            if (!m.Success)
              throw new FilterParseException($"'{this.FilterString}': Expected concatenator: '{this.remainder}'");
            this.remainder = this.remainder.Substring(m.Length);
        }
    }
}
