using Abstrakt.ML.MultiClass;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeldApp2.Application.ML
{
    public class CategoryPredictor
    {
        private MultiClassClassifier<ExpenseSample, string> classifier;

        public void Learn(IEnumerable<Expense> expenses)
        {
            this.classifier = new MultiClassClassifier<ExpenseSample, string>();

            var multiClassOptions = new MultiClassOptions<ExpenseSample>()
                                           .WithLabel(i => i.Category)
                                           .WithFeatures(i => i.Amount, i => i.CreatedHour, i => i.ExpenseDayOfWeek);

            var ffOptions = new FastForestOvaOptions()
                                    .WithNumberOfTrees(50)
                                    .WithLeaves(50)
                                    .WithExampleCountPerLeaf(1);

            this.classifier.TrainFastForestOva(expenses.Select(ExpenseSample.FromExpense), multiClassOptions, ffOptions);
        }

        public CategoryPredictionResult Predict(float amount, DateTime created, DateTime expenseDate)
        {
            var sample = new ExpenseSample
            {
                Amount = amount,
                CreatedHour = created.Hour,
                ExpenseDayOfWeek = (int)expenseDate.DayOfWeek
            };

            var result = this.classifier.Predict(sample);
            var parts = result.Split(new[] { '|' });
            return new CategoryPredictionResult(parts[0], parts[1]);
        }

        private class ExpenseSample
        {
            public static ExpenseSample FromExpense(Expense expense)
            {
                return new ExpenseSample
                {
                    Category = $"{expense.Category}|{expense.Subcategory}",
                    Amount = (float)expense.Amount,
                    CreatedHour = expense.Created.Hour,
                    ExpenseDayOfWeek = (int)expense.Date.DayOfWeek
                };
            }

            public string Category { get; set; }

            public float Amount { get; set; }

            public float CreatedHour { get; set; }

            public float ExpenseDayOfWeek { get; set; }
        }
    }
}
