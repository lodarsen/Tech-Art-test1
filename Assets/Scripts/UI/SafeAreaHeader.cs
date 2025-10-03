using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode] // Allows operation in the editor
public class SafeAreaHeader : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
    private int _lastScreenWidth = 0;
    private int _lastScreenHeight = 0;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        Refresh();
    }
#if UNITY_EDITOR
    void Update()
    {
        // Only in the editor - check for changes in screen aspect ratio
        if (!Application.isPlaying)
        {

                // Checking the change of safe area and resolution
                if (_lastSafeArea != Screen.safeArea ||
                    _lastScreenWidth != Screen.width ||
                    _lastScreenHeight != Screen.height)
                {
                    Refresh();
                }
        }
    }
#endif

    void Refresh()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        // Safe Area Coordinate Conversion
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;

        // Force immediate update
        ForceUpdateRectTransforms();
    }

    private void ForceUpdateRectTransforms()
    {
        // Forces a system recalculation
        if (_rectTransform != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
            Canvas.ForceUpdateCanvases();
            _rectTransform.ForceUpdateRectTransforms();
        }
    }
}