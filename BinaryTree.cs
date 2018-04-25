using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GeneticProgrammingOptimizer
{
    public class BinaryTree<T>
    {
        public int index;

        public T value;
        public BinaryTree<T> left;
        public BinaryTree<T> right;

        public BinaryTree(T[] values) : this(values, 0) { }
        public BinaryTree()
        {

        }

        public static int ReindexTree(ref BinaryTree<string> tree)
        {
            var stack = new Stack<BinaryTree<string>>();
            stack.Push(tree);
            var index = 0;
            while (stack.Count > 0)
            {
                var elem = stack.Pop();
                if (elem != null)
                {
                    elem.index = index++;
                }

                if (elem?.left != null)
                    stack.Push(elem.left);
                if(elem?.right != null)
                    stack.Push(elem.right);

            }

            return index-1;
        }

        public static void SwapTreeWithTarget(ref BinaryTree<string> tree, BinaryTree<string> target, int index)
        {
            var st = GetSubtreeByIndex(ref tree, index);
            st.left = target.left;
            st.right = target.right;
            st.value = target.value;
        }
        public static BinaryTree<string>[] SwapSubtrees(
            int index1,
            int index2,
            BinaryTree<string> bt1,
            BinaryTree<string> bt2)
        {
            var st1 = GetSubtreeByIndex(ref bt1, index1);
            var st2 = GetSubtreeByIndex(ref bt2, index2);

            var tmpleft = st1.left;
            var tmpright = st1.right;
            var tmpval = st1.value;

            st1.left = st2.left;
            st1.right = st2.right;
            st1.value = st2.value;
            st2.left = tmpleft;
            st2.right = tmpright;
            st2.value = tmpval;

            return new[] {bt1, bt2};
        }
        public static void SwapSubtrees(
            int index1, 
            int index2, 
            ref BinaryTree<string> bt1,
            ref BinaryTree<string> bt2)
        {
            var st1 = GetSubtreeByIndex( ref bt1, index1);
            var st2 = GetSubtreeByIndex( ref bt2, index2);

            var tmpleft = st1.left;
            var tmpright = st1.right;
            var tmpval = st1.value;
            
            st1.left = st2.left;
            st1.right = st2.right;
            st1.value = st2.value;
            st2.left = tmpleft;
            st2.right = tmpright;
            st2.value = tmpval;
        }
        public static ref BinaryTree<string> GetSubtreeByIndex(ref BinaryTree<string> tree, int index)
        {
            if (index == 0)
                return ref tree;

            Stack<BinaryTree<string>> stack = new Stack<BinaryTree<string>>();
            stack.Push(tree);

            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                if (curr?.left?.index == index)
                {
                    return ref curr.left;
                }

                if (curr?.right?.index == index)
                {
                    return ref curr.right;
                }
                if(curr?.left != null)
                stack.Push(curr.left);
                if(curr?.right != null)
                stack.Push(curr.right);
            }

            throw new ApplicationException("Index not found");
        }

        public static int MaxDepth( BinaryTree<T> root)
        {
            if (root == null) return 0;

            return Math.Max(MaxDepth(root.left), MaxDepth(root.right)) + 1;
        }

        BinaryTree(T[] values, int index)
        {
            Load(this, values, index);
        }

        void Load(BinaryTree<T> tree, T[] values, int index)
        {
            this.value = values[index];
            if (index * 2 + 1 < values.Length)
            {
                this.left = new BinaryTree<T>(values, index * 2 + 1);
            }
            if (index * 2 + 2 < values.Length)
            {
                this.right = new BinaryTree<T>(values, index * 2 + 2);
            }
        }



        public static string ToExpression(BinaryTree<T> tree)
        {
            var nulls = 0;
            if (tree.left == null)
                nulls++;
            if (tree.right == null)
                nulls++;

            if (nulls >= 2)
                return tree.value.ToString();

            if (nulls == 1)
            {
                BinaryTree<T> child;
                if (tree.left == null)
                    child = tree.right;
                else child = tree.left;

                return tree.value.ToString() + "( " + child.value.ToString() + " )";
            }

            return "( " + ToExpression(tree.left) + " )" + tree.value.ToString() + "( " + ToExpression(tree.right) +" )";
        }

        public BinaryTree<T> Clone()
        {

            BinaryTree<T> clone = (BinaryTree<T>)MemberwiseClone();
            if (left != null)
                clone.left = left.Clone();
            if (right != null)
                clone.right = right.Clone();
            return clone;
        }
    }
}