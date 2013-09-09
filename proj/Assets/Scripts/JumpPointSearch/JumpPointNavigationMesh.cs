using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JumpPointSearach;

public class JumpPointNavigationMesh : MonoBehaviour
{
	
	#region Public fields
	
	public GameObject Gizmo;
	public Material PathMaterial;
	public Transform StartTransform;
	public Transform DestinationTransform;
	public int resolution = 2;
	public int ApproximationRadius = 1;
	public float DistanceTolerance = 4.0f;
	public float MaxHeightDelta = 0.001f;
	public Vector3 InitialMappingPoint;
	
	
	#endregion
		
	#region Fields
	
	private Vector3 mTerrainPosition;
	private Vector3 mGizmoHeight = new Vector3();
	private Vector3 mMeshScale;
	private Vector3 mTerrainScale;
	private float[,] mHeightMap;
	private int mWidth = 0;
	private int mHeight = 0;
	private int mTerrainResolution = 4;

	private List<Object> mPaths = new List<Object> ();
	List<Node> mNeighbors = null;
	
	#endregion
	
	#region Logs
	
	private string terrainColliderMissing = "TerrainCollider is missing!";
	private string initialPointIsNull = "Initial mapping point";
	
	#endregion
	
	void Start ()
	{
		TerrainCollider terrainCollider = GetComponent<TerrainCollider> ();
		
		if (terrainCollider == null) {
			Debug.LogWarning (terrainColliderMissing);
			return;
		}
		
		InitTerrainData (terrainCollider);

		Point initPoint = GetGridPosition (InitialMappingPoint);
		
		if (initPoint == null) {
			Debug.LogWarning (initialPointIsNull);
			return;
		}
		
	}
	
	void OnGUI ()
	{
		if (GUI.Button (new Rect (10, 10, 100, 30), "Find path")) {
			Gizmo.renderer.material = PathMaterial;
			
			Point startCor = GetGridPosition(StartTransform.position);
			Vector3 startVec = GetWorldPosition(startCor.X, startCor.Y);
			
			Point endCor = GetGridPosition(DestinationTransform.position);
			Vector3 endVec = GetWorldPosition(endCor.X, endCor.Y);

			
			//StartCoroutine (findPath (StartTransform.position, DestinationTransform.position));
			
			StartCoroutine(FindPath(startVec, endVec));
		}
	}
	
	private void InitTerrainData (TerrainCollider terrainCollider)
	{
		mTerrainPosition = GetComponent<Transform> ().position;
		TerrainData terrainData = terrainCollider.terrainData;
		
		mWidth = terrainData.heightmapWidth;
		mHeight = terrainData.heightmapWidth;
		
		mTerrainScale = terrainData.size;
		
		mTerrainResolution = (int)Mathf.Pow (2, resolution);
		CalculateMeshScale ();
		float[,] originHeightMap = terrainData.GetHeights (0, 0, mWidth, mHeight);
		
		mWidth = (terrainData.heightmapWidth - 1) / mTerrainResolution + 1;
		mHeight = (terrainData.heightmapHeight - 1) / mTerrainResolution + 1;
		
		mHeightMap = new float[mWidth, mHeight];
		
		for (int x = 0; x < mWidth; x++) {
			for (int y = 0; y < mHeight; y++) {
				mHeightMap [y, x] = originHeightMap [x * mTerrainResolution, y * mTerrainResolution];
			}
		}
		
		originHeightMap = null;
		
		 RemoveTrees (terrainData.treeInstances);
		
	}
	
	private void CalculateMeshScale ()
	{
		float scaledX = mTerrainScale.x / (mWidth - 1) * mTerrainResolution;
		float scaledY = mTerrainScale.y;
		float scaledZ = mTerrainScale.z / (mHeight - 1) * mTerrainResolution;
		mMeshScale = new Vector3 (scaledX, scaledY, scaledZ);
		
	}
	
	private void RemoveTrees (TreeInstance[] trees)
	{
		int treeX = 0;
		int treeY = 0;
		
		foreach (TreeInstance tree in trees) {
			treeX = (int)(tree.position.x * mWidth);
			treeY = (int)(tree.position.y * mHeight);
			
			//  - - - 
			//  - x -
			//  - - -
			if (IsTreeInRange (tree.position, treeX, treeY)) {
				mHeightMap [treeX, treeY] = float.MaxValue;
			}
			
			for (int i = 1; i < ApproximationRadius; i++) {
				// // checked positions
				// x x x
				// - - -
				// - - - 
				if (IsTreeInRange (tree.position, treeX - i, treeY + i)) {
					mHeightMap [treeX - i, treeY + i] = float.MaxValue;
				}
				if (IsTreeInRange (tree.position, treeX, treeY + i)) {
					mHeightMap [treeX, treeY + i] = float.MaxValue;
				}
				if (IsTreeInRange (tree.position, treeX + i, treeY + i)) {
					mHeightMap [treeX + i, treeY + i] = float.MaxValue;
				}
				// checked positions
				// - - -
				// x - x
				// - - -
				if (IsTreeInRange (tree.position, treeX - i, treeY)) {
					mHeightMap [treeX - i, treeY] = float.MaxValue;
				}
				if (IsTreeInRange (tree.position, treeX + i, treeY)) {
					mHeightMap [treeX + i, treeY] = float.MaxValue;
				}
                
				// checked positions
				// - - -
				// - - -
				// x x x
				if (IsTreeInRange (tree.position, treeX - i, treeY - i)) {
					mHeightMap [treeX - i, treeY - i] = float.MaxValue;
				}
				if (IsTreeInRange (tree.position, treeX, treeY - i)) { 
					mHeightMap [treeX, treeY - i] = float.MaxValue;
				}
				if (IsTreeInRange (tree.position, treeX + i, treeY - i)) {
					mHeightMap [treeX + i, treeY - i] = float.MaxValue;
				}
			}
			
		}
	}
	
	
	private Point GetGridPosition (Vector3 position)
	{
		int x = (int)(position.x / mMeshScale.x);
		int y = (int)(position.z / mMeshScale.z);
		
		if (y < 0 || x < 0 || x > mWidth - 1 || y > mHeight - 1) {
			return null;
		} else {
			return new Point (x, y);
		}
		
	}
	
	private Vector3 GetWorldPosition (int x, int y)
	{
		float scaledX = mMeshScale.x * x;
		float scaledY = mMeshScale.y * mHeightMap [x, y];
		float scaledZ = mMeshScale.z * y;
		Vector3 worldPosistion = new Vector3 (scaledX, scaledY, scaledZ) + mTerrainPosition;
		return worldPosistion;
		
	}
	
	private bool IsTreeInRange (Vector3 treePosition, int x, int y)
	{
		if (x < mWidth && y < mHeight && y - 1 > -1 && x - 1 > -1) {
			Vector3 treeScale = Vector3.Scale (mTerrainScale, treePosition) + mTerrainPosition;
			float treeDistance = Vector3.Distance (treeScale, GetWorldPosition (x, y));
			return (treeDistance < DistanceTolerance);
		} else {
			return false;
		}
	}
	
	private bool isReachable(int px, int py, int x, int y){
       if(x > -1 && y > -1 && x < mWidth && y < mHeight) return (Mathf.Abs(mHeightMap[px,py] - mHeightMap[x,y]) < MaxHeightDelta);
       return false;
    }
	
	
	private List<Node> GetNeighbors (Node currentNode, Vector3 destinationNode)
	{
		mNeighbors = new List<Node> ();
		Node parentNode = currentNode.ParentNode;
		bool condition;
		if (parentNode != null) {
			int dx = Mathf.Min (Mathf.Max (-1, currentNode.X - parentNode.X), 1);
			int dy = Mathf.Min (Mathf.Max (-1, currentNode.Y - parentNode.Y), 1);
			
			// diagonal expanding
			if (dx != 0 && dy != 0) {
				condition = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + dy);
				checkAndCreateNeighbor (currentNode, 0, dy, destinationNode, condition);
				
				condition = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y);
				checkAndCreateNeighbor (currentNode, dx, 0, destinationNode, condition);
				
				condition = (mNeighbors.Count > 0 && isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y + dy)); 
				checkAndCreateNeighbor (currentNode, dx, dy, destinationNode, condition);
				
				bool tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X - dx, currentNode.Y);
				bool tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + dy);
				bool tempCondition3 = isReachable (currentNode.X, currentNode.Y, currentNode.X - dx, currentNode.Y - dy);
				
				condition = tempCondition1 == false && tempCondition2 && tempCondition3;
				checkAndCreateNeighbor (currentNode, -dx, -dy, destinationNode, condition);
				
				tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y - dy);
				tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y);
				tempCondition3 = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y - dy);
				
				condition = tempCondition1 == false && tempCondition2 && tempCondition3;
				checkAndCreateNeighbor (currentNode, dx, -dy, destinationNode, condition);
				
			} else if (dx == 0) { // (vertical || horizontal) expanding
				
				if (isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + dy)) {
					condition = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + dy);
					checkAndCreateNeighbor (currentNode, 0, dy, destinationNode, condition);
					
					bool tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X + 1, currentNode.Y);
					bool tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X + 1, currentNode.Y + dy);
					condition = tempCondition1 == false && tempCondition2;
					checkAndCreateNeighbor (currentNode, 1, dy, destinationNode, condition);
					
					tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X - 1, currentNode.Y);
					tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X - 1, currentNode.Y + dy);
					condition = tempCondition1 == false && tempCondition2;
					checkAndCreateNeighbor (currentNode, -1, dy, destinationNode, condition);
				}

			} else {
				
				if (isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y)) {
					condition = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y);
					checkAndCreateNeighbor (currentNode, dx, 0, destinationNode, condition);
					
					bool tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + 1);
					bool tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y + 1);
					condition = tempCondition1 == false && tempCondition2;
					checkAndCreateNeighbor (currentNode, dx, 1, destinationNode, condition);
					
					tempCondition1 = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y - 1);
					tempCondition2 = isReachable (currentNode.X, currentNode.Y, currentNode.X + dx, currentNode.Y - 1);
					condition = tempCondition1 == false && tempCondition2;
					checkAndCreateNeighbor (currentNode, dx, -1, destinationNode, condition);
					
				}
			}
			
		} else {	
			// top check
			// x x x
			// - - -
			// - - -
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X - 1, currentNode.Y + 1);
			checkAndCreateNeighbor (currentNode, -1, 1, destinationNode, condition);
			
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y + 1);
			checkAndCreateNeighbor (currentNode, 0, 1, destinationNode, condition);
			
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X + 1, currentNode.Y + 1);
			checkAndCreateNeighbor (currentNode, 1, 1, destinationNode, condition);
			
			// center check
			// - - - 
			// x - x
			// - - - 
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X + 1, currentNode.Y);
			checkAndCreateNeighbor (currentNode, 1, 0, destinationNode, condition);
			
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X - 1, currentNode.Y);
			checkAndCreateNeighbor (currentNode, -1, 0, destinationNode, condition);
			
			// bottom check
			// - - - 
			// - - -
			// x x x
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X - 1, currentNode.Y - 1);
			checkAndCreateNeighbor (currentNode, -1, -1, destinationNode, condition);
			
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X, currentNode.Y - 1);
			checkAndCreateNeighbor (currentNode, 0, -1, destinationNode, condition);
			
			condition = isReachable (currentNode.X, currentNode.Y, currentNode.X + 1, currentNode.Y - 1);
			checkAndCreateNeighbor (currentNode, 1, -1, destinationNode, condition);
			
		}
		
		return mNeighbors;
		
	}
	
	private void checkAndCreateNeighbor (Node currentNode, int dx, int dy, Vector3 destinationTarget, bool condition)
	{
		if (condition) {
			Vector3 worldPosition = GetWorldPosition (currentNode.X + dx, currentNode.Y + dy);
			Node neighbor = new Node (currentNode, currentNode.X + dx, currentNode.Y + dy, worldPosition, destinationTarget);
			if (neighbor.Equals (currentNode.ParentNode) == false) {
				mNeighbors.Add (neighbor);
			}
		}
	}
	
	
	private Point Jump (int x, int y, int parentX, int parentY, Vector3 destinationTarget)
	{
		int dx = x - parentX;
		int dy = y - parentY;
		
		if (isReachable (parentX, parentY, x, y) == false) {
			return null;
		} else if (GetWorldPosition (x, y) == destinationTarget) {
			return new Point (x, y);
		}
		
		if (dx != 0 && dy != 0) {
			if ((isReachable (parentX, parentY, x - dx, y + dy) && 
				isReachable (parentX, parentY, x - dx, y) == false) ||
               (isReachable (parentX, parentY, x + dx, y - dy) && 
				isReachable (parentX, parentY, x, y - dy) == false)) {
				return new Point (x, y);
			}
		} else {
			if (dx != 0) {
				if ((isReachable (parentX, parentY, x + dx, y + 1) 
					&& isReachable (parentX, parentY, x, y + 1) == false) ||
                   (isReachable (parentX, parentY, x + dx, y - 1) 
					&& isReachable (parentX, parentY, x, y - 1) == false)) {
					return new Point (x, y);
				}
			} else {
				if ((isReachable (parentX, parentY, x + 1, y + dy) 
					&& isReachable (parentX, parentY, x + 1, y) == false) ||
                   (isReachable (parentX, parentY, x - 1, y + dy) 
					&& isReachable (parentX, parentY, x - 1, y) == false)) {
					return new Point (x, y);
				}
			}
		}
		
		if (dx != 0 && dy != 0) {//check for horizontal/vertical jump points
			if (Jump (x + dx, y, x, y, destinationTarget) != null || 
				Jump (x, y + dy, x, y, destinationTarget) != null) {
				return new Point (x, y);
			}
		}

		if (isReachable (parentX, parentY, x + dx, y) || isReachable (parentX, parentY, x, y + dy)) {
			return Jump (x + dx, y + dy, x, y, destinationTarget);
		} else {
			return null;
		}
		
		
	}
	
	private IEnumerator FindPath (Vector3 start, Vector3 destinationTarget)
	{
		foreach (Object path  in mPaths) {
			Destroy (path);
		}
		
		mPaths.Clear ();
		List<Vector3> paths = new List<Vector3> ();
		
		SortedHeap<Node> openList = new SortedHeap<Node> (new NodeComparer ());
		SortedHeap<Node> closedList = new SortedHeap<Node> (new NodeComparer ()); 
	
		Point startPoint = GetGridPosition (start);
		Point destinationPoint = GetGridPosition (destinationTarget);
		
		if (startPoint.X == destinationPoint.X && startPoint.Y == destinationPoint.Y) {
			yield break;
		}
		
		Vector3 worldStart = GetWorldPosition (startPoint.X, startPoint.Y);
		Vector3 worldDestination = GetWorldPosition (destinationPoint.X, destinationPoint.Y);
		
		openList.Push (new Node (null, startPoint.X, startPoint.Y, worldStart, worldDestination));
		
		Node currentNode = null;
		Node jumpNode = null;
		Point jumpPoint = null;
		int index = 0;
		float cost = 0;
		float time = Time.realtimeSinceStartup;
		
		while (openList.Count > 0) {
			currentNode = openList.Pop ();
			if (currentNode.IsDestinationNode) {
				while (currentNode != null) {
					paths.Add (currentNode.NodePosition);
					currentNode = currentNode.ParentNode;
				}
				break;
			}
			
	
			
			foreach (Node neighbor in (currentNode.Neighbors == null) ? (currentNode.Neighbors = GetNeighbors(currentNode, worldDestination)) : currentNode.Neighbors) {
				jumpPoint = Jump (neighbor.X, neighbor.Y, currentNode.X, currentNode.Y, destinationTarget);
				
				if (jumpPoint == null)
					continue;
				
				jumpNode = new Node (currentNode, jumpPoint.X, jumpPoint.Y, GetWorldPosition (jumpPoint.X, jumpPoint.Y), destinationTarget);
				if (closedList.Contains (jumpNode))
					continue;
				
				float estimatedCost = Node.estimate(jumpNode.NodePosition, currentNode.NodePosition);
				cost = currentNode.CostFromStart + estimatedCost;
				
				
				 
				if (openList.Contains (jumpNode, ref index) && cost >= jumpNode.CostFromStart)
					continue;
				
				jumpNode.ParentNode = currentNode;
				jumpNode.CostFromStart = cost;
				jumpNode.CostFunction = jumpNode.CostFromStart + jumpNode.HeuristicEstimateCost;
				
				if (index >= 0){
					openList.RemoveAt (index);
				}
				openList.Push (neighbor);
				
			}
		
			closedList.Push (currentNode);
		}
		
		Debug.Log(string.Format(timeAndNodesFormat, (Time.realtimeSinceStartup-time)*1000f, paths.Count ));
		//Debug.Log (string.Format ("Nodes in path: {0}", paths.Count));
		
		foreach (Vector3 path in paths) {
			mPaths.Add (Instantiate (Gizmo, path + mGizmoHeight * 2, Quaternion.identity));
		}
			
	}	
	
	#region Logs
	
	private const string timeAndNodesFormat = "Time {0}ms and nodes in path {1}";
	
	#endregion

}
