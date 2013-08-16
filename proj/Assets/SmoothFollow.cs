using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {
	
	// The target we are following
	public Transform target ;
	// The distance in the x-z plane to the target
	float distance = 20.0f;
	// the height we want the camera to be above the target
	float height = 10.0f;
	// How much we 
	float heightDamping = 2.0f;
	float rotationDamping = 3.0f;
	
	
	void LateUpdate () {
    // Early out if we don't have a target
    if (!target)
        return;
 
    // Calculate the current rotation angles
    float wantedRotationAngle = target.eulerAngles.y;
    float wantedHeight = target.position.y + height;
 
    float currentRotationAngle = transform.eulerAngles.y;
    float currentHeight = transform.position.y;
 
    // Damp the rotation around the y-axis
    currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
 
    // Damp the height
    currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
 
    // Convert the angle into a rotation
    Quaternion currentRotation = Quaternion.Euler (0.0f, currentRotationAngle, 0.0f);
 
    // Set the position of the camera on the x-z plane to:
    // distance meters behind the target
    transform.position = target.position;
    transform.position -= currentRotation * Vector3.forward * distance;
 
    // Set the height of the camera
  	transform.position = transform.position + new Vector3(0.0f, currentHeight, 0.0f);
    // Always look at the target
    transform.LookAt (target);
}
}
