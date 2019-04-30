using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Experimental.Graph;

namespace GraphGen
{
    public class GraphVisNode
    {
        private readonly string _label;

        public ProjectGraphNode Node { get; }
        public ImmutableList<string> Targets { get; }
        public string Name { get; }

        public GraphVisNode(ProjectGraphNode node, ImmutableList<string> targets = null)
        {
            Node = node;
            Targets = targets;
            var (name, label) = GetNodeInfo(node);
            Name = name;
            _label = label;
        }

        internal string Create()
        {
            
            var globalPropertiesString = string.Join("\n", Node.ProjectInstance.GlobalProperties.OrderBy(kvp => kvp.Key).Where(kvp => kvp.Key != "IsGraphBuild").Select(kvp => $"{kvp.Key}={kvp.Value}"));
            if (globalPropertiesString.StartsWith("TargetFramework="))
            {
                globalPropertiesString = globalPropertiesString.Substring("TargetFramework=".Length);
            }

            if (globalPropertiesString != "")
            {
                globalPropertiesString = "\n" + globalPropertiesString;
            }

            var buildTargetsString = "";
            if (Targets != null)
            {
                buildTargetsString = "\nTargets={" + string.Join(",", Targets.Distinct()) +"}";
            }

            var additionalGraphVis = "";

            if (Node.ProjectReferences.Count == 0)
            {
                // Mark leaf nodes with light blue
                additionalGraphVis += "color=\"0.650 0.200 1.000\"";
            }

            if (additionalGraphVis != "")
            {
                additionalGraphVis = ", " + additionalGraphVis;
            }

            return $"  {Name} [label=\"{_label}{globalPropertiesString}{buildTargetsString}\", shape=box{additionalGraphVis}];";
        }

        // Ensure the same number is returned for the same ProjectGraphNode object
        private static Dictionary<ProjectGraphNode, string> _nodes = new Dictionary<ProjectGraphNode, string>();
        public static int _count = 1;

        private static (string, string) GetNodeInfo(ProjectGraphNode node)
        {
            var label = Path.GetFileNameWithoutExtension(node.ProjectInstance.FullPath);
            if (!_nodes.ContainsKey(node))
            {
                _nodes.Add(node, label.Replace(".", string.Empty) + _count);
                _count++;
            }
            var name = _nodes[node];
            //var name = _current;//label + Program.HashGlobalProps(node.ProjectInstance.GlobalProperties);
            
            return (name, label);
        }
    }
}