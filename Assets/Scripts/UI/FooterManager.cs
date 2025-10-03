using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Burst.Intrinsics;
using UnityEngine.UIElements;

public class FooterManager : MonoBehaviour
{
    [Header("Button management")]
    public List<BottomBarView> footerButtons;

    [Header("Global settings")]
    public bool allowNoActiveButton = false;
    public float animationDuration = 0.3f;
    public float scaleAmount = 0.8f;

    private BottomBarView _currentlyActiveButton;

    private void Start()
    {
        // Make sure all buttons are disabled at startup
        DeactivateAllButtons();
    }

    public void SetActiveButton(BottomBarView clickedButton)
    {
        // If an active button has already been clicked
        if (_currentlyActiveButton == clickedButton)
        {
            if (allowNoActiveButton)
            {
                // Unclicking an active button
                clickedButton.Deactivate();
                AnimateBackgroundToDefault(clickedButton);
                _currentlyActiveButton = null;
            }
            return;
        }

        // Specify the direction of the animation
        int direction = 0; // No direction by default (first activation)
        if (_currentlyActiveButton != null)
        {
            int currentIndex = footerButtons.IndexOf(_currentlyActiveButton);
            int clickedIndex = footerButtons.IndexOf(clickedButton);
            direction = (clickedIndex > currentIndex) ? 1 : -1;
        }

        // Deactivate the previous active button
        if (_currentlyActiveButton != null)
        {
            _currentlyActiveButton.Deactivate();
            AnimateBackgroundToDefault(_currentlyActiveButton);
        }

        // Activate the new button
        clickedButton.Activate();
        _currentlyActiveButton = clickedButton;

        AnimateBackground(clickedButton, direction);
    }

    private void AnimateBackground(BottomBarView targetButton, int direction)
    {
        RectTransform backgroundTransform = targetButton.activeBackgroundTransform;
        if (backgroundTransform == null) return;

        // End existing animations before starting a new one
        DOTween.Kill(backgroundTransform);




        // Set the pivot depending on the direction
        if (direction == 0)
        {
            // Set initial scale
            backgroundTransform.localScale = new Vector3(1f, scaleAmount, 1f);
            // Vertical scaling (from bottom) - initial state or no previous button
            backgroundTransform.pivot = new Vector2(0.5f, 0f);

            Sequence scaleSequence = DOTween.Sequence().SetAutoKill(true);
            scaleSequence.Append(backgroundTransform.DOScaleY(1.05f, animationDuration / 2));
            scaleSequence.Append(backgroundTransform.DOScaleY(1f, animationDuration / 2));

        }
        else
        {
            // Set initial scale
            backgroundTransform.localScale = new Vector3(scaleAmount, 1f, 1f);
            // Horizontal scaling (from the opposite side)
            backgroundTransform.pivot = new Vector2(direction == 1 ? 1f : 0f, 0.5f);

            Sequence scaleSequence = DOTween.Sequence().SetAutoKill(true);
            scaleSequence.Append(backgroundTransform.DOScaleX(1.05f, animationDuration / 2));
            scaleSequence.Append(backgroundTransform.DOScaleX(1f, animationDuration / 2));

        }
    }

    private void AnimateBackgroundToDefault(BottomBarView targetButton)
    {
        RectTransform backgroundTransform = targetButton.activeBackgroundTransform;
        if (backgroundTransform == null) return;

        DOTween.Kill(backgroundTransform);
        // Restore vertical scale
        backgroundTransform.pivot = new Vector2(0.5f, 0f);
        backgroundTransform.DOScaleY(scaleAmount, animationDuration);
    }


    public void DeactivateAllButtons()
    {
        foreach (var button in footerButtons)
        {
            button.Deactivate();
            AnimateBackgroundToDefault(button);
        }
        _currentlyActiveButton = null;
    }

    // A method to manually set the active button from another script
    public void SetActiveButtonByIndex(int index)
    {
        if (index >= 0 && index < footerButtons.Count)
        {
            SetActiveButton(footerButtons[index]);
        }
        else
        {
            Debug.LogWarning($"Index {index} is out of range of button list!");
        }
    }
    public bool AreAllButtonsDeactivated()
    {
        BottomBarView[] buttons = GetComponentsInChildren<BottomBarView>();
        foreach (BottomBarView button in buttons)
        {
            if (button.IsActive())
                return false;
        }
        return true;
    }
}