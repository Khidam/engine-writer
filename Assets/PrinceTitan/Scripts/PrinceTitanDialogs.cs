using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed partial class PrinceTitanApp
    {
        private void BuildSettingsOverlay()
        {
            settingsOverlay=UiFactory.Rect("Bunker Settings",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            UiFactory.Panel("Settings Blackout",settingsOverlay.transform,new Color(.01f,.012f,.01f,.94f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,true);
            var panel=Glass("Settings Steel Door",settingsOverlay.transform,new Vector2(.25f,.12f),new Vector2(.75f,.88f),PrinceTitanTheme.Magenta,.99f);
            UiFactory.Label("Settings Title",panel.transform,"AJUSTE DO POSTO",32,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.07f,.86f),new Vector2(.93f,.96f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Settings Copy",panel.transform,"Tipografia grande, janela redimensionável e controles adequados ao seu monitor.",19,PrinceTitanTheme.Muted,TextAnchor.UpperLeft,new Vector2(.07f,.76f),new Vector2(.93f,.86f),Vector2.zero,Vector2.zero);
            UiFactory.Label("Scale Label",panel.transform,"ESCALA DA INTERFACE",18,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.07f,.69f),new Vector2(.93f,.76f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            var scales=UiFactory.HorizontalGroup("Scale Choices",panel.transform,new Vector2(.07f,.56f),new Vector2(.93f,.68f),Vector2.zero,Vector2.zero,9f);
            AddScaleChoice(scales,"100%",1f);AddScaleChoice(scales,"125%",1.25f);AddScaleChoice(scales,"150%",1.50f);AddScaleChoice(scales,"175%",1.75f);
            UiFactory.Button("Ambient Sound",panel.transform,"RÁDIO E AMBIENTE: LIGAR / DESLIGAR",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>{ambient.Toggle();UiFactory.Report(ambient.IsEnabled?"AMBIENTE LIGADO":"AMBIENTE DESLIGADO");},new Vector2(.07f,.44f),new Vector2(.93f,.53f),Vector2.zero,Vector2.zero,18);
            UiFactory.Button("Fullscreen",panel.transform,"JANELA / TELA CHEIA",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>{Screen.fullScreen=!Screen.fullScreen;UiFactory.Report(Screen.fullScreen?"TELA CHEIA":"MODO JANELA");},new Vector2(.07f,.33f),new Vector2(.93f,.42f),Vector2.zero,Vector2.zero,18);
            UiFactory.Button("Restore Backup",panel.transform,"RESTAURAR ÚLTIMO BACKUP / PROJETO APAGADO",PrinceTitanTheme.Brass,PrinceTitanTheme.Ink,RestoreLatestProject,new Vector2(.07f,.22f),new Vector2(.93f,.31f),Vector2.zero,Vector2.zero,17);
            UiFactory.Label("Resolution",panel.transform,"RESOLUÇÃO ATUAL: "+Screen.width+" × "+Screen.height+"\nFONTES INTERATIVAS NUNCA MENORES QUE 17 PONTOS",17,PrinceTitanTheme.Success,TextAnchor.MiddleLeft,new Vector2(.07f,.11f),new Vector2(.93f,.21f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Close Settings",panel.transform,"FECHAR",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,()=>settingsOverlay.SetActive(false),new Vector2(.30f,.025f),new Vector2(.70f,.095f),Vector2.zero,Vector2.zero,18);
            settingsOverlay.SetActive(false);
        }

        private void AddScaleChoice(RectTransform parent,string caption,float scale)
        {
            var button=UiFactory.Button("Scale "+caption,parent,caption,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>ApplyUiScale(scale,true),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,19);
            UiFactory.Layout(button.image.rectTransform,66f,86f,1f);
        }

        private void ApplyUiScale(float scale,bool announce)
        {
            scale=Mathf.Clamp(scale,1f,1.75f);
            if(scaler!=null)scaler.referenceResolution=new Vector2(1600f/scale,900f/scale);
            PlayerPrefs.SetFloat("PrinceTitan.UiScale",scale);
            if(announce)UiFactory.Report("ESCALA "+Mathf.RoundToInt(scale*100f)+"%");
        }

        private void OpenSettings(){if(settingsOverlay!=null)settingsOverlay.SetActive(true);}

        private Transform BeginModal(string title,string subtitle)
        {
            UiFactory.ClearChildren(modalRoot);modalRoot.gameObject.SetActive(true);
            UiFactory.Panel("Secure Dialog Blackout",modalRoot,new Color(.005f,.006f,.005f,.94f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,true);
            var panel=Glass("Secure File",modalRoot,new Vector2(.24f,.06f),new Vector2(.76f,.94f),PrinceTitanTheme.Magenta,.995f);
            UiFactory.Label("Dialog Title",panel.transform,title,32,PrinceTitanTheme.Ivory,TextAnchor.MiddleLeft,new Vector2(.065f,.88f),new Vector2(.935f,.97f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Label("Dialog Subtitle",panel.transform,subtitle,18,PrinceTitanTheme.Brass,TextAnchor.UpperLeft,new Vector2(.065f,.78f),new Vector2(.935f,.88f),Vector2.zero,Vector2.zero);
            return panel.transform;
        }

        private void CloseModal(){if(modalRoot!=null)modalRoot.gameObject.SetActive(false);}

        private void OpenPersonCreator()
        {
            var panel=BeginModal("REGISTRAR PESSOA","Origem, habilidade, função de equipe e ascendência entram no arquivo e na formação.");
            var name=LabeledInput(panel,"NOME","","Nome ou título",.69f,.76f);
            var family=LabeledInput(panel,"FAMÍLIA / LINHAGEM","","Família, clã ou origem desconhecida",.59f,.66f);
            var role=LabeledInput(panel,"FUNÇÃO NARRATIVA","","Ex.: líder, engenheiro, artilharia",.49f,.56f);
            var ability=LabeledInput(panel,"HABILIDADE","","Som, nanomancia, gelo, pólvora...",.39f,.46f);
            var origin=LabeledInput(panel,"ORIGEM","","Nação, academia, forja ou dimensão",.29f,.36f);

            var teamRoles=new[]{"ENGINEER","SPY","HEAVY","SOLDADO","SNIPER","PYRO","MEDIC","BATEDORA","DEMOLIDOR"};
            var roleIndex=0;Button teamButton=null;
            teamButton=UiFactory.Button("Team Role",panel,"FORMAÇÃO: "+teamRoles[roleIndex],PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>{roleIndex=(roleIndex+1)%teamRoles.Length;UiFactory.SetButtonCaption(teamButton,"FORMAÇÃO: "+teamRoles[roleIndex]);},new Vector2(.065f,.20f),new Vector2(.47f,.27f),Vector2.zero,Vector2.zero,17);
            var organizationIndex=0;Button organizationButton=null;
            organizationButton=UiFactory.Button("Person Organization",panel,"ORGANIZAÇÃO: "+project.organizations[organizationIndex].shortName,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,()=>{organizationIndex=(organizationIndex+1)%project.organizations.Count;UiFactory.SetButtonCaption(organizationButton,"ORGANIZAÇÃO: "+project.organizations[organizationIndex].shortName);},new Vector2(.50f,.20f),new Vector2(.935f,.27f),Vector2.zero,Vector2.zero,16);

            var parentAIndex=-1;var parentBIndex=-1;Button parentA=null;Button parentB=null;
            parentA=UiFactory.Button("Parent A",panel,"ASCENDENTE A: NÃO REGISTRADO",PrinceTitanTheme.InkRaised,PrinceTitanTheme.Ivory,()=>{parentAIndex=NextOptionalIndex(parentAIndex,project.people.Count);UiFactory.SetButtonCaption(parentA,"ASCENDENTE A: "+(parentAIndex<0?"NÃO REGISTRADO":project.people[parentAIndex].name.ToUpperInvariant()));},new Vector2(.065f,.12f),new Vector2(.47f,.19f),Vector2.zero,Vector2.zero,15);
            parentB=UiFactory.Button("Parent B",panel,"ASCENDENTE B: NÃO REGISTRADO",PrinceTitanTheme.InkRaised,PrinceTitanTheme.Ivory,()=>{parentBIndex=NextOptionalIndex(parentBIndex,project.people.Count);UiFactory.SetButtonCaption(parentB,"ASCENDENTE B: "+(parentBIndex<0?"NÃO REGISTRADO":project.people[parentBIndex].name.ToUpperInvariant()));},new Vector2(.50f,.12f),new Vector2(.935f,.19f),Vector2.zero,Vector2.zero,15);
            var error=UiFactory.Label("Person Error",panel,"",17,PrinceTitanTheme.Danger,TextAnchor.MiddleCenter,new Vector2(.065f,.085f),new Vector2(.935f,.12f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Cancel Person",panel,"CANCELAR",PrinceTitanTheme.InkRaised,PrinceTitanTheme.Ivory,CloseModal,new Vector2(.065f,.02f),new Vector2(.43f,.08f),Vector2.zero,Vector2.zero,17);
            UiFactory.Button("Create Person",panel,"ARQUIVAR PESSOA",PrinceTitanTheme.Magenta,PrinceTitanTheme.Ivory,()=>
            {
                if(string.IsNullOrWhiteSpace(name.text)){error.text="O NOME É OBRIGATÓRIO.";return;}
                var index=project.people.Count;var column=index%4;var row=(index/4)%3;
                var person=new PersonData{id=Guid.NewGuid().ToString("N"),name=name.text.Trim(),family=string.IsNullOrWhiteSpace(family.text)?"Linhagem não registrada":family.text.Trim(),role=string.IsNullOrWhiteSpace(role.text)?"Função não definida":role.text.Trim(),teamRole=teamRoles[roleIndex],ability=string.IsNullOrWhiteSpace(ability.text)?"Habilidade não registrada":ability.text.Trim(),technique="Técnica ainda não descrita",origin=string.IsNullOrWhiteSpace(origin.text)?"Origem não registrada":origin.text.Trim(),organizationId=project.organizations[organizationIndex].id,parentAId=parentAIndex<0?null:project.people[parentAIndex].id,parentBId=parentBIndex<0?null:project.people[parentBIndex].id,progressionPhase=1,treePosition=new Vector2(.18f+column*.21f,.72f-row*.28f)};
                project.people.Add(person);selectedPerson=person;MarkDirty();RebuildPeopleCards();SelectPerson(person);CloseModal();
            },new Vector2(.46f,.02f),new Vector2(.935f,.08f),Vector2.zero,Vector2.zero,17);
        }

        private static int NextOptionalIndex(int current,int count){if(count<=0)return-1;current++;return current>=count?-1:current;}

        private void OpenDeleteChapterConfirmation()
        {
            if(activeChapter==null)return;
            var panel=BeginModal("APAGAR CAPÍTULO","O capítulo será removido do projeto. O backup automático anterior continuará disponível para restauração.");
            UiFactory.Label("Delete Chapter Name",panel,"“"+(activeChapter.title??"Sem título")+"”\n"+ProjectStore.CountWords(activeChapter.body)+" PALAVRAS",24,PrinceTitanTheme.Ivory,TextAnchor.MiddleCenter,new Vector2(.08f,.48f),new Vector2(.92f,.72f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Cancel Delete Chapter",panel,"CANCELAR",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,CloseModal,new Vector2(.08f,.16f),new Vector2(.46f,.26f),Vector2.zero,Vector2.zero,18);
            UiFactory.Button("Confirm Delete Chapter",panel,"SIM, APAGAR CAPÍTULO",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,DeleteActiveChapter,new Vector2(.54f,.16f),new Vector2(.92f,.26f),Vector2.zero,Vector2.zero,18);
        }

        private void DeleteActiveChapter()
        {
            if(activeChapter==null)return;
            SaveProject(false);
            var index=project.chapters.IndexOf(activeChapter);
            project.chapters.Remove(activeChapter);
            if(project.chapters.Count==0)
            {
                project.chapters.Add(new ChapterData{id=Guid.NewGuid().ToString("N"),title="Novo capítulo 1",body="",updatedUnix=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),locationId=project.sites[0].id,pointOfView=project.people[0].name,machineId=project.machines[0].id,classification="RELATO DE OPERAÇÃO"});
            }
            index=Mathf.Clamp(index,0,project.chapters.Count-1);activeChapter=project.chapters[index];project.activeChapterId=activeChapter.id;
            MarkDirty();SaveProject(true);LoadActiveChapter();RebuildChapterList();CloseModal();
        }

        private void OpenDeleteProjectConfirmation()
        {
            var panel=BeginModal("APAGAR PROJETO","Digite APAGAR para retirar o projeto atual. Uma cópia recuperável será colocada em “Deleted Projects”.");
            var confirmation=LabeledInput(panel,"CONFIRMAÇÃO","","APAGAR",.49f,.59f);
            var error=UiFactory.Label("Delete Project Error",panel,"",18,PrinceTitanTheme.Danger,TextAnchor.MiddleCenter,new Vector2(.08f,.38f),new Vector2(.92f,.47f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            UiFactory.Button("Cancel Delete Project",panel,"CANCELAR",PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,CloseModal,new Vector2(.08f,.17f),new Vector2(.46f,.27f),Vector2.zero,Vector2.zero,18);
            UiFactory.Button("Confirm Delete Project",panel,"APAGAR E CRIAR NOVO",PrinceTitanTheme.MagentaDark,PrinceTitanTheme.Ivory,()=>
            {
                if(!string.Equals(confirmation.text.Trim(),"APAGAR",StringComparison.Ordinal)){error.text="DIGITE APAGAR EXATAMENTE.";return;}
                CaptureActiveChapter();ProjectStore.Save(project);ProjectStore.DeleteProjectToTrash();var replacement=WorldSeed.CreateDefaultProject();ProjectStore.Save(replacement);StartCoroutine(RebuildForProject(replacement));
            },new Vector2(.54f,.17f),new Vector2(.92f,.27f),Vector2.zero,Vector2.zero,18);
        }

        private void RestoreLatestProject()
        {
            if(!ProjectStore.RestoreLatestBackup()){UiFactory.Report("NENHUM BACKUP DISPONÍVEL");return;}
            var restored=ProjectStore.LoadOrCreate();StartCoroutine(RebuildForProject(restored));
        }

        private IEnumerator RebuildForProject(ProjectData replacement)
        {
            if(simulation!=null)simulation.EventRaised-=OnWorldEvent;
            if(canvas!=null)Destroy(canvas.gameObject);
            if(reliefView!=null)Destroy(reliefView.gameObject);
            screens.Clear();navigation.Clear();missionButtons.Clear();
            yield return null;
            project=replacement;activeChapter=project.chapters.Find(chapter=>chapter.id==project.activeChapterId)??project.chapters[0];project.activeChapterId=activeChapter.id;
            simulation=new WorldSimulation(project.world);simulation.EventRaised+=OnWorldEvent;
            selectedMission=project.world.missions.Count>0?project.world.missions[0]:null;selectedSite=selectedMission==null?project.sites[0]:WorldSeed.Site(project,selectedMission.destinationSiteId);selectedPerson=project.people[0];selectedMachine=project.machines[0];
            eventLines.Clear();SeedIntelFeed();dirty=false;BuildInterface();ActivateScreen(PrinceScreen.Home);LoadActiveChapter();SelectMission(selectedMission,false);SelectPerson(selectedPerson);SelectMachine(selectedMachine);RefreshDynamic();
        }

        private InputField LabeledInput(Transform parent,string label,string value,string placeholder,float minY,float maxY,bool multiline=false)
        {
            UiFactory.Label(label+" Label",parent,label,17,PrinceTitanTheme.Brass,TextAnchor.MiddleLeft,new Vector2(.065f,maxY),new Vector2(.935f,maxY+.04f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            return UiFactory.Input(label,parent,value,placeholder,multiline?20:21,PrinceTitanTheme.Olive,PrinceTitanTheme.Ivory,new Vector2(.065f,minY),new Vector2(.935f,maxY),Vector2.zero,Vector2.zero,multiline);
        }
    }
}
