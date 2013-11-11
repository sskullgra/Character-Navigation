using UnityEngine;
using System.Collections.Generic;
using JumpPointSearach;

namespace JumpPointSearach
{
	/// <summary>
	/// Node comparer.
	/// Author: Radosław Bigaj
	/// POLITECHNIKA ŚLĄSKA 
	/// </summary>
	public class NodeComparer : IComparer<Node>
	{		
		/// <summary>
		/// Compare the specified firstNode and secondNode.
		/// </summary>
		/// <param name='firstNode'>
		/// First node.
		/// </param>
		/// <param name='secondNode'>
		/// Second node.
		/// </param>
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
