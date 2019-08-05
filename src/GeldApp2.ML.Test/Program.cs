using Abstrakt.ML.MultiClass;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace GeldApp2.ML.Test
{
    public class ExpenseInput
    {
        [LoadColumn(2)]
        public string Category { get; set; }

        [LoadColumn(3)]
        public string Subcategory { get; set; }

        [LoadColumn(5)]
        public float Amount { get; set; }

        [LoadColumn(6)]
        public DateTime Date { get; set; }

        [LoadColumn(8)]
        public DateTime Created { get; set; }
    }

    public class SampleInput
    {
        public static SampleInput FromExpense(ExpenseInput expense)
        {
            return new SampleInput
            {
                Category = $"{expense.Category} - {expense.Subcategory}",
                Amount = expense.Amount,
                CreatedHour = expense.Created.Hour,
                ExpenseDayOfWeek = (int)expense.Date.DayOfWeek
            };
        }

        public string Category { get; set; }

        public float Amount { get; set; }

        public float CreatedHour { get; set; }

        public float ExpenseDayOfWeek { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ml = new MLContext();
            var tsvData = ml.Data.LoadFromTextFile<ExpenseInput>(@"C:\Users\Hans\Desktop\Thomas.tsv", hasHeader: true);
            var rawData = ml.Data.CreateEnumerable<ExpenseInput>(tsvData, true);
            var data = rawData.Select(SampleInput.FromExpense);

            var multi = new MultiClassClassifier<SampleInput, string>();

            var tree = 50;
            var leaves = 50;
            var examples = 1;
            multi.TrainFastForestOva(data, mca => mca.WithLabel(i => i.Category).WithFeatures(i => i.Amount, i => i.CreatedHour, i => i.ExpenseDayOfWeek),
                                       ff => ff.WithNumberOfTrees(tree).WithLeaves(leaves).WithExampleCountPerLeaf(examples));

            //multi.SaveModel(@"C:\\Users\\Hans\\Desktop\\Thomas.model.zip");

            while (true)
            {
                Console.Write("Hour: ");
                var hour = int.Parse(Console.ReadLine());
                Console.Write("Day: ");
                var day = int.Parse(Console.ReadLine());
                Console.Write("Amount: ");
                var amount = float.Parse(Console.ReadLine());
                var input = new SampleInput { Amount = amount, CreatedHour = hour, ExpenseDayOfWeek = day };
                var result = multi.Predict(input);
                Console.WriteLine(result);
                Console.WriteLine("----\n\n");
            }
        }
    }

}
