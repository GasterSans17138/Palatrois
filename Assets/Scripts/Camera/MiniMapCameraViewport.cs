using UnityEngine;
using UnityEngine.UI;

public class MiniMapCameraViewport : MonoBehaviour
{
    public enum ViewportColor
    {
        Bleu,
        Rouge
    }

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera miniMapCamera;

    [Header("Minimap UI")]
    public RectTransform minimapRect;

    [Header("Affichage")]
    public ViewportColor color = ViewportColor.Bleu;
    public float transparency = 0.3f;

    [SerializeField] private float scaleMultiplier = 1f;

    private RectTransform viewportRect;

    void Start()
    {
        CreateViewportRect();
    }

    void CreateViewportRect()
    {
        GameObject rectObj = new GameObject("ViewportRect", typeof(Image));
        rectObj.transform.SetParent(minimapRect, false);

        viewportRect = rectObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.zero;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image image = rectObj.GetComponent<Image>();
        Color selectedColor = (color == ViewportColor.Bleu) ? Color.blue : Color.red;
        selectedColor.a = transparency;
        image.color = selectedColor;

        image.raycastTarget = false;
    }

    void LateUpdate()
    {
        Vector3 camPos = mainCamera.transform.position;

        float height = mainCamera.orthographic
            ? mainCamera.orthographicSize
            : camPos.y;

        height *= scaleMultiplier;
        float width = height * mainCamera.aspect;

        Vector3 bottomLeft = new Vector3(camPos.x - width, camPos.y, camPos.z - height);
        Vector3 topRight = new Vector3(camPos.x + width, camPos.y, camPos.z + height);

        Vector3 viewportBL = miniMapCamera.WorldToViewportPoint(bottomLeft);
        Vector3 viewportTR = miniMapCamera.WorldToViewportPoint(topRight);

        viewportBL.x = Mathf.Clamp01(viewportBL.x);
        viewportBL.y = Mathf.Clamp01(viewportBL.y);
        viewportTR.x = Mathf.Clamp01(viewportTR.x);
        viewportTR.y = Mathf.Clamp01(viewportTR.y);

        viewportRect.anchorMin = new Vector2(viewportBL.x, viewportBL.y);
        viewportRect.anchorMax = new Vector2(viewportTR.x, viewportTR.y);
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
    }
}
