using System;
using System.Collections.Generic;
using System.Text;

namespace Abstrakt.ML.MultiClass
{
    public class FastForestOvaOptions : Options
    {
        public int NumberOfLeaves { get; private set; } = 7;

        public int MinimumExampleCountPerLeaf { get; private set; } = 1;

        public int NumberOfTrees { get; private set; } = 100;

        public FastForestOvaOptions WithLeaves(int leaves)
        {
            var clone = (FastForestOvaOptions)this.MemberwiseClone();
            clone.NumberOfLeaves = leaves;
            return clone;
        }

        public FastForestOvaOptions WithExampleCountPerLeaf(int minCount)
        {
            var clone = (FastForestOvaOptions)this.MemberwiseClone();
            this.MinimumExampleCountPerLeaf = minCount;
            return clone;
        }

        public FastForestOvaOptions WithNumberOfTrees(int maxTrees)
        {
            var clone = (FastForestOvaOptions)this.MemberwiseClone();
            this.NumberOfTrees = maxTrees;
            return clone;
        }
    }
}
