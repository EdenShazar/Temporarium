using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    [SerializeField] [Range(0f, 1f)] float maxSpotlightScaling = 0.5f;
    [SerializeField] [Range(0f, 1f)] float secondarySpotlightSpeed = 0.02f;
    [SerializeField] [Min(1)] int gizmoResolution = 30;
#pragma warning restore CS0649

    Vector2 guardingPosition;

    float wideSpotlightBaseIntensity;
    float mediumSpotlightBaseIntensity;
    float narrowSpotlightBaseIntensity;

    float wideSpotlightBaseOuterRadius;
    float mediumSpotlightBaseOuterRadius;
    float narrowSpotlightBaseOuterRadius;

    float wideSpotlightBaseInnerRadius;
    float mediumSpotlightBaseInnerRadius;
    float narrowSpotlightBaseInnerRadius;

    #region Animator hashes

    readonly int eatHash = Animator.StringToHash("Eat");

    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        gem = transform.GetChild(0).GetComponent<SpriteRenderer>();

        Light2D[] lights = GetComponentsInChildren<Light2D>();
        wideSpotlight = lights.Single(light => light.name == "Wide spotlight");
        mediumSpotlight = lights.Single(light => light.name == "Medium spotlight");
        narrowSpotlight = lights.Single(light => light.name == "Narrow spotlight");

        wideSpotlightBaseIntensity = wideSpotlight.intensity;
        mediumSpotlightBaseIntensity = mediumSpotlight.intensity;
        narrowSpotlightBaseIntensity = narrowSpotlight.intensity;

        wideSpotlightBaseInnerRadius = wideSpotlight.pointLightInnerRadius;
        mediumSpotlightBaseInnerRadius = mediumSpotlight.pointLightInnerRadius;
        narrowSpotlightBaseInnerRadius = narrowSpotlight.pointLightInnerRadius;

        wideSpotlightBaseOuterRadius = wideSpotlight.pointLightOuterRadius;
        mediumSpotlightBaseOuterRadius = mediumSpotlight.pointLightOuterRadius;
        narrowSpotlightBaseOuterRadius = narrowSpotlight.pointLightOuterRadius;


        Random.InitState(seed: System.DateTime.Now.Millisecond);
        scanlineAngle = Random.Range(-180f, 180f);

        guardingPosition = transform.position;

        LoseGem();
    }

    void Update()
    {
        AdvanceScanline();
        UpdateLights();

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

        if (GameManager.player.transform == GameManager.GemHolder)
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
        GameManager.GemHolder = transform;
        GameManager.NotifyGemOwner(isPlayer: false);
    }

    void LoseGem()
    {
        isHoldingGem = false;
        gem.color = Color.clear;        
    }

    #region Light methods

    void AdvanceScanline()
    {
        scanlineAngle = (scanlineAngle + scanSpeed * Time.deltaTime).WrapAngleDeg();

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, WorldPointAtAngleDeg(scanlineAngle - scanWidth / 2f));
        Debug.DrawLine(transform.position, WorldPointAtAngleDeg(scanlineAngle + scanWidth / 2f));
#endif
    }

    void UpdateLights()
    {
        float desiredAngle = (narrowSpotlight.transform.position.ToVector2() - WorldPointAtAngleDeg(scanlineAngle)).GetAngleDeg() + 90;
        float factor = Mathf.Sin(-scanlineAngle * Mathf.Deg2Rad) * maxSpotlightScaling + 1;

        // Narrow spotlight
        narrowSpotlight.transform.rotation = Quaternion.Euler(0, 0, desiredAngle);

        narrowSpotlight.intensity = factor * narrowSpotlightBaseIntensity;
        narrowSpotlight.pointLightInnerRadius = factor * narrowSpotlightBaseInnerRadius;
        narrowSpotlight.pointLightOuterRadius = factor * narrowSpotlightBaseOuterRadius;

        // Medium spotlight
        float currentMediumSpotlightAngle = mediumSpotlight.transform.rotation.eulerAngles.z;
        float newMediumSpotlightAngle = Mathf.LerpAngle(currentMediumSpotlightAngle, desiredAngle, secondarySpotlightSpeed * 2);
        mediumSpotlight.transform.rotation = Quaternion.Euler(0, 0, newMediumSpotlightAngle);

        mediumSpotlight.intensity = factor * mediumSpotlightBaseIntensity;
        mediumSpotlight.pointLightInnerRadius = factor * mediumSpotlightBaseInnerRadius;
        mediumSpotlight.pointLightOuterRadius = factor * mediumSpotlightBaseOuterRadius;

        // Wide spotlight
        float currentWideSpotlightAngle = wideSpotlight.transform.rotation.eulerAngles.z;
        float newWideSpotlightAngle = Mathf.LerpAngle(currentWideSpotlightAngle, desiredAngle, secondarySpotlightSpeed);
        wideSpotlight.transform.rotation = Quaternion.Euler(0, 0, newWideSpotlightAngle);

        wideSpotlight.intensity = factor * wideSpotlightBaseIntensity;
        wideSpotlight.pointLightInnerRadius = factor * wideSpotlightBaseInnerRadius;
        wideSpotlight.pointLightOuterRadius = factor * wideSpotlightBaseOuterRadius;
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

    Vector2 WorldPointAtAngleDeg(float angle)
    {
        return new Vector2(
            transform.position.x + rX * Mathf.Cos(angle * Mathf.Deg2Rad),
            transform.position.y + rY * Mathf.Sin(angle * Mathf.Deg2Rad)
            );
    }

    Vector2 WorldPointAtAngleRad(float angle)
    {
        return new Vector2(
            transform.position.x + rX * Mathf.Cos(angle),
            transform.position.y + rY * Mathf.Sin(angle)
            );
    }

    Vector2 LocalPointAtAngleDeg(float angle)
    {
        return new Vector2(rX * Mathf.Cos(angle * Mathf.Deg2Rad), rY * Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    Vector2 LocalPointAtAngleRad(float angle)
    {
        return new Vector2(rX * Mathf.Cos(angle), rY * Mathf.Sin(angle));
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
