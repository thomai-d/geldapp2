using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Abstrakt.ML.MultiClass
{
    public class MultiClassClassifier<TInput, TResult>
        where TInput: class
    {
        private readonly MLContext ml;

        private ITransformer model;
        private PredictionEngine<TInput, PredictionOutput> predictionEngine;
        private DataViewSchema inputSchema;

        private EstimatorChain<MulticlassPredictionTransformer<OneVersusAllModelParameters>> trainingPipeline;

        public MultiClassClassifier()
        {
            this.ml = new MLContext();
        }

        public MultiClassOptions<TInput> Options { get; private set; }

        public void TrainFastForestOva(IEnumerable<TInput> trainingData, MultiClassOptions<TInput> multiClassOptions, FastForestOvaOptions fastForestOptions)
        {
            this.Options = multiClassOptions;

            // Data Preprocessing pipeline.
            var pipeline = this.ml.Transforms.Conversion.MapValueToKey(inputColumnName: this.Options.LabelName, outputColumnName: "Label")
                   .Append(this.ml.Transforms.Concatenate("Features", this.Options.FeatureColumnNames))
                   .AppendCacheCheckpoint(this.ml);

            // Training pipeline.
            var classifier = this.ml.BinaryClassification.Trainers.FastForest(numberOfLeaves: fastForestOptions.NumberOfLeaves, minimumExampleCountPerLeaf: fastForestOptions.MinimumExampleCountPerLeaf,
                                                                              numberOfTrees: fastForestOptions.NumberOfTrees, labelColumnName: "Label", featureColumnName: "Features");
            var multiClass = this.ml.MulticlassClassification.Trainers.OneVersusAll(classifier, labelColumnName: "Label");
            this.trainingPipeline = pipeline.Append(multiClass);

            // Training.
            var trainData = this.ml.Data.LoadFromEnumerable(trainingData);
            this.model = trainingPipeline
                         .Append(this.ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                         .Fit(trainData);

            this.inputSchema = trainData.Schema;

            this.predictionEngine = this.ml.Model.CreatePredictionEngine<TInput, PredictionOutput>(model);
        }

        public void SaveModel(string path)
        {
            if (this.model == null)
                throw new InvalidOperationException("No model loaded.");

            this.ml.Model.Save(this.model, this.inputSchema, path);
        }

        public void LoadModel(string path)
        {
            if (this.model == null)
                throw new InvalidOperationException("No model loaded.");

            this.model = this.ml.Model.Load(path, out this.inputSchema);
            this.predictionEngine = this.ml.Model.CreatePredictionEngine<TInput, PredictionOutput>(model);
        }

        public TResult Predict(TInput input)
        {
            if (this.predictionEngine == null)
                throw new InvalidOperationException("No model loaded.");

            return this.predictionEngine.Predict(input).Label;
        }

        public void DumpFeatureImportance(IEnumerable<TInput> data)
        {
            var evaluationData = this.model.Transform(this.ml.Data.LoadFromEnumerable(data));
            var model = this.trainingPipeline.LastEstimator.Fit(evaluationData);
            var permutationMetrics = this.ml.MulticlassClassification.PermutationFeatureImportance(model, evaluationData, permutationCount: 3);

            var sortedIndices = permutationMetrics
                .Select((metrics, index) => new { index, metrics.MicroAccuracy })
                .OrderByDescending(feature => Math.Abs(feature.MicroAccuracy.Mean))
                .Select(feature => feature.index);

            var microAccuracy = permutationMetrics.Select(x => x.MicroAccuracy)
                .ToArray();

            foreach (int i in sortedIndices)
            {
                Console.WriteLine($"{this.Options.FeatureColumnNames[i].PadLeft(25)}\t{microAccuracy[i].Mean:##0.000}\t{microAccuracy[i].StandardError:##0.000}");
            }
        }

        public void DumpEvaluation(IEnumerable<TInput> data)
        {
            var evaluationData = this.model.Transform(this.ml.Data.LoadFromEnumerable(data));
            var metrics = this.ml.MulticlassClassification.Evaluate(evaluationData);
            var micro = metrics.MicroAccuracy.ToString("F3").PadLeft(5);
            var macro = metrics.MacroAccuracy.ToString("F3").PadLeft(5);
            var logl = metrics.LogLoss.ToString("F2").PadLeft(6);
            var loglr = metrics.LogLossReduction.ToString("F2").PadLeft(6);
            Console.WriteLine($"Mic: {micro}   Mac: {macro}   LogL: {logl}   LogLRed: {loglr}");
        }

        private class PredictionOutput
        {
            [ColumnName("PredictedLabel")]
            public TResult Label;
        }
    }
}
