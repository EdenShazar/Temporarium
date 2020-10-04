using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CinemachineVirtualCamera cam;
    CinemachineBrain brain;
    ScaleManager sm;
    
    void Start()
    {
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

    void SetOrthographicSize()
    {
        cam.m_Lens.OrthographicSize = sm.GetTargetCameraScale(GameManager.Player.transform.position);
    }
}
