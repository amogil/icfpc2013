﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lib.Brute;
using lib.Lang;
using log4net;

namespace lib.AlphaProtocol
{
    public class ConcurrentWithoutShitAlphaProtocol
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (AlphaProtocol));

        public static IEnumerable<T[]> Chuncked<T>(IEnumerable<T> source, int chunkSize)
        {
            var buffer = new T[chunkSize];
            int i = 0;
            foreach (T e in source)
            {
                buffer[i] = e;
                if (i == buffer.Length - 1)
                {
                    yield return buffer;
                    buffer = new T[chunkSize];
                    i = 0;
                }
                else
                    i += 1;
            }
            T[] bufferTail = buffer.Where(e => e != null).ToArray();
            yield return bufferTail;
        }

        private static byte[][] FilterTrees(byte[][] treesToCheck, ulong[] inputs, ulong[] outputs)
        {
            var result = new List<byte[]>();
            for (int i = 0; i < treesToCheck.Length; i++)
            {
                bool finded = true;
                for (int j = 0; j < inputs.Length; j++)
                {
                    if (treesToCheck[i].Eval(inputs[j]) != outputs[j])
                    {
                        finded = false;
                        break;
                    }
                }
                if (finded)
                    result.Add(treesToCheck[i]);
            }
            return result.ToArray();
        }

        private static List<byte[][]> Results(List<Task<byte[][]>> tasks)
        {
            return tasks.Select(t => t.Result).Where(t => t.Length > 0).ToList();
        }

        private static byte[] GetSolution(List<byte[][]> results)
        {
            return results.SelectMany(result => result).FirstOrDefault();
        }

        public static string PostSolution(string problemId, int size, string[] operations)
        {
            var gsc = new GameServerClient();

            log.DebugFormat("Trying to solve problem {0}...", problemId);
            var random = new Random();

            IEnumerable<byte[]> trees = new BinaryBruteForcer(operations).Enumerate(size - 1);

            int tasksCount = Environment.ProcessorCount;
            IEnumerable<byte[][]> chunckedTrees = Chuncked(trees, 3*1024*1024);
            IEnumerable<byte[][][]> chunckedTreesPerWorker = Chuncked(chunckedTrees, tasksCount);
            var results = new byte[0][];
            var inputs = new ulong[0];
            var outputs = new ulong[0];
            bool likeAVirgin = true;
            var enumerator = chunckedTreesPerWorker.GetEnumerator();
            enumerator.MoveNext();
            while (true)
            {
                var treesPerWorkerChunk = enumerator.Current;
                var tasks = new List<Task<byte[][]>>(tasksCount);
                log.Debug("Starting creating tasks");
                if (likeAVirgin)
                {
                    inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();
                    outputs = gsc.Eval(problemId, inputs).ToArray();
                    log.Debug("Eval result for samples received");
                    likeAVirgin = false;
                }
                foreach (var treesChunk in treesPerWorkerChunk)
                {
                    byte[][] treeToCheck = treesChunk;
                    Task<byte[][]> task = Task.Factory.StartNew(() => FilterTrees(treeToCheck, inputs, outputs));
                    tasks.Add(task);
                }
                log.Debug("Finished creating tasks");
                enumerator.MoveNext();
                Task.WaitAll(tasks.ToArray<Task>());

                var tasksResults = Results(tasks);
                log.DebugFormat("All tasks completed, {0} results", results.Length);
                byte[] solution;
                while ((solution = GetSolution(tasksResults)) != null)
                {
                    log.Debug("First solution finded, asking the guess...");

                    string formula = String.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
                    WrongAnswer wrongAnswer = gsc.Guess(problemId, formula);

                    log.Debug("Guess answer received");

                    if (wrongAnswer == null)
                    {
                        log.DebugFormat("Problem solved!!!. Problem Id: {0}", problemId);
                        return formula;
                    }

                    log.Debug(string.Format("WrongAnswer received: {0}", wrongAnswer));
                    inputs = inputs.Concat(new[] {wrongAnswer.Arg}).ToArray();
                    outputs = outputs.Concat(new[] {wrongAnswer.CorrectValue}).ToArray();
                    tasksResults = tasksResults.Select(r => FilterTrees(r, new[] { wrongAnswer.Arg }, new[] { wrongAnswer.CorrectValue })).ToList();
                }
            }
        }
    }
}