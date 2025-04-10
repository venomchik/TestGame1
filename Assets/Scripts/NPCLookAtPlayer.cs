using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    public Transform player; 
    public float rotationSpeed = 5f; 

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        if (player == null)
            return;


        Vector3 directionToPlayer = player.position - transform.position;


        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
