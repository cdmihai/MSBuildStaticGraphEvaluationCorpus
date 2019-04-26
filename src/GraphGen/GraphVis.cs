using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Microsoft.Build.Execution;
using Microsoft.Build.Experimental.Graph;

namespace GraphGen
{
    public class GraphVis
    {
        public static string Create(ProjectGraph graphNodes)
        {
            return Create(graphNodes, new GraphVisOptions());
        }

        public static string Create(ProjectGraph projectGraph, GraphVisOptions graphVisOptions, ICollection<string> targets = null)
        {
            // I don't really remember why I did the hash thing. I think I was concerned with duplicate nodes?
            var projects = new ConcurrentDictionary<string, ProjectGraphNode>();

            foreach (var node in projectGraph.ProjectNodes)
            {
                var propsHash = GraphVis.HashGlobalProps(node.ProjectInstance.GlobalProperties);
                projects.TryAdd(node.ProjectInstance.FullPath + propsHash, node);
            }

            var targetLists = projectGraph.GetTargetLists(targets);

            return Create(projects, graphVisOptions, targetLists);
        }

        private static string Create(ConcurrentDictionary<string, ProjectGraphNode> projects, GraphVisOptions graphVisOptions, IReadOnlyDictionary<ProjectGraphNode, ImmutableList<string>> targetsLists = null)
        {
            HashSet<ProjectGraphNode> seen = new HashSet<ProjectGraphNode>();

            var sb = new StringBuilder();
            var edges = new StringBuilder();
            //var nodes = new StringBuilder();
            var clusters = new StringBuilder();

            foreach (var group in projects
                //.Where(n => !n.Value.ProjectInstance.FullPath.Contains("dirs.proj"))
                .GroupBy(kvp => kvp.Value.ProjectInstance.FullPath, (p, plist) => new { ProjectGroupName = p, Projects = projects.Where(p2=>p2.Value.ProjectInstance.FullPath == p).ToList()}))
            {
                GraphVisCluster graphVisCluster = new GraphVisCluster(@group.ProjectGroupName);

                foreach (var fromNode in @group.Projects)
                {
                    ImmutableList<string> fromNodeTargets = targetsLists?[fromNode.Value];
                    //if (fromNodeTargets == null)
                    //{
                    //    // Nodes with no targets are not scheduled, so we skip them in the graph
                    //    continue;
                    //}

                    var fromNodeGraphVisNode = new GraphVisNode(fromNode.Value, fromNodeTargets);
                    graphVisCluster.AddNode(fromNodeGraphVisNode);

                    if (seen.Contains(fromNode.Value))
                    {
                        continue;
                    }

                    seen.Add(fromNode.Value);
                    
                    foreach (var toNode in fromNode.Value.ProjectReferences)
                    {
                        ImmutableList<string> toNodeTargets = targetsLists?[toNode];
                        //if (toNodeTargets == null)
                        //{
                        //    // If the target node is not scheduled, then we shouldn't draw the edge!
                        //    continue;
                        //}

                        var toNodeGraphVisNode = new GraphVisNode(toNode, toNodeTargets);
                        var graphVisEdge = new GraphVisEdge(fromNodeGraphVisNode, toNodeGraphVisNode);
                        edges.AppendLine(graphVisEdge.Create());
                    }
                }

                clusters.AppendLine(graphVisCluster.Create());
            }

            sb.AppendLine("digraph prof {");
            sb.AppendLine("  ratio = fill;");
            sb.AppendLine($"  nodesep = {graphVisOptions.NodeSep};");
            sb.AppendLine($"  ranksep = {graphVisOptions.RankSep};");
            sb.AppendLine("  node [style=filled];");
            sb.Append(clusters);
            sb.Append(edges);
            sb.AppendLine("}");
            GraphVisNode._count = 1;
            return sb.ToString();
        }

        public static void Save(string graphText, string outFile)
        {
            var outFileInfo = new FileInfo(outFile);

            // These three instances can be injected via the IGetStartProcessQuery, 
            //                                               IGetProcessStartInfoQuery and 
            //                                               IRegisterLayoutPluginCommand interfaces

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                getProcessStartInfoQuery,
                registerLayoutPluginCommand);

            Enums.GraphReturnType saveType;
            switch (outFileInfo.Extension)
            {
                case ".pdf":
                    saveType = Enums.GraphReturnType.Pdf;
                    break;
                case ".jpg":
                    saveType = Enums.GraphReturnType.Jpg;
                    break;
                case ".png":
                    saveType = Enums.GraphReturnType.Png;
                    break;
                default:
                    throw new Exception($"Unknown extension: {outFileInfo.Extension}");

            }

            byte[] output = wrapper.GenerateGraph(graphText, saveType);
            File.WriteAllBytes(outFile, output);

            Console.WriteLine();
            Console.WriteLine($"{output.Length} bytes written to {outFile}.");
        }

        private const char ItemSeparatorCharacter = '\u2028';

        private static string HashGlobalProps(IDictionary<string, string> globalProperties)
        {
            using (var sha1 = SHA1.Create())
            {
                var stringBuilder = new StringBuilder();
                foreach (var item in globalProperties)
                {
                    stringBuilder.Append(item.Key);
                    stringBuilder.Append(ItemSeparatorCharacter);
                    stringBuilder.Append(item.Value);
                }

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));

                stringBuilder.Clear();

                foreach (var b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }

    public class GraphVisOptions
    {
        public double RankSep { get; set; } = 3.0;
        public double NodeSep { get; set; } = .1;
    }
}