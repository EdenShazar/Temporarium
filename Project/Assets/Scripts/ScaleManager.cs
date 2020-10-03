using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleManager : MonoBehaviour
{
    [System.Serializable]
    public struct ScaleMarker
    {
        public float yPosition;
        public float creatureScale;
        public float cameraScale;
    }

    public List<ScaleMarker> scaleMarkers;

    private void Start()
    {
        SortMarkers();
    }

    void OnDrawGizmos()
    {
        foreach (ScaleMarker s in scaleMarkers) {
            Gizmos.DrawLine(new Vector2(-100f, s.yPosition), new Vector2(100f, s.yPosition));
        }
    }

    public float GetTargetScale(Vector3 worldPosition)
    {
        float yPos = worldPosition.y;
        int next = GetNextScaleMarkerIndex(yPos);
        int prev = next - 1;
        float progress = Mathf.InverseLerp(scaleMarkers[prev].yPosition, scaleMarkers[next].yPosition, yPos);
        return Mathf.Lerp(scaleMarkers[prev].creatureScale, scaleMarkers[next].creatureScale, progress);
    }

    int GetNextScaleMarkerIndex(float screenYPosition)
    {
        for (int i = 0; i < scaleMarkers.Count; i++) {
            if (screenYPosition < scaleMarkers[i].yPosition)
                return i;
        }
        return scaleMarkers.Count - 1;
    }

    void SortMarkers() {
        scaleMarkers.Sort((a, b) => (a.yPosition.CompareTo(b.yPosition)));
    }

    public float GetTargetCameraScale(Vector3 playerWorldPosition) {
        float yPos = playerWorldPosition.y;
        int next = GetNextScaleMarkerIndex(yPos);
        int prev = next - 1;
        float progress = Mathf.InverseLerp(scaleMarkers[prev].yPosition, scaleMarkers[next].yPosition, yPos);
        return Mathf.Lerp(scaleMarkers[prev].cameraScale, scaleMarkers[next].cameraScale, progress);
    }

}
