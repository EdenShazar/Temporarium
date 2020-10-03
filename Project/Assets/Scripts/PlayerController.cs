using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement variabless
    Vector3 mouseScreenPos;
    Vector3 objectScreenPos;
    public float speed = 0.01f; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        MoveToMouse();
    }

    void MoveToMouse() {
        // calculate diff in screen space
        mouseScreenPos = Input.mousePosition;
        objectScreenPos = Camera.main.WorldToScreenPoint(transform.position);

        float deltaX = mouseScreenPos.x - objectScreenPos.x;
        float deltaY = mouseScreenPos.y - objectScreenPos.y;

        Vector2 moveVector = new Vector2(deltaX, deltaY).normalized * speed;
        transform.Translate(moveVector, this.transform);
    }
}


