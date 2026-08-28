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
    public enum PrinceScreen { Home, Simulation, Writing, Organization, Archive }
    public enum ArchiveMode { Machines, Organizations, Places, Recordings }

    public sealed partial class PrinceTitanApp : MonoBehaviour
    {
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
        private readonly Dictionary<string, Button> missionButtons = new Dictionary<string, Button>();

        private Text clockText;
        private Text saveStateText;
        private Text homeClockText;
        private Text homeIntelText;
        private Text simulationTitleText;
        private Text simulationDetailText;
        private Text simulationHelpText;
        private Text writingIntelText;
        private Text wordCountText;
        private Text personDetailText;
        private Text archiveTitleText;
        private Text archiveDetailText;
        private Text archivePlateCaptionText;
        private RawImage archivePreviewImage;
        private InputField projectNameInput;
        private InputField chapterTitleInput;
        private InputField chapterBodyInput;
        private RectTransform chapterListRoot;
        private RectTransform missionListRoot;
        private RectTransform peopleCardRoot;
        private RectTransform archiveListRoot;
        private RectTransform editorPaperRect;
        private GameObject writingIntelPanel;
        private ReliefSimulationView reliefView;
        private LineageBoardGraphic lineageGraphic;
        private SiteData selectedSite;
        private MissionData selectedMission;
        private PersonData selectedPerson;
        private MachineData selectedMachine;
        private ArchiveMode archiveMode = ArchiveMode.Machines;
        private bool simulationShowsSite;
        private bool writingIntelCollapsed;
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
            QualitySettings.antiAliasing = 4;
            QualitySettings.shadowDistance = 90f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            gameObject.AddComponent<AudioSource>();
            ambient = gameObject.AddComponent<PrinceTitanAmbient>();
        }

        private void Start()
        {
            WorldSeed.ValidateOrThrow();
            project = ProjectStore.LoadOrCreate();
            activeChapter = project.chapters.FirstOrDefault(chapter => chapter.id == project.activeChapterId) ?? project.chapters[0];
            simulation = new WorldSimulation(project.world);
            simulation.EventRaised += OnWorldEvent;
            UiFactory.Interaction += OnInteraction;
            selectedMission = project.world.missions.FirstOrDefault(mission => mission.status == MissionStatus.EnRoute) ?? project.world.missions.FirstOrDefault();
            selectedSite = selectedMission == null ? project.sites.FirstOrDefault() : WorldSeed.Site(project, selectedMission.destinationSiteId);
            selectedPerson = project.people.FirstOrDefault();
            selectedMachine = project.machines.FirstOrDefault();
            SeedIntelFeed();
            BuildInterface();
            ActivateScreen(PrinceScreen.Home);
            LoadActiveChapter();
            SelectMission(selectedMission, false);
            SelectPerson(selectedPerson);
            SelectMachine(selectedMachine);
            RefreshDynamic();
        }

        private void Update()
        {
            if (simulation != null) simulation.Tick(Time.unscaledDeltaTime);
            if (reliefView != null) reliefView.SyncVisuals(Time.unscaledDeltaTime);
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock >= .20f)
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
                if (EventSystem.current == null || EventSystem.current.currentInputModule == null) UiFactory.EnsureEventSystem();
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
            if (control && keyboard.dKey.wasPressedThisFrame) DuplicateActiveChapter();
            if (keyboard.escapeKey.wasPressedThisFrame) HandleEscape();
#else
            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (control && Input.GetKeyDown(KeyCode.S)) SaveProject(true);
            if (control && Input.GetKeyDown(KeyCode.E)) ExportActiveChapter();
            if (control && Input.GetKeyDown(KeyCode.N)) NewChapter();
            if (control && Input.GetKeyDown(KeyCode.D)) DuplicateActiveChapter();
            if (Input.GetKeyDown(KeyCode.Escape)) HandleEscape();
#endif
        }

        private void HandleEscape()
        {
            if (modalRoot != null && modalRoot.gameObject.activeSelf) { CloseModal(); return; }
            if (settingsOverlay != null && settingsOverlay.activeSelf) { settingsOverlay.SetActive(false); return; }
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
            ApplyUiScale(PlayerPrefs.GetFloat("PrinceTitan.UiScale", 1.10f), false);

            contentRoot = UiFactory.Rect("Operational Rooms", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -82f));
            BuildHomeScreen();
            BuildSimulationScreen();
            BuildWritingScreen();
            BuildOrganizationScreen();
            BuildArchiveScreen();
            BuildTopBar();
            BuildSettingsOverlay();

            modalRoot = UiFactory.Rect("Secure Dialog Layer", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            modalRoot.gameObject.SetActive(false);

            var transition = UiFactory.Panel("Blackout Transition", canvas.transform, PrinceTitanTheme.Black,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            transitionGroup = transition.gameObject.AddComponent<CanvasGroup>();
            transitionGroup.alpha = 0f;
            transitionGroup.blocksRaycasts = false;
            transitionGroup.interactable = false;
        }

        private void BuildTopBar()
        {
            var bar = UiFactory.Panel("Riveted Command Rail", canvas.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .985f),
                new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -82f), Vector2.zero);
            UiFactory.Shadow(bar, new Color(0f,0f,0f,.90f),5f);
            UiFactory.Rule("Signal Line", bar.transform, PrinceTitanTheme.Magenta, new Vector2(0f,0f), new Vector2(1f,0f), Vector2.zero, new Vector2(0f,3f));
            AddRivets(bar.transform);

            var emblem = UiFactory.Rect("Crown Cipher", bar.transform, new Vector2(0f,0f), new Vector2(.045f,1f), new Vector2(12f,7f), new Vector2(-1f,-7f));
            emblem.gameObject.AddComponent<PrinceEmblemGraphic>();
            UiFactory.Label("Brand",bar.transform,"PRINCE TITAN",25,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.046f,.35f),new Vector2(.205f,.94f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Brand Line",bar.transform,"CENTRAL DE INTELIGÊNCIA · 1944",17,PrinceTitanTheme.Brass,TextAnchor.UpperLeft,new Vector2(.046f,.04f),new Vector2(.22f,.40f),Vector2.zero,Vector2.zero,FontStyle.Bold);

            var nav=UiFactory.HorizontalGroup("File Tabs",bar.transform,new Vector2(.225f,.12f),new Vector2(.785f,.88f),Vector2.zero,Vector2.zero,6f);
            AddNavigation(nav,PrinceScreen.Home,"BUNKER");
            AddNavigation(nav,PrinceScreen.Simulation,"SIMULAÇÃO 3D");
            AddNavigation(nav,PrinceScreen.Writing,"ESCRITA");
            AddNavigation(nav,PrinceScreen.Organization,"ORGANIZAÇÃO");
            AddNavigation(nav,PrinceScreen.Archive,"ARQUIVO DE GUERRA");

            clockText=UiFactory.Label("Operation Clock",bar.transform,"DIA --- · --:--",18,PrinceTitanTheme.Success,TextAnchor.MiddleRight,new Vector2(.79f,.49f),new Vector2(.91f,.91f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            saveStateText=UiFactory.Label("Save State",bar.transform,"ARQUIVO SALVO",17,PrinceTitanTheme.Muted,TextAnchor.MiddleRight,new Vector2(.79f,.08f),new Vector2(.91f,.49f),Vector2.zero,Vector2.zero);
            UiFactory.Button("Settings",bar.transform,"AJUSTES",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,OpenSettings,new Vector2(.918f,.18f),new Vector2(.995f,.82f),Vector2.zero,Vector2.zero,17);
        }

        private void AddRivets(Transform parent)
        {
            for(var i=0;i<8;i++)
            {
                var x=.012f+i*.14f;
                var rivet=UiFactory.Panel("Rail Rivet",parent,PrinceTitanTheme.Brass,new Vector2(x,.08f),new Vector2(x,.08f),new Vector2(-3f,-3f),new Vector2(3f,3f));
                rivet.raycastTarget=false;
            }
        }

        private void AddNavigation(RectTransform parent, PrinceScreen screen, string caption)
        {
            var button=UiFactory.Button(screen+" Tab",parent,caption,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>ShowScreen(screen),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,18);
            UiFactory.Layout(button.image.rectTransform,58f,116f,1f);
            navigation[screen]=button;
        }

        private RectTransform NewScreen(PrinceScreen id, string resourcePath, Color tint)
        {
            var root=UiFactory.Rect(id+" Room",contentRoot,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            root.gameObject.SetActive(false);
            screens[id]=root.gameObject;
            if(!string.IsNullOrEmpty(resourcePath)) UiFactory.Texture(id+" Historical Plate",root,resourcePath,tint,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            return root;
        }

        private Image Glass(string name, Transform parent, Vector2 min, Vector2 max, Color accent, float alpha=.92f)
        {
            var image=UiFactory.Panel(name,parent,PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink,alpha),min,max,Vector2.zero,Vector2.zero);
            UiFactory.Outline(image,PrinceTitanTheme.WithAlpha(accent,.78f),1.5f);
            UiFactory.Shadow(image,new Color(0f,0f,0f,.72f),4f);
            return image;
        }

        private Image PaperCard(string name, Transform parent, Vector2 min, Vector2 max, Color accent)
        {
            var image=UiFactory.Panel(name,parent,PrinceTitanTheme.Paper,min,max,Vector2.zero,Vector2.zero,true);
            UiFactory.Outline(image,PrinceTitanTheme.WithAlpha(accent,.88f),2f);
            UiFactory.Shadow(image,new Color(0f,0f,0f,.76f),5f);
            var pin=UiFactory.Panel("Brass Pin",image.transform,accent,new Vector2(.5f,1f),new Vector2(.5f,1f),new Vector2(-7f,-10f),new Vector2(7f,4f));
            pin.raycastTarget=false;
            return image;
        }

        private void ShowScreen(PrinceScreen next)
        {
            if(activeScreen==next&&screens.ContainsKey(next)&&screens[next].activeSelf)return;
            if(transitionRoutine!=null)StopCoroutine(transitionRoutine);
            transitionRoutine=StartCoroutine(TransitionTo(next));
        }

        private IEnumerator TransitionTo(PrinceScreen next)
        {
            transitionGroup.gameObject.SetActive(true);
            transitionGroup.blocksRaycasts=true;
            for(var t=0f;t<1f;t+=Time.unscaledDeltaTime*8f){transitionGroup.alpha=Mathf.SmoothStep(0f,1f,t);yield return null;}
            transitionGroup.alpha=1f;
            ActivateScreen(next);
            for(var t=0f;t<1f;t+=Time.unscaledDeltaTime*5f){transitionGroup.alpha=Mathf.SmoothStep(1f,0f,t);yield return null;}
            transitionGroup.alpha=0f;transitionGroup.blocksRaycasts=false;transitionGroup.gameObject.SetActive(false);transitionRoutine=null;
        }

        private void ActivateScreen(PrinceScreen next)
        {
            activeScreen=next;
            foreach(var pair in screens)pair.Value.SetActive(pair.Key==next);
            foreach(var pair in navigation)
            {
                var colors=pair.Value.colors;
                colors.normalColor=pair.Key==next?PrinceTitanTheme.Magenta:PrinceTitanTheme.Olive;
                colors.selectedColor=colors.normalColor;
                pair.Value.colors=colors;
            }
            if(next==PrinceScreen.Writing)LoadActiveChapter();
            if(next==PrinceScreen.Simulation)RefreshMissionBoard();
            if(next==PrinceScreen.Organization)RebuildPeopleCards();
            if(next==PrinceScreen.Archive)RebuildArchiveList();
        }

        private void RefreshDynamic()
        {
            if(simulation==null)return;
            var time=simulation.ClockText();
            if(clockText!=null)clockText.text=time;
            if(homeClockText!=null)homeClockText.text=time;
            RefreshHomeIntel();
            RefreshMissionBoard();
            RefreshSelectedMission();
            UpdateWritingIntelligence();
            if(lineageGraphic!=null&&lineageGraphic.gameObject.activeInHierarchy)lineageGraphic.SetVerticesDirty();
        }

        private void SeedIntelFeed()
        {
            eventLines.Clear();
            if(project.world.eventHistory!=null)
            {
                foreach(var item in project.world.eventHistory.OrderByDescending(value=>value.day*1440f+value.minuteOfDay).Take(3).Reverse())
                    eventLines.Enqueue("◆ OCORRÊNCIA REGISTRADA · "+item.title.ToUpperInvariant()+"\n"+item.detail);
            }
            foreach(var mission in project.world.missions.Where(value=>value.status==MissionStatus.EnRoute).Take(2))
                eventLines.Enqueue("◆ OPERAÇÃO EM CURSO · "+mission.callsign+"\n"+mission.objective+" · "+simulation.EtaText(mission));
            while(eventLines.Count>5)eventLines.Dequeue();
        }

        private void OnWorldEvent(WorldEvent item)
        {
            eventLines.Enqueue("◆ "+item.title.ToUpperInvariant()+"\n"+item.detail);
            while(eventLines.Count>5)eventLines.Dequeue();
            if(!string.IsNullOrEmpty(item.missionId))SelectMission(WorldSeed.Mission(project,item.missionId),false);
            UiFactory.Report(item.title);
            MarkDirty();
        }

        private void OnInteraction(string message){if(saveStateText!=null&&!dirty)saveStateText.text=message;}

        private void MarkDirty()
        {
            dirty=true;dirtyAge=0f;
            if(saveStateText!=null){saveStateText.text="ALTERAÇÕES NÃO ARQUIVADAS";saveStateText.color=PrinceTitanTheme.Brass;}
        }

        private void SaveProject(bool explicitSave)
        {
            if(project==null)return;
            CaptureActiveChapter();
            ProjectStore.Save(project);
            dirty=false;dirtyAge=0f;
            if(saveStateText!=null){saveStateText.text=explicitSave?"ARQUIVO SALVO AGORA":"AUTOSSALVO";saveStateText.color=PrinceTitanTheme.Success;}
        }

        private void CaptureActiveChapter()
        {
            if(activeChapter==null)return;
            activeChapter.updatedUnix=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if(chapterTitleInput!=null)activeChapter.title=chapterTitleInput.text;
            if(chapterBodyInput!=null)activeChapter.body=chapterBodyInput.text;
        }

        private string OrganizationLabel(OrganizationKind kind)
        {
            switch(kind){case OrganizationKind.Empire:return"IMPÉRIO";case OrganizationKind.Government:return"GOVERNO";case OrganizationKind.Clan:return"CLÃ";case OrganizationKind.Contractor:return"EMPREITEIRA / ORGANIZAÇÃO";default:return kind.ToString().ToUpperInvariant();}
        }

        private string RealmLabel(RealmLayer value){return value==RealmLayer.RealWorld?"MUNDO REAL":"DIMENSÃO QUEBRADA";}

        private string SiteLabel(SiteKind kind)
        {
            switch(kind)
            {
                case SiteKind.Capital:return"CAPITAL";case SiteKind.Settlement:return"ROTA / ACAMPAMENTO";case SiteKind.Estate:return"PROPRIEDADE";
                case SiteKind.Airfield:return"AERÓDROMO";case SiteKind.RobotWorks:return"OFICINA DE ROBÔS";case SiteKind.Arena:return"RINGUE";
                case SiteKind.Port:return"PORTO";case SiteKind.Relay:return"RADAR / RELÉ";case SiteKind.Rift:return"QUEBRA DIMENSIONAL";
                case SiteKind.Forest:return"FLORESTA";case SiteKind.Depot:return"DEPÓSITO";case SiteKind.Academy:return"ACADEMIA";default:return kind.ToString().ToUpperInvariant();
            }
        }

        private string UnitLabel(UnitKind kind)
        {
            switch(kind){case UnitKind.ReconFighter:return"CAÇA DE RECONHECIMENTO";case UnitKind.RadialFighter:return"CAÇA RADIAL";case UnitKind.DiveAircraft:return"AERONAVE DE MERGULHO";case UnitKind.CargoRobot:return"ROBÔ DE CARGA";case UnitKind.ArenaRobot:return"ROBÔ DE ARENA";case UnitKind.GiantRobot:return"ROBÔ GIGANTE";case UnitKind.Titan:return"TITÃ";default:return kind.ToString().ToUpperInvariant();}
        }

        private static string IntegrityLabel(float value)
        {
            if(value<=.5f)return"PERDIDA";
            if(value<35f)return"CRÍTICA";
            if(value<70f)return"DANIFICADA";
            if(value<99.5f)return"MARCADA";
            return"INTEIRA";
        }
    }
}
