using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class GuardController : MonoBehaviour
{
    GemHolder gemHolder;

    Light2D wideSpotlight;
    Light2D narrowSpotlight;

    float angleToPlayer;
    float scanlineAngle = 0f;

#pragma warning disable CS0649
    [SerializeField] float rX = 5f;
    [SerializeField] float rY = 1.7f;
    [SerializeField] float theta = 0f;
    [SerializeField] float scanSpeed = 30f;
    [SerializeField] float scanWidth = 70f;
    [SerializeField] [Range(0f, 1f)] float secondarySpotlightSpeed = 0.02f;
    [SerializeField] [Min(1)] int gizmoResolution = 30;
#pragma warning restore CS0649

    void Start()
    {
        gemHolder = GetComponent<GemHolder>();

        Light2D[] spotlights = GetComponentsInChildren<Light2D>();
        wideSpotlight = spotlights.Single(light => light.name == "Wide spotlight");
        narrowSpotlight = spotlights.Single(light => light.name == "Narrow spotlight");

        Random.InitState(seed: System.DateTime.Now.Millisecond);
        scanlineAngle = Random.Range(-180f, 180f);
    }

    void Update()
    {
        AdvanceScanline();
        UpdateSpotlights();

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, PointAtAngleDeg(scanlineAngle - scanWidth / 2f));
        Debug.DrawLine(transform.position, PointAtAngleDeg(scanlineAngle + scanWidth / 2f));
#endif

        if (GameManager.player == null)
        {
            angleToPlayer = 0;
            return;
        }

        angleToPlayer = (GameManager.player.transform.position - transform.position).ToVector2().GetAngleDeg();
    }

    void AdvanceScanline()
    {
        scanlineAngle = (scanlineAngle + scanSpeed * Time.deltaTime).WrapAngleDeg();
    }

    void UpdateSpotlights()
    {
        float desiredAngle = (narrowSpotlight.transform.position.ToVector2() - PointAtAngleDeg(scanlineAngle)).GetAngleDeg() + 90;

        narrowSpotlight.transform.rotation = Quaternion.Euler(0, 0, desiredAngle);

        float currentWideSpotlightAngle = wideSpotlight.transform.rotation.eulerAngles.z;
        float newWideSpotlightAngle = Mathf.LerpAngle(currentWideSpotlightAngle, desiredAngle, secondarySpotlightSpeed);
        wideSpotlight.transform.rotation = Quaternion.Euler(0, 0, newWideSpotlightAngle);
    }

    // https://math.stackexchange.com/questions/76457/check-if-a-point-is-within-an-ellipse
    bool IsPointInEllipse(Vector3 point)
    {
        if (!IsPlayerNearScanLine())
            return false;

        return
            ((Mathf.Pow(point.x - (transform.position.x), 2)) / (rX * rX)) +
            ((Mathf.Pow(point.y - (transform.position.y), 2)) / (rY * rY)) <= 1;
    }

    public bool IsPlayerSpotted()
    {
        return GameManager.player != null && IsPlayerInEllipse() && IsPlayerNearScanLine();
    }


    bool IsPlayerInEllipse()
    {
        return IsPointInEllipse(GameManager.player.transform.position);
    }

    bool IsPlayerNearScanLine()
    {
        return
            angleToPlayer >= scanWidth - scanWidth / 2 &&
            angleToPlayer <= scanWidth + scanWidth / 2;
    }

    Vector2 PointAtAngleDeg(float angle)
    {
        return new Vector2(
            transform.position.x + rX * Mathf.Cos(angle * Mathf.Deg2Rad),
            transform.position.y + rY * Mathf.Sin(angle * Mathf.Deg2Rad)
            );
    }

    Vector2 PointAtAngleRad(float angle)
    {
        return new Vector2(
            transform.position.x + rX * Mathf.Cos(angle),
            transform.position.y + rY * Mathf.Sin(angle)
            );
    }

    #region Gizmo

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3[] positions = CreateEllipse(rX, rY, 0, 0, theta, gizmoResolution);
        for (int i = 0; i <= gizmoResolution - 1; i++)
            Gizmos.DrawLine(positions[i], positions[i + 1]);
    }

    Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution)
    {
        Vector3[] positions = new Vector3[resolution + 1];
        Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
        Vector3 center = new Vector3(h, k, 0.0f);

        for (int i = 0; i <= resolution; i++)
        {
            float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
            positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
            positions[i] = q * positions[i] + center;
            positions[i] += transform.position;
        }

        return positions;
    }

    #endregion
}
