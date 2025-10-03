using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SafeAreaFooter : MonoBehaviour
{
    [Header("Footer Backgrounds")]
    public RectTransform footerBackground; 
    public RectTransform shadowBackground; 

    [Header("Default Values")]
    public float defaultFooterHeight = 200f;
    public float defaultShadowYPosition = 180f;

    [Header("Optimization")]
    public bool refreshInEditMode = true;
    public bool refreshInRuntime = true;

    private RectTransform _rectTransform;
    private Canvas _canvas;

    private float _lastAspectRatio;
    private Vector2 _lastScreenSize;

#if UNITY_EDITOR
    void Update()
    {
        // Only in the editor - check for changes in screen aspect ratio
        if (!Application.isPlaying)
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            float currentAspectRatio = (float)Screen.width / Screen.height;

            // Check if the resolution or aspect ratio has changed
            bool screenSizeChanged = currentScreenSize != _lastScreenSize;
            bool aspectRatioChanged = Mathf.Abs(currentAspectRatio - _lastAspectRatio) > 0.01f;

            if (screenSizeChanged || aspectRatioChanged)
            {
                _lastScreenSize = currentScreenSize;
                _lastAspectRatio = currentAspectRatio;

                // A small delay so that Unity can update the safe area.
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null) Refresh();
                };
            }
        }
    }
#endif

    void OnEnable()
    {
        Refresh();
    }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        // Set default values ​​in the editor
        if (Application.isEditor && !Application.isPlaying)
        {
            SetDefaultValues();
        }
        Refresh();
    }

    void Start()
    {
        // Make sure the default values ​​are set in runtime as well
        SetDefaultValues();
        Refresh();
    }

    void SetDefaultValues()
    {
        // Set default footer height
        if (_rectTransform != null)
        {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultFooterHeight);
        }

        // Set the default height of the first background
        if (footerBackground != null)
        {
            footerBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultFooterHeight);
        }

        // Set default position of second background
        if (shadowBackground != null)
        {
            shadowBackground.anchoredPosition = new Vector2(
                shadowBackground.anchoredPosition.x,
                defaultShadowYPosition
            );
        }
    }

    void Refresh()
    {
        if (_rectTransform == null || _canvas == null || footerBackground == null) return;

        if (Application.isPlaying && !refreshInRuntime) return;
        if (!Application.isPlaying && !refreshInEditMode) return;

        // Calculate the height of the lower safe area
        float bottomSafeAreaHeight = Screen.safeArea.yMin;

        // Convert to UI units
        float bottomSafeAreaInUI = bottomSafeAreaHeight / _canvas.scaleFactor;

        // Apply changes to footer and background
        ApplyFooterHeight(bottomSafeAreaInUI);
   }

    void ApplyFooterHeight(float bottomOffset)
    {
        float newHeight = defaultFooterHeight + bottomOffset;

        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        footerBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        if (shadowBackground != null)
        {
            shadowBackground.anchoredPosition = new Vector2(
                shadowBackground.anchoredPosition.x,
                defaultShadowYPosition + bottomOffset
            );
        }

        // Force a layout refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    // Method to reset to default values
        [ContextMenu("Reset to Default Values")]
    public void ResetToDefault()
    {
        SetDefaultValues();
        Refresh();
    }

    // Method to manually refresh in the editor
    public void ForceRefresh()
    {
        Refresh();
    }
}