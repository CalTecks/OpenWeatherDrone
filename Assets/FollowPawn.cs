using UnityEngine;

public class FollowPawn : MonoBehaviour
{
    public Transform pawn;
    public float height = 40f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(pawn != null) {
            Vector3 updatedPosition = new Vector3(pawn.position.x, height, pawn.position.z);
            transform.position = updatedPosition;
        }
        
    }
}
