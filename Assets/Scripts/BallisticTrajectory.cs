using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallisticLineDrawer : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public int numberOfPoints = 20;

    public float minThrowDistance = 1f;
    public float maxThrowDistance = 10f;
    public float minArcHeight = 2f;
    public float maxArcHeight = 6f;

    public LineRenderer lineRenderer;
    public Button throwButton;
    public Slider throwPowerSlider;
    public Image sliderFillImage;

    public GameObject endBallPrefab;
    private GameObject currentEndBall;

    public Camera mainCamera;
    public GameObject targetPrefab;
    private GameObject currentTarget;

    private float currentCharge = 0f;
    private float chargeTime = 2f;
    private bool isCharging = false;
    private bool isDrawing = false;

    private EventTrigger eventTrigger;

    public Vector3[] lastTrajectoryPoints;
    public TrajectoryThrower trajectoryThrower;

    void Start()
    {
        lineRenderer.positionCount = numberOfPoints;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        Texture2D dashedTexture = new Texture2D(2, 1);
        dashedTexture.SetPixels(new Color[] { Color.red, Color.clear });
        dashedTexture.Apply();
        lineRenderer.material.mainTexture = dashedTexture;
        lineRenderer.textureMode = LineTextureMode.Tile;

        Gradient gradient = new Gradient();
        gradient.colorKeys = new GradientColorKey[] {
            new GradientColorKey(Color.red, 0f),
            new GradientColorKey(new Color(1f, 0f, 0f, 0f), 1f)
        };
        gradient.alphaKeys = new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        };
        lineRenderer.colorGradient = gradient;
        lineRenderer.enabled = false;

        eventTrigger = throwButton.GetComponent<EventTrigger>();

        EventTrigger.Entry entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((data) => StartCharging());
        eventTrigger.triggers.Add(entryDown);

        EventTrigger.Entry entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((data) => StopCharging());
        eventTrigger.triggers.Add(entryUp);

        throwPowerSlider.value = 0f;
        sliderFillImage.color = Color.green;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (!eventTrigger.enabled) return;

        if (isCharging)
        {
            currentCharge += Time.deltaTime;
            float power = Mathf.Clamp01(currentCharge / chargeTime);

            throwPowerSlider.value = power;
            sliderFillImage.color = Color.Lerp(Color.green, Color.red, power);

            UpdateTarget();
            DrawArcWithPower(power);
        }
        else if (isDrawing)
        {
            float power = throwPowerSlider.value;
            sliderFillImage.color = Color.Lerp(Color.green, Color.red, power);

            UpdateTarget();
            DrawArcWithPower(power);
        }
    }

    void StartCharging()
    {
        currentCharge = 0f;
        isCharging = true;
        isDrawing = true;
        lineRenderer.enabled = true;
        CreateTarget();
    }

    void StopCharging()
    {
        isCharging = false;
        isDrawing = false;
        currentCharge = 0f;

        throwPowerSlider.value = 0f;
        sliderFillImage.color = Color.green;

        lineRenderer.enabled = false;

        if (currentEndBall != null)
            Destroy(currentEndBall);
        if (currentTarget != null)
            Destroy(currentTarget);


        trajectoryThrower.ThrowObject();
    }



    void DrawArcWithPower(float power)
    {
        float distance = Mathf.Lerp(minThrowDistance, maxThrowDistance, power);
        float height = Mathf.Lerp(minArcHeight, maxArcHeight, power);
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        Vector3 finalTarget = startPoint.position + direction * distance;

        Vector3 hitPosition = Vector3.zero;
        bool hitDetected = false;
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            points[i] = CalculateArcPoint(startPoint.position, finalTarget, t, height);
        }

        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            lineRenderer.positionCount = Mathf.Max(i + 2, lineRenderer.positionCount); 

            lineRenderer.SetPosition(i, points[i]);

            Vector3 dir = points[i + 1] - points[i];
            float dist = dir.magnitude;

            if (Physics.Raycast(points[i], dir.normalized, out RaycastHit hit, dist))
            {
                if (hit.collider.CompareTag("Player"))
                    continue;

                hitPosition = hit.point;
                hitDetected = true;
                lineRenderer.positionCount = i + 2;
                lineRenderer.SetPosition(i + 1, hit.point);

                lastTrajectoryPoints = new Vector3[i + 2];
                for (int j = 0; j <= i; j++)
                    lastTrajectoryPoints[j] = points[j];
                lastTrajectoryPoints[i + 1] = hit.point;
                break;
            }
        }

        if (!hitDetected)
        {
            lineRenderer.SetPosition(numberOfPoints - 1, points[numberOfPoints - 1]);
            hitPosition = points[numberOfPoints - 1];
            lastTrajectoryPoints = points;
        }

        if (currentEndBall == null)
        {
            currentEndBall = Instantiate(endBallPrefab, hitPosition, Quaternion.identity);
            SetBallTransparency(currentEndBall, 0.4f);
        }
        else
        {
            currentEndBall.transform.position = hitPosition;
        }
    }


    Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t, float height)
    {
        Vector3 point = Vector3.Lerp(start, end, t);
        float arc = Mathf.Sin(t * Mathf.PI) * height;
        point.y += arc;
        return point;
    }

    void SetBallTransparency(GameObject ball, float alpha)
    {
        Renderer renderer = ball.GetComponent<Renderer>();
        if (renderer != null && renderer.material.HasProperty("_Color"))
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;

            renderer.material.SetFloat("_Mode", 2);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
    }

    void CreateTarget()
    {
        Vector3 worldPos = GetCenterRaycastPosition();
        currentTarget = Instantiate(targetPrefab, worldPos, Quaternion.identity);
        endPoint = currentTarget.transform;
    }

    void UpdateTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.transform.position = GetCenterRaycastPosition();
        }
    }

    Vector3 GetCenterRaycastPosition()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        return ray.origin + ray.direction * 10f;
    }
}
