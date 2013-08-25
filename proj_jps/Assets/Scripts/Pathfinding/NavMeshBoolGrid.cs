using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NavMeshBoolGrid : MonoBehaviour {
    public GameObject gizmo;

    public Material normal,bad,path,startend;

    public int resultion = 2, gizmo_height = 2;
    public float propDistanceTolerance = 4.0f;
    public int propApproximationRadius = 1;
    public float maxHeightDelta = 0.001f;
    private Vector3 hGizmo;

    public Vector3 initialMappingPoint;

    public int sx = 0, sy = 0, gx = 0, gy = 0; 

    private Vector3 tPos;
    private float[,] hMap;
    private int tRes = 4;
    private Vector3 meshScale,terrainScale;
    private int w = 0, h = 0;
    
    private bool[,] grid = null;
 
	void Start () {
        if (GetComponent<TerrainCollider>() == null)
        {
            Debug.LogWarning("TerrainCollider missing!");
            return;
        }

        tPos = GetComponent<Transform>().position;
        TerrainData tData = GetComponent<TerrainCollider>().terrainData;


        w = tData.heightmapWidth;
        h = tData.heightmapHeight;
        terrainScale = tData.size;
        tRes = (int)Mathf.Pow(2, resultion);
        meshScale = new Vector3(terrainScale.x / (w - 1) * tRes, terrainScale.y, terrainScale.z / (h - 1) * tRes);
        float [,] tmpHMap = tData.GetHeights(0, 0, w, h);
		
		w = (tData.heightmapWidth - 1) / tRes + 1;
        h = (tData.heightmapHeight - 1) / tRes + 1;
		
		hMap = new float [w,h];
		
		for(int x=0;x<w;x++){			
			for(int y=0;y<h;y++){				
				hMap[y,x] = tmpHMap[x*tRes,y*tRes];//swap indices
			}
		}
		//dereferece the bigger array so that gc deletes it
		tmpHMap = null;

        grid = new bool[w,h];

        Debug.Log("w " + w + " h "+ h + " size (byte)" + w*h + " res " +tRes);

        for (int y = 0; y < h; y++){
            for (int x = 0; x < w; x++){
                grid[x,y] = false;
            }
        }

        Coordinate initPoint = getGridPos(initialMappingPoint);

        if(initPoint == null){
            Debug.Log("Mapping startpoint invalid! (" + initialMappingPoint + ")");
            return;
        }

        Debug.Log("Mapping start point: (" + initPoint.x + "," + initPoint.y + ")");
        Debug.Log("Mapping started " + Time.realtimeSinceStartup);
        map(initPoint.x, initPoint.y);
        Debug.Log("Mapping finished " + Time.realtimeSinceStartup);

        //remove trees
        int tX = 0, tY = 0;
        foreach (TreeInstance tree in tData.treeInstances){
            tX = (int)(tree.position.x * w);
            tY = (int)(tree.position.z * h);
            //center
            if (isTreeInRange(tree.position, tX, tY)) grid[tX,tY] = false;

            for (int i = 1; i <= propApproximationRadius; i++){
                //top 3
                if (isTreeInRange(tree.position, tX - i, tY + i)) grid[tX - i, tY + i] = false;
                if (isTreeInRange(tree.position, tX, tY + i)) grid[tX, tY + i] = false;
                if (isTreeInRange(tree.position, tX + i, tY + i)) grid[tX + i, tY + i] = false;
                //left & right from center
                if (isTreeInRange(tree.position, tX - i, tY)) grid[tX - i, tY] = false;
                if (isTreeInRange(tree.position, tX + i, tY)) grid[tX + i, tY] = false;
                //bottom 3
                if (isTreeInRange(tree.position, tX - i, tY - i)) grid[tX - i, tY - i] = false;
                if (isTreeInRange(tree.position, tX, tY - i)) grid[tX, tY - i] = false;
                if (isTreeInRange(tree.position, tX + i, tY - i)) grid[tX + i, tY - i] = false;
            }
        }    

        hGizmo = new Vector3(0, gizmo_height,0);

        //visualize stuff
        gizmo.renderer.material = bad;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if ((grid[x, y]) == false)
                {
                    Instantiate(gizmo, getWorldPos(x, y) + hGizmo, Quaternion.identity);
                }
            }
        }
       
	}

    public GUIContent sincontent;
    public GUIStyle sinstyle;
    void OnGUI(){
        PathFindingWindowRect = GUI.Window(0, PathFindingWindowRect, PathfindingWindow, "Pathfinder - " + Application.loadedLevelName);
        GUI.Label(new Rect(10, Screen.height - sinstyle.CalcHeight(sincontent,200), 200, 100), sincontent, sinstyle);
    }

    private List<Object> paths = new List<Object>();

    Rect PathFindingWindowRect = new Rect(10,10,300,180);
    void PathfindingWindow(int id){
        GUILayout.BeginHorizontal();
        GUILayout.Label("Start X: " +sx);
        sx = (int)GUILayout.HorizontalSlider(sx, 0, w-1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Start Y: "+sy);
        sy = (int)GUILayout.HorizontalSlider(sy, 0, h-1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Goal X: "+gx);
        gx = (int)GUILayout.HorizontalSlider(gx, 0, w-1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Goal Y: " +gy);
        gy = (int)GUILayout.HorizontalSlider(gy, 0, h-1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Vector")) Application.LoadLevel("VectorGridTest");
        if (GUILayout.Button("Bool")) Application.LoadLevel("BoolGridTest");
        if (GUILayout.Button("NoGrid")) Application.LoadLevel("NoGridTest");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (grid[sx, sy] && grid[gx, gy] && GUILayout.Button("Find Path"))
        {
            gizmo.renderer.material = startend;
            Vector3 start = getWorldPos(sx, sy);
            
            Vector3 goal = getWorldPos(gx, gy);
            
            Coordinate gpos = getGridPos(goal);

            gizmo.renderer.material = path;
            
            StartCoroutine(findPath(start, goal));
        }
        if(GUILayout.Button("Clear")){
            foreach(Object o in paths){
                Destroy(o);
            }
            paths.Clear();
            visited.Clear();
        }
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    //for structures only
    public void Occupy(Vector3 worldPos){
        int x = (int)(worldPos.x / w);
        int y = (int)(worldPos.z / h);
        if(x < w && y < h) grid[x,y] = false;
    }

    public void Free(Vector3 worldPos){
        int x = (int)(worldPos.x / w);
        int y = (int)(worldPos.z / h);
        if(x < w && y < h) grid[x,y] = true;
    }

    public Coordinate getGridPos(Vector3 pos){        
        int x = (int)(pos.x / meshScale.x);
        int y = (int)(pos.z / meshScale.z);
        if(y < 0 || x < 0 || x > w-1 || y > h-1) {
            return null;
        }
        return new Coordinate(x,y);
    }

    public Vector3 getWorldPos(int x, int y){
        return new Vector3(meshScale.x * x, meshScale.y * hMap[x,y] , meshScale.z * y) + tPos;
    }

    private bool isTreeInRange(Vector3 treePos, int x, int y) {
        if (x < w && y < h && y -1 > -1 && x-1 > -1){
            return (Vector3.Distance(Vector3.Scale(terrainScale, treePos) + tPos, getWorldPos(x,y)) < propDistanceTolerance);
        } else {
            return false;
        }
    }

    private void map(int x, int y) {
        grid[x,y] = true;

        //top 3
        if(y+1 < h){
			if(x-1 > -1){
				if(grid[x-1,y+1] == false){
                	if(Mathf.Abs(hMap[x-1,y+1] - hMap[x,y]) < maxHeightDelta) map(x-1,y+1);
            	}				
			}
			if (grid[x, y + 1] == false){
                if (Mathf.Abs(hMap[x,y+1] - hMap[x,y]) < maxHeightDelta) map(x, y + 1);
            }
			if (x + 1 < w){
	            if (grid[x + 1, y + 1] == false){
	                if (Mathf.Abs(hMap[x+1,y+1] - hMap[x,y]) < maxHeightDelta) map(x + 1, y + 1);
	            }
       		}
        }

        //center 2
        if (x - 1 > -1){
            if (grid[x - 1, y] == false){
                if (Mathf.Abs(hMap[x - 1,y] - hMap[x,y]) < maxHeightDelta) map(x - 1, y);
            }
        }
        if (x + 1 < w){
            if (grid[x + 1, y] == false){
                if (Mathf.Abs(hMap[x + 1,y] - hMap[x,y]) < maxHeightDelta) map(x + 1, y);
            }
        }

        //bottom 3
		
		if(y - 1 > -1){
			if (x - 1 > -1){
	            if (grid[x - 1, y - 1] == false){
	                if (Mathf.Abs(hMap[x-1,y-1] - hMap[x,y]) < maxHeightDelta) map(x - 1, y - 1);
	            }
	        }
            if (grid[x, y-1] == false){
                if (Mathf.Abs(hMap[x,y-1] - hMap[x,y]) < maxHeightDelta) map(x, y-1);
            }
	        if (x + 1 < w ){
	            if (grid[x + 1, y - 1] == false){
	                if (Mathf.Abs(hMap[x + 1,y-1] - hMap[x,y]) < maxHeightDelta) map(x + 1, y - 1);
	            }
	        }			
		}
    }

    private List<Node> getAllNeighbors(Node cur, Vector3 goal){
        List<Node> neighbors = new List<Node>();
        Node nb = null;
        //top 3
        if (cur.y + 1 < h)
        {
            if (cur.x - 1 > -1)
            {
                if ((grid[cur.x - 1, cur.y + 1]) == true){
                    nb = new Node(cur, cur.x - 1, cur.y + 1, getWorldPos(cur.x - 1, cur.y + 1), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }                
            }

            if (grid[cur.x, cur.y + 1])
            {
                nb = new Node(cur, cur.x, cur.y + 1, getWorldPos(cur.x, cur.y + 1), goal);
                if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
            }            

            if (cur.x + 1 < w)
            {
                if (grid[cur.x + 1, cur.y + 1])
                {
                    nb = new Node(cur, cur.x + 1, cur.y + 1,getWorldPos(cur.x + 1, cur.y + 1), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }                
            }
        }

        //center 2
        if (cur.x + 1 < w)
        {
            if (grid[cur.x + 1, cur.y])
            {
                nb = new Node(cur, cur.x + 1, cur.y, getWorldPos(cur.x + 1, cur.y), goal);
                if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
            }            
        }

        if (cur.x - 1 > -1){
            if (grid[cur.x - 1, cur.y])
            {
                nb = new Node(cur, cur.x - 1, cur.y, getWorldPos(cur.x - 1, cur.y), goal);
                if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
            }            
        }

        //bottom 3
        if (cur.y - 1 > -1)
        {
            if (cur.x - 1 > -1)
            {
                if (grid[cur.x - 1, cur.y - 1])
                {
                    nb = new Node(cur, cur.x - 1, cur.y - 1, getWorldPos(cur.x - 1, cur.y - 1), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }                
            }

            if (grid[cur.x, cur.y - 1])
            {
                nb = new Node(cur, cur.x, cur.y - 1, getWorldPos(cur.x, cur.y - 1), goal);
                if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
            }
            

            if (cur.x + 1 < w)
            {
                if (grid[cur.x + 1, cur.y - 1])
                {
                    nb = new Node(cur, cur.x + 1, cur.y - 1, getWorldPos(cur.x + 1, cur.y - 1), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }                
            }
        }

        return neighbors;
    }

    private List<Node> getNeighbors(Node cur, Vector3 goal){
        if(cur.parent != null){
            List<Node> neighbors = new List<Node>();
            Node nb = null;
            //relative direction
            int dx = Mathf.Min(Mathf.Max(-1, cur.x - cur.parent.x), 1);
            int dy = Mathf.Min(Mathf.Max(-1, cur.y - cur.parent.y), 1);

            //diagonal case
            if(dx != 0 && dy != 0){
                if (isReachable(cur.x, cur.y + dy)) {
                    nb = new Node(cur, cur.x, cur.y + dy, getWorldPos(cur.x, cur.y + dy), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }
                if (isReachable(cur.x + dx, cur.y)) {
                    nb = new Node(cur, cur.x + dx, cur.y, getWorldPos(cur.x + dx, cur.y), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }
                if (neighbors.Count > 0 && isReachable(cur.x + dx, cur.y + dy)) {
                    nb = new Node(cur, cur.x + dx, cur.y + dy, getWorldPos(cur.x + dx, cur.y + dy), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }
                if (isReachable(cur.x - dx, cur.y) == false && isReachable(cur.x, cur.y + dy) && isReachable(cur.x - dx, cur.y - dy)) {
                    nb = new Node(cur, cur.x - dx, cur.y - dy, getWorldPos(cur.x - dx, cur.y - dy), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }
                if (isReachable(cur.x, cur.y - dy) == false && isReachable(cur.x + dx, cur.y) && isReachable(cur.x + dx, cur.y - dy)) {
                    nb = new Node(cur, cur.x + dx, cur.y - dy, getWorldPos(cur.x + dx, cur.y - dy), goal);
                    if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                }
            }else if(dx == 0){//vertical/horizontal
             	if (isReachable(cur.x, cur.y + dy)){
                    if (isReachable(cur.x, cur.y + dy)) {
                        nb = new Node(cur, cur.x, cur.y + dy, getWorldPos(cur.x, cur.y + dy), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                    if (isReachable(cur.x + 1, cur.y) == false && isReachable(cur.x + 1, cur.y + dy)){
                        nb = new Node(cur, cur.x + 1, cur.y + dy,getWorldPos(cur.x + 1, cur.y + dy), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                    if (isReachable(cur.x - 1, cur.y) == false && isReachable(cur.x - 1, cur.y + dy)) {
                        nb = new Node(cur, cur.x - 1, cur.y + dy, getWorldPos(cur.x - 1, cur.y + dy), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                }
            }else{
                if (isReachable(cur.x + dx, cur.y)){
                    if (isReachable(cur.x + dx, cur.y)){
                        nb = new Node(cur, cur.x + dx, cur.y, getWorldPos(cur.x + dx, cur.y), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                    if (isReachable(cur.x, cur.y + 1) == false && isReachable(cur.x + dx, cur.y + 1)) {
                        nb = new Node(cur, cur.x + dx, cur.y + 1, getWorldPos(cur.x + dx, cur.y + 1), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                    if (isReachable(cur.x, cur.y - 1) == false && isReachable(cur.x + dx, cur.y - 1)) {
                        nb = new Node(cur, cur.x + dx, cur.y - 1, getWorldPos(cur.x + dx, cur.y - 1), goal);
                        if (nb.Equals(cur.parent) == false) neighbors.Add(nb);
                    }
                }
            }
            return neighbors;
        }else{
            return getAllNeighbors(cur,goal);
        }
    }

    //child: x,y
    //partent: px,py
    private Coordinate Jump(int x, int y, int px, int py, Vector3 goal){
        int dx = x - px;
        int dy = y - py;

        if(isReachable(x,y) == false){
            return null;
        }else if (getWorldPos(x,y) == goal){
            return new Coordinate(x,y);
        }

        //forced diagonal neighbors
        if(dx != 0 && dy != 0){
            if((isReachable(x-dx,y+dy) && isReachable(x-dx,y) == false) ||
               (isReachable(x+dx,y-dy) && isReachable(x,y-dy) == false)){
                    return new Coordinate(x,y);
                }
        }else{//horizontal/vertical
            if(dx != 0){
                if((isReachable(x+dx,y+1) && isReachable(x,y+1) == false) || 
                   (isReachable(x+dx,y-1) && isReachable(x,y-1) == false)){
                    return new Coordinate(x,y);
                }
            }else{
                if((isReachable(x+1,y+dy) && isReachable(x+1,y) == false) ||
                   (isReachable(x-1,y+dy) && isReachable(x-1,y) == false)){
                        return new Coordinate(x,y);
                   }
            }
        }

        if(dx != 0 && dy != 0){//check for horizontal/vertical jump points
            if(Jump(x+dx,y,x,y,goal) != null || Jump(x,y+dy,x,y,goal) != null){
                return new Coordinate(x,y);
            }
        }

        if(isReachable(x+dx,y) || isReachable(x,y+dy)){
            return Jump(x+dx,y+dy,x,y,goal);
        }else{
            return null;
        }
    }

    private bool isReachable(int x, int y){
        return (x > -1 && y > -1 && x < w && y < h) ? grid[x,y] : false;
    }

    List<Vector3> visited = new List<Vector3>();

    // public List<Vector3>
    private IEnumerator findPath(Vector3 start, Vector3 goal)//
    {
        foreach (Object o in paths)
        {
            Destroy(o);
        }
        paths.Clear();
        visited.Clear();
        List<Vector3> path = new List<Vector3>();

        SortedHeap<Node> openList = new SortedHeap<Node>(new NodeComparer());
        SortedHeap<Node> closedList = new SortedHeap<Node>(new NodeComparer());
        
        Coordinate sco = getGridPos(start);
        if(sco == null || grid[sco.x,sco.y] == false) yield break; //return path;

        Coordinate gco = getGridPos(goal);
        if (gco == null || grid[gco.x, gco.y] == false) yield break;//return path;

        if(sco.x == gco.x && sco.y == gco.y) yield break;//return path;

        Vector3 correctedGoal = getWorldPos(gco.x,gco.y);
        Vector3 correctedStart = getWorldPos(sco.x,sco.y);

        openList.Push(new Node(null,sco.x,sco.y,correctedStart,correctedGoal));
        Node cur = null,jn = null; Coordinate jmp = null;
        int index = 0;
        float cost = 0;
        float time = Time.realtimeSinceStartup;

        while(openList.Count > 0){
            cur = openList.Pop();
            if(cur.isGoal) {
                while (cur != null){
                    //visited.Remove(cur.pos);
                    path.Add(cur.pos);
                    cur = cur.parent;
                }
                break;
            }
                
            //if(!visited.Contains(cur.pos))visited.Add(cur.pos);
            foreach(Node neighbor in (cur.neighbors == null) ? (cur.neighbors = getNeighbors(cur, correctedGoal)) : cur.neighbors){
                jmp = Jump(neighbor.x,neighbor.y,cur.x,cur.y,goal);
                if(jmp == null) continue;

                jn = new Node(cur,jmp.x,jmp.y,getWorldPos(jmp.x,jmp.y),goal);
                if (closedList.Contains(jn)) continue;

                cost = cur.g + Node.estimate(jn.pos,cur.pos);

                if(openList.Contains(jn,ref index) && cost >= jn.g) continue;
                
                jn.parent = cur;
                jn.g = cost;
                jn.f = cost + jn.h;
                
                if(index >= 0) openList.RemoveAt(index);     
                openList.Push(neighbor);            
            }

            closedList.Push(cur);
        }

        Debug.Log("Time: "+ (Time.realtimeSinceStartup-time)*1000f + "ms");

        //foreach(Vector3 v in visited){
        //    paths.Add(Instantiate(gizmo, v + hGizmo * 2, Quaternion.identity));
        //}
        //gizmo.renderer.material = startend;
        foreach (Vector3 p in path)
        {
            paths.Add(Instantiate(gizmo, p + hGizmo * 2, Quaternion.identity));
        }
        //return path;
    }
}
