using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPCTrajectoryThrower : MonoBehaviour
{
    public NPCThrowProjectile npcThrowProjectile; 
    public GameObject thrownObjectPrefab;
    public float moveSpeed = 20f;
    public Vector3 initialRotation;
    public float rotationSpeed = 360f;

    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.X;

    public string ignoredTag = "Player";
    public string enemyTag = "Enemy";
    public Transform playerCameraTransform;

    public float attackDistance = 5f;

    private bool isAttacking = false;

    public void ThrowObject()
    {
        if (npcThrowProjectile == null || npcThrowProjectile.GetTrajectoryPoints() == null || npcThrowProjectile.GetTrajectoryPoints().Length < 2)
        {
            Debug.LogWarning("No trajectory to follow!");
            return;
        }


        Vector3 directionToPlayer = (playerCameraTransform.position - transform.position).normalized;


        Quaternion throwRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(initialRotation);


        GameObject newObject = Instantiate(thrownObjectPrefab, npcThrowProjectile.GetTrajectoryPoints()[0], throwRotation);
        Rigidbody rb = newObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        IgnorePhysicsWithTaggedObjects(newObject, ignoredTag);


        Vector3[] path = (Vector3[])npcThrowProjectile.GetTrajectoryPoints().Clone();


        StartCoroutine(FollowTrajectory(newObject, path));
    }

    void IgnorePhysicsWithTaggedObjects(GameObject obj, string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;

        Collider[] objectColliders = obj.GetComponentsInChildren<Collider>();
        GameObject[] ignoreObjects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject go in ignoreObjects)
        {
            Collider[] ignoreColliders = go.GetComponentsInChildren<Collider>();
            foreach (var col1 in objectColliders)
            {
                foreach (var col2 in ignoreColliders)
                {
                    Physics.IgnoreCollision(col1, col2);
                }
            }
        }
    }

    IEnumerator FollowTrajectory(GameObject movingObject, Vector3[] path)
    {
        for (int i = 1; i < path.Length; i++)
        {
            Vector3 start = movingObject.transform.position;
            Vector3 target = path[i];
            float distance = Vector3.Distance(start, target);
            float duration = distance / moveSpeed;
            float time = 0f;

            while (time < duration)
            {
                RotateAlongSelectedAxis(movingObject);

                time += Time.deltaTime;
                float t = time / duration;
                movingObject.transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            movingObject.transform.position = target;
        }

        EnablePhysics(movingObject);
        StartCoroutine(DestroyAfterDelay(movingObject, 5f));
    }

    void EnablePhysics(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }

    void RotateAlongSelectedAxis(GameObject obj)
    {
        Vector3 axisVector = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X: axisVector = Vector3.right; break;
            case RotationAxis.Y: axisVector = Vector3.up; break;
            case RotationAxis.Z: axisVector = Vector3.forward; break;
        }

        obj.transform.Rotate(axisVector * rotationSpeed * Time.deltaTime);
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
