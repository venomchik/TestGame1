using UnityEngine;

public class NPCThrowProjectile : MonoBehaviour
{
    public Transform startPoint;
    public int numberOfPoints = 20;

    public float minThrowDistance = 1f;
    public float maxThrowDistance = 10f;
    public float minArcHeight = 2f;
    public float maxArcHeight = 6f;

    public LineRenderer lineRenderer;

    private GameObject player;

    private float currentCharge = 0f;
    private float chargeTime = 2f;
    private bool isCharging = false;
    private bool isDrawing = false;

    private float attackCooldown = 2f;
    private float attackTimer = 0f;

    private bool isReadyForNextCharge = true;
    public NPCTrajectoryThrower trajectoryThrower;

    public Vector3[] trajectoryPoints;
    public Animator npcAnimator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lineRenderer.positionCount = numberOfPoints;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.blue;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (player == null)
            return;

        if (npcAnimator != null && npcAnimator.GetBool("Death"))
            return;

        if (isCharging)
        {
            currentCharge += Time.deltaTime;
            float power = Mathf.Clamp01(currentCharge / chargeTime);
            DrawArcWithPower(power);


            if (Vector3.Distance(lineRenderer.GetPosition(numberOfPoints - 1), player.transform.position) < 1f)
            {
                

                StopChargingAndPrepareForNext();
                for (int i = 0; i < numberOfPoints; i++)
                {
                    trajectoryPoints[i].y += 1f;
                }
                trajectoryThrower.ThrowObject();
            }
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
        else if (!isCharging && isReadyForNextCharge)
        {

            StartCharging();
        }


        if (isCharging && currentCharge >= chargeTime)
        {
            if (Vector3.Distance(lineRenderer.GetPosition(numberOfPoints - 1), player.transform.position) >= 1f)
            {

                StopChargingAndPrepareForNext();
                StartCharging();  
            }
        }
    }


    void StartCharging()
    {
        currentCharge = 0f;
        isCharging = true;
        isDrawing = true;
        lineRenderer.enabled = false;
    }

    void StopChargingAndPrepareForNext()
    {
        isCharging = false;
        isDrawing = false;
        currentCharge = 0f;
        lineRenderer.enabled = false;


        isReadyForNextCharge = false;
        Invoke("ResetChargeReady", attackCooldown);
    }

    void ResetChargeReady()
    {
        isReadyForNextCharge = true;
    }

    void DrawArcWithPower(float power)
    {
        float distance = Mathf.Lerp(minThrowDistance, maxThrowDistance, power);
        float height = Mathf.Lerp(minArcHeight, maxArcHeight, power);
        Vector3 direction = (player.transform.position - startPoint.position).normalized;
        Vector3 finalTarget = startPoint.position + direction * distance;

        trajectoryPoints = new Vector3[numberOfPoints];


        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            trajectoryPoints[i] = CalculateArcPoint(startPoint.position, finalTarget, t, height);
        }


        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            lineRenderer.positionCount = Mathf.Max(i + 2, lineRenderer.positionCount);
            lineRenderer.SetPosition(i, trajectoryPoints[i]);
        }

        lineRenderer.SetPosition(numberOfPoints - 1, trajectoryPoints[numberOfPoints - 1]);
    }

    Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t, float height)
    {
        Vector3 point = Vector3.Lerp(start, end, t);
        float arc = Mathf.Sin(t * Mathf.PI) * height;
        point.y += arc;
        return point;
    }

    public Vector3[] GetTrajectoryPoints()
    {
        return trajectoryPoints;
    }


}
