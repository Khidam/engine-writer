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

        public static Image Panel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static RawImage Texture(string name, Transform parent, string resourcePath, Color tint,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<RawImage>();
            image.texture = Resources.Load<Texture2D>(resourcePath);
            image.color = image.texture == null ? PrinceTitanTheme.Ink : tint;
            image.raycastTarget = false;
            rect.gameObject.AddComponent<CoverRawImage>();
            return image;
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

        public static Text Label(string name, Transform parent, string value, int size, Color color, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = PrinceTitanTheme.Font;
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.fontStyle = style;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        public static Button Button(string name, Transform parent, string caption, Color background, Color foreground, UnityAction action,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize = 13)
        {
            var image = Panel(name, parent, Color.white, anchorMin, anchorMax, offsetMin, offsetMax);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            var highlighted = Color.Lerp(background, PrinceTitanTheme.Magenta, background == PrinceTitanTheme.Magenta ? .12f : .58f);
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = highlighted;
            colors.pressedColor = PrinceTitanTheme.Brass;
            colors.selectedColor = highlighted;
            colors.disabledColor = PrinceTitanTheme.WithAlpha(background, .32f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = .055f;
            button.colors = colors;

            Shadow(image, new Color(0f, 0f, 0f, .44f), 2f);
            Outline(image, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ivory, .18f), 1f);
            image.gameObject.AddComponent<PressFeedback>();
            if (action != null) button.onClick.AddListener(action);
            button.onClick.AddListener(() => Report(caption));
            Label("Label", image.transform, caption, fontSize, foreground, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(6f, 3f), new Vector2(-6f, -3f), FontStyle.Bold);
            return button;
        }

        public static InputField Input(string name, Transform parent, string value, string placeholder, int fontSize, Color background, Color foreground,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool multiline)
        {
            var image = Panel(name, parent, Color.white, anchorMin, anchorMax, offsetMin, offsetMax);
            image.raycastTarget = true;
            var field = image.gameObject.AddComponent<InputField>();
            field.targetGraphic = image;
            field.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            field.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            field.textComponent = Label("Text", image.transform, value, fontSize, foreground,
                multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, multiline ? new Vector2(20f, 16f) : new Vector2(14f, 4f),
                multiline ? new Vector2(-20f, -16f) : new Vector2(-14f, -4f));
            field.textComponent.supportRichText = false;
            field.placeholder = Label("Placeholder", image.transform, placeholder, fontSize,
                PrinceTitanTheme.WithAlpha(foreground, .38f), multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, multiline ? new Vector2(20f, 16f) : new Vector2(14f, 4f),
                multiline ? new Vector2(-20f, -16f) : new Vector2(-14f, -4f), FontStyle.Italic);
            field.text = value;
            field.selectionColor = PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, .46f);
            field.caretColor = PrinceTitanTheme.Magenta;
            field.customCaretColor = true;
            field.caretWidth = 2;

            var colors = field.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, PrinceTitanTheme.Ivory, .12f);
            colors.selectedColor = Color.Lerp(background, PrinceTitanTheme.Ivory, .20f);
            colors.pressedColor = colors.selectedColor;
            colors.fadeDuration = .06f;
            field.colors = colors;
            Outline(image, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, multiline ? .32f : .45f), 1f);
            if (multiline) image.gameObject.AddComponent<RectMask2D>();
            var status = image.gameObject.AddComponent<InputStatusRelay>();
            status.message = multiline ? "EDITOR READY — TYPE YOUR SCENE" : name.ToUpperInvariant() + " SELECTED";
            return field;
        }

        public static ScrollParts Scroll(string name, Transform parent, Color background, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rootImage = Panel(name, parent, background, anchorMin, anchorMax, offsetMin, offsetMax);
            rootImage.raycastTarget = true;
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
            input.deselectOnBackgroundClick = false;
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

    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = new Vector3(.975f, .975f, 1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }
    }

    public sealed class InputStatusRelay : MonoBehaviour, ISelectHandler
    {
        public string message;

        public void OnSelect(BaseEventData eventData)
        {
            UiFactory.Report(message);
        }
    }
}
