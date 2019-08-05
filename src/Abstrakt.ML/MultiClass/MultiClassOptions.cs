using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Abstrakt.ML.MultiClass
{
    public class MultiClassOptions<TInput> : Options
    {
        public string LabelName { get; private set; }

        public string[] FeatureColumnNames { get; private set; }

        public MultiClassOptions<TInput> WithLabel<TResult>(Expression<Func<TInput, TResult>> labelSelector)
        {
            var clone = (MultiClassOptions<TInput>)this.MemberwiseClone();
            clone.LabelName = GetMemberName(labelSelector);
            return clone;
        }

        public MultiClassOptions<TInput> WithFeatures<TResult>(params Expression<Func<TInput, TResult>>[] featureSelectors)
        {
            var clone = (MultiClassOptions<TInput>)this.MemberwiseClone();
            clone.FeatureColumnNames = featureSelectors.Select(GetMemberName).ToArray();
            return clone;
        }
    }
}
