using UnityEngine;
using System.Collections;
using Pathfinding;

public class AI_Pather : MonoBehaviour {

	
	public Transform target;
	private Seeker seeker;
	Path path;
	CharacterController charController;
	int currentWaypoint;
	
	float speed = 20.0f;
	float maxWaypointDistance = 2.0f;
	
	void Start () {
	seeker = GetComponent<Seeker>();
	seeker.StartPath(transform.position, target.position, OnPathCompleted);
		charController = GetComponent<CharacterController>();
	}
	
	public void OnPathCompleted(Path p)
	{
		if (!p.error)
		{
		path = p ;
		currentWaypoint = 0 ;
		}
		else{
			Debug.Log(p.error);
		}
	}
	
	void FixedUpdate()
	{
		if (path == null)
			return;
	
		
		if (currentWaypoint >= path.vectorPath.Count)
			return;
		
		
		Vector3 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized * speed;
		charController.SimpleMove(direction);
		
		if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < maxWaypointDistance){ 
		currentWaypoint++;
		}
	}
	

}
