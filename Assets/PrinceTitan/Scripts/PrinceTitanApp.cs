using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PrinceTitan
{
    public enum PrinceScreen { Home, Map, Writing, People, Powers, Economy }

    public sealed partial class PrinceTitanApp : MonoBehaviour
    {
        private sealed class SiteWidget
        {
            public SiteData site;
            public CanvasGroup group;
            public RectTransform rect;
        }

        private sealed class MoverWidget
        {
            public MoverState mover;
            public RectTransform rect;
            public CanvasGroup group;
        }

        private sealed class PowerWidget
        {
            public Text value;
            public RectTransform fill;
        }

        private ProjectData project;
        private ChapterData activeChapter;
        private WorldSimulation simulation;
        private Canvas canvas;
        private CanvasScaler scaler;
        private RectTransform contentRoot;
        private RectTransform modalRoot;
        private GameObject settingsOverlay;
        private CanvasGroup transitionGroup;
        private PrinceTitanAmbient ambient;
        private Coroutine transitionRoutine;
        private PrinceScreen activeScreen;
        private readonly Dictionary<PrinceScreen, GameObject> screens = new Dictionary<PrinceScreen, GameObject>();
        private readonly Dictionary<PrinceScreen, Button> navigation = new Dictionary<PrinceScreen, Button>();
        private readonly Queue<string> eventLines = new Queue<string>();
        private readonly List<SiteWidget> siteWidgets = new List<SiteWidget>();
        private readonly List<MoverWidget> moverWidgets = new List<MoverWidget>();
        private readonly Dictionary<string, PowerWidget> powerWidgets = new Dictionary<string, PowerWidget>();

        private Text clockText;
        private Text saveStateText;
        private Text homeClockText;
        private Text homePulseText;
        private Text mapDetailText;
        private Text mapEventText;
        private Text mapScaleText;
        private Text writingIntelText;
        private Text wordCountText;
        private Text personDetailText;
        private Text economyDetailText;
        private Text economyPulseText;
        private Text regionTitleText;
        private Text regionDetailText;
        private RawImage regionImage;
        private InputField projectNameInput;
        private InputField chapterTitleInput;
        private InputField chapterBodyInput;
        private RectTransform chapterListRoot;
        private RectTransform mapContent;
        private RectTransform mapMarkerRoot;
        private RectTransform mapUnitRoot;
        private RectTransform peopleCardRoot;
        private RectTransform economyListRoot;
        private WorldOverlayGraphic worldOverlay;
        private LineageBoardGraphic lineageGraphic;
        private MapPanZoom mapPanZoom;
        private SiteData selectedSite;
        private PersonData selectedPerson;
        private SiteData selectedEconomySite;
        private SiteKind? economyFilter;
        private string mapFactionFilter;
        private bool loadingChapter;
        private bool dirty;
        private float dirtyAge;
        private float refreshClock;
        private float inputWatchdog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<PrinceTitanApp>() != null) return;
            var go = new GameObject("Prince Titan Runtime");
            DontDestroyOnLoad(go);
            go.AddComponent<PrinceTitanApp>();
        }

        private void Awake()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ambient = gameObject.AddComponent<AudioSource>().gameObject.AddComponent<PrinceTitanAmbient>();
        }

        private void Start()
        {
            WorldSeed.ValidateOrThrow();
            project = ProjectStore.LoadOrCreate();
            activeChapter = project.chapters.FirstOrDefault(c => c.id == project.activeChapterId) ?? project.chapters[0];
            simulation = new WorldSimulation(project.world);
            simulation.EventRaised += OnWorldEvent;
            UiFactory.Interaction += OnInteraction;
            BuildInterface();
            ActivateScreen(PrinceScreen.Home);
            LoadActiveChapter();
            selectedSite = project.sites[0];
            selectedPerson = project.people[0];
            selectedEconomySite = project.sites.FirstOrDefault(s => s.kind == SiteKind.Market) ?? project.sites[0];
            SelectSite(selectedSite);
            SelectPerson(selectedPerson);
            SelectEconomySite(selectedEconomySite);
            OnWorldEvent(new WorldEvent("Mapa Vivo iniciado", "Aeronaves, Titãs, mercados, companhias e casas estão em movimento.", "aurelia"));
            RefreshDynamic();
        }

        private void Update()
        {
            if (simulation != null) simulation.Tick(Time.unscaledDeltaTime);
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock >= .16f)
            {
                refreshClock = 0f;
                RefreshDynamic();
            }

            if (dirty)
            {
                dirtyAge += Time.unscaledDeltaTime;
                if (dirtyAge >= 2.2f) SaveProject(false);
            }

            HandleKeyboard();
            inputWatchdog += Time.unscaledDeltaTime;
            if (inputWatchdog >= 1f)
            {
                inputWatchdog = 0f;
                if (EventSystem.current == null || EventSystem.current.currentInputModule == null)
                    UiFactory.EnsureEventSystem();
            }
        }

        private void HandleKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            var control = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            if (control && keyboard.sKey.wasPressedThisFrame) SaveProject(true);
            if (control && keyboard.eKey.wasPressedThisFrame) ExportActiveChapter();
            if (control && keyboard.nKey.wasPressedThisFrame) NewChapter();
            if (keyboard.escapeKey.wasPressedThisFrame) HandleEscape();
#else
            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (control && Input.GetKeyDown(KeyCode.S)) SaveProject(true);
            if (control && Input.GetKeyDown(KeyCode.E)) ExportActiveChapter();
            if (control && Input.GetKeyDown(KeyCode.N)) NewChapter();
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscape();
#endif
        }

        private void HandleEscape()
        {
            if (siteObservationRoot != null && siteObservationRoot.activeSelf) { CloseSiteObservation(); return; }
            if (modalRoot != null && modalRoot.gameObject.activeSelf) { CloseModal(); return; }
            if (settingsOverlay != null && settingsOverlay.activeSelf) { settingsOverlay.SetActive(false); return; }
            if (regionImage != null && regionImage.transform.parent.gameObject.activeSelf) { CloseRegion(); return; }
            if (activeScreen != PrinceScreen.Home) ShowScreen(PrinceScreen.Home);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && project != null) SaveProject(false);
            if (focused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UiFactory.EnsureEventSystem();
            }
        }

        private void OnApplicationQuit()
        {
            if (project != null) SaveProject(false);
        }

        private void OnDestroy()
        {
            UiFactory.Interaction -= OnInteraction;
            if (simulation != null) simulation.EventRaised -= OnWorldEvent;
        }

        private void BuildInterface()
        {
            UiFactory.EnsureEventSystem();
            var canvasObject = new GameObject("Prince Titan Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 100;
            scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            ApplyUiScale(PlayerPrefs.GetFloat("PrinceTitan.UiScale", 1.15f), false);

            contentRoot = UiFactory.Rect("Room Layer", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -88f));
            BuildHomeScreen();
            BuildMapScreen();
            BuildWritingScreen();
            BuildPeopleScreen();
            BuildPowersScreen();
            BuildEconomyScreen();
            BuildTopBar();
            BuildSettingsOverlay();

            modalRoot = UiFactory.Rect("Dialog Layer", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            modalRoot.gameObject.SetActive(false);

            var transition = UiFactory.Panel("Room Transition", canvas.transform, PrinceTitanTheme.Black,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            transitionGroup = transition.gameObject.AddComponent<CanvasGroup>();
            transitionGroup.alpha = 0f;
            transitionGroup.blocksRaycasts = false;
            transitionGroup.interactable = false;
        }

        private void BuildTopBar()
        {
            var bar = UiFactory.Panel("Command Bar", canvas.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .97f),
                new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -88f), Vector2.zero);
            UiFactory.Shadow(bar, new Color(0f, 0f, 0f, .85f), 5f);
            UiFactory.Rule("Signal Line", bar.transform, PrinceTitanTheme.Magenta,
                new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 3f));

            var emblem = UiFactory.Rect("Crown Mark", bar.transform, new Vector2(0f, 0f), new Vector2(.052f, 1f),
                new Vector2(14f, 9f), new Vector2(-2f, -9f));
            emblem.gameObject.AddComponent<PrinceEmblemGraphic>();
            UiFactory.Label("Brand", bar.transform, "PRINCE TITAN", 26, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.052f, .34f), new Vector2(.205f, .94f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Brand Line", bar.transform, "ENGINE DE MUNDO E ESCRITA", 15, PrinceTitanTheme.Brass, TextAnchor.UpperLeft,
                new Vector2(.052f, .06f), new Vector2(.205f, .39f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            var nav = UiFactory.HorizontalGroup("Navigation", bar.transform, new Vector2(.215f, .13f), new Vector2(.84f, .87f),
                Vector2.zero, Vector2.zero, 7f);
            AddNavigation(nav, PrinceScreen.Home, "INÍCIO");
            AddNavigation(nav, PrinceScreen.Map, "MAPA VIVO");
            AddNavigation(nav, PrinceScreen.Writing, "ESCRITA");
            AddNavigation(nav, PrinceScreen.People, "PESSOAS");
            AddNavigation(nav, PrinceScreen.Powers, "PODERES");
            AddNavigation(nav, PrinceScreen.Economy, "ECONOMIA");

            clockText = UiFactory.Label("World Clock", bar.transform, "DIA 000 · 00:00", 17, PrinceTitanTheme.Success,
                TextAnchor.MiddleRight, new Vector2(.845f, .48f), new Vector2(.955f, .91f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            saveStateText = UiFactory.Label("Save State", bar.transform, "SALVO", 15, PrinceTitanTheme.Muted,
                TextAnchor.MiddleRight, new Vector2(.845f, .08f), new Vector2(.955f, .48f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Settings", bar.transform, "⚙", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, OpenSettings,
                new Vector2(.958f, .17f), new Vector2(.993f, .83f), Vector2.zero, Vector2.zero, 25);
        }

        private void AddNavigation(RectTransform parent, PrinceScreen screen, string caption)
        {
            var button = UiFactory.Button(screen + " Navigation", parent, caption, PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, () => ShowScreen(screen), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 17);
            UiFactory.Layout(button.image.rectTransform, 58f, 90f, 1f);
            navigation[screen] = button;
        }

        private RectTransform NewScreen(PrinceScreen id, string resourcePath, Color tint)
        {
            var root = UiFactory.Rect(id + " Room", contentRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.SetActive(false);
            screens[id] = root.gameObject;
            if (!string.IsNullOrEmpty(resourcePath))
                UiFactory.Texture(id + " Cinematic Plate", root, resourcePath, tint, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return root;
        }

        private Image Glass(string name, Transform parent, Vector2 min, Vector2 max, Color accent, float alpha = .88f)
        {
            var image = UiFactory.Panel(name, parent, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, alpha),
                min, max, Vector2.zero, Vector2.zero);
            UiFactory.Outline(image, PrinceTitanTheme.WithAlpha(accent, .72f), 1.5f);
            UiFactory.Shadow(image, new Color(0f, 0f, 0f, .68f), 4f);
            return image;
        }

        private RectTransform AddMeter(Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var track = UiFactory.Panel("Meter Track", parent, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black, .78f),
                min, max, Vector2.zero, Vector2.zero);
            UiFactory.Outline(track, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ivory, .18f), 1f);
            var fill = UiFactory.Panel("Meter Fill", track.transform, color, Vector2.zero, Vector2.one,
                new Vector2(3f, 3f), new Vector2(-3f, -3f));
            fill.rectTransform.anchorMax = new Vector2(.5f, 1f);
            return fill.rectTransform;
        }

        private static void SetMeter(RectTransform fill, float percent)
        {
            if (fill == null) return;
            fill.anchorMax = new Vector2(Mathf.Clamp01(percent / 100f), 1f);
        }

        private void ShowScreen(PrinceScreen next)
        {
            if (activeScreen == next && screens.ContainsKey(next) && screens[next].activeSelf) return;
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransitionTo(next));
        }

        private IEnumerator TransitionTo(PrinceScreen next)
        {
            transitionGroup.gameObject.SetActive(true);
            transitionGroup.blocksRaycasts = true;
            for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime * 7f)
            {
                transitionGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            transitionGroup.alpha = 1f;
            ActivateScreen(next);
            for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime * 4f)
            {
                transitionGroup.alpha = Mathf.SmoothStep(1f, 0f, t);
                yield return null;
            }
            transitionGroup.alpha = 0f;
            transitionGroup.blocksRaycasts = false;
            transitionGroup.gameObject.SetActive(false);
            transitionRoutine = null;
        }

        private void ActivateScreen(PrinceScreen next)
        {
            activeScreen = next;
            foreach (var pair in screens) pair.Value.SetActive(pair.Key == next);
            foreach (var pair in navigation)
            {
                var colors = pair.Value.colors;
                colors.normalColor = pair.Key == next ? PrinceTitanTheme.Magenta : PrinceTitanTheme.InkRaised;
                colors.selectedColor = colors.normalColor;
                pair.Value.colors = colors;
            }
            if (next == PrinceScreen.Writing) LoadActiveChapter();
            if (next == PrinceScreen.Map) UpdateMapWidgets();
            if (next == PrinceScreen.People) RebuildPeopleCards();
            if (next == PrinceScreen.Economy) RebuildEconomyList();
        }

        private void RefreshDynamic()
        {
            if (simulation == null) return;
            var time = simulation.ClockText();
            if (clockText != null) clockText.text = time;
            if (homeClockText != null) homeClockText.text = time;
            if (homePulseText != null)
            {
                var top = project.world.factions.OrderByDescending(f => f.influence).FirstOrDefault();
                var leader = top == null ? project.factions[0] : WorldSeed.Faction(project, top.factionId);
                homePulseText.text = "PULSO DO MUNDO\n" + project.sites.Count + " LUGARES  ·  " + project.people.Count + " PESSOAS\nMAIOR INFLUÊNCIA: " + leader.shortName;
            }
            if (worldOverlay != null && worldOverlay.gameObject.activeInHierarchy) worldOverlay.SetVerticesDirty();
            if (lineageGraphic != null && lineageGraphic.gameObject.activeInHierarchy) lineageGraphic.SetVerticesDirty();
            RefreshPowerWidgets();
            UpdateMapWidgets();
            UpdateEconomyPulse();
            UpdateWritingIntelligence();
        }

        private void RefreshPowerWidgets()
        {
            foreach (var state in project.world.factions)
            {
                PowerWidget widget;
                if (!powerWidgets.TryGetValue(state.factionId, out widget)) continue;
                if (widget.value != null) widget.value.text = state.influence.ToString("0") + "% DE INFLUÊNCIA";
                SetMeter(widget.fill, state.influence);
            }
        }

        private void OnWorldEvent(WorldEvent item)
        {
            var faction = WorldSeed.Faction(project, item.factionId);
            eventLines.Enqueue("◆ " + item.title.ToUpperInvariant() + "\n" + item.detail);
            while (eventLines.Count > 4) eventLines.Dequeue();
            var feed = string.Join("\n\n", eventLines.ToArray());
            if (mapEventText != null) mapEventText.text = feed;
            if (writingIntelText != null) writingIntelText.text = feed;
            UiFactory.Report(faction.shortName + " · " + item.title);
            MarkDirty();
        }

        private void OnInteraction(string message)
        {
            if (saveStateText != null && !dirty) saveStateText.text = message;
        }

        private void MarkDirty()
        {
            dirty = true;
            dirtyAge = 0f;
            if (saveStateText != null)
            {
                saveStateText.text = "ALTERAÇÕES NÃO SALVAS";
                saveStateText.color = PrinceTitanTheme.Brass;
            }
        }

        private void SaveProject(bool explicitSave)
        {
            if (project == null) return;
            if (activeChapter != null)
            {
                activeChapter.updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (chapterTitleInput != null) activeChapter.title = chapterTitleInput.text;
                if (chapterBodyInput != null) activeChapter.body = chapterBodyInput.text;
            }
            ProjectStore.Save(project);
            dirty = false;
            dirtyAge = 0f;
            if (saveStateText != null)
            {
                saveStateText.text = explicitSave ? "PROJETO SALVO AGORA" : "AUTOSSALVO";
                saveStateText.color = PrinceTitanTheme.Success;
            }
        }

        private string FactionLabel(PowerKind kind)
        {
            switch (kind)
            {
                case PowerKind.Empire: return "IMPÉRIO";
                case PowerKind.Government: return "GOVERNO";
                case PowerKind.Clan: return "CLÃ";
                case PowerKind.Contractor: return "EMPREITEIRA";
                default: return kind.ToString().ToUpperInvariant();
            }
        }

        private string SiteLabel(SiteKind kind)
        {
            switch (kind)
            {
                case SiteKind.City: return "CIDADE";
                case SiteKind.Market: return "MERCADO";
                case SiteKind.Company: return "COMPANHIA";
                case SiteKind.Estate: return "CASA / PROPRIEDADE";
                case SiteKind.Airfield: return "AERÓDROMO";
                case SiteKind.RobotWorks: return "FÁBRICA DE ROBÔS";
                case SiteKind.Port: return "PORTO";
                case SiteKind.Relay: return "RELÉ";
                default: return kind.ToString().ToUpperInvariant();
            }
        }
    }
}
