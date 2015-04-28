using MathNet.Numerics.LinearAlgebra.Single;

namespace KaptureLibrary.Trees.KdTree
{
    /// <summary>
    /// Represents a <see cref="Vector"/> node within a <see cref="KdTree"/>
    /// </summary>
    public class KdTreeNode
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KdTreeNode"/> class.
        /// </summary>
        public KdTreeNode(Vector value)
        {
            this.Value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        /// <value>The value of the node.</value>
        public Vector Value { get; internal set; }

        /// <summary>
        /// Gets the left child node of the current node.
        /// </summary>
        /// <value>The left child node.</value>
        public KdTreeNode LeftChild { get; internal set; }

        /// <summary>
        /// Gets the right child node of the current node.
        /// </summary>
        /// <value>The right child node.</value>
        public KdTreeNode RightChild { get; internal set; }
        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            const string nullNodeString = "-";

            return string.Format("{0} -> {1} ; {2}", this.Value,
                this.LeftChild == null ? nullNodeString : this.LeftChild.Value.ToString(),
                this.RightChild == null ? nullNodeString : this.RightChild.Value.ToString());
        }
    }
}
