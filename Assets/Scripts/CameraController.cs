using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Tilemap tilemap;
    public Camera mainCamera;
    public Transform target;  // ��������Ŀ�꣨����ң�
    public float smoothSpeed = 0.125f;  // ƽ�������ٶ�

    private Bounds mapBounds;
    private float minX, maxX, minY, maxY;

    void Start()
    {


        // ��ȡ��ͼ�߽�
        mapBounds = tilemap.localBounds;

        // ��������߽�
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // ����������ƶ��ı߽�
        minX = mapBounds.min.x + cameraWidth / 2;
        maxX = mapBounds.max.x - cameraWidth / 2;
        minY = mapBounds.min.y + cameraHeight / 2;
        maxY = mapBounds.max.y - cameraHeight / 2;

        // ��ʼ����
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

        // ����Ŀ��λ�ã���ƽ��Ч����
        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x, minX, maxX),
            Mathf.Clamp(target.position.y, minY, maxY),
            transform.position.z
        );

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}