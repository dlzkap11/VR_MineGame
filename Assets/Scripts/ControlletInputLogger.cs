using UnityEngine;
using UnityEngine.InputSystem;

public class ControlletInputLogger : MonoBehaviour
{
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty stickAction;
    [SerializeField] private InputActionProperty rightAction;
    [SerializeField] private float logInterval;

    private float nextLogTime;

    private void OnEnable()
    {
        triggerAction.action.Enable();
        gripAction.action.Enable();
        stickAction.action.Enable();
        rightAction.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
        gripAction.action.Disable();
        stickAction.action.Disable();
        rightAction.action.Disable();
    }

    private void Update()
    {
        if (Time.time < nextLogTime)
            return;

        nextLogTime = Time.time + logInterval;

        float trigger = triggerAction.action.ReadValue<float>();
        float grip = gripAction.action.ReadValue<float>();
        Vector2 stick = stickAction.action.ReadValue<Vector2>();

        Debug.Log($"Trigger : {trigger:F2} | Grip : {grip:F2} | Stick : {stick} | Pos {rightAction.action.ReadValue<Vector3>()}");
    }

}
