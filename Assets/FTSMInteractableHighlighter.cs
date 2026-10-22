using System.Collections.Generic;
using UnityEngine;

public sealed class FTSMInteractableHighlighter : MonoBehaviour
{
    private static readonly Color OutlineColor = new Color(1f, 0.45f, 0f, 1f);
    private static readonly Vector3 OutlineScale = new Vector3(1.015f, 1.015f, 1.015f);

    private static Material sharedOutlineMaterial;

    [SerializeField] private MeshFilter[] meshFilters;

    private readonly List<GameObject> outlineObjects = new();
    private bool isHighlighted;

    public static void SetHighlighted(Transform targetRoot, bool highlighted)
    {
        if (targetRoot == null)
            return;

        FTSMInteractableHighlighter highlighter = targetRoot.GetComponent<FTSMInteractableHighlighter>();
        if (highlighter == null)
            highlighter = targetRoot.gameObject.AddComponent<FTSMInteractableHighlighter>();

        highlighter.SetHighlighted(highlighted);
    }

    private void Awake()
    {
        CacheMeshFilters();
    }

    private void OnDisable()
    {
        SetHighlighted(false);
    }

    private void OnDestroy()
    {
        DestroyOutlineObjects();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isHighlighted == highlighted)
            return;

        isHighlighted = highlighted;

        if (highlighted)
        {
            EnsureOutlineObjects();
            SetOutlineObjectsActive(true);
            return;
        }

        SetOutlineObjectsActive(false);
    }

    private void CacheMeshFilters()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>(true);
    }

    private void EnsureOutlineObjects()
    {
        if (outlineObjects.Count > 0)
            return;

        if (meshFilters == null || meshFilters.Length == 0)
            CacheMeshFilters();

        Material outlineMaterial = GetSharedOutlineMaterial();
        if (outlineMaterial == null)
            return;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter sourceFilter = meshFilters[i];
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
                continue;

            MeshRenderer sourceRenderer = sourceFilter.GetComponent<MeshRenderer>();
            if (sourceRenderer == null)
                continue;

            GameObject outlineObject = new GameObject(sourceFilter.name + "_Outline");
            outlineObject.hideFlags = HideFlags.DontSave;
            outlineObject.layer = sourceFilter.gameObject.layer;

            Transform outlineTransform = outlineObject.transform;
            outlineTransform.SetParent(sourceFilter.transform, false);
            outlineTransform.localPosition = Vector3.zero;
            outlineTransform.localRotation = Quaternion.identity;
            outlineTransform.localScale = OutlineScale;

            MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
            outlineFilter.sharedMesh = sourceFilter.sharedMesh;

            MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
            outlineRenderer.sharedMaterial = outlineMaterial;
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            outlineRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            outlineRenderer.allowOcclusionWhenDynamic = false;

            outlineObjects.Add(outlineObject);
        }
    }

    private void SetOutlineObjectsActive(bool active)
    {
        for (int i = outlineObjects.Count - 1; i >= 0; i--)
        {
            GameObject outlineObject = outlineObjects[i];
            if (outlineObject == null)
            {
                outlineObjects.RemoveAt(i);
                continue;
            }

            outlineObject.SetActive(active);
        }
    }

    private void DestroyOutlineObjects()
    {
        for (int i = 0; i < outlineObjects.Count; i++)
        {
            if (outlineObjects[i] != null)
                Destroy(outlineObjects[i]);
        }

        outlineObjects.Clear();
    }

    private static Material GetSharedOutlineMaterial()
    {
        if (sharedOutlineMaterial != null)
            return sharedOutlineMaterial;

        Shader shader = Shader.Find("FTSM/Outline Highlight");
        if (shader == null)
            return null;

        sharedOutlineMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave,
            color = OutlineColor
        };
        sharedOutlineMaterial.SetColor("_OutlineColor", OutlineColor);
        return sharedOutlineMaterial;
    }
}
