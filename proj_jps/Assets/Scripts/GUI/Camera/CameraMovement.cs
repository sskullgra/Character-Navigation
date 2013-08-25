using System.Collections;
using UnityEngine;

class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 50f;

    public Vector4 limitMapSize;   
    public Vector2 limitMapHeight;

    private Vector3 input;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float t;
    float x, y, z;
    Vector3 mpos;
    public float mouseTolerance = 100f;
    float horizontal, vertical;
    public void Update()
    {
        mpos = Input.mousePosition;

        horizontal = Input.GetAxis("Horizontal");
        if(horizontal == 0f){
            //horizontal = Input.GetAxis("Mouse X");
            if (mpos.x + mouseTolerance > (float)Screen.width){
                if(horizontal == 0f) horizontal = 20f;
            }else if (mpos.x - mouseTolerance < 0f)
            {
                if (horizontal == 0f) horizontal = -20f;
            }          
        }

        vertical = Input.GetAxis("Vertical");
        if (vertical == 0f){
            //vertical = Input.GetAxis("Mouse Y");
            if (mpos.y + mouseTolerance > (float)Screen.height){
                if(vertical == 0f) vertical = 20f;
            }else if( mpos.y - mouseTolerance < 0f){
                if (vertical == 0f) vertical = -20f;
            }
        }

        input = new Vector3(horizontal, Input.GetAxis("Mouse ScrollWheel"), vertical);

        if (input != Vector3.zero)
        {
            startPosition = transform.position;

            x = startPosition.x + System.Math.Sign(input.x);
            y = startPosition.y - input.y*10f;
            z = startPosition.z + System.Math.Sign(input.z);

            if (x < limitMapSize.x)
            {
                x = limitMapSize.x;
            }
            else if (x > limitMapSize.y)
            {
                x = limitMapSize.y;
            }

            if (z < limitMapSize.z)
            {
                z = limitMapSize.z;
            }
            else if (z > limitMapSize.w)
            {
                z = limitMapSize.w;
            }

            if (y < limitMapHeight.x)
            {
                y = limitMapHeight.x;
            }
            else if (y > limitMapHeight.y)
            {
                y = limitMapHeight.y;
            }
            
            endPosition = new Vector3(x, y, z);
            t = Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
        }

    }
}