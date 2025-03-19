using System;
using System.Collections.Generic;
using TestGraph.Components;

namespace Reconnect.Electronics.Graphs
{
    public static class GraphUtils
    {
        /// <summary>
        /// Determines the list of all parallel branches, grouped by branches in parallel with each other
        /// </summary>
        /// <param name="branches">The branches from which extract the parallel ones</param>
        /// <returns>The list of all parallel branches groups</returns>
        public static List<List<Branch>> GetParallelBranchGroups(List<Branch> branches)
        {
            var visited = new HashSet<Branch>(); // stores the already visited branches
            var groups = new List<List<Branch>>(); // stores the list of all parallel branches groups

            foreach (var branch in branches)
            {
                if (visited.Contains(branch)) continue; // ignore all the branches already processed by the rec function

                var group = BuildParallelBranchGroup(branch, branches, visited); // fetch all the branches in parallel, including this branch
            
                // only add if there are more than 1 branch, then they are in parallel
                if (group.Count > 1)
                    groups.Add(group);
            }

            return groups;
        }

        /// <summary>
        /// Adds to the <paramref name="group"/> of <see cref="Branch"/> the branches in parallel with the given <paramref name="branch"/>
        /// </summary>
        /// <param name="branch">The branch taken as reference</param>
        /// <param name="branches">The branches to test parallelness</param>
        /// <param name="visited">A set keeping track of already visited branches</param>
        /// <returns>The group of branches in which are added the branches in parallel with branch</returns>
        private static List<Branch> BuildParallelBranchGroup(Branch branch, List<Branch> branches, HashSet<Branch> visited)
        {
            List<Branch> group = new List<Branch>();
            visited.Add(branch);
            group.Add(branch);

            // Iterate through branches and find parallel branches that are not already visited
            foreach (var other in branches)
            {
                if (!visited.Contains(other) && branch.AreParallelBranches(other))
                {
                    visited.Add(other);
                    group.Add(other);
                }
            }

            return group;
        }

        /// <summary>
        /// Merge branches in series with the given branch
        /// </summary>
        /// <param name="branch">The branch taken as reference</param>
        /// <param name="branches">The list of branch to try merging in series if possible</param>
        public static void MergeBranchInSeries(Branch branch, List<Branch> branches)
        {
            int i = 0;
            // repeats the process until all branches have been checked or there is only one branch (cannot be merged)
            while (i < branches.Count && branches.Count > 1)
            {
                Branch graphBranch = branches[i];
                if (graphBranch == branch) // ignore if the branch is the one being tested
                {
                    i++;
                    continue;
                }

                Node? commonNode = FindCommonNode(branch, graphBranch);
                if (commonNode is not null && commonNode.AdjacentComponents.Count == 2)
                    ApplyMergeBranchesInSeries(graphBranch, branch, commonNode, branches);
                i++;
            }
        }
    
        /// <summary>
        /// Determines the node these branches have in common, if they have one
        /// </summary>
        /// <param name="branch">The first branch</param>
        /// <param name="other">The second branch</param>
        /// <returns>null if the branches have no node in common, else the common Node</returns>
        private static Node? FindCommonNode(Branch branch, Branch other)
        {
            if (branch.StartNode == other.StartNode || branch.StartNode == other.EndNode)
                return branch.StartNode;
            if (branch.EndNode == other.StartNode || branch.EndNode == other.EndNode)
                return branch.EndNode;
            return null;
        }
    
        /// <summary>
        /// Merge the branches by adding their components and removing the one to be merged from the list of branches
        /// </summary>
        /// <param name="branch">The branch that will be the result of the merge</param>
        /// <param name="branchToMerge">The branch that will be merged with branch and removed from the list branches</param>
        /// <param name="commonNode">The node these branches have in common, making them in series</param>
        /// <param name="branches">The list of branches</param>
        /// <exception cref="ArgumentException">Thrown if the commonNode is not a common node of the branches</exception>
        private static void ApplyMergeBranchesInSeries(Branch branch, Branch branchToMerge, Node commonNode, List<Branch> branches)
        {
            if (commonNode != FindCommonNode(branch, branchToMerge))
                throw new ArgumentException("The common node is not common to the branches");
            branch.StartNode = commonNode;
            branch.AddVertex(branchToMerge.Components);
            branches.Remove(branchToMerge);
        }
    }
}