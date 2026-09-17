using System.Collections.Generic;
using ShatterStone;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// 잡은 물체를 휘두르는 속도에 따라 히트 콜라이더를 켜고 끄고, 켜진 동안 광석 타격을 처리한다.
/// PickaxeTrigger 대신 사용한다. (콜라이더를 끄면 OnTriggerExit가 호출되지 않아 PickaxeTrigger의 hasEntered가 고착되기 때문)
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class SwingHitbox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider hitCollider;
    [SerializeField] private Transform tip;

    [Header("Swing Speed (m/s)")]
    [SerializeField] private float enableSpeed = 2.5f;
    [SerializeField] private float disableSpeed = 1.5f;
    [SerializeField] private float minActiveTime = 0.15f;

    [Header("Hit")]
    [SerializeField] private string oreTag = "OreNode";
    [SerializeField] private float hitCooldown = 0.3f;

    [Header("Haptics")]
    [SerializeField] private float hapticAmplitude = 0.6f;
    [SerializeField] private float hapticDuration = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLog;

    private XRGrabInteractable grabInteractable;
    private XRBaseInputInteractor holder;

    private Vector3 lastTipPosition;
    private float activatedTime;
    private readonly Dictionary<OreNode, float> lastHitTimes = new Dictionary<OreNode, float>();

    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (hitCollider == null)
        {
            Debug.LogError($"[SwingHitbox] {name}: hitCollider가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        hitCollider.isTrigger = true;
        SetHitboxActive(false);
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);

        holder = null;
        if (hitCollider != null)
            SetHitboxActive(false);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        holder = args.interactorObject as XRBaseInputInteractor;
        lastTipPosition = GetTipPosition();
        CurrentSpeed = 0f;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // 다른 손이 아직 잡고 있으면 유지
        if (grabInteractable.isSelected)
        {
            holder = grabInteractable.interactorsSelecting[0] as XRBaseInputInteractor;
            return;
        }

        holder = null;
        CurrentSpeed = 0f;
        SetHitboxActive(false);
    }

    private void FixedUpdate()
    {
        if (!grabInteractable.isSelected)
            return;

        // 손목 회전까지 반영되도록 날 끝 위치의 속도를 잰다.
        Vector3 tipPosition = GetTipPosition();
        CurrentSpeed = (tipPosition - lastTipPosition).magnitude / Time.fixedDeltaTime;
        lastTipPosition = tipPosition;

        if (!hitCollider.enabled)
        {
            if (CurrentSpeed >= enableSpeed)
            {
                SetHitboxActive(true);
                activatedTime = Time.time;

                if (debugLog)
                    Debug.Log($"[SwingHitbox] ON  speed={CurrentSpeed:F2}");
            }
        }
        else if (CurrentSpeed < disableSpeed && Time.time - activatedTime >= minActiveTime)
        {
            SetHitboxActive(false);

            if (debugLog)
                Debug.Log($"[SwingHitbox] OFF speed={CurrentSpeed:F2}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitCollider.enabled || !other.CompareTag(oreTag))
            return;

        OreNode ore = other.GetComponent<OreNode>();
        if (ore == null)
            return;

        if (lastHitTimes.TryGetValue(ore, out float lastHit) && Time.time - lastHit < hitCooldown)
            return;

        lastHitTimes[ore] = Time.time;
        ore.Interact();

        if (holder != null)
            holder.SendHapticImpulse(hapticAmplitude, hapticDuration);

        if (debugLog)
            Debug.Log($"[SwingHitbox] HIT {ore.name} speed={CurrentSpeed:F2}");
    }

    private Vector3 GetTipPosition()
    {
        return tip != null ? tip.position : hitCollider.bounds.center;
    }

    private void SetHitboxActive(bool active)
    {
        hitCollider.enabled = active;
    }
}
