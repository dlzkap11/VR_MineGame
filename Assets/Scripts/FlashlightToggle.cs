using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [SerializeField] private Light spotLight;

    private bool isOn;

    private void Awake()
    {
        if(spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }

        spotLight.enabled = false;
        isOn = false;
    }

    public void Toggle()
    {
        if (spotLight == null)
            return;


        Debug.Log("토글 완료");
        isOn = !isOn;
        spotLight.enabled = isOn;
    }

    public void TurnOn()
    {
        if (spotLight == null)
            return;

        spotLight.enabled = true;
    }

    public void TurnOff()
    {
        if (spotLight == null)
            return;

        spotLight.enabled = false;
    }
}
