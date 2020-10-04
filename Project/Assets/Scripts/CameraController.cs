using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    new static CinemachineVirtualCamera camera;
    CinemachineBrain brain;
    ScaleManager scaleManager;
    
    void Start()
    {
        scaleManager = FindObjectOfType<ScaleManager>();
        brain = FindObjectOfType<CinemachineBrain>();
        camera = (CinemachineVirtualCamera)brain.ActiveVirtualCamera;

        SetOrthographicSize();
    }

    void Update()
    {
        SetOrthographicSize();
    }

    public static void SetFollowTarget(Transform target)
    {
        camera.Follow = target;
    }

    void SetOrthographicSize()
    {
        camera.m_Lens.OrthographicSize = scaleManager.GetTargetCameraScale(GameManager.Player.transform.position);
    }
}
