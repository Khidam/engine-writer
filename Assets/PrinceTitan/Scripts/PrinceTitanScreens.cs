using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed partial class PrinceTitanApp
    {
        private FactionData selectedRegionFaction;
        private GameObject siteObservationRoot;
        private RawImage siteObservationImage;
        private Text siteObservationTitle;
        private Text siteObservationDetail;

        private void BuildHomeScreen()
        {
            var root = NewScreen(PrinceScreen.Home, "PrinceTitan/Scenes/command_room_qhd", Color.white);
            UiFactory.Panel("Left Cinematic Shade", root, new Color(.025f, .018f, .028f, .78f),
                Vector2.zero, new Vector2(.60f, 1f), Vector2.zero, Vector2.zero);
            UiFactory.Panel("Bottom Cinematic Shade", root, new Color(.02f, .015f, .02f, .86f),
                new Vector2(0f, 0f), new Vector2(1f, .22f), Vector2.zero, Vector2.zero);

            UiFactory.Label("Home Eyebrow", root, "SALA DE COMANDO · SISTEMA OPERACIONAL", 19, PrinceTitanTheme.Brass,
                TextAnchor.MiddleLeft, new Vector2(.045f, .78f), new Vector2(.59f, .88f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Home Title", root, "UM MUNDO QUE\nCONTINUA VIVO.", 54, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleLeft, new Vector2(.045f, .54f), new Vector2(.58f, .80f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Home Copy", root,
                "Escreva dentro da história. Observe aviões e Titãs, acompanhe casas, mercados, companhias, famílias e os quatro poderes sem perder nenhum movimento.",
                23, PrinceTitanTheme.Ivory, TextAnchor.UpperLeft,
                new Vector2(.047f, .38f), new Vector2(.54f, .55f), Vector2.zero, Vector2.zero);

            var pulse = Glass("World Pulse", root, new Vector2(.72f, .68f), new Vector2(.975f, .91f), PrinceTitanTheme.Magenta, .83f);
            UiFactory.Label("Pulse Label", pulse.transform, "OBSERVATÓRIO TOTAL", 18, PrinceTitanTheme.Magenta,
                TextAnchor.MiddleLeft, new Vector2(.07f, .70f), new Vector2(.93f, .92f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            homeClockText = UiFactory.Label("Pulse Clock", pulse.transform, "DIA 000 · 00:00", 29, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleLeft, new Vector2(.07f, .44f), new Vector2(.93f, .72f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            homePulseText = UiFactory.Label("Pulse Details", pulse.transform, "PULSO DO MUNDO", 18, PrinceTitanTheme.Muted,
                TextAnchor.UpperLeft, new Vector2(.07f, .08f), new Vector2(.93f, .44f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            var doors = UiFactory.HorizontalGroup("Room Doors", root, new Vector2(.035f, .035f), new Vector2(.965f, .17f),
                Vector2.zero, Vector2.zero, 12f);
            AddDoor(doors, "MAPA VIVO", "Tudo em movimento", PrinceTitanTheme.Magenta, () => ShowScreen(PrinceScreen.Map));
            AddDoor(doors, "ESCRITA", "A máquina de cenas", PrinceTitanTheme.Brass, () => ShowScreen(PrinceScreen.Writing));
            AddDoor(doors, "PESSOAS", "Famílias e papéis", PrinceTitanTheme.Ivory, () => ShowScreen(PrinceScreen.People));
            AddDoor(doors, "PODERES", "Influência e regiões", PrinceTitanTheme.Government, () => ShowScreen(PrinceScreen.Powers));
            AddDoor(doors, "ECONOMIA", "Mercados e companhias", PrinceTitanTheme.Contractor, () => ShowScreen(PrinceScreen.Economy));
        }

        private void AddDoor(RectTransform parent, string title, string subtitle, Color accent, UnityEngine.Events.UnityAction action)
        {
            var button = UiFactory.Button(title + " Door", parent, title + "\n" + subtitle, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.InkRaised, .96f),
                accent, action, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 21);
            UiFactory.Layout(button.image.rectTransform, 96f, 150f, 1f);
        }

        private void BuildMapScreen()
        {
            var root = NewScreen(PrinceScreen.Map, null, Color.white);
            UiFactory.Panel("Map Foundation", root, PrinceTitanTheme.Black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var filters = UiFactory.HorizontalGroup("Map Filters", root, new Vector2(.015f, .92f), new Vector2(.775f, .992f),
                Vector2.zero, Vector2.zero, 8f);
            AddMapFilterButton(filters, "TUDO", null, PrinceTitanTheme.Magenta);
            foreach (var faction in project.factions)
                AddMapFilterButton(filters, FactionLabel(faction.kind), faction.id, faction.Color);
            var reset = UiFactory.Button("Reset Map", filters, "CENTRALIZAR", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                () => { if (mapPanZoom != null) mapPanZoom.ResetView(); }, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16);
            UiFactory.Layout(reset.image.rectTransform, 52f, 112f, 1f);

            var viewportImage = UiFactory.Panel("Living Map Viewport", root, PrinceTitanTheme.Black,
                new Vector2(.015f, .035f), new Vector2(.775f, .905f), Vector2.zero, Vector2.zero, true);
            UiFactory.Outline(viewportImage, PrinceTitanTheme.Brass, 2f);
            viewportImage.gameObject.AddComponent<RectMask2D>();
            var viewport = viewportImage.rectTransform;
            mapContent = UiFactory.Rect("Living World Surface", viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mapContent.pivot = new Vector2(.5f, .5f);
            var mapImage = UiFactory.Texture("World Relief", mapContent, "PrinceTitan/Scenes/world_map_qhd", Color.white,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
            mapImage.raycastTarget = true;
            mapPanZoom = mapImage.gameObject.AddComponent<MapPanZoom>();
            mapPanZoom.viewport = viewport;
            mapPanZoom.target = mapContent;

            var overlayRect = UiFactory.Rect("Live Routes", mapContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            worldOverlay = overlayRect.gameObject.AddComponent<WorldOverlayGraphic>();
            worldOverlay.Configure(project);
            mapMarkerRoot = UiFactory.Rect("Places", mapContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mapUnitRoot = UiFactory.Rect("Moving Units", mapContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            BuildMapMarkers();

            var dossier = Glass("Map Intelligence", root, new Vector2(.79f, .035f), new Vector2(.985f, .992f), PrinceTitanTheme.Magenta, .94f);
            UiFactory.Label("Map Header", dossier.transform, "MAPA VIVO", 34, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.07f, .90f), new Vector2(.93f, .98f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            mapScaleText = UiFactory.Label("Map Help", dossier.transform, "ARRASTE PARA MOVER · RODA PARA ZOOM", 16, PrinceTitanTheme.Brass,
                TextAnchor.MiddleLeft, new Vector2(.07f, .855f), new Vector2(.93f, .91f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Rule("Map Detail Rule", dossier.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ivory, .22f),
                new Vector2(.07f, .842f), new Vector2(.93f, .845f), Vector2.zero, Vector2.zero);
            mapDetailText = UiFactory.Label("Map Detail", dossier.transform, "Selecione um lugar.", 20, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .59f), new Vector2(.93f, .83f), Vector2.zero, Vector2.zero);
            UiFactory.Label("Intelligence Header", dossier.transform, "ACONTECENDO AGORA", 17, PrinceTitanTheme.Magenta,
                TextAnchor.MiddleLeft, new Vector2(.07f, .51f), new Vector2(.93f, .575f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            mapEventText = UiFactory.Label("Map Events", dossier.transform, "O mundo está acordando.", 17, PrinceTitanTheme.Muted,
                TextAnchor.UpperLeft, new Vector2(.07f, .28f), new Vector2(.93f, .51f), Vector2.zero, Vector2.zero);

            UiFactory.Button("Observe Site", dossier.transform, "OBSERVAR DE PERTO", PrinceTitanTheme.Brass, PrinceTitanTheme.Ink,
                OpenSiteObservation, new Vector2(.07f, .195f), new Vector2(.93f, .265f), Vector2.zero, Vector2.zero, 17);
            UiFactory.Button("Pause World", dossier.transform, "PAUSAR / CONTINUAR", PrinceTitanTheme.Magenta,
                PrinceTitanTheme.Ivory, TogglePause, new Vector2(.07f, .10f), new Vector2(.93f, .17f), Vector2.zero, Vector2.zero, 17);
            UiFactory.Button("World Speed", dossier.transform, "VELOCIDADE 1× / 2× / 4×", PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, CycleSpeed, new Vector2(.07f, .025f), new Vector2(.93f, .09f), Vector2.zero, Vector2.zero, 16);
            BuildSiteObservation(root);
        }

        private void AddMapFilterButton(RectTransform parent, string caption, string factionId, Color accent)
        {
            var button = UiFactory.Button(caption + " Filter", parent, caption, PrinceTitanTheme.InkRaised, accent,
                () => SetMapFilter(factionId), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16);
            UiFactory.Layout(button.image.rectTransform, 52f, 92f, 1f);
        }

        private void BuildMapMarkers()
        {
            if (mapMarkerRoot == null || mapUnitRoot == null) return;
            UiFactory.ClearChildren(mapMarkerRoot);
            UiFactory.ClearChildren(mapUnitRoot);
            siteWidgets.Clear();
            moverWidgets.Clear();

            foreach (var site in project.sites)
            {
                var faction = WorldSeed.Faction(project, site.factionId);
                var rect = UiFactory.Rect(site.name + " Marker", mapMarkerRoot, site.position, site.position,
                    new Vector2(-27f, -27f), new Vector2(27f, 27f));
                var icon = rect.gameObject.AddComponent<MapIconGraphic>();
                icon.iconKind = MapIconKind.Site;
                icon.siteKind = site.kind;
                icon.tint = faction.Color;
                icon.raycastTarget = true;
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = icon;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => SelectSite(site));
                rect.gameObject.AddComponent<ButtonMotion>();
                var group = rect.gameObject.AddComponent<CanvasGroup>();

                var tag = UiFactory.Panel("Place Tag", rect, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .90f),
                    new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(7f, -20f), new Vector2(172f, 20f));
                UiFactory.Outline(tag, PrinceTitanTheme.WithAlpha(faction.Color, .78f), 1f);
                UiFactory.Label("Name", tag.transform, site.name.ToUpperInvariant(), 17, PrinceTitanTheme.Ivory,
                    TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(8f, 2f), new Vector2(-6f, -2f), FontStyle.Bold);
                siteWidgets.Add(new SiteWidget { site = site, group = group, rect = rect });
            }

            foreach (var mover in project.world.movers)
            {
                var faction = WorldSeed.Faction(project, mover.factionId);
                var rect = UiFactory.Rect(mover.id, mapUnitRoot, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                    new Vector2(-31f, -31f), new Vector2(31f, 31f));
                var icon = rect.gameObject.AddComponent<MapIconGraphic>();
                icon.iconKind = mover.kind == MoverKind.Aircraft ? MapIconKind.Aircraft : MapIconKind.Robot;
                icon.tint = faction.Color;
                icon.raycastTarget = false;
                var group = rect.gameObject.AddComponent<CanvasGroup>();
                var label = UiFactory.Panel("Unit Tag", rect, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .90f),
                    new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(6f, -18f), new Vector2(128f, 18f));
                UiFactory.Label("Name", label.transform, mover.kind == MoverKind.Aircraft ? "AERONAVE" : "TITÃ",
                    16, faction.Color, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(7f, 1f), new Vector2(-4f, -1f), FontStyle.Bold);
                moverWidgets.Add(new MoverWidget { mover = mover, rect = rect, group = group });
            }
        }

        private void SetMapFilter(string factionId)
        {
            mapFactionFilter = factionId;
            if (worldOverlay != null) worldOverlay.SetFilter(factionId);
            if (!string.IsNullOrEmpty(factionId))
            {
                var faction = WorldSeed.Faction(project, factionId);
                if (mapPanZoom != null) mapPanZoom.Focus(faction.capital, 1.8f);
            }
            else if (mapPanZoom != null) mapPanZoom.ResetView();
            UpdateMapWidgets();
        }

        private void UpdateMapWidgets()
        {
            foreach (var widget in siteWidgets)
            {
                var visible = string.IsNullOrEmpty(mapFactionFilter) || widget.site.factionId == mapFactionFilter;
                if (widget.group != null)
                {
                    widget.group.alpha = visible ? 1f : .24f;
                    widget.group.interactable = visible;
                    widget.group.blocksRaycasts = visible;
                }
            }

            foreach (var widget in moverWidgets)
            {
                var from = project.sites.FirstOrDefault(s => s.id == widget.mover.fromSiteId);
                var to = project.sites.FirstOrDefault(s => s.id == widget.mover.toSiteId);
                if (from == null || to == null) continue;
                var position = Vector2.Lerp(from.position, to.position, widget.mover.progress);
                widget.rect.anchorMin = position;
                widget.rect.anchorMax = position;
                var visible = string.IsNullOrEmpty(mapFactionFilter) || widget.mover.factionId == mapFactionFilter;
                widget.group.alpha = visible ? 1f : .12f;
            }

            if (selectedSite != null && mapDetailText != null)
            {
                var faction = WorldSeed.Faction(project, selectedSite.factionId);
                var state = project.world.factions.FirstOrDefault(f => f.factionId == faction.id);
                var influence = state == null ? 0f : state.influence;
                var activity = project.world.markets.FirstOrDefault(m => m.siteId == selectedSite.id);
                var activityLine = activity == null ? "" : "\nATIVIDADE DO MERCADO  " + activity.activity.ToString("0") + "%";
                mapDetailText.text = selectedSite.name.ToUpperInvariant() + "\n" + SiteLabel(selectedSite.kind) + " · " +
                    faction.name.ToUpperInvariant() + "\nINFLUÊNCIA REGIONAL  " + influence.ToString("0") + "%" + activityLine +
                    "\n\n" + selectedSite.note;
                mapDetailText.color = PrinceTitanTheme.Ivory;
            }
        }

        private void SelectSite(SiteData site)
        {
            if (site == null) return;
            selectedSite = site;
            UpdateMapWidgets();
        }

        private void BuildSiteObservation(RectTransform root)
        {
            var observation = UiFactory.Rect("Close Observation", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            siteObservationRoot = observation.gameObject;
            siteObservationImage = UiFactory.Texture("Observation Plate", observation, "PrinceTitan/Scenes/observe_estates_qhd",
                Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Panel("Observation Shade", observation, new Color(.01f, .008f, .012f, .24f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var card = Glass("Observation Card", observation, new Vector2(.63f, .08f), new Vector2(.965f, .55f), PrinceTitanTheme.Magenta, .94f);
            siteObservationTitle = UiFactory.Label("Observation Title", card.transform, "LUGAR", 31, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .69f), new Vector2(.93f, .93f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            siteObservationDetail = UiFactory.Label("Observation Detail", card.transform, "", 20, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .26f), new Vector2(.93f, .68f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Observation Back", card.transform, "VOLTAR AO MAPA", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                CloseSiteObservation, new Vector2(.07f, .06f), new Vector2(.93f, .21f), Vector2.zero, Vector2.zero, 18);
            observation.gameObject.SetActive(false);
        }

        private void OpenSiteObservation()
        {
            if (selectedSite == null || siteObservationRoot == null) return;
            var resource = "PrinceTitan/Scenes/observe_estates_qhd";
            if (selectedSite.kind == SiteKind.Airfield) resource = "PrinceTitan/Scenes/observe_airfield_qhd";
            if (selectedSite.kind == SiteKind.RobotWorks) resource = "PrinceTitan/Scenes/observe_titan_foundry_qhd";
            if (selectedSite.kind == SiteKind.Market || selectedSite.kind == SiteKind.Port) resource = "PrinceTitan/Scenes/observe_market_qhd";
            siteObservationImage.texture = Resources.Load<Texture2D>(resource);
            var cover = siteObservationImage.GetComponent<CoverRawImage>();
            if (cover != null) { cover.enabled = false; cover.enabled = true; }
            var faction = WorldSeed.Faction(project, selectedSite.factionId);
            siteObservationTitle.text = selectedSite.name.ToUpperInvariant() + "\n" + SiteLabel(selectedSite.kind);
            siteObservationDetail.text = "CONTROLADO POR " + faction.name.ToUpperInvariant() + "\n\n" + selectedSite.note +
                "\n\nO mundo continua em movimento enquanto você observa.";
            siteObservationRoot.SetActive(true);
        }

        private void CloseSiteObservation()
        {
            if (siteObservationRoot != null) siteObservationRoot.SetActive(false);
        }

        private void TogglePause()
        {
            project.world.paused = !project.world.paused;
            UiFactory.Report(project.world.paused ? "SIMULAÇÃO PAUSADA" : "SIMULAÇÃO EM MOVIMENTO");
            MarkDirty();
        }

        private void CycleSpeed()
        {
            project.world.timeScale = project.world.timeScale < 1.5f ? 2f : project.world.timeScale < 3f ? 4f : 1f;
            UiFactory.Report("VELOCIDADE " + project.world.timeScale.ToString("0") + "×");
            MarkDirty();
        }

        private void BuildWritingScreen()
        {
            var root = NewScreen(PrinceScreen.Writing, "PrinceTitan/Scenes/writing_room_qhd", Color.white);
            var rail = Glass("Chapter Cabinet", root, new Vector2(.012f, .045f), new Vector2(.245f, .97f), PrinceTitanTheme.Brass, .92f);
            UiFactory.Label("Project Label", rail.transform, "PROJETO", 17, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f, .91f), new Vector2(.93f, .97f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            projectNameInput = UiFactory.Input("Project Name", rail.transform, project.projectName, "Nome do projeto", 20,
                PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, new Vector2(.06f, .83f), new Vector2(.94f, .91f),
                Vector2.zero, Vector2.zero, false);
            projectNameInput.onValueChanged.AddListener(value => { project.projectName = value; MarkDirty(); });
            UiFactory.Label("Chapters Label", rail.transform, "CAPÍTULOS", 17, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f, .765f), new Vector2(.93f, .825f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var scroll = UiFactory.Scroll("Chapter List", rail.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black, .34f),
                new Vector2(.05f, .28f), new Vector2(.95f, .765f), Vector2.zero, Vector2.zero);
            chapterListRoot = scroll.content;
            UiFactory.Button("New Chapter", rail.transform, "+ NOVO CAPÍTULO", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                NewChapter, new Vector2(.06f, .19f), new Vector2(.94f, .26f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Save Chapter", rail.transform, "SALVAR AGORA", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                () => SaveProject(true), new Vector2(.06f, .105f), new Vector2(.94f, .175f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Export Chapter", rail.transform, "EXPORTAR CAPÍTULO", PrinceTitanTheme.Brass, PrinceTitanTheme.Ink,
                ExportActiveChapter, new Vector2(.06f, .025f), new Vector2(.94f, .09f), Vector2.zero, Vector2.zero, 17);

            chapterTitleInput = UiFactory.Input("Chapter Title", root, "", "Título do capítulo", 25,
                Color.clear, PrinceTitanTheme.PaperInk, new Vector2(.294f, .785f), new Vector2(.713f, .87f),
                Vector2.zero, Vector2.zero, false, true);
            chapterBodyInput = UiFactory.Input("Manuscript", root, "", "Comece a cena...", 23,
                Color.clear, PrinceTitanTheme.PaperInk, new Vector2(.294f, .335f), new Vector2(.713f, .785f),
                Vector2.zero, Vector2.zero, true, true);
            chapterTitleInput.onValueChanged.AddListener(value =>
            {
                if (loadingChapter || activeChapter == null) return;
                activeChapter.title = value;
                MarkDirty();
                RebuildChapterList();
            });
            chapterBodyInput.onValueChanged.AddListener(value =>
            {
                if (loadingChapter || activeChapter == null) return;
                activeChapter.body = value;
                UpdateWordCount();
                MarkDirty();
            });
            wordCountText = UiFactory.Label("Word Count", root, "0 PALAVRAS", 18, PrinceTitanTheme.Brass,
                TextAnchor.MiddleRight, new Vector2(.55f, .292f), new Vector2(.713f, .335f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            var intel = Glass("Writing Intelligence", root, new Vector2(.755f, .09f), new Vector2(.982f, .88f), PrinceTitanTheme.Magenta, .89f);
            UiFactory.Label("Intel Header", intel.transform, "JANELA DO MUNDO", 26, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.07f, .88f), new Vector2(.93f, .97f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Intel Copy", intel.transform, "A escrita não congela a simulação.", 18, PrinceTitanTheme.Brass,
                TextAnchor.UpperLeft, new Vector2(.07f, .78f), new Vector2(.93f, .88f), Vector2.zero, Vector2.zero);
            writingIntelText = UiFactory.Label("Writing Events", intel.transform, "O mundo está acordando.", 18, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .25f), new Vector2(.93f, .76f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Open Map From Writing", intel.transform, "ABRIR MAPA VIVO", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                () => ShowScreen(PrinceScreen.Map), new Vector2(.07f, .10f), new Vector2(.93f, .19f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Label("Writing Shortcuts", intel.transform, "CTRL+S SALVA · CTRL+E EXPORTA · CTRL+N NOVO", 16, PrinceTitanTheme.Muted,
                TextAnchor.MiddleLeft, new Vector2(.07f, .02f), new Vector2(.93f, .09f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            RebuildChapterList();
        }

        private void RebuildChapterList()
        {
            if (chapterListRoot == null) return;
            UiFactory.ClearChildren(chapterListRoot);
            foreach (var chapter in project.chapters.OrderByDescending(c => c.updatedUnix))
            {
                var selected = activeChapter != null && chapter.id == activeChapter.id;
                var button = UiFactory.Button("Chapter " + chapter.id, chapterListRoot,
                    (selected ? "◆ " : "") + (string.IsNullOrWhiteSpace(chapter.title) ? "Sem título" : chapter.title),
                    selected ? PrinceTitanTheme.MagentaDark : PrinceTitanTheme.InkRaised,
                    PrinceTitanTheme.Ivory, () => SelectChapter(chapter), Vector2.zero, Vector2.one,
                    Vector2.zero, Vector2.zero, 17);
                UiFactory.Layout(button.image.rectTransform, 62f);
            }
        }

        private void SelectChapter(ChapterData chapter)
        {
            if (chapter == null) return;
            if (activeChapter != null)
            {
                activeChapter.title = chapterTitleInput.text;
                activeChapter.body = chapterBodyInput.text;
            }
            activeChapter = chapter;
            project.activeChapterId = chapter.id;
            LoadActiveChapter();
            RebuildChapterList();
            MarkDirty();
        }

        private void LoadActiveChapter()
        {
            if (chapterTitleInput == null || chapterBodyInput == null || activeChapter == null) return;
            loadingChapter = true;
            chapterTitleInput.text = activeChapter.title ?? "";
            chapterBodyInput.text = activeChapter.body ?? "";
            loadingChapter = false;
            UpdateWordCount();
        }

        private void UpdateWordCount()
        {
            if (wordCountText == null || activeChapter == null) return;
            wordCountText.text = ProjectStore.CountWords(activeChapter.body).ToString("N0") + " PALAVRAS";
        }

        private void NewChapter()
        {
            if (activeChapter != null && chapterTitleInput != null)
            {
                activeChapter.title = chapterTitleInput.text;
                activeChapter.body = chapterBodyInput.text;
            }
            var chapter = new ChapterData
            {
                id = Guid.NewGuid().ToString("N"),
                title = "Novo capítulo " + (project.chapters.Count + 1),
                body = "",
                updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            project.chapters.Add(chapter);
            activeChapter = chapter;
            project.activeChapterId = chapter.id;
            LoadActiveChapter();
            RebuildChapterList();
            MarkDirty();
            ShowScreen(PrinceScreen.Writing);
        }

        private void ExportActiveChapter()
        {
            if (activeChapter == null) return;
            SaveProject(false);
            var path = ProjectStore.ExportChapter(project, activeChapter);
            if (saveStateText != null)
            {
                saveStateText.text = "EXPORTADO: " + System.IO.Path.GetFileName(path);
                saveStateText.color = PrinceTitanTheme.Brass;
            }
        }

        private void UpdateWritingIntelligence()
        {
            if (writingIntelText == null || eventLines.Count == 0) return;
            writingIntelText.text = string.Join("\n\n", eventLines.ToArray());
        }

        private void BuildPeopleScreen()
        {
            var root = NewScreen(PrinceScreen.People, "PrinceTitan/Scenes/lineage_room_qhd", Color.white);
            UiFactory.Panel("Lineage Tint", root, new Color(.02f, .015f, .02f, .20f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Label("People Title", root, "PESSOAS E LINHAGENS", 35, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.025f, .90f), new Vector2(.56f, .985f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("People Help", root, "BRANCO: VÍNCULO BIOLÓGICO · MAGENTA: VÍNCULO POLÍTICO", 17, PrinceTitanTheme.Brass,
                TextAnchor.MiddleLeft, new Vector2(.025f, .855f), new Vector2(.62f, .91f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Button("Add Person", root, "+ NOVA PESSOA", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                OpenPersonCreator, new Vector2(.82f, .90f), new Vector2(.98f, .978f), Vector2.zero, Vector2.zero, 18);

            var lines = UiFactory.Rect("Living Genealogy", root, new Vector2(.04f, .10f), new Vector2(.96f, .88f), Vector2.zero, Vector2.zero);
            lineageGraphic = lines.gameObject.AddComponent<LineageBoardGraphic>();
            lineageGraphic.Configure(project.people);
            peopleCardRoot = UiFactory.Rect("People Cards", root, new Vector2(.04f, .10f), new Vector2(.96f, .88f), Vector2.zero, Vector2.zero);

            var detail = Glass("Person Dossier", root, new Vector2(.25f, .015f), new Vector2(.75f, .15f), PrinceTitanTheme.Magenta, .95f);
            personDetailText = UiFactory.Label("Person Detail", detail.transform, "Selecione uma pessoa.", 19, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleCenter, new Vector2(.035f, .08f), new Vector2(.965f, .92f), Vector2.zero, Vector2.zero);
            RebuildPeopleCards();
        }

        private void RebuildPeopleCards()
        {
            if (peopleCardRoot == null) return;
            UiFactory.ClearChildren(peopleCardRoot);
            if (lineageGraphic != null) lineageGraphic.Configure(project.people);
            foreach (var person in project.people)
            {
                var faction = WorldSeed.Faction(project, person.factionId);
                var position = person.treePosition;
                var button = UiFactory.Button("Person " + person.id, peopleCardRoot,
                    person.name.ToUpperInvariant() + "\n" + person.role,
                    PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .94f), faction.Color,
                    () => SelectPerson(person), position, position, new Vector2(-86f, -41f), new Vector2(86f, 41f), 17);
                if (selectedPerson != null && selectedPerson.id == person.id)
                    UiFactory.Outline(button.image, PrinceTitanTheme.Ivory, 2f);
            }
        }

        private void SelectPerson(PersonData person)
        {
            if (person == null) return;
            selectedPerson = person;
            var faction = WorldSeed.Faction(project, person.factionId);
            var parents = project.people.Where(p => p.id == person.parentAId || p.id == person.parentBId).Select(p => p.name).ToArray();
            var parentLine = parents.Length == 0 ? "ASCENDÊNCIA NÃO REGISTRADA" : "ASCENDÊNCIA: " + string.Join(" + ", parents);
            if (personDetailText != null)
                personDetailText.text = person.name.ToUpperInvariant() + " · FAMÍLIA " + person.family.ToUpperInvariant() + "\n" +
                    person.role.ToUpperInvariant() + " · ORIGEM: " + person.origin.ToUpperInvariant() + " · NASC. " + person.birthYear +
                    "\n" + parentLine + " · ALIANÇA: " + faction.shortName;
        }

        private void BuildPowersScreen()
        {
            var root = NewScreen(PrinceScreen.Powers, "PrinceTitan/Scenes/powers_hall_qhd", Color.white);
            UiFactory.Panel("Powers Shade", root, new Color(.02f, .015f, .02f, .17f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Label("Powers Title", root, "OS QUATRO PODERES", 37, PrinceTitanTheme.Ivory, TextAnchor.MiddleCenter,
                new Vector2(.24f, .89f), new Vector2(.76f, .985f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Powers Help", root, "Cada poder move influência, território e contratos em tempo real.", 19, PrinceTitanTheme.Brass,
                TextAnchor.MiddleCenter, new Vector2(.20f, .84f), new Vector2(.80f, .90f), Vector2.zero, Vector2.zero);

            BuildPowerCard(root, project.factions[0], new Vector2(.045f, .55f), new Vector2(.30f, .81f));
            BuildPowerCard(root, project.factions[1], new Vector2(.36f, .61f), new Vector2(.64f, .86f));
            BuildPowerCard(root, project.factions[2], new Vector2(.70f, .54f), new Vector2(.96f, .80f));
            BuildPowerCard(root, project.factions[3], new Vector2(.66f, .08f), new Vector2(.94f, .36f));

            var family = Glass("Families Portal", root, new Vector2(.055f, .08f), new Vector2(.34f, .34f), PrinceTitanTheme.Brass, .91f);
            UiFactory.Label("Family Type", family.transform, "FAMÍLIAS", 18, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f, .71f), new Vector2(.93f, .92f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Family Count", family.transform, project.people.Select(p => p.family).Distinct().Count() + " LINHAGENS REGISTRADAS",
                24, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft, new Vector2(.07f, .42f), new Vector2(.93f, .72f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Button("Open Families", family.transform, "ABRIR ÁRVORE", PrinceTitanTheme.Brass, PrinceTitanTheme.Ink,
                () => ShowScreen(PrinceScreen.People), new Vector2(.07f, .08f), new Vector2(.93f, .36f), Vector2.zero, Vector2.zero, 18);

            BuildRegionOverlay(root);
        }

        private void BuildPowerCard(RectTransform root, FactionData faction, Vector2 min, Vector2 max)
        {
            var card = Glass(faction.name, root, min, max, faction.Color, .91f);
            UiFactory.Label("Type", card.transform, FactionLabel(faction.kind), 18, faction.Color, TextAnchor.MiddleLeft,
                new Vector2(.07f, .74f), new Vector2(.93f, .93f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Name", card.transform, faction.name.ToUpperInvariant(), 25, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.07f, .52f), new Vector2(.93f, .76f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var state = project.world.factions.First(s => s.factionId == faction.id);
            var value = UiFactory.Label("Influence", card.transform, state.influence.ToString("0") + "% DE INFLUÊNCIA", 18,
                PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft, new Vector2(.07f, .34f), new Vector2(.93f, .52f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var fill = AddMeter(card.transform, new Vector2(.07f, .25f), new Vector2(.93f, .32f), faction.Color);
            SetMeter(fill, state.influence);
            UiFactory.Button("Open " + faction.id, card.transform, "OBSERVAR REGIÃO", faction.Color,
                faction.kind == PowerKind.Government ? PrinceTitanTheme.Ink : PrinceTitanTheme.Ivory,
                () => OpenFactionRegion(faction), new Vector2(.07f, .055f), new Vector2(.93f, .20f), Vector2.zero, Vector2.zero, 17);
            powerWidgets[faction.id] = new PowerWidget { value = value, fill = fill };
        }

        private void BuildRegionOverlay(RectTransform root)
        {
            var region = UiFactory.Rect("Region Observatory", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            regionImage = UiFactory.Texture("Region Plate", region, "PrinceTitan/Scenes/region_empire_qhd", Color.white,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Panel("Region Shade", region, new Color(.015f, .012f, .018f, .45f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dossier = Glass("Region Dossier", region, new Vector2(.63f, .08f), new Vector2(.975f, .92f), PrinceTitanTheme.Magenta, .94f);
            regionTitleText = UiFactory.Label("Region Title", dossier.transform, "REGIÃO", 32, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .76f), new Vector2(.93f, .94f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            regionDetailText = UiFactory.Label("Region Detail", dossier.transform, "", 20, PrinceTitanTheme.Ivory,
                TextAnchor.UpperLeft, new Vector2(.07f, .28f), new Vector2(.93f, .75f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Region To Map", dossier.transform, "VER NO MAPA VIVO", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                FocusRegionOnMap, new Vector2(.07f, .15f), new Vector2(.93f, .24f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Close Region", dossier.transform, "VOLTAR AOS PODERES", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                CloseRegion, new Vector2(.07f, .045f), new Vector2(.93f, .13f), Vector2.zero, Vector2.zero, 18);
            region.gameObject.SetActive(false);
        }

        private void OpenFactionRegion(FactionData faction)
        {
            selectedRegionFaction = faction;
            var resource = "PrinceTitan/Scenes/region_empire_qhd";
            if (faction.kind == PowerKind.Government) resource = "PrinceTitan/Scenes/region_government_qhd";
            if (faction.kind == PowerKind.Clan) resource = "PrinceTitan/Scenes/region_clan_qhd";
            if (faction.kind == PowerKind.Contractor) resource = "PrinceTitan/Scenes/region_contractor_qhd";
            regionImage.texture = Resources.Load<Texture2D>(resource);
            var cover = regionImage.GetComponent<CoverRawImage>();
            if (cover != null) { cover.enabled = false; cover.enabled = true; }
            var state = project.world.factions.First(s => s.factionId == faction.id);
            var sites = project.sites.Where(s => s.factionId == faction.id).ToArray();
            regionTitleText.text = FactionLabel(faction.kind) + "\n" + faction.name.ToUpperInvariant();
            regionDetailText.text = state.influence.ToString("0") + "% DE INFLUÊNCIA\n\n“" + faction.motto + "”\n\n" +
                sites.Length + " LUGARES SOB OBSERVAÇÃO\n" + string.Join("\n", sites.Select(s => "◆ " + s.name + " · " + SiteLabel(s.kind)).ToArray());
            regionImage.transform.parent.gameObject.SetActive(true);
        }

        private void CloseRegion()
        {
            if (regionImage != null) regionImage.transform.parent.gameObject.SetActive(false);
        }

        private void FocusRegionOnMap()
        {
            if (selectedRegionFaction == null) return;
            CloseRegion();
            mapFactionFilter = selectedRegionFaction.id;
            if (worldOverlay != null) worldOverlay.SetFilter(mapFactionFilter);
            ShowScreen(PrinceScreen.Map);
            StartCoroutine(FocusMapNextFrame(selectedRegionFaction.capital));
        }

        private System.Collections.IEnumerator FocusMapNextFrame(Vector2 position)
        {
            yield return null;
            if (mapPanZoom != null) mapPanZoom.Focus(position, 2.1f);
        }

        private void BuildEconomyScreen()
        {
            var root = NewScreen(PrinceScreen.Economy, "PrinceTitan/Scenes/economy_room_qhd", Color.white);
            UiFactory.Panel("Economy Shade", root, new Color(.02f, .015f, .02f, .18f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.Label("Economy Title", root, "ECONOMIA OBSERVÁVEL", 35, PrinceTitanTheme.Ivory, TextAnchor.MiddleLeft,
                new Vector2(.025f, .905f), new Vector2(.43f, .985f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            economyPulseText = UiFactory.Label("Economy Pulse", root, "MERCADOS EM MOVIMENTO", 18, PrinceTitanTheme.Success,
                TextAnchor.MiddleRight, new Vector2(.67f, .91f), new Vector2(.975f, .98f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            var filters = UiFactory.HorizontalGroup("Economy Filters", root, new Vector2(.28f, .82f), new Vector2(.975f, .895f),
                Vector2.zero, Vector2.zero, 8f);
            AddEconomyFilter(filters, "TUDO", null, PrinceTitanTheme.Ivory);
            AddEconomyFilter(filters, "MERCADOS", SiteKind.Market, PrinceTitanTheme.Clan);
            AddEconomyFilter(filters, "COMPANHIAS", SiteKind.Company, PrinceTitanTheme.Contractor);
            AddEconomyFilter(filters, "CASAS", SiteKind.Estate, PrinceTitanTheme.Magenta);
            AddEconomyFilter(filters, "AVIAÇÃO", SiteKind.Airfield, PrinceTitanTheme.Government);
            AddEconomyFilter(filters, "ROBÓTICA", SiteKind.RobotWorks, PrinceTitanTheme.Brass);

            var list = Glass("Economy Register", root, new Vector2(.018f, .13f), new Vector2(.285f, .86f), PrinceTitanTheme.Brass, .91f);
            UiFactory.Label("Register Title", list.transform, "REGISTRO DO MUNDO", 22, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleLeft, new Vector2(.06f, .90f), new Vector2(.94f, .98f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var scroll = UiFactory.Scroll("Economy Places", list.transform, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black, .28f),
                new Vector2(.04f, .12f), new Vector2(.96f, .89f), Vector2.zero, Vector2.zero);
            economyListRoot = scroll.content;
            UiFactory.Button("Add Place", list.transform, "+ NOVO LUGAR", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                OpenPlaceCreator, new Vector2(.05f, .025f), new Vector2(.95f, .105f), Vector2.zero, Vector2.zero, 18);

            var detail = Glass("Economy Dossier", root, new Vector2(.33f, .035f), new Vector2(.75f, .30f), PrinceTitanTheme.Magenta, .93f);
            economyDetailText = UiFactory.Label("Economy Detail", detail.transform, "Selecione um lugar.", 20,
                PrinceTitanTheme.Ivory, TextAnchor.UpperLeft, new Vector2(.06f, .08f), new Vector2(.94f, .92f), Vector2.zero, Vector2.zero);
            UiFactory.Button("Export World Book", root, "EXPORTAR DOSSIÊ DO MUNDO", PrinceTitanTheme.Brass, PrinceTitanTheme.Ink,
                ExportWorldBook, new Vector2(.77f, .035f), new Vector2(.975f, .12f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Observe Economy Site", root, "OBSERVAR DE PERTO", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                ObserveEconomySite, new Vector2(.77f, .13f), new Vector2(.975f, .215f), Vector2.zero, Vector2.zero, 18);
            RebuildEconomyList();
        }

        private void AddEconomyFilter(RectTransform parent, string caption, SiteKind? kind, Color accent)
        {
            var button = UiFactory.Button(caption + " Economy Filter", parent, caption, PrinceTitanTheme.InkRaised, accent,
                () => SetEconomyFilter(kind), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16);
            UiFactory.Layout(button.image.rectTransform, 52f, 104f, 1f);
        }

        private void SetEconomyFilter(SiteKind? kind)
        {
            economyFilter = kind;
            RebuildEconomyList();
        }

        private void RebuildEconomyList()
        {
            if (economyListRoot == null) return;
            UiFactory.ClearChildren(economyListRoot);
            var places = economyFilter.HasValue ? project.sites.Where(s => s.kind == economyFilter.Value) : project.sites;
            foreach (var site in places.OrderBy(s => s.kind).ThenBy(s => s.name))
            {
                var faction = WorldSeed.Faction(project, site.factionId);
                var selected = selectedEconomySite != null && selectedEconomySite.id == site.id;
                var button = UiFactory.Button("Economy " + site.id,
                    economyListRoot, site.name.ToUpperInvariant() + "\n" + SiteLabel(site.kind),
                    selected ? faction.Color : PrinceTitanTheme.InkRaised,
                    selected && faction.kind == PowerKind.Government ? PrinceTitanTheme.Ink : PrinceTitanTheme.Ivory,
                    () => SelectEconomySite(site), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 17);
                UiFactory.Layout(button.image.rectTransform, 66f);
            }
        }

        private void SelectEconomySite(SiteData site)
        {
            if (site == null) return;
            selectedEconomySite = site;
            var faction = WorldSeed.Faction(project, site.factionId);
            var activity = project.world.markets.FirstOrDefault(m => m.siteId == site.id);
            var pulse = activity == null ? "STATUS OPERACIONAL" : "ATIVIDADE " + activity.activity.ToString("0") + "%";
            if (economyDetailText != null)
                economyDetailText.text = site.name.ToUpperInvariant() + "\n" + SiteLabel(site.kind) + " · " + faction.name.ToUpperInvariant() +
                    "\n" + pulse + "\n\n" + site.note;
        }

        private void UpdateEconomyPulse()
        {
            if (economyPulseText == null) return;
            var active = project.world.markets.Count == 0 ? 0f : project.world.markets.Average(m => m.activity);
            economyPulseText.text = "MERCADOS " + active.ToString("0") + "% · " + project.sites.Count(s => s.kind == SiteKind.Company) +
                " COMPANHIAS · " + project.sites.Count(s => s.kind == SiteKind.Estate) + " CASAS";
            if (selectedEconomySite != null && economyDetailText != null) SelectEconomySite(selectedEconomySite);
        }

        private void ExportWorldBook()
        {
            SaveProject(false);
            var path = ProjectStore.ExportWorldBook(project);
            if (saveStateText != null)
            {
                saveStateText.text = "DOSSIÊ EXPORTADO: " + System.IO.Path.GetFileName(path);
                saveStateText.color = PrinceTitanTheme.Brass;
            }
        }

        private void ObserveEconomySite()
        {
            if (selectedEconomySite == null) return;
            selectedSite = selectedEconomySite;
            ShowScreen(PrinceScreen.Map);
            StartCoroutine(OpenObservationNextFrame());
        }

        private System.Collections.IEnumerator OpenObservationNextFrame()
        {
            yield return new WaitForSecondsRealtime(.46f);
            OpenSiteObservation();
        }
    }
}
