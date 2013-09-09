using UnityEngine;
using System.Collections.Generic;
using JumpPointSearach;

namespace JumpPointSearach
{
	public class NodeComparer : IComparer<Node>
	{			
		public int Compare (Node firstNode, Node secondNode)
		{
			if (firstNode.CostFunction > secondNode.CostFunction) {
				return -1;
			} else if (firstNode.CostFunction < secondNode.CostFunction) {
				return 1;
			} else {
				if (firstNode.HeuristicEstimateCost > secondNode.HeuristicEstimateCost) {
					return -1;
				} else if (firstNode.HeuristicEstimateCost < secondNode.HeuristicEstimateCost) {
					return 1;
				} else {
					return 0;
				}
			}
		}

	}
}
