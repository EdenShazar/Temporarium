using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CinemachineVirtualCamera cam;
    CinemachineBrain brain;
    GameObject player;
    ScaleManager sm;
    
    void Awake()
    {
        SetPlayer();
        sm = FindObjectOfType<ScaleManager>();
        brain = FindObjectOfType<CinemachineBrain>();
        cam = (CinemachineVirtualCamera)brain.ActiveVirtualCamera;
        
        SetOrthographicSize();
    }

    // Update is called once per frame
    void Update()
    {
        SetOrthographicSize();
    }

    public void SetPlayer() {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    void SetOrthographicSize()
    {
        cam.m_Lens.OrthographicSize = sm.GetTargetCameraScale(player.transform.position);
    }
}
