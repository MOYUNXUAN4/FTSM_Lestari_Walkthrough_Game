using UnityEngine;
using UnityEngine.UI;

public sealed class FTSMInteractionPromptUI : MonoBehaviour
{
    private const string PromptRootName = "Interaction Prompt";

    [SerializeField] private Image background;
    [SerializeField] private Text label;

    public static FTSMInteractionPromptUI GetOrCreate(Canvas canvas)
    {
        if (canvas == null)
            return null;

        Transform existing = canvas.transform.Find(PromptRootName);
        if (existing != null)
        {
            FTSMInteractionPromptUI existingPrompt = existing.GetComponent<FTSMInteractionPromptUI>();
            if (existingPrompt != null)
                return existingPrompt;
        }

        GameObject root = new GameObject(PromptRootName, typeof(RectTransform), typeof(Image), typeof(FTSMInteractionPromptUI));
        root.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, -48f);
        rootRect.sizeDelta = new Vector2(150f, 34f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.45f);
        background.raycastTarget = false;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(root.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 5f);
        textRect.offsetMax = new Vector2(-10f, -5f);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 1f, 1f, 0.9f);
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        FTSMInteractionPromptUI prompt = root.GetComponent<FTSMInteractionPromptUI>();
        prompt.background = background;
        prompt.label = text;
        prompt.Hide();
        return prompt;
    }

    public void Show(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            Hide();
            return;
        }

        if (label != null)
            label.text = displayName;

        if (background != null)
            background.enabled = true;
        if (label != null)
            label.enabled = true;
    }

    public void Hide()
    {
        if (background != null)
            background.enabled = false;
        if (label != null)
        {
            label.text = string.Empty;
            label.enabled = false;
        }
    }
}
