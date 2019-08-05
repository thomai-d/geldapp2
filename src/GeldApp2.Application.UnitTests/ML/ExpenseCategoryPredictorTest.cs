using FluentAssertions;
using GeldApp2.Application.ML;
using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GeldApp2.Application.UnitTests.ML
{
    public class ExpenseCategoryPredictorTest
    {
        [Fact]
        public void LearnPredictTest()
        {
            var trainSet = new[]
            {
                new Expense(-24.4M, "Haushalt", "Wocheneinkauf"),
                new Expense(-23.1M, "Haushalt", "Wocheneinkauf"),
                new Expense(-22.4M, "Haushalt", "Wocheneinkauf"),
                new Expense(-21.2M, "Haushalt", "Wocheneinkauf"),
                new Expense(-500M, "Haushalt", "Elektrogeräte"),
                new Expense(-2.5M, "Verpflegung", "Leberkäs"),
            };

            var predictor = new CategoryPredictor();
            predictor.Learn(trainSet);

            predictor.Predict(-22.0f, DateTime.Now, DateTime.Now).ShouldBe("Haushalt", "Wocheneinkauf");
            predictor.Predict(-600.0f, DateTime.Now, DateTime.Now).ShouldBe("Haushalt", "Elektrogeräte");
            predictor.Predict(-2.0f, DateTime.Now, DateTime.Now).ShouldBe("Verpflegung", "Leberkäs");
        }
    }

    public static class ExpensePredictionResultExtension
    {
        public static void ShouldBe(this CategoryPredictionResult result, string category, string subcategory)
        {
            result.Category.Should().Be(category);
            result.Subcategory.Should().Be(subcategory);
        }
    }
}
