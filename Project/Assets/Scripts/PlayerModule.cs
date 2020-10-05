using UnityEngine;

public class PlayerModule
{
    public bool enabled;
    public readonly Transform transform;

    public PlayerModule(Transform transform)
    {
        enabled = false;
        this.transform = transform;
    }

    public float GetMoveAngle()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 objectScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        return (mouseScreenPos - objectScreenPos).ToVector2().GetAngleRad();
    }
}


