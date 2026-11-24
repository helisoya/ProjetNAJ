using ANF.Utils;
using Leguar.TotalJSON;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NAJ.Persistent
{
    /// <summary>
	/// Represents a deduction node
	/// </summary>
    public class DeductionNode
    {
        /// <summary>
		/// The node's label anchor point
		/// </summary>
        public enum LabelPosition
        {
            Left,
            Right,
            Top,
            Bottom
        }
        public string id { get; private set; }
        public bool isVisible { get; private set; }
        public string linkedNode { get; private set; }
        public bool isActiveLink { get; private set; }
        public bool isEndNode { get; private set; }
        public string endScript { get; private set; }
        public string[] prerequistes { get; private set; }
        public Vector2Int nodePosition { get; private set; }
        public LabelPosition labelPosition { get; private set; }
        public float uiSizeFactor { get; private set; }
        public bool deleteOnReset { get; private set; }

        public bool defaultIsVisible { get; private set; }
        public bool defaultIsActiveLink { get; private set; }


        public DeductionNode(string id, bool isVisible, Vector2Int nodePosition, LabelPosition labelPosition, float uiSizeFactor = 1.0f,
            bool deleteOnReset = false, string linkedNode = null, bool isActiveLink = true,
            bool isEndNode = true, string endScript = null, string[] prerequistes = null)
        {
            this.id = id;
            this.isVisible = isVisible;
            this.nodePosition = nodePosition;
            this.linkedNode = linkedNode;
            this.isActiveLink = isActiveLink;
            this.isEndNode = isEndNode;
            this.endScript = endScript;
            this.prerequistes = prerequistes;
            this.deleteOnReset = deleteOnReset;
            this.labelPosition = labelPosition;
            this.uiSizeFactor = uiSizeFactor;

            this.defaultIsVisible = isVisible;
            this.defaultIsActiveLink = isActiveLink;
        }

        /// <summary>
		/// Gets if the node is currently visible and active
		/// </summary>
		/// <returns>True if fully visible</returns>
        public bool IsActiveAndVisible()
        {
            return isVisible && (string.IsNullOrEmpty(linkedNode) || isActiveLink);
        }

        /// <summary>
		/// Gets the node's name key
		/// </summary>
		/// <returns>The node's name key</returns>
        public string GetName()
        {
            return id + "_name";
        }

        /// <summary>
		/// Gets the node's description key
		/// </summary>
		/// <returns>The node's description key</returns>
        public string GetDesc()
        {
            return id + "_desc";
        }

        /// <summary>
		/// Sets if the node is visible
		/// </summary>
		/// <param name="isVisible">True if visible</param>
        public void SetVisible(bool isVisible)
        {
            this.isVisible = isVisible;
        }

        /// <summary>
		/// Sets if the node is the one active in a link
		/// </summary>
		/// <param name="isActiveLink">True if it is active</param>
        public void SetIsActiveLink(bool isActiveLink)
        {
            this.isActiveLink = isActiveLink;
        }
    }
}

