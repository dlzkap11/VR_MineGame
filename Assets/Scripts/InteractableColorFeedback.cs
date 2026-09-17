using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InteractableColorFeedback : MonoBehaviour
{
    [SerializeField]private Color hoverColor;
    [SerializeField]private Color selectColor;
    [SerializeField]private Renderer targetRen;

    private XRGrabInteractable grab;
    private Color originColor;
    private bool isReady;

    private void Awake()
    {
        if (targetRen == null)
            targetRen = GetComponent<Renderer>();

        grab = GetComponent<XRGrabInteractable>();

        originColor = targetRen.material.color;
        isReady = true;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(SetSelectColor);
        grab.hoverEntered.AddListener(SetHoverColor);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(SetSelectColor);
        grab.hoverEntered.RemoveListener(SetHoverColor);
    }

    public void SetHoverColor(HoverEnterEventArgs args)
    {
        if (!isReady) 
            return;

        targetRen.material.color = hoverColor;
    }

    public void SetSelectColor(SelectEnterEventArgs args)
    {
        if (!isReady)
            return;

        targetRen.material.color = selectColor;
    }

    public void ResetColor()
    {
        if (!isReady)
            return;

        targetRen.material.color = originColor;
    }
}
