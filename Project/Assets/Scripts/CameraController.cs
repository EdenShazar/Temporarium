using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CinemachineVirtualCamera cam;
    CinemachineBrain brain;
    GameObject player;
    ScaleManager sm;
    
    // Start is called before the first frame update
    void Start()
    {
        SetPlayer();
        sm = FindObjectOfType<ScaleManager>();
        brain = FindObjectOfType<CinemachineBrain>();
        cam = (CinemachineVirtualCamera)brain.ActiveVirtualCamera;
    }

    // Update is called once per frame
    void Update()
    {
        cam.m_Lens.OrthographicSize = sm.GetTargetCameraScale(player.transform.position);
    }

    public void SetPlayer() {
        player = FindObjectOfType<PlayerController>().gameObject;
    }
}
