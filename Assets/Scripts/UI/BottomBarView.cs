using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarView : MonoBehaviour
{
    [Header("References")]
    public LayoutElement layoutElement;
    public RectTransform iconTransform;
    public TextMeshProUGUI textElement;
    public Image activeBackground;
    public RectTransform activeBackgroundTransform;
    public Button buttonComponent;
    public Image iconImage;

    [Header("State settings")]
    public float activeFlexibleWidth = 0.3f;
    public float inactiveFlexibleWidth = 0.1f;
    public float activeIconYPos = 40f;
    public float activeAnimationDuration = 0.2f;
    public float deactiveAnimationDuration = 0.01f;
    public float lockedIconScale = 0.8f;

    public bool isLocked = false;

    [Header("Lock Settings")]
    public Sprite lockedIconSprite;
    public Sprite normalIconSprite;

    // Delegate for content events
    public System.Action<BottomBarView> OnContentActivated;
    public System.Action OnAllContentClosed;

    private Vector3 _originalIconPosition;
    private bool _isActive = false;

    // Sequences for animation
    private Sequence _activateSequence;
    private Sequence _deactivateSequence;

    private FooterManager _footerManager;

    private void Start()
    {
        // Save initial icon position
        _originalIconPosition = iconTransform.localPosition;

        // Download components if not assigned
        if (buttonComponent == null)
            buttonComponent = GetComponent<Button>();

        if (iconImage == null)
            iconImage = iconTransform.GetComponent<Image>();

        // Save normal icon if not assigned
        if (normalIconSprite == null && iconImage != null)
            normalIconSprite = iconImage.sprite;

        // Find a manager in the hierarchy
        _footerManager = GetComponentInParent<FooterManager>();
        if (_footerManager == null)
        {
            Debug.LogWarning("FooterManager not found in parent hierarchy!");
        }

        // Assign a method to the click event
        if (buttonComponent != null)
            buttonComponent.onClick.AddListener(OnButtonClick);

        // Prepare animation sequences
        PrepareActivateSequence();
        PrepareDeactivateSequence();

        // Set the initial state
        SetInitialState();
    }

    private void PrepareActivateSequence()
    {
        _activateSequence = DOTween.Sequence().SetAutoKill(false).Pause();

        _activateSequence.Join(DOTween.To(
            () => layoutElement.flexibleWidth,
            x => layoutElement.flexibleWidth = x,
            activeFlexibleWidth,
            activeAnimationDuration
        ));
        _activateSequence.Join(iconTransform.DOLocalMoveY(activeIconYPos, activeAnimationDuration));
        _activateSequence.Join(textElement.DOFade(1f, activeAnimationDuration));
        _activateSequence.Join(activeBackground.DOFade(1f, activeAnimationDuration));

        // Add event when activation animation completes
        _activateSequence.OnComplete(() => OnActivationComplete());
    }

    private void PrepareDeactivateSequence()
    {
        _deactivateSequence = DOTween.Sequence().SetAutoKill(false).Pause();

        _deactivateSequence.Join(DOTween.To(
            () => layoutElement.flexibleWidth,
            x => layoutElement.flexibleWidth = x,
            inactiveFlexibleWidth,
            deactiveAnimationDuration
        ));
        _deactivateSequence.Join(iconTransform.DOLocalMoveY(_originalIconPosition.y, deactiveAnimationDuration));
        _deactivateSequence.Join(textElement.DOFade(0f, deactiveAnimationDuration));
        _deactivateSequence.Join(activeBackground.DOFade(0f, deactiveAnimationDuration));
        _deactivateSequence.Join(iconTransform.DOScale(1f, deactiveAnimationDuration));

        // Add event when deactivation animation completes
        _deactivateSequence.OnComplete(() => OnDeactivationComplete());
    }

    private void OnActivationComplete()
    {
        // Invoke content activated event
        OnContentActivated?.Invoke(this);
    }

    private void OnDeactivationComplete()
    {
        // Check if all buttons are deactivated and invoke closed event
        if (AreAllButtonsDeactivated())
        {
            OnAllContentClosed?.Invoke();
        }
    }

    private bool AreAllButtonsDeactivated()
    {
        if (_footerManager != null)
        {
            // Use FooterManager to check if all buttons are deactivated
            return _footerManager.AreAllButtonsDeactivated();
        }
        else
        {
            // Fallback solution - check all BottomBarView components in scene
            BottomBarView[] allButtons = FindObjectsOfType<BottomBarView>();
            foreach (BottomBarView button in allButtons)
            {
                if (button.IsActive())
                    return false;
            }
            return true;
        }
    }

    // Locked state managing
    public void SetLocked(bool locked)
    {
        if (locked)
        {
            // Lock button
            buttonComponent.interactable = false;

            if (iconImage == null)
                iconImage = GetComponent<Image>();

            // Change the icon to locked icon
            if (iconImage != null && lockedIconSprite != null)
            {
                RectTransform iconRectTransform = iconImage.GetComponent<RectTransform>();
                iconRectTransform.localScale = new Vector3(lockedIconScale, lockedIconScale, 1f);
                iconImage.sprite = lockedIconSprite;
            }

            if (_isActive)
            {
                textElement.DOFade(0f, 0.2f);
            }
        }
        else
        {
            // Unlock button
            buttonComponent.interactable = true;

            // Change the icon to normal icon
            if (iconImage != null && normalIconSprite != null)
            {
                RectTransform iconRectTransform = iconImage.GetComponent<RectTransform>();
                iconRectTransform.localScale = Vector3.one;
                iconImage.sprite = normalIconSprite;
            }

            if (_isActive)
            {
                textElement.DOFade(1f, 0.2f);
            }
        }

        isLocked = locked;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    // Modified Activate/Deactivate methods
    public void Activate()
    {
        if (_isActive || isLocked) return;
        _isActive = true;

        _deactivateSequence.Pause();
        _activateSequence.Restart();
    }

    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;

        _activateSequence.Pause();
        _deactivateSequence.Restart();
    }

    public void ForceDeactivateWithoutEvent()
    {
        if (!_isActive) return;
        _isActive = false;

        _activateSequence.Pause();
        _deactivateSequence.OnComplete(null); // Remove event for forced deactivation
        _deactivateSequence.Restart();

        // Reattach the event after forced deactivation
        _deactivateSequence.OnComplete(() => OnDeactivationComplete());
    }

    private void SetInitialState()
    {
        layoutElement.flexibleWidth = inactiveFlexibleWidth;
        textElement.alpha = 0f;
        activeBackground.DOFade(0f, 0f);
        iconTransform.localPosition = _originalIconPosition;
        if (isLocked) SetLocked(true);
    }

    private void OnButtonClick()
    {
        // Block click if button is locked
        if (isLocked) return;

        // Send information to the manager about clicking this button
        if (_footerManager != null)
        {
            _footerManager.SetActiveButton(this);
        }
        else
        {
            // Fallback solution if manager not found
            if (_isActive)
                Deactivate();
            else
                Activate();
        }
    }

    private void OnDestroy()
    {
        if (_activateSequence != null) _activateSequence.Kill();
        if (_deactivateSequence != null) _deactivateSequence.Kill();
    }

    // Additional methods to check the status
    public bool IsActive()
    {
        return _isActive;
    }
}