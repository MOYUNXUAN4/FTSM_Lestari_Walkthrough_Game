using System.Collections;
using UnityEngine;

public sealed class FTSMXRSimulatorBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject leftControllerVisual;
    [SerializeField] private GameObject rightControllerVisual;
    [SerializeField] private GameObject leftNearFarLineVisual;
    [SerializeField] private GameObject rightNearFarLineVisual;
    [SerializeField] private GameObject leftForcedLine;
    [SerializeField] private GameObject rightForcedLine;
    [SerializeField] private GameObject leftHitDot;
    [SerializeField] private GameObject rightHitDot;
    [SerializeField] private float controllerVisualScale = 300f;
    [SerializeField] private Vector3 leftControllerVisualLocalPosition = new Vector3(0f, 0f, -0.05f);
    [SerializeField] private Vector3 rightControllerVisualLocalPosition = new Vector3(0f, 0f, -0.05f);

    private Coroutine bootstrapCoroutine;

    private void OnEnable()
    {
        if (bootstrapCoroutine != null)
            StopCoroutine(bootstrapCoroutine);

        bootstrapCoroutine = StartCoroutine(Bootstrap());
    }

    private void OnDisable()
    {
        if (bootstrapCoroutine != null)
        {
            StopCoroutine(bootstrapCoroutine);
            bootstrapCoroutine = null;
        }
    }

    private IEnumerator Bootstrap()
    {
        CacheReferences();

        while (enabled)
        {
            ForceVisualState();
            yield return null;
        }

        bootstrapCoroutine = null;
    }

    private void CacheReferences()
    {
        if (leftControllerVisual == null)
            leftControllerVisual = transform.Find("Camera Offset/Left Controller/Left Controller Visual")?.gameObject;
        if (rightControllerVisual == null)
            rightControllerVisual = transform.Find("Camera Offset/Right Controller/Right Controller Visual")?.gameObject;
        if (leftNearFarLineVisual == null)
            leftNearFarLineVisual = transform.Find("Camera Offset/Left Controller/Near-Far Interactor/LineVisual")?.gameObject;
        if (rightNearFarLineVisual == null)
            rightNearFarLineVisual = transform.Find("Camera Offset/Right Controller/Near-Far Interactor/LineVisual")?.gameObject;
        if (leftForcedLine == null)
            leftForcedLine = transform.Find("FTSM Forced Left Teleport Ray")?.gameObject;
        if (rightForcedLine == null)
            rightForcedLine = transform.Find("FTSM Forced Right Interact Ray")?.gameObject;
        if (leftHitDot == null)
            leftHitDot = transform.Find("FTSM XR Left Hit Dot")?.gameObject;
        if (rightHitDot == null)
            rightHitDot = transform.Find("FTSM XR Right Hit Dot")?.gameObject;
    }

    private void ForceVisualState()
    {
        if (leftControllerVisual != null)
        {
            leftControllerVisual.SetActive(true);
            leftControllerVisual.transform.localPosition = leftControllerVisualLocalPosition;
            leftControllerVisual.transform.localScale = Vector3.one * controllerVisualScale;
        }

        if (rightControllerVisual != null)
        {
            rightControllerVisual.SetActive(true);
            rightControllerVisual.transform.localPosition = rightControllerVisualLocalPosition;
            rightControllerVisual.transform.localScale = new Vector3(-controllerVisualScale, controllerVisualScale, controllerVisualScale);
        }

        if (leftNearFarLineVisual != null)
            leftNearFarLineVisual.SetActive(false);
        if (rightNearFarLineVisual != null)
            rightNearFarLineVisual.SetActive(false);
        if (leftForcedLine != null)
            leftForcedLine.SetActive(true);
        if (rightForcedLine != null)
            rightForcedLine.SetActive(true);
        if (leftHitDot != null)
            leftHitDot.SetActive(false);
        if (rightHitDot != null)
            rightHitDot.SetActive(false);
    }
}
