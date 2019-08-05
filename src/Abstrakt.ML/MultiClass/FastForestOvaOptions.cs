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
            this.NumberOfLeaves = leaves;
            return this;
        }

        public FastForestOvaOptions WithExampleCountPerLeaf(int minCount)
        {
            this.MinimumExampleCountPerLeaf = minCount;
            return this;
        }

        public FastForestOvaOptions WithNumberOfTrees(int maxTrees)
        {
            this.NumberOfTrees = maxTrees;
            return this;
        }
    }
}
