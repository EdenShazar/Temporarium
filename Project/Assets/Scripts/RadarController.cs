using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    // Ellipse vars
    public float rx = 2f;
    public float ry = 1f;
    public float theta = 0f;
    Ellipse ellipse;
    public bool isPlayerInRange = false;
    public float angleToPlayer = 0f;

    //scan vars
    public float scanSpeed = 30f;
    public float scanlineAngle = 0f;
    public float scanWidth = 70f;

    // Start is called before the first frame update
    void Start()
    {
        ellipse = GetComponent<Ellipse>();
        UpdateEllipse();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEllipse();
        AdvanceScanline();

        angleToPlayer = (GameManager.player.transform.position - transform.position).ToVector2().GetAngleDeg();
        isPlayerInRange = IsPointInEllipse(GameManager.player.transform.position);

        DebugVizualize();
    }

    void DebugVizualize() {
        ellipse.Draw();
        DrawScanline(scanlineAngle - scanWidth/2f);
        DrawScanline(scanlineAngle + scanWidth/2f);
    }

    void DrawScanline(float angle) {
        Debug.DrawLine(transform.position,
            new Vector3(
            transform.position.x + rx * Mathf.Cos(angle * Mathf.PI / 180.0f),
            transform.position.y + ry * Mathf.Sin(angle * Mathf.PI / 180.0f),
            transform.position.z));
    }

    void AdvanceScanline() {
        float add = (Time.deltaTime * scanSpeed);
        scanlineAngle = ((scanlineAngle + 180f + add) % 360f) - 180f;
    }

    void UpdateEllipse() {
        ellipse.centerX = 0f; //line rendrer is local space
        ellipse.centerY = 0f;
        ellipse.rX = rx;
        ellipse.rY = ry;
        ellipse.theta = theta;
    }

    // https://math.stackexchange.com/questions/76457/check-if-a-point-is-within-an-ellipse
    bool IsPointInEllipse(Vector3 pnt) {
        return
            ((Mathf.Pow(pnt.x - (ellipse.centerX + transform.position.x), 2)) / (ellipse.rX * ellipse.rX)) +
            ((Mathf.Pow(pnt.y - (ellipse.centerY + transform.position.y), 2)) / (ellipse.rY * ellipse.rY)) <= 1;
    }

    public bool IsPlayerSpoetted() {
        return IsPlayerInEllipse() && IsPlayerNearScanLine();
    }


    bool IsPlayerInEllipse() {
        return IsPointInEllipse(GameManager.player.transform.position);
    }

    bool IsPlayerNearScanLine()
    {
        return
            angleToPlayer >= scanWidth - scanWidth / 2 &&
            angleToPlayer <= scanWidth + scanWidth / 2;
    }

}
