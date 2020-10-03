using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    public float rx = 2f;
    public float ry = 1f;
    public float theta = 0f;
    Ellipse ellipse;

    // for debugging TODO: remove
    public bool isPlayerInRange = false; 
    public float angleToPlayer = 0f; 

    // Start is called before the first frame update
    void Start()
    {
        ellipse = GetComponent<Ellipse>();
    }

    // Update is called once per frame
    void Update()
    {
        angleToPlayer = (GameManager.player.transform.position - transform.position).ToVector2().GetAngleDeg();
        UpdateEllipse();
        ellipse.Draw();
        isPlayerInRange = IsPointInEllipse(GameManager.player.transform.position);
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
}
