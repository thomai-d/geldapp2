using System;
using System.Collections.Generic;
using System.Text;

namespace GeldApp2.Application.ML
{
    public class CategoryPredictionResult
    {
        public readonly static CategoryPredictionResult Empty = new CategoryPredictionResult();

        private CategoryPredictionResult()
        {
            // Only for EmptyResult.
            this.Category = string.Empty;
            this.Subcategory = string.Empty;
        }

        public CategoryPredictionResult(string category, string subcategory)
        {
            this.Category = category;
            this.Subcategory = subcategory;
        }

        public string Category { get; }

        public string Subcategory { get; }
    }
}
