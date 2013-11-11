using UnityEngine;
using System.Collections;
using Pathfinding;

/// <summary>
/// Class responsible for a character movement.
/// Author: Radosław Bigaj
/// POLITECHNIKA ŚLĄSKA
/// </summary>
public class AI_Shifter : MonoBehaviour
{
	
	/// <summary>
	/// Coordinates of target point.
	/// </summary>
	public Transform target;
	
	private Seeker seeker;
	private Path path;
	
	/// <summary>
	/// The character controller. It is needed to move given character.
	/// </summary>
	private CharacterController charController;
	
	private int currentWaypoint;
	/// <summary>
	/// The speed of 3D character.
	/// </summary>
	private float speed = 10.0f;
	
	/// <summary>
	/// The max distance to waypoint.
	/// </summary>
	private float maxWaypointDistance = 2.0f;
	
	/// <summary>
	/// Start initializes 'seeker' component and compute path. 
	/// </summary>
	void Start ()
	{
		seeker = GetComponent<Seeker> ();
		seeker.StartPath (transform.position, target.position, OnPathCompleted);
		charController = GetComponent<CharacterController> ();
	}
	
	/// <summary>
	/// Raises the path completed event.
	/// </summary>
	/// <param name='p'>
	/// p is computed path.
	/// </param>
	public void OnPathCompleted (Path p)
	{
		if (!p.error) {
			path = p;
			currentWaypoint = 0;
		} else {
			UnityEngine.Debug.Log (p.error);
		}
	}
	
	/// <summary>
	/// The method moves the 3D character by computer path.
	/// </summary>
	void FixedUpdate ()
	{
		if (path == null)
			return;
	
		if (currentWaypoint >= path.vectorPath.Count) {
			return;
		}
		
		Vector3 direction = (path.vectorPath [currentWaypoint] - transform.position).normalized * speed;
		charController.SimpleMove (direction);
		gameObject.transform.forward = direction.normalized;
		float distance = Vector3.Distance (transform.position, path.vectorPath [currentWaypoint]);
		if (distance < maxWaypointDistance) { 
			currentWaypoint++;
		}
	}
	

}
