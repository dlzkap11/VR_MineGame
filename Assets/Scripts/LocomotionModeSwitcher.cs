using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class LocomotionModeSwitcher : MonoBehaviour
{
    private enum Mode
    {
        Teleport,
        Continuous
    }

   [SerializeField] private LocomotionProvider teleportProvider;
   [SerializeField] private LocomotionProvider continuousProvider;
   [SerializeField] private TMP_Text modeText;
   [SerializeField] private Mode startMode;

    private Mode currentMode;
    private bool isInitialized;

    private void Start()
    {
        isInitialized = false;

        if(startMode == Mode.Teleport)
        {
            SwitchToTeleport();
        }
        else
        {
            SwitchToCotinuous();
        }
    }

    public void SwitchToCotinuous()
    {
        if (isInitialized && currentMode == Mode.Continuous)
            return;

        ApplyMode(Mode.Continuous);
    }

    public void SwitchToTeleport()
    {
        if (isInitialized && currentMode == Mode.Teleport)
            return;

        ApplyMode(Mode.Teleport);
    }

    private void ApplyMode(Mode mode)
    {
        bool isTeleport = (mode == Mode.Teleport);

        if(teleportProvider != null)
        {
            teleportProvider.enabled = isTeleport;
        }
        if (continuousProvider != null)
        {
            continuousProvider.enabled = !isTeleport;
        }

        if(modeText != null)
        {
            modeText.text = mode.ToString();
        }

        currentMode = mode;
        isInitialized = true;
    }
}
