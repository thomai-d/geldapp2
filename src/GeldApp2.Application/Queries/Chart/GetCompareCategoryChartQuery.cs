using GeldApp2.Application.Commands;
using GeldApp2.Database.Abstractions;
using GeldApp2.Database.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Queries.Chart
{
    public class GetCompareCategoryChartQuery : AccountRelatedRequest<ExpenseRevenueLineChartsDto>
    {
        public List<CategoryFilterItem> Categories { get; set; } = new List<CategoryFilterItem>();
    }

    public class CategoryFilterItem
    {
        public string CategoryName { get; set; }

        public string SubcategoryName { get; set; }

        public bool HasSubcategory => !string.IsNullOrEmpty(this.SubcategoryName);

        public override string ToString() => this.HasSubcategory
                                            ? $"{this.CategoryName} - {this.SubcategoryName}"
                                            : this.CategoryName;
    }

    public class GetCompareCategoryChartQueryHandler
        : IRequestHandler<GetCompareCategoryChartQuery, ExpenseRevenueLineChartsDto>
    {
        private readonly ISqlQuery sql;

        public GetCompareCategoryChartQueryHandler(ISqlQuery sql)
        {
            this.sql = sql;
        }

        public async Task<ExpenseRevenueLineChartsDto> Handle(GetCompareCategoryChartQuery request, CancellationToken cancellationToken)
        {
            var result = new ExpenseRevenueLineChartsDto();

            foreach (var filter in request.Categories)
            {
                if (filter.HasSubcategory)
                {
                    // Expense - Main categories.
                    var items = await this.sql.Query<MonthlyDataItem>(
                                $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                                   FROM Expenses
                                   WHERE AccountId = {request.Account.Id}
                                   AND Category = {filter.CategoryName} AND Subcategory = {filter.SubcategoryName}
                                   AND Amount < 0
                                   GROUP BY MONTH(Date), YEAR(Date)
                                   ORDER BY YEAR(Date), MONTH(Date)");

                    var itemsNoGap = items.WithoutGaps().Select(DateChartItem.From);
                    var chart = new DateLineChartDto($"{filter.CategoryName} - {filter.SubcategoryName}", itemsNoGap);
                    chart.Tag = $"category:'{filter.CategoryName}' and subcategory:'{filter.SubcategoryName}' and amount<0";
                    if (chart.Items.Any(i => i.Y != 0))
                        result.Expense.Add(chart);
                }
                else
                {
                    // Expense - Subcategories.
                    var items = await this.sql.Query<MonthlyDataItem>(
                                $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                                   FROM Expenses
                                   WHERE AccountId = {request.Account.Id}
                                   AND Category = {filter.CategoryName}
                                   AND Amount < 0
                                   GROUP BY MONTH(Date), YEAR(Date)
                                   ORDER BY YEAR(Date), MONTH(Date)");

                    var itemsNoGap = items.WithoutGaps().Select(DateChartItem.From);
                    var chart = new DateLineChartDto($"{filter.CategoryName}", itemsNoGap);
                    chart.Tag = $"category:'{filter.CategoryName}' and amount<0";
                    if (chart.Items.Any(i => i.Y != 0))
                        result.Expense.Add(chart);
                }

                if (filter.HasSubcategory)
                {
                    // Revenue - Main categories.
                    var items = await this.sql.Query<MonthlyDataItem>(
                                $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                                   FROM Expenses
                                   WHERE AccountId = {request.Account.Id}
                                   AND Category = {filter.CategoryName} AND Subcategory = {filter.SubcategoryName}
                                   AND Amount > 0
                                   GROUP BY MONTH(Date), YEAR(Date)
                                   ORDER BY YEAR(Date), MONTH(Date)");

                    var itemsNoGap = items.WithoutGaps().Select(DateChartItem.From);
                    var chart = new DateLineChartDto($"{filter.CategoryName} - {filter.SubcategoryName}", itemsNoGap);
                    chart.Tag = $"category:'{filter.CategoryName}' and subcategory:'{filter.SubcategoryName}' and amount>0";
                    if (chart.Items.Any(i => i.Y != 0))
                        result.Revenue.Add(chart);
                }
                else
                {
                    // Revenue - Subcategories.
                    var items = await this.sql.Query<MonthlyDataItem>(
                                $@"SELECT MONTH(Date) as Month, YEAR(Date) as Year, SUM(Amount) as Amount
                                   FROM Expenses
                                   WHERE AccountId = {request.Account.Id}
                                   AND Category = {filter.CategoryName}
                                   AND Amount > 0
                                   GROUP BY MONTH(Date), YEAR(Date)
                                   ORDER BY YEAR(Date), MONTH(Date)");

                    var itemsNoGap = items.WithoutGaps().Select(DateChartItem.From);
                    var chart = new DateLineChartDto($"{filter.CategoryName}", itemsNoGap);
                    chart.Tag = $"category:'{filter.CategoryName}' and amount>0";
                    if (chart.Items.Any(i => i.Y != 0))
                        result.Revenue.Add(chart);
                }
            }

            return result;
        }
    }
}
