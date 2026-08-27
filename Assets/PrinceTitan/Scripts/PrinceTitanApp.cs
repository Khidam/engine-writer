using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class PrinceTitanApp : MonoBehaviour
    {
        private sealed class SiteOverlayRef
        {
            public SiteData site;
            public GameObject button;
            public GameObject label;
        }

        private ProjectData project;
        private ChapterData activeChapter;
        private WorldSimulation simulation;
        private Canvas canvas;
        private InputField projectNameInput;
        private InputField titleInput;
        private InputField bodyInput;
        private RectTransform chapterContent;
        private Text wordCountText;
        private Text saveStateText;
        private Text clockText;
        private Text eventFeedText;
        private Text siteDetailText;
        private Text expandedSiteDetailText;
        private Text pauseButtonText;
        private Text speedButtonText;
        private AtlasGraphic compactAtlas;
        private AtlasGraphic expandedAtlas;
        private GameObject atlasOverlay;
        private GameObject lineageOverlay;
        private bool loadingChapter;
        private bool dirty;
        private float dirtyAge;
        private float visualClock;
        private PowerKind? activeFilter;
        private readonly Queue<string> eventLines = new Queue<string>();
        private readonly List<SiteOverlayRef> siteOverlays = new List<SiteOverlayRef>();

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
            Screen.fullScreenMode = FullScreenMode.Windowed;
            if (Screen.width < 1200 || Screen.height < 720) Screen.SetResolution(1600, 900, false);
        }

        private void Start()
        {
            WorldSeed.ValidateOrThrow();
            project = ProjectStore.LoadOrCreate();
            simulation = new WorldSimulation(project.world);
            simulation.EventRaised += OnWorldEvent;
            BuildInterface();
            LoadActiveChapter();
            SelectSite(WorldSeed.Sites[0]);
            OnWorldEvent(new WorldEvent("Atlas awake", "Aircraft, machines, houses and markets are now moving in real time.", "aurelia"));
        }

        private void Update()
        {
            simulation.Tick(Time.unscaledDeltaTime);
            visualClock += Time.unscaledDeltaTime;
            if (visualClock >= .12f)
            {
                visualClock = 0f;
                if (compactAtlas != null) compactAtlas.SetVerticesDirty();
                if (expandedAtlas != null && expandedAtlas.gameObject.activeInHierarchy) expandedAtlas.SetVerticesDirty();
                if (clockText != null) clockText.text = simulation.ClockText();
            }

            if (dirty)
            {
                dirtyAge += Time.unscaledDeltaTime;
                if (dirtyAge >= 2.4f) SaveProject(false);
            }

            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (control && Input.GetKeyDown(KeyCode.S)) SaveProject(true);
            if (control && Input.GetKeyDown(KeyCode.E)) ExportActiveChapter();
            if (control && Input.GetKeyDown(KeyCode.N)) NewChapter();
            if (Input.GetKeyDown(KeyCode.Escape)) CloseOverlays();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && project != null) SaveProject(false);
        }

        private void OnApplicationQuit()
        {
            if (project != null) SaveProject(false);
        }

        private void BuildInterface()
        {
            UiFactory.EnsureEventSystem();
            var canvasObject = new GameObject("Prince Titan Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .55f;

            UiFactory.Panel("Backdrop", canvas.transform, PrinceTitanTheme.Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            BuildTopBar();
            BuildChapterRail();
            BuildWritingDesk();
            BuildCompactAtlas();
            BuildAtlasOverlay();
            BuildLineageOverlay();
        }

        private void BuildTopBar()
        {
            var bar = UiFactory.Panel("Top Bar", canvas.transform, PrinceTitanTheme.InkSoft,
                new Vector2(0f,.93f), Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Rule("Magenta Rule", bar.transform, PrinceTitanTheme.Magenta,
                new Vector2(0f,0f), new Vector2(1f,0f), Vector2.zero, new Vector2(0f,3f));

            var emblemRect = UiFactory.Rect("Emblem", bar.transform, new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(20f,8f), new Vector2(82f,-8f));
            emblemRect.gameObject.AddComponent<PrinceEmblemGraphic>();
            UiFactory.Label("Brand", bar.transform, "PRINCE TITAN", 24, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(0f,0f), new Vector2(.25f,1f), new Vector2(91f,0f), Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Subtitle", bar.transform, "WRITER  /  LIVING ATLAS", 10, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(0f,0f), new Vector2(.25f,.52f), new Vector2(94f,0f), Vector2.zero, FontStyle.Bold);

            UiFactory.Button("Write Tab", bar.transform, "WRITE", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory, CloseOverlays,
                new Vector2(.34f,.18f), new Vector2(.43f,.82f), Vector2.zero, Vector2.zero, 12);
            UiFactory.Button("Atlas Tab", bar.transform, "ATLAS", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, OpenAtlas,
                new Vector2(.435f,.18f), new Vector2(.525f,.82f), Vector2.zero, Vector2.zero, 12);
            UiFactory.Button("Lineage Tab", bar.transform, "LINEAGES", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, OpenLineages,
                new Vector2(.53f,.18f), new Vector2(.63f,.82f), Vector2.zero, Vector2.zero, 12);

            UiFactory.Label("Shortcuts", bar.transform, "CTRL+S  SAVE   •   CTRL+E  EXPORT   •   CTRL+N  NEW CHAPTER", 10,
                PrinceTitanTheme.Muted, TextAnchor.MiddleRight, new Vector2(.65f,0f), new Vector2(.98f,1f), Vector2.zero, Vector2.zero);
        }

        private void BuildChapterRail()
        {
            var rail = UiFactory.Panel("Chapter Rail", canvas.transform, PrinceTitanTheme.InkSoft,
                new Vector2(0f,0f), new Vector2(.16f,.93f), Vector2.zero, new Vector2(-2f,0f));
            UiFactory.Label("Project Label", rail.transform, "PROJECT", 10, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f,.905f), new Vector2(.93f,.95f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            projectNameInput = UiFactory.Input("Project Name", rail.transform, project.projectName, "Project name", 15,
                PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, new Vector2(.06f,.84f), new Vector2(.94f,.905f), Vector2.zero, Vector2.zero, false);
            projectNameInput.onValueChanged.AddListener(value => { project.projectName = value; MarkDirty(); });

            UiFactory.Label("Chapter Label", rail.transform, "MANUSCRIPT", 10, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f,.78f), new Vector2(.93f,.825f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var scroll = UiFactory.Scroll("Chapter Scroll", rail.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .28f),
                new Vector2(.04f,.20f), new Vector2(.96f,.78f), Vector2.zero, Vector2.zero);
            chapterContent = scroll.content;

            UiFactory.Button("New Chapter", rail.transform, "+  NEW CHAPTER", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory, NewChapter,
                new Vector2(.06f,.125f), new Vector2(.94f,.18f), Vector2.zero, Vector2.zero, 12);
            UiFactory.Button("Save", rail.transform, "SAVE PROJECT", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, () => SaveProject(true),
                new Vector2(.06f,.06f), new Vector2(.54f,.112f), Vector2.zero, Vector2.zero, 11);
            UiFactory.Button("Export", rail.transform, "EXPORT TXT", PrinceTitanTheme.Brass, PrinceTitanTheme.Ink, ExportActiveChapter,
                new Vector2(.56f,.06f), new Vector2(.94f,.112f), Vector2.zero, Vector2.zero, 11);
            saveStateText = UiFactory.Label("Save State", rail.transform, "LOCAL AUTOSAVE READY", 9, PrinceTitanTheme.Muted, TextAnchor.MiddleCenter,
                new Vector2(.05f,.008f), new Vector2(.95f,.05f), Vector2.zero, Vector2.zero);
            RebuildChapterList();
        }

        private void BuildWritingDesk()
        {
            var desk = UiFactory.Panel("Writing Desk", canvas.transform, PrinceTitanTheme.Ivory,
                new Vector2(.16f,0f), new Vector2(.685f,.93f), new Vector2(2f,0f), new Vector2(-2f,0f));
            UiFactory.Label("Desk Eyebrow", desk.transform, "MANUSCRIPT  /  ACTIVE CHAPTER", 10, PrinceTitanTheme.Magenta, TextAnchor.MiddleLeft,
                new Vector2(.05f,.925f), new Vector2(.70f,.975f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            wordCountText = UiFactory.Label("Word Count", desk.transform, "0 WORDS", 10, PrinceTitanTheme.PaperInk, TextAnchor.MiddleRight,
                new Vector2(.70f,.925f), new Vector2(.95f,.975f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            titleInput = UiFactory.Input("Chapter Title", desk.transform, string.Empty, "Chapter title", 23,
                PrinceTitanTheme.Ivory, PrinceTitanTheme.Ink, new Vector2(.045f,.85f), new Vector2(.955f,.925f), Vector2.zero, Vector2.zero, false);
            UiFactory.Rule("Title Rule", desk.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Magenta, .35f),
                new Vector2(.05f,.845f), new Vector2(.95f,.845f), Vector2.zero, new Vector2(0f,1f));

            bodyInput = UiFactory.Input("Chapter Body", desk.transform, string.Empty,
                "Write the scene. The atlas will keep breathing beside you.", 18, PrinceTitanTheme.Ivory, PrinceTitanTheme.Ink,
                new Vector2(.04f,.075f), new Vector2(.96f,.835f), Vector2.zero, Vector2.zero, true);
            bodyInput.textComponent.lineSpacing = 1.18f;
            bodyInput.characterLimit = 0;
            titleInput.onValueChanged.AddListener(OnTitleChanged);
            bodyInput.onValueChanged.AddListener(OnBodyChanged);

            UiFactory.Rule("Bottom Rule", desk.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .16f),
                new Vector2(.05f,.063f), new Vector2(.95f,.063f), Vector2.zero, new Vector2(0f,1f));
            UiFactory.Label("Desk Hint", desk.transform, "Everything saves locally. Your world simulation never rewrites the manuscript.", 10,
                PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk,.68f), TextAnchor.MiddleLeft,
                new Vector2(.05f,.015f), new Vector2(.75f,.06f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Focus Atlas", desk.transform, "EXPAND ATLAS", PrinceTitanTheme.MagentaDark, PrinceTitanTheme.Ivory, OpenAtlas,
                new Vector2(.78f,.016f), new Vector2(.95f,.058f), Vector2.zero, Vector2.zero, 10);
        }

        private void BuildCompactAtlas()
        {
            var panel = UiFactory.Panel("Atlas Side", canvas.transform, PrinceTitanTheme.InkSoft,
                new Vector2(.685f,0f), new Vector2(1f,.93f), new Vector2(2f,0f), Vector2.zero);
            UiFactory.Label("Atlas Title", panel.transform, "THE LIVING ATLAS", 17, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.05f,.93f), new Vector2(.57f,.99f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            clockText = UiFactory.Label("Clock", panel.transform, "DAY 128  •  08:30", 10, PrinceTitanTheme.Brass, TextAnchor.MiddleRight,
                new Vector2(.56f,.93f), new Vector2(.95f,.99f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            BuildSimulationControls(panel.transform);
            BuildFilterRow(panel.transform, .87f, .915f);

            var mapRect = UiFactory.Rect("Compact Map", panel.transform, new Vector2(.035f,.32f), new Vector2(.965f,.865f), Vector2.zero, Vector2.zero);
            compactAtlas = mapRect.gameObject.AddComponent<AtlasGraphic>();
            compactAtlas.Bind(project.world);
            CreateSiteOverlays(mapRect, false);

            UiFactory.Label("Intel Label", panel.transform, "CURRENT LOCATION", 9, PrinceTitanTheme.Magenta, TextAnchor.MiddleLeft,
                new Vector2(.05f,.265f), new Vector2(.95f,.31f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            siteDetailText = UiFactory.Label("Site Detail", panel.transform, string.Empty, 11, PrinceTitanTheme.Ivory, TextAnchor.UpperLeft,
                new Vector2(.05f,.15f), new Vector2(.95f,.27f), Vector2.zero, Vector2.zero);
            UiFactory.Rule("Feed Rule", panel.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Brass,.32f),
                new Vector2(.05f,.14f), new Vector2(.95f,.14f), Vector2.zero, new Vector2(0f,1f));
            eventFeedText = UiFactory.Label("Event Feed", panel.transform, string.Empty, 10, PrinceTitanTheme.Muted, TextAnchor.UpperLeft,
                new Vector2(.05f,.015f), new Vector2(.95f,.13f), Vector2.zero, Vector2.zero);
        }

        private void BuildSimulationControls(Transform parent)
        {
            var pause = UiFactory.Button("Pause", parent, project.world.paused ? "PLAY" : "PAUSE", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                TogglePause, new Vector2(.05f,.875f), new Vector2(.18f,.922f), Vector2.zero, Vector2.zero, 9);
            pauseButtonText = pause.GetComponentInChildren<Text>();
            var speed = UiFactory.Button("Speed", parent, "1×", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Brass,
                CycleSpeed, new Vector2(.19f,.875f), new Vector2(.29f,.922f), Vector2.zero, Vector2.zero, 9);
            speedButtonText = speed.GetComponentInChildren<Text>();
            speedButtonText.text = FormatSpeed(project.world.timeScale);
            UiFactory.Label("Sim Label", parent, "AMBIENT WORLD — independent from writing", 9, PrinceTitanTheme.Muted, TextAnchor.MiddleRight,
                new Vector2(.31f,.875f), new Vector2(.95f,.922f), Vector2.zero, Vector2.zero);
        }

        private void BuildFilterRow(Transform parent, float bottom, float top)
        {
            var captions = new[] { "ALL", "EMPIRE", "GOV", "CLAN", "CONTRACT" };
            var filters = new PowerKind?[] { null, PowerKind.Empire, PowerKind.Government, PowerKind.Clan, PowerKind.Contractor };
            var widths = new[] { .12f, .19f, .15f, .15f, .25f };
            var x = .05f;
            for (var i = 0; i < captions.Length; i++)
            {
                var captured = filters[i];
                var width = widths[i];
                UiFactory.Button("Filter " + captions[i], parent, captions[i], i == 0 ? PrinceTitanTheme.MagentaDark : PrinceTitanTheme.InkRaised,
                    PrinceTitanTheme.Ivory, () => SetMapFilter(captured), new Vector2(x,bottom), new Vector2(x+width,top), Vector2.zero, Vector2.zero, 8);
                x += width + .012f;
            }
        }

        private void BuildAtlasOverlay()
        {
            atlasOverlay = UiFactory.Panel("Expanded Atlas Overlay", canvas.transform, new Color(.055f,.047f,.06f,.995f),
                new Vector2(0f,0f), new Vector2(1f,.93f), Vector2.zero, Vector2.zero).gameObject;
            UiFactory.Label("Title", atlasOverlay.transform, "ATLAS OF INFLUENCE", 24, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.025f,.91f), new Vector2(.45f,.985f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Sub", atlasOverlay.transform, "Every house, market, company, machine and flight remains visible.", 11,
                PrinceTitanTheme.Brass, TextAnchor.MiddleLeft, new Vector2(.025f,.875f), new Vector2(.60f,.925f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Close", atlasOverlay.transform, "CLOSE  ×", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory, CloseOverlays,
                new Vector2(.89f,.92f), new Vector2(.975f,.975f), Vector2.zero, Vector2.zero, 11);

            var mapFrame = UiFactory.Panel("Map Frame", atlasOverlay.transform, PrinceTitanTheme.PaperInk,
                new Vector2(.025f,.055f), new Vector2(.735f,.865f), Vector2.zero, Vector2.zero);
            var mapRect = UiFactory.Rect("Large Map", mapFrame.transform, Vector2.zero, Vector2.one, new Vector2(4f,4f), new Vector2(-4f,-4f));
            expandedAtlas = mapRect.gameObject.AddComponent<AtlasGraphic>();
            expandedAtlas.Bind(project.world);
            CreateSiteOverlays(mapRect, true);

            var right = UiFactory.Panel("Atlas Ledger", atlasOverlay.transform, PrinceTitanTheme.InkSoft,
                new Vector2(.755f,.055f), new Vector2(.975f,.865f), Vector2.zero, Vector2.zero);
            UiFactory.Label("Power Label", right.transform, "FOUR POWERS", 11, PrinceTitanTheme.Magenta, TextAnchor.MiddleLeft,
                new Vector2(.07f,.91f), new Vector2(.93f,.97f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var y = .83f;
            foreach (var faction in WorldSeed.Factions)
            {
                var state = project.world.factions.First(s => s.factionId == faction.id);
                UiFactory.Label("Name " + faction.id, right.transform, faction.name.ToUpperInvariant(), 11, faction.Color, TextAnchor.MiddleLeft,
                    new Vector2(.07f,y), new Vector2(.93f,y+.055f), Vector2.zero, Vector2.zero, FontStyle.Bold);
                UiFactory.Label("Kind " + faction.id, right.transform, faction.kind.ToString().ToUpperInvariant() + "  •  " + Mathf.RoundToInt(state.influence) + "% INFLUENCE",
                    9, PrinceTitanTheme.Muted, TextAnchor.MiddleLeft, new Vector2(.07f,y-.04f), new Vector2(.93f,y+.005f), Vector2.zero, Vector2.zero);
                var track = UiFactory.Panel("Track " + faction.id, right.transform, PrinceTitanTheme.Ink,
                    new Vector2(.07f,y-.065f), new Vector2(.93f,y-.048f), Vector2.zero, Vector2.zero);
                UiFactory.Panel("Fill", track.transform, faction.Color, Vector2.zero, new Vector2(state.influence/100f,1f), Vector2.zero, Vector2.zero);
                y -= .16f;
            }
            UiFactory.Label("Meaning", right.transform, "Influence changes slowly as the ambient simulation runs. It never rewards or punishes your word count.", 10,
                PrinceTitanTheme.Ivory, TextAnchor.UpperLeft, new Vector2(.07f,.16f), new Vector2(.93f,.27f), Vector2.zero, Vector2.zero);
            expandedSiteDetailText = UiFactory.Label("Selected Place", right.transform, "Select a place on the map.", 9,
                PrinceTitanTheme.Muted, TextAnchor.UpperLeft, new Vector2(.07f,.055f), new Vector2(.93f,.15f), Vector2.zero, Vector2.zero);
            UiFactory.Label("Legend", right.transform, "AIRCRAFT  •  ROBOTS  •  MARKETS\nCOMPANIES  •  HOUSES  •  CITIES", 9,
                PrinceTitanTheme.Brass, TextAnchor.UpperLeft, new Vector2(.07f,.005f), new Vector2(.93f,.052f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            atlasOverlay.SetActive(false);
        }

        private void BuildLineageOverlay()
        {
            lineageOverlay = UiFactory.Panel("Lineage Overlay", canvas.transform, new Color(.055f,.047f,.06f,.995f),
                new Vector2(0f,0f), new Vector2(1f,.93f), Vector2.zero, Vector2.zero).gameObject;
            UiFactory.Label("Title", lineageOverlay.transform, "BIOLOGICAL & POLITICAL LINEAGES", 24, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.025f,.91f), new Vector2(.60f,.985f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Sub", lineageOverlay.transform, "Origin, family, allegiance and every role a person has held.", 11,
                PrinceTitanTheme.Brass, TextAnchor.MiddleLeft, new Vector2(.025f,.875f), new Vector2(.60f,.925f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Close", lineageOverlay.transform, "CLOSE  ×", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory, CloseOverlays,
                new Vector2(.89f,.92f), new Vector2(.975f,.975f), Vector2.zero, Vector2.zero, 11);

            var tree = UiFactory.Panel("Tree Paper", lineageOverlay.transform, PrinceTitanTheme.PaperLight,
                new Vector2(.025f,.05f), new Vector2(.975f,.86f), Vector2.zero, Vector2.zero);
            var connectionsRect = UiFactory.Rect("Connections", tree.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            connectionsRect.gameObject.AddComponent<LineageConnectionsGraphic>();
            foreach (var person in WorldSeed.People) CreatePersonCard(tree.transform, person);
            UiFactory.Label("Tree Note", tree.transform, "SOLID LINES SHOW RECORDED PARENTAGE  •  CARD COLOR SHOWS CURRENT ALLEGIANCE", 9,
                PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk,.75f), TextAnchor.MiddleCenter,
                new Vector2(.18f,.005f), new Vector2(.82f,.055f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            lineageOverlay.SetActive(false);
        }

        private void CreatePersonCard(Transform parent, PersonData person)
        {
            var faction = WorldSeed.Faction(person.factionId);
            var outer = UiFactory.Panel("Person " + person.id, parent, faction.Color,
                person.treePosition, person.treePosition, new Vector2(-105f,-44f), new Vector2(105f,44f));
            var inner = UiFactory.Panel("Card", outer.transform, PrinceTitanTheme.InkSoft, Vector2.zero, Vector2.one, new Vector2(3f,3f), new Vector2(-3f,-3f));
            UiFactory.Label("Name", inner.transform, person.name.ToUpperInvariant(), 12, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.06f,.58f), new Vector2(.94f,.92f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Role", inner.transform, person.role, 10, faction.Color, TextAnchor.MiddleLeft,
                new Vector2(.06f,.34f), new Vector2(.94f,.62f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Origin", inner.transform, person.origin + "  •  b. " + person.birthYear, 9, PrinceTitanTheme.Muted, TextAnchor.MiddleLeft,
                new Vector2(.06f,.08f), new Vector2(.94f,.36f), Vector2.zero, Vector2.zero);
        }

        private void CreateSiteOverlays(RectTransform map, bool large)
        {
            foreach (var site in WorldSeed.Sites)
            {
                var captured = site;
                var hit = UiFactory.Panel("Hit " + site.id, map, new Color(1f,1f,1f,.001f), site.position, site.position,
                    new Vector2(large ? -18f : -12f, large ? -18f : -12f), new Vector2(large ? 18f : 12f, large ? 18f : 12f));
                var button = hit.gameObject.AddComponent<Button>();
                button.targetGraphic = hit;
                button.onClick.AddListener(() => SelectSite(captured));
                var label = UiFactory.Label("Label " + site.id, map, site.name, large ? 10 : 7,
                    PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .88f), TextAnchor.MiddleLeft,
                    site.position, site.position, new Vector2(large ? 14f : 9f, large ? -9f : -7f),
                    new Vector2(large ? 155f : 90f, large ? 10f : 7f), FontStyle.Bold);
                siteOverlays.Add(new SiteOverlayRef { site = site, button = hit.gameObject, label = label.gameObject });
            }
        }

        private void RebuildChapterList()
        {
            if (chapterContent == null) return;
            for (var i = chapterContent.childCount - 1; i >= 0; i--) Destroy(chapterContent.GetChild(i).gameObject);
            foreach (var chapter in project.chapters)
            {
                var captured = chapter;
                var active = chapter.id == project.activeChapterId;
                var panel = UiFactory.Panel("Chapter " + chapter.id, chapterContent, active ? PrinceTitanTheme.InkRaised : PrinceTitanTheme.Ink,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).rectTransform;
                UiFactory.Layout(panel, 58f);
                var button = panel.gameObject.AddComponent<Button>();
                button.targetGraphic = panel.GetComponent<Image>();
                button.onClick.AddListener(() => SwitchChapter(captured.id));
                UiFactory.Panel("Active", panel, active ? PrinceTitanTheme.Magenta : Color.clear,
                    new Vector2(0f,0f), new Vector2(0f,1f), Vector2.zero, new Vector2(4f,0f));
                UiFactory.Label("Title", panel, string.IsNullOrWhiteSpace(chapter.title) ? "Untitled chapter" : chapter.title, 11,
                    PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft, new Vector2(.06f,.35f), new Vector2(.94f,.93f), Vector2.zero, Vector2.zero, active ? FontStyle.Bold : FontStyle.Normal);
                UiFactory.Label("Count", panel, ProjectStore.CountWords(chapter.body) + " WORDS", 8,
                    active ? PrinceTitanTheme.Brass : PrinceTitanTheme.Muted, TextAnchor.MiddleLeft,
                    new Vector2(.06f,.04f), new Vector2(.94f,.36f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            }
            Canvas.ForceUpdateCanvases();
        }

        private void LoadActiveChapter()
        {
            activeChapter = project.chapters.FirstOrDefault(c => c.id == project.activeChapterId) ?? project.chapters[0];
            project.activeChapterId = activeChapter.id;
            loadingChapter = true;
            titleInput.text = activeChapter.title ?? string.Empty;
            bodyInput.text = activeChapter.body ?? string.Empty;
            loadingChapter = false;
            UpdateWordCount();
            RebuildChapterList();
        }

        private void SwitchChapter(string id)
        {
            if (activeChapter != null && activeChapter.id == id) return;
            CommitEditorToActive();
            project.activeChapterId = id;
            LoadActiveChapter();
            MarkDirty();
        }

        private void NewChapter()
        {
            CommitEditorToActive();
            var chapter = new ChapterData
            {
                id = Guid.NewGuid().ToString("N"),
                title = "Chapter " + (project.chapters.Count + 1),
                body = string.Empty,
                updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            project.chapters.Add(chapter);
            project.activeChapterId = chapter.id;
            LoadActiveChapter();
            MarkDirty();
            bodyInput.ActivateInputField();
        }

        private void OnTitleChanged(string value)
        {
            if (loadingChapter || activeChapter == null) return;
            activeChapter.title = value;
            activeChapter.updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            MarkDirty();
        }

        private void OnBodyChanged(string value)
        {
            if (loadingChapter || activeChapter == null) return;
            activeChapter.body = value;
            activeChapter.updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            UpdateWordCount();
            MarkDirty();
        }

        private void CommitEditorToActive()
        {
            if (activeChapter == null || titleInput == null || bodyInput == null) return;
            activeChapter.title = titleInput.text;
            activeChapter.body = bodyInput.text;
            activeChapter.updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void UpdateWordCount()
        {
            if (wordCountText == null || activeChapter == null) return;
            var words = ProjectStore.CountWords(activeChapter.body);
            wordCountText.text = words.ToString("N0") + " WORDS  •  " + activeChapter.body.Length.ToString("N0") + " CHARACTERS";
        }

        private void MarkDirty()
        {
            dirty = true;
            dirtyAge = 0f;
            if (saveStateText != null)
            {
                saveStateText.text = "UNSAVED CHANGES";
                saveStateText.color = PrinceTitanTheme.Brass;
            }
        }

        private void SaveProject(bool explicitSave)
        {
            try
            {
                CommitEditorToActive();
                ProjectStore.Save(project);
                dirty = false;
                dirtyAge = 0f;
                if (saveStateText != null)
                {
                    saveStateText.text = (explicitSave ? "SAVED  •  " : "AUTOSAVED  •  ") + DateTime.Now.ToString("HH:mm:ss");
                    saveStateText.color = PrinceTitanTheme.Success;
                }
                if (explicitSave) RebuildChapterList();
            }
            catch (Exception exception)
            {
                if (saveStateText != null)
                {
                    saveStateText.text = "SAVE FAILED  •  " + exception.Message;
                    saveStateText.color = PrinceTitanTheme.Magenta;
                }
                Debug.LogException(exception);
            }
        }

        private void ExportActiveChapter()
        {
            try
            {
                CommitEditorToActive();
                SaveProject(false);
                var path = ProjectStore.ExportChapter(project, activeChapter);
                saveStateText.text = "EXPORTED  •  " + System.IO.Path.GetFileName(path);
                saveStateText.color = PrinceTitanTheme.Brass;
            }
            catch (Exception exception)
            {
                saveStateText.text = "EXPORT FAILED  •  " + exception.Message;
                saveStateText.color = PrinceTitanTheme.Magenta;
            }
        }

        private void SelectSite(SiteData site)
        {
            var faction = WorldSeed.Faction(site.factionId);
            var state = project.world.factions.FirstOrDefault(f => f.factionId == site.factionId);
            var influence = state == null ? 0 : Mathf.RoundToInt(state.influence);
            var market = project.world.markets.FirstOrDefault(m => m.siteId == site.id);
            var marketLine = market == null ? string.Empty : "  •  MARKET " + Mathf.RoundToInt(market.activity) + "%";
            if (siteDetailText != null)
            {
                siteDetailText.text = "<b>" + site.name.ToUpperInvariant() + "</b>\n" + site.kind.ToString().ToUpperInvariant() + "  •  " +
                    faction.shortName + "  •  " + influence + "% INFLUENCE" + marketLine + "\n" + site.note;
            }
            if (expandedSiteDetailText != null)
            {
                expandedSiteDetailText.text = "<b>" + site.name.ToUpperInvariant() + "</b>\n" + faction.name + "  •  " + site.kind + "\n" + site.note;
                expandedSiteDetailText.color = faction.Color;
            }
        }

        private void SetMapFilter(PowerKind? filter)
        {
            activeFilter = filter;
            if (compactAtlas != null) compactAtlas.SetFilter(filter);
            if (expandedAtlas != null) expandedAtlas.SetFilter(filter);
            foreach (var overlay in siteOverlays)
            {
                var visible = !filter.HasValue || WorldSeed.Faction(overlay.site.factionId).kind == filter.Value;
                overlay.button.SetActive(visible);
                overlay.label.SetActive(visible);
            }
            var message = filter.HasValue ? filter.Value.ToString().ToUpperInvariant() : "ALL POWERS";
            OnWorldEvent(new WorldEvent("Atlas filter", message + " is visible on both maps.", filter.HasValue ? WorldSeed.Factions.First(f => f.kind == filter.Value).id : "vesper"));
        }

        private void TogglePause()
        {
            project.world.paused = !project.world.paused;
            if (pauseButtonText != null) pauseButtonText.text = project.world.paused ? "PLAY" : "PAUSE";
            MarkDirty();
        }

        private void CycleSpeed()
        {
            var speed = project.world.timeScale;
            if (speed < .75f) speed = 1f;
            else if (speed < 1.5f) speed = 2f;
            else if (speed < 3f) speed = 4f;
            else speed = .5f;
            project.world.timeScale = speed;
            if (speedButtonText != null) speedButtonText.text = FormatSpeed(speed);
            MarkDirty();
        }

        private static string FormatSpeed(float speed)
        {
            return speed < .75f ? "½×" : Mathf.RoundToInt(speed) + "×";
        }

        private void OnWorldEvent(WorldEvent item)
        {
            var faction = WorldSeed.Faction(item.factionId);
            eventLines.Enqueue("<color=#" + ColorUtility.ToHtmlStringRGB(faction.Color) + "><b>" + item.title.ToUpperInvariant() + "</b></color>  " + item.detail);
            while (eventLines.Count > 3) eventLines.Dequeue();
            if (eventFeedText != null) eventFeedText.text = string.Join("\n\n", eventLines.ToArray());
        }

        private void OpenAtlas()
        {
            lineageOverlay.SetActive(false);
            atlasOverlay.SetActive(true);
            atlasOverlay.transform.SetAsLastSibling();
        }

        private void OpenLineages()
        {
            atlasOverlay.SetActive(false);
            lineageOverlay.SetActive(true);
            lineageOverlay.transform.SetAsLastSibling();
        }

        private void CloseOverlays()
        {
            if (atlasOverlay != null) atlasOverlay.SetActive(false);
            if (lineageOverlay != null) lineageOverlay.SetActive(false);
        }
    }
}
