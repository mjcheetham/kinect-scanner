using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KaptureLibrary.Trees.KdTree
{
    /// <summary>
    /// Represents a kd-tree which stores objects derived from a Math.NET Numerics Single <see cref="Vector"/>.
    /// </summary>
    public class KdTree : ICollection<Vector>, IEnumerable<Vector>
    {
        #region Private Fields
        private int numNodes;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        /// <value>The root node.</value>
        public KdTreeNode Root { get; private set; }
        /// <summary>
        /// Gets the dimensionality of the tree.
        /// </summary>
        public int Dimensionality { get; private set; }
        #endregion

        #region Static Constructors
        /// <summary>
        /// Constructs a kd-tree from the specified collection of elements.
        /// </summary>
        /// <param name="dimensionality">The dimensionality of each location Vector.</param>
        /// <param name="elements">The collection of elements from which to create the tree.</param>
        /// <returns>The constructed kd-tree.</returns>
        public static KdTree Construct(int dimensionality, ICollection<Vector> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            // Create and initialize kd-tree.
            var tree = new KdTree();
            tree.Dimensionality = dimensionality;

            // Construct nodes of tree.
            var elementsArray = elements.ToArray();
            tree.Root = Construct(tree, elementsArray, 0);

            // Set number of element value
            tree.numNodes = elementsArray.GetLength(0);

            return tree;
        }

        private static KdTreeNode Construct(KdTree tree, Vector[] elements, int depth)
        {
            var length = elements.GetLength(0);
            if (length == 0) return null;

            // Sort array by dimension
            var dimension = depth % tree.Dimensionality;
            Array.Sort(
                elements,
                (a, b) => a[dimension].CompareTo(b[dimension]) // fancy delegate here.. ooh urr..
            );

            // Get median element from sorted array
            var medianIndex = length / 2;
            var medianElement = elements[medianIndex];

            // Get sub-tree elements
            var leftElements = elements.Take(medianIndex).ToArray();
            var rightElements = elements.Skip(medianIndex + 1).ToArray();

            // Create node and connect sub-trees
            var node = new KdTreeNode(medianElement);
            node.LeftChild = Construct(tree, leftElements, depth + 1);
            node.RightChild = Construct(tree, rightElements, depth + 1);

            return node;
        }
        #endregion

        #region Constructors
        private KdTree() { } // private empty constructor
        /// <summary>
        /// Initialises a new instance of the <see cref="KdTree"/> class with the specified root node.
        /// </summary>
        /// <param name="dimensionality">The dimensionality of the underlying <see cref="Vector"/>.</param>
        /// <param name="root">The root node of the tree. Cannot be null.</param>
        public KdTree(int dimensionality, KdTreeNode root)
            : this()
        {
            if (root == null)
                throw new ArgumentNullException("root");

            this.Dimensionality = dimensionality;
            this.Root = root;
        }
        #endregion

        #region Nearest Neighbour Search
        /// <summary>
        /// Finds the value in the tree that is nearest to the specified location.
        /// </summary>
        /// <param name="location">The location for which to find the nearest neighbour.</param>
        /// <returns>The value whose location is nearest to <paramref name="location"/>.</returns>
        public Vector FindNearestNeighbour(Vector location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            return FindNearestNeighbour(location, this.Root, this.Root.Value, Single.MaxValue, 0);
        }

        private Vector FindNearestNeighbour(Vector location,
            KdTreeNode node, Vector bestValue, float bestDistance, int depth)
        {
            if (node == null) // no where left to search, return best value
                return bestValue;

            var dimension = depth % this.Dimensionality;
            var nodeValue = node.Value;
            var distance = (nodeValue - location).Norm(this.Dimensionality);

            // Check if current node is better than best node.
            // Current node cannot be same as search location.
            if (!Precision.AlmostEqual(distance, 0) &&
                (distance < bestDistance))
            {
                bestValue = nodeValue;
                bestDistance = distance;
            }

            // Check for best node in sub-tree of near child. i.e., which side of Node is the query point?
            var nearChildNode = (location[dimension] < nodeValue[dimension]) ?
                node.LeftChild : node.RightChild;

            if (nearChildNode != null)
            {
                var nearBestValue = FindNearestNeighbour(location, nearChildNode, bestValue, bestDistance, depth + 1);
                var nearBestDistance = (nearBestValue - location).Norm(this.Dimensionality);
                bestValue = nearBestValue;
                bestDistance = nearBestDistance;
            }

            // Check whether splitting hyperplane given by current node intersects with hypersphere of current smallest
            // distance around given location.
            if (bestDistance > Math.Abs(nodeValue[dimension] - location[dimension]))
            {
                // Check for best node in sub-tree of far child.
                var farChildNode = nearChildNode == node.LeftChild ? node.RightChild : node.LeftChild; // i.e. not the near child

                if (farChildNode != null)
                {
                    var farBestValue = FindNearestNeighbour(location, farChildNode, bestValue, bestDistance, depth + 1);
                    var farBestDistance = (farBestValue - location).Norm(this.Dimensionality);
                    bestValue = farBestValue;
                    bestDistance = farBestDistance;
                }
            }

            return bestValue;
        }
        #endregion

        #region Clear Nodes
        /// <summary>
        /// Removes all nodes in the tree except for the root node.
        /// </summary>
        public void Clear()
        {
            this.Root.LeftChild = null;
            this.Root.RightChild = null;
        }
        #endregion

        #region Add Methods
        /// <summary>
        /// Adds a node with the specified value to the tree.
        /// </summary>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>The node that was added.</returns>
        /// <remarks>
        /// Nodes with duplicate values may be added to the tree.
        /// </remarks>
        public KdTreeNode Add(Vector value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return Add(value, this.Root, 0);
        }

        private KdTreeNode Add(Vector value, KdTreeNode node, int depth)
        {
            if (node == null)
            {
                node = new KdTreeNode(value);
            }
            else
            {
                // Check if node should be added to left or right sub-tree of current node.
                var dimension = depth % this.Dimensionality;

                if (!(value[dimension] > node.Value[dimension]))
                {
                    node.LeftChild = Add(value, node.LeftChild, depth + 1);
                }
                else
                {
                    node.RightChild = Add(value, node.RightChild, depth + 1);
                }
            }

            return node;
        }
        #endregion

        #region Find Methods
        /// <summary>
        /// Finds the node with the specified value.
        /// </summary>
        /// <param name="value">The value to find.</param>
        public KdTreeNode Find(Vector value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return Find(value, this.Root, 0);
        }

        private KdTreeNode Find(Vector value, KdTreeNode node, int depth)
        {
            if (node == null)
                return null;
            if (node.Value.Equals(value))
                return node;

            var dimension = depth % this.Dimensionality;
            var foundNode = (value[dimension] < node.Value[dimension]) ?
                Find(value, node.LeftChild, depth + 1) : Find(value, node.RightChild, depth + 1);

            return foundNode;
        }

        /// <summary>
        /// Finds all nodes in the tree that lie within the specified range of a location.
        /// </summary>
        /// <param name="location">The location for which to find the nearest node.</param>
        /// <param name="range">The range in which to search for nodes.</param>
        /// <returns>A collection of nodes with distance from <paramref name="location"/> less than
        /// <paramref name="range"/>.</returns>
        public IEnumerable<Vector> FindInRange(Vector location, float range)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            var nodesList = new List<Vector>();
            FindInRange(location, this.Root, range, nodesList, 0);

            return nodesList.AsReadOnly();
        }

        private void FindInRange(Vector location,
            KdTreeNode node, float range, IList<Vector> valuesList, int depth)
        {
            if (node == null)
                return;

            var dimension = depth % this.Dimensionality;
            var distance = (node.Value - location).Norm(this.Dimensionality);

            // Add current node to list if it lies within given range.
            // Current node cannot be same as search location.
            if (!Precision.AlmostEqual(distance, 0.0f) &&
                (distance < range))
            {
                valuesList.Add(node.Value);
            }

            // Check for nodes in sub-tree of near child.
            var nearChildNode = (location[dimension] < node.Value[dimension]) ?
                node.LeftChild : node.RightChild;

            if (nearChildNode != null)
            {
                FindInRange(location, nearChildNode, range, valuesList, depth + 1);
            }

            // Check whether splitting hyperplane given by current node intersects with hypersphere of current
            // smallest distance around given location.
            if ((range > Math.Abs(node.Value[dimension] - location[dimension])))
            {
                // Check for nodes in sub-tree of far child.
                var farChildNode = nearChildNode == node.LeftChild ? node.RightChild : node.LeftChild;

                if (farChildNode != null)
                {
                    FindInRange(location, farChildNode, range, valuesList, depth + 1);
                }
            }
        }
        #endregion

        #region ICollection Methods
        /// <summary>
        /// Determines whether the specified value is the value of any node in the tree.
        /// </summary>
        /// <param name="value">The value to locate in the tree.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> was found in the tree;
        /// <see langword="false"/>, otherwise. </returns>
        public bool Contains(Vector value)
        {
            return Find(value) != null;
        }

        bool ICollection<Vector>.IsReadOnly
        {
            get { return false; }
        }

        int ICollection<Vector>.Count
        {
            get { return this.numNodes; }
        }

        void ICollection<Vector>.Add(Vector item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<Vector>.Remove(Vector item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies the values of all the nodes in the tree to the specified array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array that is the destination of the copied elements.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(Vector[] array, int arrayIndex)
        {
            var enumerator = GetEnumerator();
            var index = arrayIndex;
            while (enumerator.MoveNext())
                array[index++] = enumerator.Current;
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through the nodes in the tree.
        /// </summary>
        /// <returns>An enumerator for the tree.</returns>
        public IEnumerator<Vector> GetEnumerator()
        {
            // Perform breadth-first search of tree, yielding every node found.
            var visitedNodes = new Stack<KdTreeNode>();
            visitedNodes.Push(this.Root);

            while (visitedNodes.Count > 0)
            {
                var node = visitedNodes.Pop();
                yield return node.Value;

                if (node.LeftChild != null)
                    visitedNodes.Push(node.LeftChild);
                if (node.RightChild != null)
                    visitedNodes.Push(node.RightChild);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        #endregion
    }

}
