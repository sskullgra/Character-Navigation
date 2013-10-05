using UnityEngine;
using System.Collections.Generic;
using JumpPointSearach;

namespace JumpPointSearach
{
	/// <summary>
	/// Node class.
	/// Author: Radosław Bigaj
	/// POLITECHNIKA ŚLĄSKA 
	/// </summary>
	public class Node
	{
		#region Consts
		
		/// <summary>
		/// Constant SQRT(2).
		/// </summary>
		private const float SQRT_2 = 1.414213562373095f;
		
		#endregion
		
		#region Fields
		
		/// <summary>
		/// The cost function value.
		/// </summary>
		private float mCostFunction;
		
		/// <summary>
		/// The heuristic estimate cost value.
		/// </summary>
		private readonly float mHeuristicEstimateCost = 0;
		
		/// <summary>
		/// The current cost from start.
		/// </summary>
		private float mCostFromStart = 0;
		
		/// <summary>
		/// The neighbors list.
		/// </summary>
		private List<Node> mNeighbors = null;
		
		/// <summary>
		/// The m x coordinate.
		/// </summary>
		private readonly int mX = 0;
		/// <summary>
		/// The y coordinate.
		/// </summary>
		private readonly int mY = 0;
		
		/// <summary>
		/// The node position.
		/// </summary>
		private Vector3 mNodePosition;
		
		/// <summary>
		/// The parent node.
		/// </summary>
		private Node mParentNode = null;
		/// <summary>
		/// Is destination node.
		/// </summary>
		private readonly bool mIsDestinationNode = false;
		
		#endregion
		
		#region Properties
		
		/// <summary>
		/// Gets or sets the neighbors.
		/// </summary>
		/// <value>
		/// The neighbors.
		/// </value>
		public List<Node> Neighbors {
			get { return mNeighbors; }
			set { mNeighbors = value;}
		}
		
		/// <summary>
		/// Gets a value indicating whether this instance is destination node.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is destination node; otherwise, <c>false</c>.
		/// </value>
		public bool IsDestinationNode {
			get { return mIsDestinationNode;}
		}
		
		/// <summary>
		/// Gets the x.
		/// </summary>
		/// <value>
		/// The x.
		/// </value>
		public int X {
			get { return mX;}
		}
		
		/// <summary>
		/// Gets the y.
		/// </summary>
		/// <value>
		/// The y.
		/// </value>
		public int Y {
			get { return mY;}
		}
		
		/// <summary>
		/// Gets or sets the parent node.
		/// </summary>
		/// <value>
		/// The parent node.
		/// </value>
		public Node ParentNode {
			get { return mParentNode;}
			set { mParentNode = value;}
		}
		
		/// <summary>
		/// Gets or sets the node position.
		/// </summary>
		/// <value>
		/// The node position.
		/// </value>
		public Vector3 NodePosition {
			get { return mNodePosition;}
			set { mNodePosition = value;}
		}
		
		/// <summary>
		/// Gets or sets the cost function.
		/// </summary>
		/// <value>
		/// The cost function.
		/// </value>
		public float CostFunction {
			get { return mCostFunction;}
			set { mCostFunction = value;}
		}
		
		/// <summary>
		/// Gets the heuristic estimate cost.
		/// </summary>
		/// <value>
		/// The heuristic estimate cost.
		/// </value>
		public float HeuristicEstimateCost {
			get { return mHeuristicEstimateCost;}
		}
		
		/// <summary>
		/// Gets or sets the cost from start.
		/// </summary>
		/// <value>
		/// The cost from start.
		/// </value>
		public float CostFromStart {
			get { return mCostFromStart;}
			set { mCostFromStart = value;}
		}
		

		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JumpPointSearach.Node"/> class.
		/// </summary>
		/// <param name='parentNode'>
		/// Parent node.
		/// </param>
		/// <param name='x'>
		/// X.
		/// </param>
		/// <param name='y'>
		/// Y.
		/// </param>
		/// <param name='nodePosition'>
		/// Node position.
		/// </param>
		/// <param name='destinationPosition'>
		/// Destination position.
		/// </param>
		public Node (Node parentNode, int x, int y, Vector3 nodePosition, Vector3 destinationPosition)
		{
			
			if (parentNode != null) {
				mParentNode = parentNode; 
				mCostFromStart = mParentNode.CostFromStart + Node.estimate (mParentNode.NodePosition, nodePosition);
				;
			}
			
			mX = x;
			mY = y;
			mNodePosition = nodePosition;
			
			
			mIsDestinationNode = (mNodePosition == destinationPosition);
	
			
			mHeuristicEstimateCost = Node.estimate (nodePosition, destinationPosition);
			
			//  f = g + h
			mCostFunction = mCostFromStart + mHeuristicEstimateCost;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JumpPointSearach.Node"/> class.
		/// </summary>
		/// <param name='parentNode'>
		/// Parent node.
		/// </param>
		/// <param name='x'>
		/// X.
		/// </param>
		/// <param name='y'>
		/// Y.
		/// </param>
		/// <param name='nodePosition'>
		/// Node position.
		/// </param>
		/// <param name='destinationPosition'>
		/// Destination position.
		/// </param>
		public Node (Node parentNode, int x, int y, Vector3? nodePosition, Vector3? destinationPosition)
		{
			
			
			if (parentNode != null) {
				mParentNode = parentNode;
				mCostFromStart = mParentNode.CostFromStart + Node.estimate (mParentNode.NodePosition, nodePosition.Value);
				;
			}
			
			mX = x;
			mY = y;
			mNodePosition = nodePosition.Value;
			
			mIsDestinationNode = (mNodePosition == destinationPosition.Value);
			
			mHeuristicEstimateCost = Node.estimate (nodePosition.Value, destinationPosition.Value);
			
			//  f = g + h
			mCostFunction = mCostFromStart + mHeuristicEstimateCost;
		}
		
		/// <summary>
		/// Estimate the specified current and goal position.
		/// </summary>
		/// <param name='cur'>
		/// Current node position.
		/// </param>
		/// <param name='goal'>
		/// Goal node position.
		/// </param>
		public static float estimate (Vector3 cur, Vector3 goal)
		{
			float diagonal = Mathf.Min (Mathf.Abs (cur.x - goal.x), Mathf.Abs (cur.z - goal.z));
			return SQRT_2 * diagonal + ((Mathf.Abs (cur.x - goal.x) + Mathf.Abs (cur.z - goal.z)) - 2 * diagonal);
		}
		
		/// <summary>
		/// Determines whether the specified <see cref="Node"/> is equal to the current <see cref="JumpPointSearach.Node"/>.
		/// </summary>
		/// <param name='item'>
		/// The <see cref="Node"/> to compare with the current <see cref="JumpPointSearach.Node"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="Node"/> is equal to the current <see cref="JumpPointSearach.Node"/>;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool Equals (Node item)
		{
			if (item == null)
				return false;
			
			return (mX == item.X && mY == item.Y);
		}
		
		#endregion
	} 
	
}