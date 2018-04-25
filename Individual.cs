using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneticProgrammingOptimizer
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Individual
    {
        public static List<string> AvailableLeaf = new List<string>(new string[] {
            "A", "B", "C", "D","E",
            "1", "2", "3", "4", "5"
        });
        public static List<string> AvailableOperators = new List<string>(new string[] { "+", "-", "*", "/" });
        public BinaryTree<string> Genes;
        public double Fitness { get; set; }
        static Random rnd = new Random();

        public static BinaryTree<string> GenerateSimpleTree()
        {
            var tree = new BinaryTree<string>();
            tree.value = AvailableOperators[rnd.Next(0, AvailableOperators.Count)];
            tree.right = new BinaryTree<string> {value = AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)]};
            tree.left = new BinaryTree<string>
            {
                value = AvailableOperators[rnd.Next(0, AvailableOperators.Count)],
                left = new BinaryTree<string> { value =  AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)] },
                right = new BinaryTree<string> { value = AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)] },
            };
            return tree;
        }
        public static BinaryTree<string> GenerateRandomTree(int maxDepth = 5, int currentDepth = 0)
        {
            var tree = new BinaryTree<string>() { };

            if (maxDepth >= currentDepth)
            {
                tree.left = GenerateRandomTree(maxDepth, ++currentDepth);
                tree.right = GenerateRandomTree(maxDepth, ++currentDepth);
            }
            else
            {
                tree.left = new BinaryTree<string> { value = AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)] };
                tree.right = new BinaryTree<string> { value = AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)] };
            }

            if (tree.left != null && tree.right != null)
                tree.value = AvailableOperators[rnd.Next(0, AvailableOperators.Count)];
            else
                tree.value = AvailableLeaf[rnd.Next(0, AvailableLeaf.Count)];
            return tree;
        }

        public override string ToString()
        {
            return BinaryTree<string>.ToExpression(this.Genes);
        }

        public Individual(int treeDepth)
        {
            if (treeDepth == 0)
                Genes = GenerateSimpleTree();
            Genes = GenerateRandomTree(treeDepth);
        }

        public Individual(BinaryTree<string> genes)
        {
            Genes = genes;
        }

        public Individual()
        {
            Genes = GenerateSimpleTree();
        }

        public Individual Clone()
        {
            return new Individual(this.Genes.Clone());
        }
    }
}
