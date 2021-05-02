using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float tileOffset;
    [SerializeField] Vector3 offsetPos;
    [SerializeField] Vector3 boundsMin;
    [SerializeField] Vector3 boundsMax;

    
    // Start is called before the first frame update
    private void LateUpdate() {
        
        if (player != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = player.position; // follows player object

            targetPos.x += offsetPos.x;
            targetPos.y += offsetPos.y;
            targetPos.z += transform.position.z;
                        
            targetPos.x = Mathf.Clamp(targetPos.x, boundsMin.x, boundsMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, boundsMin.y, boundsMax.y);
            
            float t = 1f - Mathf.Pow(1f - tileOffset, Time.deltaTime * 30); // easing function
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }

    }
    
}
