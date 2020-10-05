using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class GuardController : MonoBehaviour
{
    bool isHoldingGem;

    Animator animator;
    SpriteRenderer gem;
    Light2D wideSpotlight;
    Light2D mediumSpotlight;
    Light2D narrowSpotlight;

    float scanlineAngle = 0f;

#pragma warning disable CS0649
    [SerializeField] Altar nearestAltar;
    [SerializeField] [Min(0.1f)] float chaseSpeed = 2f;
    [SerializeField] [Min(0.1f)] float returnSpeed = 1f;
    [SerializeField] float rX = 5f;
    [SerializeField] float rY = 1.7f;
    [SerializeField] float theta = 0f;
    [SerializeField] float scanSpeed = 30f;
    [SerializeField] float scanWidth = 70f;
    [SerializeField] [Range(0f, 1f)] float maxSpotlightIntensity = 0.5f;
    [SerializeField] [Range(0f, 1f)] float secondarySpotlightSpeed = 0.02f;
    [SerializeField] [Min(1)] int gizmoResolution = 30;
#pragma warning restore CS0649

    Vector2 guardingPosition;

    float wideSpotlightBaseIntensity;
    float mediumSpotlightBaseIntensity;
    float narrowSpotlightBaseIntensity;

    #region Animator hashes

    readonly int eatHash = Animator.StringToHash("Eat");

    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        gem = transform.GetChild(0).GetComponent<SpriteRenderer>();

        Light2D[] spotlights = GetComponentsInChildren<Light2D>();
        wideSpotlight = spotlights.Single(light => light.name == "Wide spotlight");
        mediumSpotlight = spotlights.Single(light => light.name == "Medium spotlight");
        narrowSpotlight = spotlights.Single(light => light.name == "Narrow spotlight");

        wideSpotlightBaseIntensity = wideSpotlight.intensity;
        mediumSpotlightBaseIntensity = mediumSpotlight.intensity;
        narrowSpotlightBaseIntensity = narrowSpotlight.intensity;

        Random.InitState(seed: System.DateTime.Now.Millisecond);
        scanlineAngle = Random.Range(-180f, 180f);

        guardingPosition = transform.position;

        LoseGem();
    }

    void Update()
    {
        AdvanceScanline();
        UpdateSpotlights();

        if (isHoldingGem)
        {
            ReturnGemToAltar();
            return;
        }

        if (IsPlayerSpotted() && GameManager.player.IsSpottable)
            ChasePlayer();
        else
            ReturnToGuardingPosition();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform != GameManager.player.transform)
            return;

        if (isHoldingGem)
        { 
            LoseGem();
            GameManager.player.CarryGem();
            return;
        }

        if (GameManager.player.transform == GameManager.gemHolder)
            EatPlayer();
    }

    void ReturnGemToAltar()
    {
        Vector2 toNearestAltar = nearestAltar.transform.position - transform.position;

        if (toNearestAltar.sqrMagnitude <= 0.5f)
        {
            LoseGem();
            nearestAltar.StoreGem();
            return;
        }

        transform.Translate(toNearestAltar.normalized * returnSpeed * Time.deltaTime);
    }

    void ChasePlayer()
    {
        Vector2 toPlayer = GameManager.player.transform.position - transform.position;

        transform.Translate(toPlayer.normalized * chaseSpeed * Time.deltaTime);
    }

    void ReturnToGuardingPosition()
    {
        Vector2 toGuardingPosition = guardingPosition - transform.position.ToVector2();

        if (toGuardingPosition.sqrMagnitude <= 0.05f)
            return;

        transform.Translate(toGuardingPosition.normalized * returnSpeed * Time.deltaTime);
    }

    void EatPlayer()
    {
        animator.SetTrigger(eatHash);
        GameManager.player.Eaten();
        HoldGem();
    }

    void HoldGem()
    {
        isHoldingGem = true;
        gem.color = Color.white;
        GameManager.gemHolder = transform;
    }

    void LoseGem()
    {
        isHoldingGem = false;
        gem.color = Color.clear;
    }

    #region Spotlight methods

    void AdvanceScanline()
    {
        scanlineAngle = (scanlineAngle + scanSpeed * Time.deltaTime).WrapAngleDeg();

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, PointAtAngleDeg(scanlineAngle - scanWidth / 2f));
        Debug.DrawLine(transform.position, PointAtAngleDeg(scanlineAngle + scanWidth / 2f));
#endif
    }

    void UpdateSpotlights()
    {
        float desiredAngle = (narrowSpotlight.transform.position.ToVector2() - PointAtAngleDeg(scanlineAngle)).GetAngleDeg() + 90;
        float intensityFactor = Mathf.Sin(-scanlineAngle * Mathf.Deg2Rad);

        // Narrow
        narrowSpotlight.transform.rotation = Quaternion.Euler(0, 0, desiredAngle);

        narrowSpotlight.intensity = (intensityFactor * maxSpotlightIntensity + 1) * narrowSpotlightBaseIntensity;

        // Medium
        float currentMediumSpotlightAngle = mediumSpotlight.transform.rotation.eulerAngles.z;
        float newMediumSpotlightAngle = Mathf.LerpAngle(currentMediumSpotlightAngle, desiredAngle, secondarySpotlightSpeed * 2);
        mediumSpotlight.transform.rotation = Quaternion.Euler(0, 0, newMediumSpotlightAngle);

        mediumSpotlight.intensity = (intensityFactor * maxSpotlightIntensity + 1) * mediumSpotlightBaseIntensity;

        // Wide
        float currentWideSpotlightAngle = wideSpotlight.transform.rotation.eulerAngles.z;
        float newWideSpotlightAngle = Mathf.LerpAngle(currentWideSpotlightAngle, desiredAngle, secondarySpotlightSpeed);
        wideSpotlight.transform.rotation = Quaternion.Euler(0, 0, newWideSpotlightAngle);

        wideSpotlight.intensity = (intensityFactor * maxSpotlightIntensity + 1) * wideSpotlightBaseIntensity;
    }

    // https://math.stackexchange.com/questions/76457/check-if-a-point-is-within-an-ellipse
    bool IsPointInEllipse(Vector3 point)
    {
        return
            ((Mathf.Pow(point.x - (transform.position.x), 2)) / (rX * rX)) +
            ((Mathf.Pow(point.y - (transform.position.y), 2)) / (rY * rY)) <= 1;
    }

    bool IsPlayerSpotted()
    {
        return GameManager.player != null && IsPlayerInEllipse();
    }

    bool IsPlayerInEllipse()
    {
        //if (!IsPlayerNearScanLine())
        //    return false;

        return IsPointInEllipse(GameManager.player.transform.position);
    }

    bool IsPlayerNearScanLine()
    {
        float angleToPlayer = (GameManager.player.transform.position - transform.position).ToVector2().GetAngleDeg();

        return angleToPlayer >= scanWidth - scanWidth / 2 && angleToPlayer <= scanWidth + scanWidth / 2;
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

#endregion

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
