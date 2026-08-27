using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class ScrollParts
    {
        public ScrollRect scroll;
        public RectTransform viewport;
        public RectTransform content;
    }

    public static class UiFactory
    {
        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Image Panel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(string name, Transform parent, string value, int size, Color color, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = PrinceTitanTheme.Font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(string name, Transform parent, string caption, Color background, Color foreground, UnityAction action,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize = 13)
        {
            var image = Panel(name, parent, background, anchorMin, anchorMax, offsetMin, offsetMax);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(.78f, .78f, .78f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = .08f;
            button.colors = colors;
            if (action != null) button.onClick.AddListener(action);
            Label("Label", image.transform, caption, fontSize, foreground, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(6f, 3f), new Vector2(-6f, -3f), FontStyle.Bold);
            return button;
        }

        public static InputField Input(string name, Transform parent, string value, string placeholder, int fontSize, Color background, Color foreground,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool multiline)
        {
            var image = Panel(name, parent, background, anchorMin, anchorMax, offsetMin, offsetMax);
            var field = image.gameObject.AddComponent<InputField>();
            field.targetGraphic = image;
            field.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            field.textComponent = Label("Text", image.transform, value, fontSize, foreground,
                multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, multiline ? new Vector2(18f, 14f) : new Vector2(14f, 4f),
                multiline ? new Vector2(-18f, -14f) : new Vector2(-14f, -4f));
            field.placeholder = Label("Placeholder", image.transform, placeholder, fontSize,
                PrinceTitanTheme.WithAlpha(foreground, .38f), multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, multiline ? new Vector2(18f, 14f) : new Vector2(14f, 4f),
                multiline ? new Vector2(-18f, -14f) : new Vector2(-14f, -4f), FontStyle.Italic);
            field.text = value;
            field.selectionColor = PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, .46f);
            field.caretColor = PrinceTitanTheme.Magenta;
            field.customCaretColor = true;
            if (multiline) image.gameObject.AddComponent<RectMask2D>();
            return field;
        }

        public static ScrollParts Scroll(string name, Transform parent, Color background, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rootImage = Panel(name, parent, background, anchorMin, anchorMax, offsetMin, offsetMax);
            var root = rootImage.rectTransform;
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 34f;

            var viewport = Rect("Viewport", root, Vector2.zero, Vector2.one, new Vector2(1f, 1f), new Vector2(-5f, -1f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 6f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return new ScrollParts { scroll = scroll, viewport = viewport, content = content };
        }

        public static LayoutElement Layout(RectTransform rect, float height, float minWidth = -1f, float flexibleWidth = -1f)
        {
            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            if (minWidth >= 0f) element.minWidth = minWidth;
            if (flexibleWidth >= 0f) element.flexibleWidth = flexibleWidth;
            return element;
        }

        public static Image Rule(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            return Panel(name, parent, color, anchorMin, anchorMax, offsetMin, offsetMax);
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(go);
        }
    }
}
