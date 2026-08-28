using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

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
        public static event Action<string> Interaction;

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

        public static Image Panel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool raycast = false)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static RawImage Texture(string name, Transform parent, string resourcePath, Color tint,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool cover = true)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<RawImage>();
            image.texture = Resources.Load<Texture2D>(resourcePath);
            image.color = image.texture == null ? PrinceTitanTheme.Ink : tint;
            image.raycastTarget = false;
            if (cover) rect.gameObject.AddComponent<CoverRawImage>();
            return image;
        }

        public static Text Label(string name, Transform parent, string value, int size, Color color, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = PrinceTitanTheme.Font;
            label.text = value;
            label.fontSize = Mathf.Max(15, size);
            label.color = color;
            label.alignment = alignment;
            label.fontStyle = style;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        public static Button Button(string name, Transform parent, string caption, Color background, Color foreground, UnityAction action,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize = 18)
        {
            var image = Panel(name, parent, Color.white, anchorMin, anchorMax, offsetMin, offsetMax, true);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            var highlighted = Color.Lerp(background, PrinceTitanTheme.Magenta, background == PrinceTitanTheme.Magenta ? .10f : .50f);
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = highlighted;
            colors.pressedColor = PrinceTitanTheme.Brass;
            colors.selectedColor = highlighted;
            colors.disabledColor = PrinceTitanTheme.WithAlpha(background, .30f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = .08f;
            button.colors = colors;

            Shadow(image, new Color(0f, 0f, 0f, .60f), 3f);
            Outline(image, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ivory, .24f), 1f);
            image.gameObject.AddComponent<ButtonMotion>();
            if (action != null) button.onClick.AddListener(action);
            button.onClick.AddListener(() => Report(caption));
            var label = Label("Caption", image.transform, caption, Mathf.Max(17, fontSize), foreground, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(10f, 5f), new Vector2(-10f, -5f), FontStyle.Bold);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 15;
            label.resizeTextMaxSize = Mathf.Max(17, fontSize);
            return button;
        }

        public static InputField Input(string name, Transform parent, string value, string placeholder, int fontSize,
            Color background, Color foreground, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            bool multiline, bool transparent = false)
        {
            var image = Panel(name, parent, transparent ? Color.clear : Color.white, anchorMin, anchorMax, offsetMin, offsetMax, true);
            var field = image.gameObject.AddComponent<InputField>();
            field.targetGraphic = image;
            field.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            field.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            var pad = multiline ? new Vector2(20f, 16f) : new Vector2(16f, 6f);
            field.textComponent = Label("Text", image.transform, value, Mathf.Max(18, fontSize), foreground,
                multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                pad, -pad);
            field.textComponent.supportRichText = false;
            field.placeholder = Label("Placeholder", image.transform, placeholder, Mathf.Max(18, fontSize),
                PrinceTitanTheme.WithAlpha(foreground, .44f), multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, pad, -pad, FontStyle.Italic);
            field.text = value;
            field.selectionColor = PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, .46f);
            field.caretColor = PrinceTitanTheme.Magenta;
            field.customCaretColor = true;
            field.caretWidth = 3;

            var colors = field.colors;
            colors.normalColor = transparent ? Color.clear : background;
            colors.highlightedColor = transparent ? PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperLight, .08f) : Color.Lerp(background, PrinceTitanTheme.Ivory, .10f);
            colors.selectedColor = transparent ? PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperLight, .12f) : Color.Lerp(background, PrinceTitanTheme.Ivory, .18f);
            colors.pressedColor = colors.selectedColor;
            colors.fadeDuration = .08f;
            field.colors = colors;
            if (!transparent) Outline(image, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, multiline ? .32f : .48f), 1f);
            if (multiline) image.gameObject.AddComponent<RectMask2D>();
            return field;
        }

        public static ScrollParts Scroll(string name, Transform parent, Color background, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rootImage = Panel(name, parent, background, anchorMin, anchorMax, offsetMin, offsetMax, true);
            var root = rootImage.rectTransform;
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.inertia = true;
            scroll.decelerationRate = .12f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 38f;

            var viewport = Rect("Viewport", root, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-8f, -2f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
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

        public static RectTransform HorizontalGroup(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, float spacing, int left = 0, int right = 0)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var group = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.padding = new RectOffset(left, right, 0, 0);
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = true;
            group.childForceExpandWidth = true;
            return rect;
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

        public static void Outline(Graphic graphic, Color color, float distance = 1f)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        public static void Shadow(Graphic graphic, Color color, float distance = 3f)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = new Vector2(distance, -distance);
            shadow.useGraphicAlpha = true;
        }

        public static void SetButtonCaption(Button button, string caption)
        {
            if (button == null) return;
            var label = button.GetComponentInChildren<Text>();
            if (label != null) label.text = caption;
        }

        public static void ClearChildren(Transform transform)
        {
            if (transform == null) return;
            for (var i = transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }

        public static EventSystem EnsureEventSystem()
        {
            var eventSystem = EventSystem.current ?? UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject go;
            if (eventSystem == null)
            {
                go = new GameObject("Prince Titan Input", typeof(EventSystem));
                eventSystem = go.GetComponent<EventSystem>();
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            else
            {
                go = eventSystem.gameObject;
            }

            eventSystem.enabled = true;
            eventSystem.sendNavigationEvents = true;
            eventSystem.pixelDragThreshold = 7;
            EventSystem.current = eventSystem;

#if ENABLE_INPUT_SYSTEM
            foreach (var legacy in go.GetComponents<StandaloneInputModule>()) legacy.enabled = false;
            var input = go.GetComponent<InputSystemUIInputModule>();
            if (input == null) input = go.AddComponent<InputSystemUIInputModule>();
            if (input.actionsAsset == null) input.AssignDefaultActions();
            input.deselectOnBackgroundClick = true;
            input.enabled = true;
#else
            var input = go.GetComponent<StandaloneInputModule>();
            if (input == null) input = go.AddComponent<StandaloneInputModule>();
            input.forceModuleActive = true;
            input.inputActionsPerSecond = 18f;
            input.repeatDelay = .32f;
            input.enabled = true;
#endif
            return eventSystem;
        }

        public static void Report(string message)
        {
            var handler = Interaction;
            if (handler != null) handler(message);
        }
    }

    [RequireComponent(typeof(RawImage))]
    public sealed class CoverRawImage : MonoBehaviour
    {
        private RawImage image;

        private void OnEnable()
        {
            image = GetComponent<RawImage>();
            Crop();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (image == null) image = GetComponent<RawImage>();
            Crop();
        }

        private void Crop()
        {
            if (image == null || image.texture == null) return;
            var rect = ((RectTransform)transform).rect;
            if (rect.width <= 1f || rect.height <= 1f) return;
            var viewportAspect = rect.width / rect.height;
            var textureAspect = (float)image.texture.width / image.texture.height;
            if (textureAspect > viewportAspect)
            {
                var width = viewportAspect / textureAspect;
                image.uvRect = new Rect((1f - width) * .5f, 0f, width, 1f);
            }
            else
            {
                var height = textureAspect / viewportAspect;
                image.uvRect = new Rect(0f, (1f - height) * .5f, 1f, height);
            }
        }
    }

    public sealed class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector3 target = Vector3.one;

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * 18f);
        }

        public void OnPointerEnter(PointerEventData eventData) { target = new Vector3(1.025f, 1.025f, 1f); }
        public void OnPointerExit(PointerEventData eventData) { target = Vector3.one; }
        public void OnPointerDown(PointerEventData eventData) { target = new Vector3(.975f, .975f, 1f); }
        public void OnPointerUp(PointerEventData eventData) { target = new Vector3(1.025f, 1.025f, 1f); }
    }
}
