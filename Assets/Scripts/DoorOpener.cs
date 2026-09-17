using System.Collections;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{
   [SerializeField] private float openHeight;
   [SerializeField] private float openDuration;

    private Vector3 closedPosition;
    private bool isOpened;
    private Coroutine openRoutine;

    private void Awake()
    {
        closedPosition = transform.position;
    }

    public void OpenDoor()
    {
        if (isOpened)
            return;

        if (openRoutine != null)
            return;

        openRoutine = StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector2 targetPosition = closedPosition + Vector3.up * openHeight;

        float elapsedTime = 0f;

        while(elapsedTime < openDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / openDuration);
            transform.position = Vector3.Lerp(closedPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;

        isOpened = true;

        openRoutine = null;
    }


}
