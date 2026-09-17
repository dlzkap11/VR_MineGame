using UnityEngine;
using UnityEngine.InputSystem;

public class MenuSummoner : MonoBehaviour
{
    [SerializeField] private InputActionProperty toggleAction;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distance;
    [SerializeField] private float heightOffset;
    [SerializeField] private bool startHidden;

    private void Awake()
    {
        if(cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if(menuRoot != null)
        {
            menuRoot.SetActive(!startHidden);
        }
    }

    private void OnEnable()
    {
        if (toggleAction.action == null)
            return;

        toggleAction.action.Enable();
        toggleAction.action.performed += OnTogglePressed;
    }
    private void OnDisable()
    {
        if (toggleAction.action == null)
            return;

        toggleAction.action.performed -= OnTogglePressed;
        toggleAction.action.Disable();
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        // 메뉴 키고 끄는거
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (menuRoot == null)
            return;

        PlaceInFrontOfUser();
        menuRoot.SetActive(!menuRoot.activeSelf);
    }

    private void PlaceInFrontOfUser()
    {
        if (cameraTransform == null)
            return;

        Vector3 forward = cameraTransform.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        menuRoot.transform.position = cameraTransform.position + forward * distance + Vector3.up * heightOffset;

        menuRoot.transform.rotation = Quaternion.LookRotation(forward);
    }

}
