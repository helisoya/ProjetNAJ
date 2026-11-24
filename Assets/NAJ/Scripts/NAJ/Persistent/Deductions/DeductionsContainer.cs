using System.Collections.Generic;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using TMPro;
using UnityEngine;

namespace NAJ.Persistent
{
    /// <summary>
	/// Handles the game's deductions
	/// </summary>
    [System.Serializable]
    public class DeductionsContainer : DataContainer
    {
        private Dictionary<string, DeductionNode> nodes;

        public DataContainer CloneContainer()
        {
            return new DeductionsContainer()
            {

            };
        }

        public void Initialize(ANFSettings settings)
        {
            GenerateVariablesList(settings.generalDataPath + "deductions");
            Reset();
        }

        public void Reset()
        {
            int currentIdx = 0;
            List<string> ids = new List<string>(nodes.Keys);

            while (currentIdx < nodes.Count)
            {
                string currentId = ids[currentIdx];
                if (nodes[currentId].deleteOnReset)
                    nodes.Remove(currentId);
                else
                {
                    nodes[currentId].SetVisible(nodes[currentId].defaultIsVisible);
                    nodes[currentId].SetIsActiveLink(nodes[currentId].defaultIsActiveLink);

                }
                currentIdx++;
            }
        }

        /// <summary>
		/// Creates a user deduction node that will be destroyed on reset
		/// </summary>
		/// <param name="nodeId">The node's id</param>
		/// <param name="isVisible">True if the node is visible by default</param>
		/// <param name="nodePosition">The node's position</param>
        /// <param name="labelPosition">The node's label position</param>
        /// <param name="uiSizeFactor">The node's size factor</param>
		/// <param name="linkedNode">The linked node if applicable</param>
		/// <param name="isActiveLink">True if active when linked to a node</param>
		/// <param name="isEndNode">True if it is an end node</param>
		/// <param name="endScript">The end script for an end node</param>
		/// <param name="prerequistes">The node prerequiste nodes</param>
        public void CreateUserNode(string nodeId, bool isVisible, Vector2Int nodePosition,
            DeductionNode.LabelPosition labelPosition, float uiSizeFactor, string linkedNode = null, bool isActiveLink = true,
            bool isEndNode = true, string endScript = null, string[] prerequistes = null)
        {
            if (!nodes.ContainsKey(nodeId))
            {
                nodes.Add(nodeId, new DeductionNode(nodeId, isVisible, nodePosition, labelPosition,
                    uiSizeFactor, true, linkedNode, isActiveLink,
                    isEndNode, endScript, prerequistes));
            }
        }

        /// <summary>
		/// Gets all existing nodes
		/// </summary>
		/// <returns>The nodes</returns>
        public Dictionary<string, DeductionNode> GetNodes()
        {
            return nodes;
        }

        /// <summary>
		/// Gets a specific deduction node
		/// </summary>
		/// <param name="nodeId">The node's id</param>
		/// <returns>The node if found</returns>
        public DeductionNode GetNode(string nodeId)
        {
            if (nodes.TryGetValue(nodeId, out DeductionNode node))
                return node;

            return null;
        }

        public void Save(JSON json)
        {
            JArray array = new JArray();
            JSON temp;
            foreach (DeductionNode node in nodes.Values)
            {
                temp = new JSON();
                temp.Add("id", node.id);
                temp.Add("isVisible", node.isVisible);
                temp.Add("isActiveLink", node.isActiveLink);
                temp.Add("deleteOnReset", node.deleteOnReset);

                if (node.deleteOnReset)
                {
                    if (node.linkedNode != null)
                        temp.Add("linkedNode", node.linkedNode);
                    temp.Add("isEndNode", node.isEndNode);
                    if (node.endScript != null)
                        temp.Add("endScript", node.endScript);
                    temp.Add("prerequistes", node.prerequistes);
                    temp.Add("nodePosition", node.nodePosition);
                    temp.Add("uiSizeFactor", node.uiSizeFactor);
                    temp.Add("labelPosition", (int)node.labelPosition);
                    temp.Add("defaultIsVisible", node.defaultIsVisible);
                    temp.Add("defaultIsActiveLink", node.defaultIsActiveLink);
                }
                array.Add(temp);
            }

            json.Add("nodes", array);
        }

        public bool Load(JSON json)
        {
            Reset();

            if (json.ContainsKey("nodes"))
            {
                JArray array = json.GetJArray("nodes");
                JSON tmp;

                foreach (JValue variable in array.Values)
                {
                    if (variable is JSON)
                    {
                        tmp = variable as JSON;

                        bool deleteOnReset = tmp.GetBool("deleteOnReset");
                        string id = tmp.GetString("id");
                        bool isActiveLink = tmp.GetBool("isActiveLink");
                        bool isVisible = tmp.GetBool("isVisible");

                        if (!deleteOnReset)
                        {
                            if (nodes.TryGetValue(id, out DeductionNode node))
                            {
                                node.SetVisible(isVisible);
                                node.SetIsActiveLink(isActiveLink);
                                break;
                            }
                        }
                        else
                        {
                            string linkedNode = null;
                            bool isEndNode = false;
                            string endScript = null;
                            string[] prerequistes = null;
                            Vector2Int nodePosition = Vector2Int.zero;
                            DeductionNode.LabelPosition labelPosition = DeductionNode.LabelPosition.Left;
                            float uiSizeFactor = 1.0f;
                            bool defaultIsVisible = true;
                            bool defaultIsActiveLink = true;

                            if (tmp.ContainsKey("linkedNode"))
                                linkedNode = tmp.GetString("linkedNode");
                            if (tmp.ContainsKey("uiSizeFactor"))
                                uiSizeFactor = tmp.GetFloat("uiSizeFactor");
                            if (tmp.ContainsKey("endScript"))
                                endScript = tmp.GetString("endScript");
                            if (tmp.ContainsKey("isEndNode"))
                                isEndNode = tmp.GetBool("isEndNode");
                            if (tmp.ContainsKey("prerequistes"))
                                prerequistes = tmp.GetJArray("prerequistes").AsStringArray();
                            if (tmp.ContainsKey("nodePosition"))
                                nodePosition = tmp.GetJArray("nodePosition").AsVector2Int();
                            if (tmp.ContainsKey("labelPosition"))
                                labelPosition = (DeductionNode.LabelPosition)tmp.GetInt("labelPosition");
                            if (tmp.ContainsKey("defaultIsVisible"))
                                defaultIsVisible = tmp.GetBool("defaultIsVisible");
                            if (tmp.ContainsKey("defaultIsActiveLink"))
                                defaultIsActiveLink = tmp.GetBool("defaultIsActiveLink");

                            DeductionNode deductionNode = new DeductionNode(id, defaultIsVisible, nodePosition, labelPosition,
                                uiSizeFactor, deleteOnReset, linkedNode, defaultIsActiveLink, isEndNode, endScript, prerequistes);
                            deductionNode.SetVisible(isVisible);
                            deductionNode.SetIsActiveLink(isActiveLink);
                            nodes.Add(id, deductionNode);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
		/// Generates the deduction node list
		/// </summary>
		/// <param name="filePath">The path to the node file</param>
        private void GenerateVariablesList(string filePath)
        {
            nodes = new Dictionary<string, DeductionNode>();

            if (!PersistentDataManager.instance.GetPlayerData().GetComponent(out ResourceManager resourceManager))
                return;

            string currentId = null;
            Vector2Int position = Vector2Int.zero;
            DeductionNode.LabelPosition labelPosition = DeductionNode.LabelPosition.Left;
            bool isVisible = true;
            string linkedNode = null;
            bool isActiveLink = true;
            bool isEndNode = false;
            string endScript = null;
            float uiSizeFactor = 1.0f;
            string[] prerequistes = null;

            List<string> lines = FileManager.ReadTextAsset(resourceManager.GetResource<TextAsset>(filePath));
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2)
                {
                    string newId = line.Substring(1, line.Length - 2);

                    if (currentId != null)
                    {
                        nodes.Add(currentId, new DeductionNode(currentId, isVisible,
                            position, labelPosition, uiSizeFactor, false, linkedNode, isActiveLink,
                            isEndNode, endScript, prerequistes));
                    }

                    currentId = newId;
                    position = Vector2Int.zero;
                    isVisible = true;
                    linkedNode = null;
                    isActiveLink = true;
                    isEndNode = false;
                    endScript = null;
                    prerequistes = null;
                    uiSizeFactor = 1.0f;
                    labelPosition = DeductionNode.LabelPosition.Left;
                }
                else
                {
                    string cleanedLine;
                    string[] split;
                    if (line.StartsWith("position:"))
                    {
                        cleanedLine = line.Substring("position:".Length).Replace(" ", "").Replace("\t", "");
                        split = cleanedLine.Split(',');

                        if (split.Length == 2)
                        {
                            if (int.TryParse(split[0], out int x))
                                position.x = x;
                            if (int.TryParse(split[1], out int y))
                                position.y = y;
                        }
                    }
                    else if (line.StartsWith("isVisible:"))
                    {
                        cleanedLine = line.Substring("isVisible:".Length).Replace(" ", "").Replace("\t", "").ToLower();
                        if (bool.TryParse(cleanedLine, out bool result))
                            isVisible = result;
                    }
                    else if (line.StartsWith("uiSize:"))
                    {
                        cleanedLine = line.Substring("uiSize:".Length).Replace(" ", "").Replace("\t", "").ToLower();
                        if (float.TryParse(cleanedLine, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                            uiSizeFactor = result;
                    }
                    else if (line.StartsWith("linkedNode:"))
                    {
                        cleanedLine = line.Substring("linkedNode:".Length).Replace(" ", "").Replace("\t", "");
                        linkedNode = cleanedLine;
                    }
                    else if (line.StartsWith("isActiveLink:"))
                    {
                        cleanedLine = line.Substring("isActiveLink:".Length).Replace(" ", "").Replace("\t", "").ToLower();
                        if (bool.TryParse(cleanedLine, out bool result))
                            isActiveLink = result;
                    }
                    else if (line.StartsWith("isEndNode:"))
                    {
                        cleanedLine = line.Substring("isEndNode:".Length).Replace(" ", "").Replace("\t", "").ToLower();
                        if (bool.TryParse(cleanedLine, out bool result))
                            isEndNode = result;
                    }
                    else if (line.StartsWith("endScript:"))
                    {
                        cleanedLine = line.Substring("endScript:".Length).Replace(" ", "").Replace("\t", "");
                        endScript = cleanedLine;
                    }
                    else if (line.StartsWith("prerequistes:"))
                    {
                        cleanedLine = line.Substring("prerequistes:".Length).Replace(" ", "").Replace("\t", "");
                        prerequistes = cleanedLine.Split(',');
                    }
                    else if (line.StartsWith("textPosition:"))
                    {
                        cleanedLine = line.Substring("textPosition:".Length).Replace(" ", "").Replace("\t", "").ToLower();
                        switch (cleanedLine)
                        {
                            case "left":
                                labelPosition = DeductionNode.LabelPosition.Left;
                                break;
                            case "top":
                                labelPosition = DeductionNode.LabelPosition.Top;
                                break;
                            case "bottom":
                                labelPosition = DeductionNode.LabelPosition.Bottom;
                                break;
                            case "right":
                                labelPosition = DeductionNode.LabelPosition.Right;
                                break;
                        }
                    }

                }
            }

            if (currentId != null)
            {
                nodes.Add(currentId, new DeductionNode(currentId, isVisible,
                    position, labelPosition, uiSizeFactor, false, linkedNode, isActiveLink,
                    isEndNode, endScript, prerequistes));
            }

            lines = null;
            resourceManager.ReleaseResource<TextAsset>(filePath);
        }
    }
}

