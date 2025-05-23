using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Tilemap tilemap;
    public Camera mainCamera;
    public Transform target;  // 相机跟随的目标（如玩家）
    public float smoothSpeed = 0.125f;  // 平滑跟随速度

    private Bounds mapBounds;
    private float minX, maxX, minY, maxY;

    void Start()
    {


        // 获取地图边界
        mapBounds = tilemap.localBounds;

        // 计算相机边界
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // 计算相机可移动的边界
        minX = mapBounds.min.x + cameraWidth / 2;
        maxX = mapBounds.max.x - cameraWidth / 2;
        minY = mapBounds.min.y + cameraHeight / 2;
        maxY = mapBounds.max.y - cameraHeight / 2;

        // 初始居中
        if (target == null)
        {
            transform.position = new Vector3(
                Mathf.Clamp(mapBounds.center.x, minX, maxX),
                Mathf.Clamp(mapBounds.center.y, minY, maxY),
                transform.position.z
            );
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置（带平滑效果）
        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x, minX, maxX),
            Mathf.Clamp(target.position.y, minY, maxY),
            transform.position.z
        );

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}