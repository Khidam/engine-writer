using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed partial class PrinceTitanApp
    {
        private void BuildHomeScreen()
        {
            var root=NewScreen(PrinceScreen.Home,"PrinceTitan/Scenes/bunker_spy_1944_qhd",Color.white);
            UiFactory.Panel("Bunker Left Shade",root,new Color(.015f,.018f,.015f,.76f),Vector2.zero,new Vector2(.52f,1f),Vector2.zero,Vector2.zero);
            UiFactory.Panel("Bunker Lower Shade",root,new Color(.01f,.012f,.01f,.86f),new Vector2(0f,0f),new Vector2(1f,.18f),Vector2.zero,Vector2.zero);

            UiFactory.Label("Bunker Kicker",root,"POSTO CLANDESTINO · RÁDIO / CIFRA / RECONHECIMENTO",19,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.04f,.83f),new Vector2(.58f,.91f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Bunker Title",root,"BUNKER 1944",52,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.04f,.68f),new Vector2(.52f,.84f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Bunker Function",root,"CENTRAL DE INTERCEPTAÇÃO DA ORGANIZAÇÃO",22,PrinceTitanTheme.Magenta,TextAnchor.MiddleLeft,new Vector2(.042f,.62f),new Vector2(.56f,.70f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Bunker Copy",root,"Sinais dos besouros, rotas aéreas, gravações de robôs e fraturas dimensionais chegam nesta mesa. Cada cartão abaixo vem do estado real da simulação.",21,PrinceTitanTheme.Ivory,TextAnchor.UpperLeft,new Vector2(.042f,.45f),new Vector2(.50f,.62f),Vector2.zero,Vector2.zero);

            var radio=Glass("Valve Radio Report",root,new Vector2(.04f,.20f),new Vector2(.49f,.43f),PrinceTitanTheme.Success,.92f);
            UiFactory.Label("Radio Header",radio.transform,"RECEPTOR DE VÁLVULAS · CANAL DA SUSSURRO",18,PrinceTitanTheme.Success,TextAnchor.MiddleLeft,new Vector2(.06f,.70f),new Vector2(.94f,.94f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            homeClockText=UiFactory.Label("Bunker Clock",radio.transform,"DIA --- · --:--",28,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.06f,.42f),new Vector2(.94f,.72f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            homeIntelText=UiFactory.Label("Bunker Signal",radio.transform,"AGUARDANDO SINAL",18,PrinceTitanTheme.Muted,TextAnchor.UpperLeft,new Vector2(.06f,.08f),new Vector2(.94f,.42f),Vector2.zero,Vector2.zero);

            var dispatch=PaperCard("Pinned Orders",root,new Vector2(.70f,.22f),new Vector2(.97f,.88f),PrinceTitanTheme.Magenta);
            UiFactory.Label("Orders Stamp",dispatch.transform,"INTERCEPTAÇÕES NO AR",24,PrinceTitanTheme.MagentaDark,TextAnchor.MiddleLeft,new Vector2(.07f,.84f),new Vector2(.93f,.95f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var activeMissions=project.world.missions.Where(mission=>mission.status==MissionStatus.EnRoute).Take(4).ToArray();
            var orderText=string.Join("\n\n",activeMissions.Select(mission=>"● "+mission.callsign+"\n"+mission.title+" · "+simulation.EtaText(mission)).ToArray());
            UiFactory.Label("Orders",dispatch.transform,orderText,18,PrinceTitanTheme.PaperInk,TextAnchor.UpperLeft,new Vector2(.07f,.28f),new Vector2(.93f,.82f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Open Relief",dispatch.transform,"ABRIR MESA DE RELEVO",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,()=>ShowScreen(PrinceScreen.Simulation),new Vector2(.07f,.15f),new Vector2(.93f,.25f),Vector2.zero,Vector2.zero,19);
            UiFactory.Button("Open Writer",dispatch.transform,"ABRIR DOSSIÊ DE ESCRITA",PrinceTitanTheme.PaperInk,PrinceTitanTheme.Ivory,()=>ShowScreen(PrinceScreen.Writing),new Vector2(.07f,.035f),new Vector2(.93f,.135f),Vector2.zero,Vector2.zero,18);

            UiFactory.Label("Bunker Footer",root,"RÁDIO ATIVO  ·  ARQUIVO LOCAL  ·  SIMULAÇÃO CONTINUA ENQUANTO VOCÊ ESCREVE",18,PrinceTitanTheme.Brass,TextAnchor.MiddleCenter,new Vector2(.05f,.04f),new Vector2(.95f,.14f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        }

        private void RefreshHomeIntel()
        {
            if(homeIntelText==null||project==null||project.world==null)return;
            var active=project.world.missions.Count(mission=>mission.status==MissionStatus.EnRoute);
            var dimension=project.world.missions.Count(mission=>mission.realm==RealmLayer.BrokenDimension&&mission.status==MissionStatus.EnRoute);
            var latest=project.world.eventHistory==null?null:project.world.eventHistory.LastOrDefault();
            homeIntelText.text=active+" MISSÕES EM CURSO · "+dimension+" SINAL NA DIMENSÃO QUEBRADA\n"+(latest==null?"SEM NOVA GRAVAÇÃO":latest.title+" — "+latest.detail);
        }

        private void BuildSimulationScreen()
        {
            var root=NewScreen(PrinceScreen.Simulation,null,Color.white);
            UiFactory.Panel("Simulation Foundation",root,PrinceTitanTheme.Black,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);

            var viewportFrame=UiFactory.Panel("Relief View Frame",root,PrinceTitanTheme.Ink,new Vector2(.012f,.035f),new Vector2(.735f,.975f),Vector2.zero,Vector2.zero,true);
            UiFactory.Outline(viewportFrame,PrinceTitanTheme.Brass,2f);
            var viewport=UiFactory.Rect("Relief Render",viewportFrame.transform,Vector2.zero,Vector2.one,new Vector2(5f,5f),new Vector2(-5f,-5f));
            var image=viewport.gameObject.AddComponent<RawImage>();
            image.color=Color.white;
            image.raycastTarget=true;
            var viewObject=new GameObject("Runtime Relief Simulation");
            viewObject.transform.SetParent(transform,false);
            reliefView=viewObject.AddComponent<ReliefSimulationView>();
            image.texture=reliefView.Configure(project,1600,900);
            var input=viewport.gameObject.AddComponent<ReliefMapInput>();
            input.view=reliefView;
            reliefView.MissionSelected+=mission=>SelectMission(mission,false);
            reliefView.SiteSelected+=SelectSimulationSite;

            var titlePlate=Glass("Relief Title Plate",root,new Vector2(.028f,.865f),new Vector2(.43f,.955f),PrinceTitanTheme.Magenta,.87f);
            UiFactory.Label("Relief Title",titlePlate.transform,"SIMULAÇÃO DE RELEVO 3D",28,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.04f,.35f),new Vector2(.96f,.92f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            simulationHelpText=UiFactory.Label("Relief Help",titlePlate.transform,"ARRASTE: ORBITAR · BOTÃO DIREITO: MOVER · RODA: ZOOM",17,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.04f,.05f),new Vector2(.96f,.38f),Vector2.zero,Vector2.zero,FontStyle.Bold);

            var layerControls=UiFactory.HorizontalGroup("Realm Switches",root,new Vector2(.44f,.89f),new Vector2(.72f,.958f),Vector2.zero,Vector2.zero,7f);
            var real=UiFactory.Button("Real World",layerControls,"MUNDO REAL",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SwitchRealm(RealmLayer.RealWorld),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);
            UiFactory.Layout(real.image.rectTransform,54f,105f,1f);
            var broken=UiFactory.Button("Broken Dimension",layerControls,"DIMENSÃO QUEBRADA",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,()=>SwitchRealm(RealmLayer.BrokenDimension),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);
            UiFactory.Layout(broken.image.rectTransform,54f,150f,1f);

            var board=Glass("Pinned Mission Board",root,new Vector2(.745f,.025f),new Vector2(.99f,.985f),PrinceTitanTheme.Magenta,.975f);
            simulationTitleText=UiFactory.Label("Selected Signal",board.transform,"SINAL SELECIONADO",24,PrinceTitanTheme.Magenta,TextAnchor.MiddleLeft,new Vector2(.055f,.90f),new Vector2(.945f,.975f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            simulationDetailText=UiFactory.Label("Selected Signal Detail",board.transform,"Selecione uma missão ou miniatura.",18,PrinceTitanTheme.Ivory,TextAnchor.UpperLeft,new Vector2(.055f,.61f),new Vector2(.945f,.90f),Vector2.zero,Vector2.zero);
            simulationDetailText.resizeTextForBestFit=true;simulationDetailText.resizeTextMinSize=17;simulationDetailText.resizeTextMaxSize=18;simulationDetailText.lineSpacing=.94f;
            UiFactory.Label("Pinned Header",board.transform,"CARTÕES EM MOVIMENTO",18,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.055f,.555f),new Vector2(.945f,.615f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var scroll=UiFactory.Scroll("Mission Pins",board.transform,PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black,.30f),new Vector2(.035f,.20f),new Vector2(.965f,.555f),Vector2.zero,Vector2.zero);
            missionListRoot=scroll.content;

            UiFactory.Button("Pause Simulation",board.transform,"PAUSAR / CONTINUAR",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,ToggleSimulation,new Vector2(.05f,.125f),new Vector2(.48f,.19f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Simulation Speed",board.transform,"VELOCIDADE 1× / 2× / 4× / 8×",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,CycleSimulationSpeed,new Vector2(.51f,.125f),new Vector2(.95f,.19f),Vector2.zero,Vector2.zero,16);
            UiFactory.Button("Reset Relief",board.transform,"VISÃO GERAL",PrinceTitanTheme.Brass,PrinceTitanTheme.Ink,()=>reliefView.ResetView(),new Vector2(.05f,.045f),new Vector2(.48f,.11f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Follow Signal",board.transform,"SEGUIR SINAL",PrinceTitanTheme.InkRaised,PrinceTitanTheme.Ivory,()=>{if(selectedMission!=null)reliefView.FocusMission(selectedMission);},new Vector2(.51f,.045f),new Vector2(.95f,.11f),Vector2.zero,Vector2.zero,17);
            RebuildMissionCards();
        }

        private void RebuildMissionCards()
        {
            if(missionListRoot==null)return;
            UiFactory.ClearChildren(missionListRoot);
            missionButtons.Clear();
            foreach(var mission in project.world.missions.OrderBy(value=>value.realm).ThenBy(value=>value.status).ThenBy(value=>value.durationMinutes-value.elapsedMinutes))
            {
                var rect=UiFactory.Rect("Pin "+mission.id,missionListRoot,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                UiFactory.Layout(rect,102f);
                var paper=rect.gameObject.AddComponent<Image>();
                paper.color=mission.realm==RealmLayer.BrokenDimension?new Color(.34f,.22f,.37f):PrinceTitanTheme.Paper;
                UiFactory.Outline(paper,mission==selectedMission?PrinceTitanTheme.Magenta:PrinceTitanTheme.PaperDark,2f);
                UiFactory.Shadow(paper,new Color(0f,0f,0f,.68f),3f);
                var button=rect.gameObject.AddComponent<Button>();
                button.targetGraphic=paper;
                button.onClick.AddListener(()=>SelectMission(mission,true));
                rect.gameObject.AddComponent<ButtonMotion>();
                var pin=UiFactory.Panel("Map Pin",rect,mission.realm==RealmLayer.BrokenDimension?new Color(.70f,.32f,.86f):PrinceTitanTheme.Magenta,new Vector2(.035f,.76f),new Vector2(.035f,.76f),new Vector2(-6f,-6f),new Vector2(6f,6f));
                pin.raycastTarget=false;
                var label=UiFactory.Label("Mission Text",rect,MissionCardCaption(mission),17,mission.realm==RealmLayer.BrokenDimension?PrinceTitanTheme.Ivory:PrinceTitanTheme.PaperInk,TextAnchor.MiddleLeft,Vector2.zero,Vector2.one,new Vector2(18f,7f),new Vector2(-8f,-7f),FontStyle.Bold);
                label.resizeTextForBestFit=true;label.resizeTextMinSize=16;label.resizeTextMaxSize=18;
                missionButtons[mission.id]=button;
            }
        }

        private string MissionCardCaption(MissionData mission)
        {
            var origin=WorldSeed.Site(project,mission.originSiteId);
            var destination=WorldSeed.Site(project,mission.destinationSiteId);
            return mission.callsign+" · "+simulation.StatusText(mission)+"\n"+(origin==null?"?":origin.name)+"  →  "+(destination==null?"?":destination.name)+"\n"+simulation.EtaText(mission)+" · "+mission.objective;
        }

        private void RefreshMissionBoard()
        {
            if(missionButtons.Count==0)return;
            foreach(var mission in project.world.missions)
            {
                Button button;
                if(missionButtons.TryGetValue(mission.id,out button))UiFactory.SetButtonCaption(button,MissionCardCaption(mission));
            }
        }

        private void SelectMission(MissionData mission, bool focusMap)
        {
            if(mission==null)return;
            selectedMission=mission;
            selectedSite=WorldSeed.Site(project,mission.destinationSiteId);
            simulationShowsSite=false;
            if(focusMap&&reliefView!=null)reliefView.FocusMission(mission);
            RefreshSelectedMission();
        }

        private void SelectSimulationSite(SiteData site)
        {
            if(site==null)return;
            selectedSite=site;
            simulationShowsSite=true;
            RefreshSelectedMission();
        }

        private void RefreshSelectedMission()
        {
            if(simulationTitleText==null||simulationDetailText==null)return;
            if(simulationShowsSite&&selectedSite!=null)
            {
                var organization=WorldSeed.Organization(project,selectedSite.organizationId);
                simulationTitleText.text="LOCAL SELECIONADO · "+RealmLabel(selectedSite.realm);
                simulationDetailText.text=selectedSite.name.ToUpperInvariant()+"\n"+SiteLabel(selectedSite.kind)+" · "+(organization==null?"SEM ORGANIZAÇÃO":organization.name.ToUpperInvariant())+"\nESTADO: "+selectedSite.operationalState+"\n\n"+selectedSite.note;
                return;
            }
            if(selectedMission==null)return;
            var machine=WorldSeed.Machine(project,selectedMission.unitId);
            var origin=WorldSeed.Site(project,selectedMission.originSiteId);
            var destination=WorldSeed.Site(project,selectedMission.destinationSiteId);
            simulationTitleText.text=selectedMission.callsign+" · "+simulation.StatusText(selectedMission);
            simulationDetailText.text=(machine==null?"UNIDADE":machine.name.ToUpperInvariant()+" / "+machine.model)+"\n"+
                (origin==null?"?":origin.name)+"  →  "+(destination==null?"?":destination.name)+"\n"+simulation.EtaText(selectedMission)+" · "+RealmLabel(selectedMission.realm)+"\n"+
                "PARTIDA: DIA "+selectedMission.departureDay.ToString("000")+" · "+ClockFromMinutes(selectedMission.departureMinute)+"\n\nOBJETIVO: "+selectedMission.objective+"\nCARGA: "+selectedMission.cargo+"\nCONTEXTO: "+selectedMission.context;
        }

        private void SwitchRealm(RealmLayer layer)
        {
            project.world.visibleRealm=layer;
            if(reliefView!=null)reliefView.SetRealm(layer,true);
            selectedMission=project.world.missions.FirstOrDefault(mission=>mission.realm==layer&&mission.status==MissionStatus.EnRoute)??project.world.missions.FirstOrDefault(mission=>mission.realm==layer);
            simulationShowsSite=false;
            RefreshSelectedMission();
            MarkDirty();
        }

        private void ToggleSimulation(){simulation.TogglePause();UiFactory.Report(project.world.paused?"SIMULAÇÃO PAUSADA":"SIMULAÇÃO EM CURSO");MarkDirty();}
        private void CycleSimulationSpeed(){simulation.CycleSpeed();UiFactory.Report("VELOCIDADE "+project.world.timeScale.ToString("0")+"×");MarkDirty();}
        private static string ClockFromMinutes(float value){var total=Mathf.FloorToInt(value)%1440;return(total/60).ToString("00")+":"+(total%60).ToString("00");}

        private void BuildWritingScreen()
        {
            var root=NewScreen(PrinceScreen.Writing,"PrinceTitan/Scenes/spy_writer_1944_qhd",Color.white);
            UiFactory.Panel("Writing Shade",root,new Color(.01f,.012f,.01f,.22f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var cabinet=Glass("Steel Chapter Cabinet",root,new Vector2(.012f,.025f),new Vector2(.245f,.975f),PrinceTitanTheme.Brass,.97f);
            UiFactory.Label("Project File Label",cabinet.transform,"ARQUIVO DO PROJETO",18,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.06f,.925f),new Vector2(.94f,.98f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            projectNameInput=UiFactory.Input("Project Name",cabinet.transform,project.projectName,"Nome do projeto",20,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,new Vector2(.05f,.85f),new Vector2(.95f,.925f),Vector2.zero,Vector2.zero,false);
            projectNameInput.onValueChanged.AddListener(value=>{project.projectName=value;MarkDirty();});
            UiFactory.Label("Chapter Files Label",cabinet.transform,"PASTAS DE CAPÍTULOS",18,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.06f,.795f),new Vector2(.94f,.85f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var scroll=UiFactory.Scroll("Chapter Files",cabinet.transform,PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black,.42f),new Vector2(.04f,.35f),new Vector2(.96f,.795f),Vector2.zero,Vector2.zero);
            chapterListRoot=scroll.content;
            UiFactory.Button("New Chapter",cabinet.transform,"+ NOVO CAPÍTULO",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,NewChapter,new Vector2(.05f,.275f),new Vector2(.48f,.335f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Duplicate Chapter",cabinet.transform,"DUPLICAR",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,DuplicateActiveChapter,new Vector2(.52f,.275f),new Vector2(.95f,.335f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Delete Chapter",cabinet.transform,"APAGAR CAPÍTULO",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,OpenDeleteChapterConfirmation,new Vector2(.05f,.205f),new Vector2(.95f,.265f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Save Chapter",cabinet.transform,"ARQUIVAR AGORA",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SaveProject(true),new Vector2(.05f,.135f),new Vector2(.48f,.195f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Export Chapter",cabinet.transform,"EXPORTAR",PrinceTitanTheme.Brass,PrinceTitanTheme.Ink,ExportActiveChapter,new Vector2(.52f,.135f),new Vector2(.95f,.195f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Delete Project",cabinet.transform,"APAGAR PROJETO",PrinceTitanTheme.Black,PrinceTitanTheme.Danger,OpenDeleteProjectConfirmation,new Vector2(.05f,.055f),new Vector2(.48f,.12f),Vector2.zero,Vector2.zero,16);
            UiFactory.Button("Restore Project",cabinet.transform,"RESTAURAR",PrinceTitanTheme.InkRaised,PrinceTitanTheme.Success,RestoreLatestProject,new Vector2(.52f,.055f),new Vector2(.95f,.12f),Vector2.zero,Vector2.zero,16);

            var paper=PaperCard("Classified Manuscript",root,new Vector2(.265f,.085f),new Vector2(.755f,.935f),PrinceTitanTheme.Brass);
            editorPaperRect=paper.rectTransform;
            UiFactory.Label("Classification",paper.transform,"DOCUMENTO OPERACIONAL · CÓPIA LOCAL",17,PrinceTitanTheme.MagentaDark,TextAnchor.MiddleLeft,new Vector2(.05f,.91f),new Vector2(.95f,.965f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            chapterTitleInput=UiFactory.Input("Chapter Title",paper.transform,"","Título do capítulo",27,Color.clear,PrinceTitanTheme.PaperInk,new Vector2(.045f,.82f),new Vector2(.955f,.91f),Vector2.zero,Vector2.zero,false,true);
            chapterBodyInput=UiFactory.Input("Typewriter Manuscript",paper.transform,"","Comece a registrar a cena...",22,Color.clear,PrinceTitanTheme.PaperInk,new Vector2(.045f,.20f),new Vector2(.955f,.81f),Vector2.zero,Vector2.zero,true,true);
            chapterTitleInput.textComponent.font=PrinceTitanTheme.MonoFont;
            chapterBodyInput.textComponent.font=PrinceTitanTheme.MonoFont;
            ((Text)chapterTitleInput.placeholder).font=PrinceTitanTheme.MonoFont;
            ((Text)chapterBodyInput.placeholder).font=PrinceTitanTheme.MonoFont;
            chapterTitleInput.onValueChanged.AddListener(value=>{if(loadingChapter||activeChapter==null)return;activeChapter.title=value;MarkDirty();RebuildChapterList();});
            chapterBodyInput.onValueChanged.AddListener(value=>{if(loadingChapter||activeChapter==null)return;activeChapter.body=value;UpdateWordCount();MarkDirty();});
            wordCountText=UiFactory.Label("Word Counter",paper.transform,"0 PALAVRAS",18,PrinceTitanTheme.MagentaDark,TextAnchor.MiddleRight,new Vector2(.70f,.15f),new Vector2(.95f,.20f),Vector2.zero,Vector2.zero,FontStyle.Bold);

            var context=UiFactory.HorizontalGroup("Scene Context",paper.transform,new Vector2(.045f,.045f),new Vector2(.955f,.14f),Vector2.zero,Vector2.zero,8f);
            var location=UiFactory.Button("Scene Location",context,"LOCAL",PrinceTitanTheme.PaperDark,PrinceTitanTheme.Ivory,CycleChapterLocation,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,16);
            UiFactory.Layout(location.image.rectTransform,58f,100f,1f);
            var pov=UiFactory.Button("Scene POV",context,"PONTO DE VISTA",PrinceTitanTheme.PaperDark,PrinceTitanTheme.Ivory,CycleChapterPointOfView,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,16);
            UiFactory.Layout(pov.image.rectTransform,58f,120f,1f);
            var machine=UiFactory.Button("Scene Machine",context,"MÁQUINA",PrinceTitanTheme.PaperDark,PrinceTitanTheme.Ivory,CycleChapterMachine,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,16);
            UiFactory.Layout(machine.image.rectTransform,58f,100f,1f);

            writingIntelPanel=Glass("Intercepted Telegrams",root,new Vector2(.77f,.085f),new Vector2(.988f,.935f),PrinceTitanTheme.Magenta,.97f).gameObject;
            UiFactory.Label("Telegram Header",writingIntelPanel.transform,"TELEGRAMAS INTERCEPTADOS",22,PrinceTitanTheme.Magenta,TextAnchor.MiddleLeft,new Vector2(.065f,.88f),new Vector2(.935f,.965f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Telegram Sub",writingIntelPanel.transform,"A simulação continua; este painel pode ser recolhido.",17,PrinceTitanTheme.Brass,TextAnchor.UpperLeft,new Vector2(.065f,.80f),new Vector2(.935f,.88f),Vector2.zero,Vector2.zero);
            writingIntelText=UiFactory.Label("Telegram Feed",writingIntelPanel.transform,"SEM SINAL",18,PrinceTitanTheme.Ivory,TextAnchor.UpperLeft,new Vector2(.065f,.20f),new Vector2(.935f,.79f),Vector2.zero,Vector2.zero);
            UiFactory.Button("Open Simulation From Writing",writingIntelPanel.transform,"ABRIR SIMULAÇÃO 3D",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,()=>ShowScreen(PrinceScreen.Simulation),new Vector2(.065f,.10f),new Vector2(.935f,.18f),Vector2.zero,Vector2.zero,17);
            UiFactory.Label("Writing Shortcuts",writingIntelPanel.transform,"CTRL+S SALVA · CTRL+E EXPORTA · CTRL+N NOVO · CTRL+D DUPLICA",16,PrinceTitanTheme.Muted,TextAnchor.MiddleLeft,new Vector2(.065f,.02f),new Vector2(.935f,.09f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Collapse Telegrams",root,"RECOLHER / ABRIR TELEGRAMAS",PrinceTitanTheme.Ink,PrinceTitanTheme.Brass,ToggleWritingIntel,new Vector2(.79f,.945f),new Vector2(.985f,.99f),Vector2.zero,Vector2.zero,15);
            RebuildChapterList();
        }

        private void RebuildChapterList()
        {
            if(chapterListRoot==null)return;
            UiFactory.ClearChildren(chapterListRoot);
            foreach(var chapter in project.chapters.OrderByDescending(value=>value.updatedUnix))
            {
                var selected=activeChapter!=null&&chapter.id==activeChapter.id;
                var updated=chapter.updatedUnix<=0?"SEM DATA":DateTimeOffset.FromUnixTimeSeconds(chapter.updatedUnix).LocalDateTime.ToString("dd/MM · HH:mm");
                var caption=(selected?"◆ ":"")+(string.IsNullOrWhiteSpace(chapter.title)?"Sem título":chapter.title)+"\n"+ProjectStore.CountWords(chapter.body)+" PALAVRAS · "+updated;
                var button=UiFactory.Button("Chapter "+chapter.id,chapterListRoot,caption,selected?PrinceTitanTheme.MagentaDark:PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SelectChapter(chapter),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);
                UiFactory.Layout(button.image.rectTransform,72f);
            }
        }

        private void SelectChapter(ChapterData chapter)
        {
            if(chapter==null)return;
            CaptureActiveChapter();
            activeChapter=chapter;project.activeChapterId=chapter.id;
            LoadActiveChapter();RebuildChapterList();MarkDirty();
        }

        private void LoadActiveChapter()
        {
            if(chapterTitleInput==null||chapterBodyInput==null||activeChapter==null)return;
            loadingChapter=true;chapterTitleInput.text=activeChapter.title??"";chapterBodyInput.text=activeChapter.body??"";loadingChapter=false;UpdateWordCount();UpdateContextButtons();
        }

        private void UpdateWordCount(){if(wordCountText!=null&&activeChapter!=null)wordCountText.text=ProjectStore.CountWords(activeChapter.body).ToString("N0")+" PALAVRAS";}

        private void NewChapter()
        {
            CaptureActiveChapter();
            var chapter=new ChapterData{id=Guid.NewGuid().ToString("N"),title="Novo capítulo "+(project.chapters.Count+1),body="",updatedUnix=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),locationId=activeChapter==null?project.sites[0].id:activeChapter.locationId,pointOfView=activeChapter==null?project.people[0].name:activeChapter.pointOfView,machineId=activeChapter==null?project.machines[0].id:activeChapter.machineId,classification="RELATO DE OPERAÇÃO"};
            project.chapters.Add(chapter);activeChapter=chapter;project.activeChapterId=chapter.id;LoadActiveChapter();RebuildChapterList();MarkDirty();ShowScreen(PrinceScreen.Writing);
        }

        private void DuplicateActiveChapter()
        {
            if(activeChapter==null)return;
            CaptureActiveChapter();
            var copy=new ChapterData{id=Guid.NewGuid().ToString("N"),title=(activeChapter.title??"Sem título")+" — cópia",body=activeChapter.body,updatedUnix=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),locationId=activeChapter.locationId,pointOfView=activeChapter.pointOfView,machineId=activeChapter.machineId,classification=activeChapter.classification};
            project.chapters.Add(copy);activeChapter=copy;project.activeChapterId=copy.id;LoadActiveChapter();RebuildChapterList();MarkDirty();
        }

        private void ExportActiveChapter()
        {
            if(activeChapter==null)return;SaveProject(false);var path=ProjectStore.ExportChapter(project,activeChapter);
            if(saveStateText!=null){saveStateText.text="EXPORTADO: "+System.IO.Path.GetFileName(path);saveStateText.color=PrinceTitanTheme.Brass;}
        }

        private void UpdateWritingIntelligence(){if(writingIntelText!=null)writingIntelText.text=eventLines.Count==0?"SEM NOVOS SINAIS":string.Join("\n\n",eventLines.ToArray());}

        private void ToggleWritingIntel()
        {
            writingIntelCollapsed=!writingIntelCollapsed;
            if(writingIntelPanel!=null)writingIntelPanel.SetActive(!writingIntelCollapsed);
            if(editorPaperRect!=null)editorPaperRect.anchorMax=new Vector2(writingIntelCollapsed?.975f:.755f,.935f);
        }

        private void CycleChapterLocation()
        {
            if(activeChapter==null||project.sites.Count==0)return;
            var index=project.sites.FindIndex(site=>site.id==activeChapter.locationId);index=(index+1)%project.sites.Count;activeChapter.locationId=project.sites[index].id;UpdateContextButtons();MarkDirty();
        }

        private void CycleChapterPointOfView()
        {
            if(activeChapter==null||project.people.Count==0)return;
            var index=project.people.FindIndex(person=>person.name==activeChapter.pointOfView);index=(index+1)%project.people.Count;activeChapter.pointOfView=project.people[index].name;UpdateContextButtons();MarkDirty();
        }

        private void CycleChapterMachine()
        {
            if(activeChapter==null||project.machines.Count==0)return;
            var index=project.machines.FindIndex(machine=>machine.id==activeChapter.machineId);index=(index+1)%project.machines.Count;activeChapter.machineId=project.machines[index].id;UpdateContextButtons();MarkDirty();
        }

        private void UpdateContextButtons()
        {
            if(activeChapter==null||editorPaperRect==null)return;
            var buttons=editorPaperRect.GetComponentsInChildren<Button>(true);
            foreach(var button in buttons)
            {
                if(button.name=="Scene Location")UiFactory.SetButtonCaption(button,"LOCAL\n"+(WorldSeed.Site(project,activeChapter.locationId)?.name??"NÃO DEFINIDO"));
                if(button.name=="Scene POV")UiFactory.SetButtonCaption(button,"PONTO DE VISTA\n"+(activeChapter.pointOfView??"NÃO DEFINIDO"));
                if(button.name=="Scene Machine")UiFactory.SetButtonCaption(button,"MÁQUINA\n"+(WorldSeed.Machine(project,activeChapter.machineId)?.name??"NENHUMA"));
            }
        }

        private void BuildOrganizationScreen()
        {
            var root=NewScreen(PrinceScreen.Organization,"PrinceTitan/Scenes/organization_titan_1944_qhd",Color.white);
            UiFactory.Panel("Organization Shade",root,new Color(.01f,.012f,.01f,.18f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            UiFactory.Label("Organization Title",root,"ORGANIZAÇÃO · FORMAÇÃO TORRE DE TROIA",34,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.025f,.90f),new Vector2(.72f,.985f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Organization Key",root,"CADA PESSOA TEM HABILIDADE, FUNÇÃO E POSIÇÃO DENTRO OU AO REDOR DO TITÃ",18,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.027f,.85f),new Vector2(.78f,.91f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Add Person",root,"+ REGISTRAR PESSOA",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,OpenPersonCreator,new Vector2(.80f,.905f),new Vector2(.98f,.975f),Vector2.zero,Vector2.zero,18);
            var lines=UiFactory.Rect("Operational Connections",root,new Vector2(.035f,.17f),new Vector2(.965f,.86f),Vector2.zero,Vector2.zero);
            lineageGraphic=lines.gameObject.AddComponent<LineageBoardGraphic>();lineageGraphic.Configure(project.people);
            peopleCardRoot=UiFactory.Rect("Operative Files",root,new Vector2(.035f,.17f),new Vector2(.965f,.86f),Vector2.zero,Vector2.zero);
            var detail=Glass("Operative Dossier",root,new Vector2(.13f,.015f),new Vector2(.87f,.18f),PrinceTitanTheme.Magenta,.97f);
            personDetailText=UiFactory.Label("Operative Detail",detail.transform,"Selecione uma pessoa.",18,PrinceTitanTheme.Ivory,TextAnchor.MiddleCenter,new Vector2(.025f,.06f),new Vector2(.975f,.94f),Vector2.zero,Vector2.zero);
            RebuildPeopleCards();
        }

        private void RebuildPeopleCards()
        {
            if(peopleCardRoot==null)return;
            UiFactory.ClearChildren(peopleCardRoot);if(lineageGraphic!=null)lineageGraphic.Configure(project.people);
            foreach(var person in project.people)
            {
                var position=person.treePosition;
                var selected=selectedPerson!=null&&selectedPerson.id==person.id;
                var button=UiFactory.Button("Person "+person.id,peopleCardRoot,person.name.ToUpperInvariant()+"\n"+person.teamRole,selected?PrinceTitanTheme.MagentaDark:PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink,.95f),selected?PrinceTitanTheme.Ivory:PrinceTitanTheme.Brass,()=>SelectPerson(person),position,position,new Vector2(-91f,-42f),new Vector2(91f,42f),17);
                if(selected)UiFactory.Outline(button.image,PrinceTitanTheme.Ivory,2f);
            }
        }

        private void SelectPerson(PersonData person)
        {
            if(person==null)return;selectedPerson=person;
            var organization=WorldSeed.Organization(project,person.organizationId);
            if(personDetailText!=null)personDetailText.text=person.name.ToUpperInvariant()+" · "+person.teamRole+"\n"+person.role.ToUpperInvariant()+" · FASE "+person.progressionPhase+" · "+(organization==null?"SEM ORGANIZAÇÃO":organization.name.ToUpperInvariant())+"\nHABILIDADE: "+person.ability+" · TÉCNICA: "+person.technique+" · ORIGEM: "+person.origin;
        }

        private void BuildArchiveScreen()
        {
            var root=NewScreen(PrinceScreen.Archive,"PrinceTitan/Scenes/war_archive_1944_qhd",Color.white);
            UiFactory.Panel("Archive Shade",root,new Color(.008f,.01f,.008f,.22f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            UiFactory.Label("Archive Heading",root,"ARQUIVO DE GUERRA E GRAVAÇÕES",34,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.02f,.91f),new Vector2(.52f,.985f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var modes=UiFactory.HorizontalGroup("Archive Drawers",root,new Vector2(.49f,.91f),new Vector2(.985f,.98f),Vector2.zero,Vector2.zero,7f);
            AddArchiveMode(modes,"MÁQUINAS",ArchiveMode.Machines);AddArchiveMode(modes,"ORGANIZAÇÕES",ArchiveMode.Organizations);AddArchiveMode(modes,"LUGARES",ArchiveMode.Places);AddArchiveMode(modes,"GRAVAÇÕES",ArchiveMode.Recordings);
            var register=Glass("Archive Register",root,new Vector2(.015f,.055f),new Vector2(.32f,.89f),PrinceTitanTheme.Brass,.96f);
            UiFactory.Label("Register Label",register.transform,"GAVETAS DO ARQUIVO",21,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.055f,.90f),new Vector2(.945f,.98f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var scroll=UiFactory.Scroll("Archive Entries",register.transform,PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Black,.36f),new Vector2(.035f,.11f),new Vector2(.965f,.89f),Vector2.zero,Vector2.zero);archiveListRoot=scroll.content;
            UiFactory.Button("Export War Archive",register.transform,"EXPORTAR ARQUIVO COMPLETO",PrinceTitanTheme.Brass,PrinceTitanTheme.Ink,ExportWorldBook,new Vector2(.045f,.025f),new Vector2(.955f,.095f),Vector2.zero,Vector2.zero,17);

            var observation=Glass("Archive Observation Plate",root,new Vector2(.335f,.075f),new Vector2(.605f,.88f),PrinceTitanTheme.Brass,.985f);
            UiFactory.Label("Observation Header",observation.transform,"CHAPA DE OBSERVAÇÃO",20,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.055f,.91f),new Vector2(.945f,.975f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            archivePreviewImage=UiFactory.Texture("Observation Photograph",observation.transform,"PrinceTitan/Scenes/giant_robot_blueprint_qhd",Color.white,new Vector2(.045f,.18f),new Vector2(.955f,.90f),Vector2.zero,Vector2.zero,true);
            UiFactory.Outline(archivePreviewImage,PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Brass,.86f),2f);
            archivePlateCaptionText=UiFactory.Label("Observation Caption",observation.transform,"PLANTA RECUPERADA · ROBÔ LUTADOR G-44",17,PrinceTitanTheme.Ivory,TextAnchor.UpperLeft,new Vector2(.055f,.035f),new Vector2(.945f,.17f),Vector2.zero,Vector2.zero,FontStyle.Bold);

            var dossier=Glass("Archive Dossier",root,new Vector2(.62f,.075f),new Vector2(.985f,.88f),PrinceTitanTheme.Magenta,.955f);
            archiveTitleText=UiFactory.Label("Archive Title",dossier.transform,"MÁQUINA",28,PrinceTitanTheme.Magenta,TextAnchor.UpperLeft,new Vector2(.06f,.83f),new Vector2(.94f,.96f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            archiveDetailText=UiFactory.Label("Archive Detail",dossier.transform,"Selecione um registro.",18,PrinceTitanTheme.Ivory,TextAnchor.UpperLeft,new Vector2(.06f,.26f),new Vector2(.94f,.83f),Vector2.zero,Vector2.zero);
            archiveDetailText.resizeTextForBestFit=true;archiveDetailText.resizeTextMinSize=17;archiveDetailText.resizeTextMaxSize=18;archiveDetailText.lineSpacing=.94f;
            UiFactory.Button("Damage Head",dossier.transform,"DANO NA CABEÇA",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,()=>DamageSelectedMachine("head"),new Vector2(.06f,.17f),new Vector2(.46f,.24f),Vector2.zero,Vector2.zero,16);
            UiFactory.Button("Damage Cooling",dossier.transform,"DANO NO RESFRIAMENTO",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,()=>DamageSelectedMachine("cooling"),new Vector2(.49f,.17f),new Vector2(.94f,.24f),Vector2.zero,Vector2.zero,15);
            UiFactory.Button("Repair Machine",dossier.transform,"REPARAR / RESTAURAR PLACAS",PrinceTitanTheme.Success,PrinceTitanTheme.Ink,RepairSelectedMachine,new Vector2(.06f,.08f),new Vector2(.63f,.15f),Vector2.zero,Vector2.zero,16);
            UiFactory.Button("Locate Machine",dossier.transform,"LOCALIZAR NO RELEVO",PrinceTitanTheme.Brass,PrinceTitanTheme.Ink,LocateSelectedMachine,new Vector2(.66f,.08f),new Vector2(.94f,.15f),Vector2.zero,Vector2.zero,16);
            RebuildArchiveList();
        }

        private void AddArchiveMode(RectTransform parent,string caption,ArchiveMode mode)
        {
            var button=UiFactory.Button("Archive "+mode,parent,caption,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SetArchiveMode(mode),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);UiFactory.Layout(button.image.rectTransform,54f,95f,1f);
        }

        private void SetArchiveMode(ArchiveMode mode){archiveMode=mode;RebuildArchiveList();}

        private void RebuildArchiveList()
        {
            if(archiveListRoot==null)return;UiFactory.ClearChildren(archiveListRoot);
            if(archiveMode==ArchiveMode.Machines)
            {
                foreach(var machine in project.machines){var button=UiFactory.Button("Machine "+machine.id,archiveListRoot,machine.name.ToUpperInvariant()+"\n"+machine.model,selectedMachine==machine?PrinceTitanTheme.MagentaDark:PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SelectMachine(machine),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);UiFactory.Layout(button.image.rectTransform,70f);}
            }
            else if(archiveMode==ArchiveMode.Organizations)
            {
                foreach(var organization in project.organizations){var button=UiFactory.Button("Organization "+organization.id,archiveListRoot,organization.name.ToUpperInvariant()+"\n"+OrganizationLabel(organization.kind),PrinceTitanTheme.Olive,organization.Color,()=>SelectOrganizationArchive(organization),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);UiFactory.Layout(button.image.rectTransform,72f);}
            }
            else if(archiveMode==ArchiveMode.Places)
            {
                foreach(var site in project.sites){var button=UiFactory.Button("Site "+site.id,archiveListRoot,site.name.ToUpperInvariant()+"\n"+SiteLabel(site.kind)+" · "+RealmLabel(site.realm),PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>SelectSiteArchive(site),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);UiFactory.Layout(button.image.rectTransform,72f);}
            }
            else
            {
                foreach(var recording in project.world.recordings){var button=UiFactory.Button("Recording "+recording.id,archiveListRoot,recording.title.ToUpperInvariant()+"\nDIA "+recording.day.ToString("000")+" · "+recording.location,PrinceTitanTheme.Olive,PrinceTitanTheme.Brass,()=>SelectRecordingArchive(recording),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,17);UiFactory.Layout(button.image.rectTransform,76f);}
            }
        }

        private void SelectMachine(MachineData machine)
        {
            if(machine==null)return;selectedMachine=machine;
            var organization=WorldSeed.Organization(project,machine.organizationId);
            if(archiveTitleText!=null)archiveTitleText.text=machine.name.ToUpperInvariant()+"\n"+machine.model;
            if(archiveDetailText!=null)archiveDetailText.text=UnitLabel(machine.kind)+" · "+(organization==null?"SEM ORGANIZAÇÃO":organization.name.ToUpperInvariant())+"\nESTADO: "+machine.currentState+"\nESCALA: "+(machine.heightMeters>0f?machine.heightMeters.ToString("0.#")+" M":"AERONAVE")+" · "+machine.weightTons.ToString("0.#")+" T\n\nTRIPULAÇÃO: "+machine.crew+"\nCONTROLADORES: "+machine.controllers+"\nSISTEMAS: "+machine.systems+"\n\nDANOS — CABEÇA "+IntegrityLabel(machine.headIntegrity)+" · TORSO "+IntegrityLabel(machine.torsoIntegrity)+" · BRAÇOS "+IntegrityLabel(Mathf.Min(machine.leftArmIntegrity,machine.rightArmIntegrity))+" · PERNAS "+IntegrityLabel(machine.legsIntegrity)+" · RESFRIAMENTO "+IntegrityLabel(machine.coolingIntegrity)+"\n\nGRAVAÇÃO: "+machine.recordingNote;
            SetArchivePlate(MachinePlate(machine),MachinePlateCaption(machine));
        }

        private void SelectOrganizationArchive(OrganizationData organization){if(organization==null)return;if(archiveTitleText!=null)archiveTitleText.text=OrganizationLabel(organization.kind)+"\n"+organization.name.ToUpperInvariant();if(archiveDetailText!=null)archiveDetailText.text="DOUTRINA: "+organization.doctrine+"\n\nTERRITÓRIO: "+organization.territory+"\n\nRECURSOS: "+organization.resources+"\n\nTECNOLOGIA: "+organization.technology;SetArchivePlate(OrganizationPlate(organization),"DOSSIÊ FOTOGRÁFICO · "+organization.name.ToUpperInvariant());}
        private void SelectSiteArchive(SiteData site){if(site==null)return;if(archiveTitleText!=null)archiveTitleText.text=site.name.ToUpperInvariant();if(archiveDetailText!=null)archiveDetailText.text=SiteLabel(site.kind)+" · "+RealmLabel(site.realm)+"\nESTADO: "+site.operationalState+"\n\n"+site.note;selectedSite=site;SetArchivePlate(SitePlate(site),"OBSERVAÇÃO · "+site.name.ToUpperInvariant());}
        private void SelectRecordingArchive(RecordingData recording){if(recording==null)return;var machine=WorldSeed.Machine(project,recording.machineId);if(archiveTitleText!=null)archiveTitleText.text="GRAVAÇÃO RECUPERADA\n"+recording.title.ToUpperInvariant();if(archiveDetailText!=null)archiveDetailText.text="MÁQUINA: "+(machine==null?"DESCONHECIDA":machine.name+" / "+machine.model)+"\nLOCAL: "+recording.location+" · DIA "+recording.day.ToString("000")+"\nBATALHA: "+recording.battle+"\nRECUPERADA POR: "+recording.recoveredBy+"\n\n"+recording.summary;SetArchivePlate(RecordingPlate(recording),"FOTOGRAMA MECÂNICO · "+recording.title.ToUpperInvariant());}

        private void SetArchivePlate(string resourcePath,string caption)
        {
            if(archivePreviewImage!=null)
            {
                archivePreviewImage.texture=Resources.Load<Texture2D>(resourcePath);
                archivePreviewImage.color=archivePreviewImage.texture==null?PrinceTitanTheme.Ink:Color.white;
                var cover=archivePreviewImage.GetComponent<CoverRawImage>();
                if(cover!=null){cover.enabled=false;cover.enabled=true;}
            }
            if(archivePlateCaptionText!=null)archivePlateCaptionText.text=caption;
        }

        private static string MachinePlate(MachineData machine)
        {
            if(machine==null)return"PrinceTitan/Scenes/war_archive_1944_qhd";
            if(machine.kind==UnitKind.ArenaRobot)return"PrinceTitan/Scenes/arena_ring_1944_qhd";
            if(machine.kind==UnitKind.GiantRobot)return"PrinceTitan/Scenes/giant_robot_blueprint_qhd";
            if(machine.kind==UnitKind.Titan)return"PrinceTitan/Scenes/titan_tower_formation_qhd";
            if(machine.kind==UnitKind.ReconFighter||machine.kind==UnitKind.RadialFighter||machine.kind==UnitKind.DiveAircraft)return"PrinceTitan/Scenes/aircraft_hangar_1944_qhd";
            return"PrinceTitan/Scenes/war_archive_1944_qhd";
        }

        private static string MachinePlateCaption(MachineData machine){return machine==null?"REGISTRO SEM CHAPA":"CHAPA TÉCNICA · "+machine.name.ToUpperInvariant()+" / "+machine.model.ToUpperInvariant();}

        private static string OrganizationPlate(OrganizationData organization)
        {
            if(organization!=null&&organization.id=="titan-organization")return"PrinceTitan/Scenes/radio_beetle_network_1944_qhd";
            if(organization!=null&&organization.kind==OrganizationKind.Government)return"PrinceTitan/Scenes/giant_robot_blueprint_qhd";
            if(organization!=null&&organization.kind==OrganizationKind.Empire)return"PrinceTitan/Scenes/aircraft_hangar_1944_qhd";
            if(organization!=null&&organization.kind==OrganizationKind.Clan)return"PrinceTitan/Scenes/organization_titan_1944_qhd";
            return"PrinceTitan/Scenes/war_archive_1944_qhd";
        }

        private static string SitePlate(SiteData site)
        {
            if(site==null)return"PrinceTitan/Scenes/war_archive_1944_qhd";
            if(site.id=="forge-district")return"PrinceTitan/Scenes/nanomancer_forge_awakening_qhd";
            if(site.kind==SiteKind.Arena)return"PrinceTitan/Scenes/arena_ring_1944_qhd";
            if(site.kind==SiteKind.Airfield)return"PrinceTitan/Scenes/aircraft_hangar_1944_qhd";
            if(site.kind==SiteKind.Forest||site.realm==RealmLayer.BrokenDimension)return"PrinceTitan/Scenes/broken_dimension_forest_qhd";
            if(site.kind==SiteKind.Relay)return"PrinceTitan/Scenes/missile_interception_briefing_qhd";
            if(site.kind==SiteKind.Rift)return"PrinceTitan/Scenes/rift_battle_1944_qhd";
            if(site.kind==SiteKind.RobotWorks)return"PrinceTitan/Scenes/giant_robot_blueprint_qhd";
            if(site.id=="titan-route")return"PrinceTitan/Scenes/titan_tower_formation_qhd";
            return"PrinceTitan/Scenes/war_archive_1944_qhd";
        }

        private static string RecordingPlate(RecordingData recording)
        {
            if(recording==null)return"PrinceTitan/Scenes/war_archive_1944_qhd";
            if(recording.id=="rec-ring")return"PrinceTitan/Scenes/arena_ring_1944_qhd";
            if(recording.id=="rec-giant")return"PrinceTitan/Scenes/robot_maintenance_cutaway_qhd";
            if(recording.id=="rec-titan")return"PrinceTitan/Scenes/titan_tower_formation_qhd";
            return"PrinceTitan/Scenes/war_archive_1944_qhd";
        }

        private void DamageSelectedMachine(string part)
        {
            if(selectedMachine==null)return;
            if(part=="head")selectedMachine.headIntegrity=Mathf.Max(0f,selectedMachine.headIntegrity-20f);
            if(part=="cooling")selectedMachine.coolingIntegrity=Mathf.Max(0f,selectedMachine.coolingIntegrity-20f);
            selectedMachine.currentState=selectedMachine.headIntegrity<=0f?"CABEÇA DESTRUÍDA · COMANDO ABDOMINAL ATIVO":selectedMachine.coolingIntegrity<50f?"SUPERAQUECIMENTO / PLACAS SENDO DESCARTADAS":"DANIFICADO";
            SelectMachine(selectedMachine);MarkDirty();
        }

        private void RepairSelectedMachine(){if(selectedMachine==null)return;selectedMachine.headIntegrity=selectedMachine.torsoIntegrity=selectedMachine.leftArmIntegrity=selectedMachine.rightArmIntegrity=selectedMachine.legsIntegrity=selectedMachine.coolingIntegrity=100f;selectedMachine.currentState="REPARADO E OPERACIONAL";SelectMachine(selectedMachine);MarkDirty();}
        private void LocateSelectedMachine(){if(selectedMachine==null)return;var mission=project.world.missions.FirstOrDefault(value=>value.unitId==selectedMachine.id);if(mission!=null){selectedMission=mission;ShowScreen(PrinceScreen.Simulation);StartCoroutine(FocusMissionAfterRoomChange(mission));}}
        private System.Collections.IEnumerator FocusMissionAfterRoomChange(MissionData mission){yield return new WaitForSecondsRealtime(.45f);SelectMission(mission,true);}

        private void ExportWorldBook(){SaveProject(false);var path=ProjectStore.ExportWorldBook(project);if(saveStateText!=null){saveStateText.text="ARQUIVO EXPORTADO: "+System.IO.Path.GetFileName(path);saveStateText.color=PrinceTitanTheme.Brass;}}
    }
}
