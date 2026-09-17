using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketCompleteChecker : MonoBehaviour
{
    [SerializeField] private string correctTag;
    [SerializeField] private UnityEvent onSuccess;
    [SerializeField] private UnityEvent onFail;

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnInserted);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnInserted);
    }

    private void OnInserted(SelectEnterEventArgs args)
    {

        Transform inserted = args.interactableObject.transform;
        if(inserted.CompareTag(correctTag))
        {
            Debug.Log($"{name} 정답 : {inserted.name}");
            onSuccess.Invoke();
        }
        else
        {
            Debug.Log($"{name} 오답 : {inserted.name}");
            onFail.Invoke();
        }
    }
}
