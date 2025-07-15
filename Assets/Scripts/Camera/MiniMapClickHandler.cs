using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMapClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Camera miniMapCamera;
    public Camera mainCamera;
    public RectTransform minimapRect;
    public RenderTexture minimapTexture;

    public LayerMask groundLayer;
    TopCamera topCamera;

    void Start()
    {
        topCamera = mainCamera.GetComponent<TopCamera>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        Rect rect = minimapRect.rect;
        float uvX = (localCursor.x - rect.x) / rect.width;
        float uvY = (localCursor.y - rect.y) / rect.height;

        float pixelX = uvX * minimapTexture.width;
        float pixelY = uvY * minimapTexture.height;

        Ray ray = miniMapCamera.ScreenPointToRay(new Vector3(pixelX, pixelY, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
        {
            topCamera.MoveToPosition(hit.point);
        }
    }
}
