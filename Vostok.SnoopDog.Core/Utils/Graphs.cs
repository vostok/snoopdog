using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.SnoopDog.Core.Utils
{
    public static class Graphs
    {
        public static IEnumerable<IEnumerable<Tuple<TN, TE>>> FindCycles<TN, TE>(IEnumerable<TN> nodes,
            Func<TN, IEnumerable<TE>> getEdges, Func<TE, TN> edge2Node, IEqualityComparer<TN> comparer)
        {
            var notUsedNodes = new HashSet<TN>(nodes, comparer);
            var stack = new Stack<Tuple<TN, IEnumerator<TE>>>();
            var stackSet = new HashSet<TN>(comparer);
            while (notUsedNodes.Count > 0)
            {
                var root = notUsedNodes.First();
                AddNodeToStack(root, getEdges, notUsedNodes, stack, stackSet);

                while (stack.Count > 0)
                {
                    var enumerator = stack.Peek().Item2;
                    if (enumerator.MoveNext())
                    {
                        var neighbour = edge2Node(enumerator.Current);
                        if (stackSet.Contains(neighbour))
                            yield return ExtractCycle(neighbour, stack, comparer);

                        AddNodeToStack(neighbour, getEdges, notUsedNodes, stack, stackSet);
                    }
                    else
                        PopFromStack(stackSet, stack);
                }
            }
        }

        private static void PopFromStack<TN, TE>(HashSet<TN> stackSet, Stack<Tuple<TN, IEnumerator<TE>>> stack)
        {
            stackSet.Remove(stack.Peek().Item1);
            stack.Pop();
        }

        private static IEnumerable<Tuple<TN, TE>> ExtractCycle<TN, TE>(TN cycleEnd, Stack<Tuple<TN, IEnumerator<TE>>> stack, IEqualityComparer<TN> comparer)
        {
            var endMet = false;
            return stack
                .TakeWhile(t =>
                {
                    var pred = endMet;
                    endMet = comparer.Equals(t.Item1, cycleEnd);
                    return !pred;
                })
                .Select(t => Tuple.Create(t.Item1, t.Item2.Current))
                .ToArray();
        }

        private static void AddNodeToStack<TN, TE>(TN node, Func<TN, IEnumerable<TE>> getNeigbours, ICollection<TN> notUsedNodes, Stack<Tuple<TN, IEnumerator<TE>>> stack, ISet<TN> stackSet)
        {
            if (!notUsedNodes.Remove(node)) return;
            stack.Push(Tuple.Create(node, getNeigbours(node).GetEnumerator()));
            stackSet.Add(node);
        }
    }
}