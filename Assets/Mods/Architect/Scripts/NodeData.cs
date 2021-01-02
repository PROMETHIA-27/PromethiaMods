using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Networking;

namespace Architect
{
    class NodeData : MonoBehaviour
    {
        public NodeElement element;

        public NodeForm form = NodeForm.Program;

        private static GameObject effectPrefab = ArchitectPlugin.ArchitectPrefabs.LoadAsset<GameObject>("ConnectionEdge");

        private Dictionary<OutputColor, NodeData> outputs;
        private Dictionary<OutputColor, GameObject> outputEffects;

        //public Dictionary<OutputColor, List<NodeData>> inputs = new Dictionary<OutputColor, List<NodeData>>();
        private List<(NodeData, OutputColor)> inputs = new List<(NodeData, OutputColor)>();

        private static Dictionary<NodeData, List<(OutputColor, NodeElement, int)>> nodeConnectionInfo = new Dictionary<NodeData, List<(OutputColor, NodeElement, int)>>();

        public static bool calculating;
        public static NodeData activeData;

        //public List<NodeData> GetAllMatchingRecursiveInputs(OutputColor color, NodeElement element, int recurses, bool onlyFinalRecursion = false, List<NodeData> results = null)
        //{
        //    if (results == null)
        //        results = new List<NodeData>();
        //    if ((!onlyFinalRecursion || recurses == 0) && inputs.TryGetValue(color, out var colorData))
        //        foreach (var data in colorData)
        //            if (data.element == element)
        //                results.Add(data);
        //    if (recurses > 0)
        //        foreach (var dataList in inputs.Values)
        //            foreach (var data in dataList)
        //                data.GetAllMatchingRecursiveInputs(color, element, recurses - 1, onlyFinalRecursion, results);
        //    return results;
        //}

        public int GetMatchingInputCount(OutputColor color, NodeElement element, int distance)
        {
            return GetConnectionInfo(this).Count(x => x == (color, element, distance));
        }

        public static List<(OutputColor, NodeElement, int)> GetConnectionInfo(NodeData data)
        {
            if (nodeConnectionInfo.TryGetValue(data, out var info))
                return info;
            nodeConnectionInfo.Add(data, new List<(OutputColor, NodeElement, int)>());
            return nodeConnectionInfo[data];
        }

        public void ConnectTo(NodeData outputTarget, OutputColor color)
        {
            if (form == NodeForm.Program)
                form = NodeForm.Provider;
            if (outputs.ContainsKey(color))
                DisconnectFrom(outputs[color], color);
            outputs[color] = outputTarget;
            outputTarget.inputs.Add((this, color));
            CreateEffect(outputTarget, color);
            var inputList = GetConnectionInfo(outputTarget);
            foreach (var tup in inputs)
                tup.Item1.ConnectTo(outputTarget, tup.Item2, 1, inputList);
            inputList.Add((color, element, 0));
        }

        private void ConnectTo(NodeData outputTarget, OutputColor color, int distance, List<(OutputColor, NodeElement, int)> inputList)
        {
            foreach (var tup in inputs)
                tup.Item1.ConnectTo(outputTarget, tup.Item2, distance + 1, inputList);
            inputList.Add((color, element, distance));
        }

        public void DisconnectFrom(NodeData outputTarget, OutputColor color)
        {
            if (outputs[color] != outputTarget)
                throw new ArgumentException("Cannot disconnect outputTarget from socket that it was not connected to!");
            outputs.Remove(color);
            if (outputs.Count == 0)
                form = NodeForm.Program;
            outputTarget.inputs.Remove((this, color));
            Destroy(outputEffects[color]);
            var inputList = GetConnectionInfo(outputTarget);
            foreach (var tup in inputs)
                tup.Item1.DisconnectFrom(outputTarget, tup.Item2, 1, inputList);
            if (!inputList.Contains((color, element, 0)))
                throw new ArgumentException("Cannot remove input from inputList that does not contain the same arguments!");
            inputList.Remove((color, element, 0));
        }

        private void DisconnectFrom(NodeData outputTarget, OutputColor color, int distance, List<(OutputColor, NodeElement, int)> inputList)
        {
            foreach (var tup in inputs)
                tup.Item1.DisconnectFrom(outputTarget, tup.Item2, distance + 1, inputList);
            if (!inputList.Contains((color, element, distance)))
                throw new ArgumentException("Cannot remove input from inputList that does not contain the same arguments!");
            inputList.Remove((color, element, distance));
        }

        private void CreateEffect(NodeData target, OutputColor color)
        {
            var line = Instantiate(effectPrefab).GetComponent<LineBetweenTransforms>();

            NetworkServer.Spawn(line.gameObject);

            line.transformNodes = new Transform[] { transform, target.transform };
        }

        private void OnDestroy()
        {
            foreach (var output in outputs)
                DisconnectFrom(output.Value, output.Key);
        }
    }

    enum NodeElement
    {
        Mass,
        Design,
        Blood,
        Soul
    }

    enum NodeForm
    {
        Provider,
        Program
    }

    enum OutputColor
    {
        Blue,
        Green,
        Red
    }
}
