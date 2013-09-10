using UnityEngine;
using System.Collections.Generic;
using JumpPointSearach;

namespace JumpPointSearach
{
	public class Node
	{
		#region Consts
		
		private const float SQRT_2 = 1.414213562373095f;
		
		#endregion
		
		#region Fields
		private float mCostFunction;
		private readonly float mHeuristicEstimateCost = 0;
		private float mCostFromStart = 0;
		private List<Node> mNeighbors = null;
		private readonly int mX = 0;
		private readonly int mY = 0;
		private Vector3 mNodePosition;
		private Node mParentNode = null;
		private readonly bool mIsDestinationNode = false;
		
		
		#endregion
		
		#region Properties
		
		public List<Node> Neighbors {
			get { return mNeighbors; }
			set { mNeighbors = value;}
		}
		
		public bool IsDestinationNode {
			get { return mIsDestinationNode;}
		}
		
		public int X {
			get { return mX;}
		}
		
		public int Y {
			get { return mY;}
		}
		
		public Node ParentNode {
			get { return mParentNode;}
			set { mParentNode = value;}
		}
		
		public Vector3 NodePosition {
			get { return mNodePosition;}
			set { mNodePosition = value;}
		}
		
		public float CostFunction {
			get { return mCostFunction;}
			set { mCostFunction = value;}
		}
		
		public float HeuristicEstimateCost {
			get { return mHeuristicEstimateCost;}
		}
		
		public float CostFromStart {
			get { return mCostFromStart;}
			set { mCostFromStart = value;}
		}
		

		#endregion
		
		#region Constructors
		
		public Node (Node parentNode, int x, int y, Vector3 nodePosition, Vector3 destinationPosition)
		{
			
			if (parentNode != null) {
				mParentNode = parentNode; 
				mCostFromStart = mParentNode.CostFromStart + Node.estimate (mParentNode.NodePosition, nodePosition);;
			}
			
			mX = x;
			mY = y;
			mNodePosition = nodePosition;
			
			
			mIsDestinationNode = (mNodePosition == destinationPosition);
	
			
			mHeuristicEstimateCost = Node.estimate (nodePosition, destinationPosition);
			
			//  f = g + h
			mCostFunction = mCostFromStart + mHeuristicEstimateCost;
		}
		
		public Node (Node parentNode, int x, int y, Vector3? nodePosition, Vector3? destinationPosition)
		{
			
			
			if (parentNode != null) {
				mParentNode = parentNode;
				mCostFromStart = mParentNode.CostFromStart + Node.estimate (mParentNode.NodePosition, nodePosition.Value);;
			}
			
			mX = x;
			mY = y;
			mNodePosition = nodePosition.Value;
			
			mIsDestinationNode = (mNodePosition == destinationPosition.Value);
			
			mHeuristicEstimateCost = Node.estimate (nodePosition.Value, destinationPosition.Value);
			
			//  f = g + h
			mCostFunction = mCostFromStart + mHeuristicEstimateCost;
		}
		
		public static float estimate (Vector3 cur, Vector3 goal)
		{
			float diagonal = Mathf.Min (Mathf.Abs (cur.x - goal.x), Mathf.Abs (cur.z - goal.z));
			return SQRT_2 * diagonal + ((Mathf.Abs (cur.x - goal.x) + Mathf.Abs (cur.z - goal.z)) - 2 * diagonal);
		}
		
		public bool Equals (Node item)
		{
			if (item == null)
				return false;
			
			return (mX == item.X && mY == item.Y);
		}
		
		#endregion
	} 
	
}