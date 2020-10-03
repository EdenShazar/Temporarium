using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    public float rx = 2f;
    public float ry = 1f;
    public float theta = 0f;
    Ellipse ellipse;
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
        angleToPlayer = Vector2.Angle(transform.position, GameManager.player.transform.position);
        UpdateEllipse();
        ellipse.Draw();
        isPlayerInRange = IsPointInEllipse(GameManager.player.transform.position);
    }

    void UpdateEllipse() {
        ellipse.h = transform.position.x;
        ellipse.k = transform.position.y;
        ellipse.a = rx;
        ellipse.b = ry;
        ellipse.theta = theta;
    }

 
        bool IsPointInEllipse(Vector3 pnt) {
        return
            ((-(pnt.x - transform.position.x) / (rx * rx)) +
            (-(pnt.y - transform.position.y) / (ry * ry))) <= 1;
    }
}
