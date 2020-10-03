using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement variabless
    Vector3 mouseScreenPos;
    Vector3 objectScreenPos;

    public float MoveAngle { get; private set; }

    void Update()
    {
        MoveToMouse();
    }

    void MoveToMouse() {
        // calculate diff in screen space
        mouseScreenPos = Input.mousePosition;
        objectScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        MoveAngle = (mouseScreenPos - objectScreenPos).ToVector2().GetAngleRad();
    }
}


