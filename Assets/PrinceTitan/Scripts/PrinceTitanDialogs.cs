using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed partial class PrinceTitanApp
    {
        private void BuildSettingsOverlay()
        {
            settingsOverlay = UiFactory.Rect("Settings Overlay", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            UiFactory.Panel("Settings Dim", settingsOverlay.transform, new Color(.015f, .012f, .018f, .91f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            var panel = Glass("Settings Panel", settingsOverlay.transform, new Vector2(.27f, .16f), new Vector2(.73f, .84f), PrinceTitanTheme.Magenta, .98f);
            UiFactory.Label("Settings Title", panel.transform, "CONFORTO E ESCALA", 34, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleLeft, new Vector2(.07f, .84f), new Vector2(.93f, .96f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Settings Copy", panel.transform,
                "A interface nunca depende de fonte minúscula. Escolha o tamanho que fica confortável no seu monitor.",
                20, PrinceTitanTheme.Muted, TextAnchor.UpperLeft,
                new Vector2(.07f, .71f), new Vector2(.93f, .84f), Vector2.zero, Vector2.zero);
            UiFactory.Label("Scale Label", panel.transform, "ESCALA DA INTERFACE", 18, PrinceTitanTheme.Brass,
                TextAnchor.MiddleLeft, new Vector2(.07f, .62f), new Vector2(.93f, .70f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            var scales = UiFactory.HorizontalGroup("Scale Choices", panel.transform, new Vector2(.07f, .48f), new Vector2(.93f, .61f),
                Vector2.zero, Vector2.zero, 9f);
            AddScaleChoice(scales, "100%", 1f);
            AddScaleChoice(scales, "125%", 1.25f);
            AddScaleChoice(scales, "150%", 1.50f);
            AddScaleChoice(scales, "175%", 1.75f);

            var ambientButton = UiFactory.Button("Ambient Sound", panel.transform, "AMBIENTE SONORO: LIGAR / DESLIGAR",
                PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, () =>
                {
                    ambient.Toggle();
                    UiFactory.Report(ambient.IsEnabled ? "AMBIENTE SONORO LIGADO" : "AMBIENTE SONORO DESLIGADO");
                }, new Vector2(.07f, .35f), new Vector2(.93f, .44f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Fullscreen", panel.transform, "JANELA / TELA CHEIA", PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, () =>
                {
                    Screen.fullScreen = !Screen.fullScreen;
                    UiFactory.Report(Screen.fullScreen ? "TELA CHEIA" : "MODO JANELA");
                }, new Vector2(.07f, .24f), new Vector2(.93f, .33f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Label("Resolution", panel.transform,
                "RESOLUÇÃO ATUAL: " + Screen.width + " × " + Screen.height + "\nO enquadramento se adapta sem forçar uma resolução.",
                17, PrinceTitanTheme.Success, TextAnchor.MiddleLeft,
                new Vector2(.07f, .12f), new Vector2(.93f, .23f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Button("Close Settings", panel.transform, "FECHAR", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                () => settingsOverlay.SetActive(false), new Vector2(.30f, .025f), new Vector2(.70f, .105f), Vector2.zero, Vector2.zero, 18);
            settingsOverlay.SetActive(false);
        }

        private void AddScaleChoice(RectTransform parent, string caption, float scale)
        {
            var button = UiFactory.Button("Scale " + caption, parent, caption, PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, () => ApplyUiScale(scale, true), Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, 19);
            UiFactory.Layout(button.image.rectTransform, 66f, 86f, 1f);
        }

        private void ApplyUiScale(float scale, bool announce)
        {
            scale = Mathf.Clamp(scale, 1f, 1.75f);
            if (scaler != null)
                scaler.referenceResolution = new Vector2(1600f / scale, 900f / scale);
            PlayerPrefs.SetFloat("PrinceTitan.UiScale", scale);
            if (announce) UiFactory.Report("ESCALA " + Mathf.RoundToInt(scale * 100f) + "%");
        }

        private void OpenSettings()
        {
            if (settingsOverlay != null) settingsOverlay.SetActive(true);
        }

        private Transform BeginModal(string title, string subtitle)
        {
            UiFactory.ClearChildren(modalRoot);
            modalRoot.gameObject.SetActive(true);
            UiFactory.Panel("Dialog Dim", modalRoot, new Color(.01f, .008f, .012f, .92f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            var panel = Glass("Dialog", modalRoot, new Vector2(.25f, .07f), new Vector2(.75f, .93f), PrinceTitanTheme.Magenta, .99f);
            UiFactory.Label("Dialog Title", panel.transform, title, 34, PrinceTitanTheme.Ivory,
                TextAnchor.MiddleLeft, new Vector2(.07f, .87f), new Vector2(.93f, .97f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Label("Dialog Subtitle", panel.transform, subtitle, 18, PrinceTitanTheme.Brass,
                TextAnchor.UpperLeft, new Vector2(.07f, .78f), new Vector2(.93f, .87f), Vector2.zero, Vector2.zero);
            return panel.transform;
        }

        private void CloseModal()
        {
            if (modalRoot != null) modalRoot.gameObject.SetActive(false);
        }

        private void OpenPersonCreator()
        {
            var panel = BeginModal("NOVA PESSOA", "Registre origem, papel, família e ascendência. Ela aparecerá imediatamente na árvore.");
            var name = LabeledInput(panel, "NOME", "", "Nome completo", .69f, .77f);
            var family = LabeledInput(panel, "FAMÍLIA", "", "Nome da família", .58f, .66f);
            var role = LabeledInput(panel, "PAPEL NO MUNDO", "", "Ex.: Piloto, diplomata, herdeira", .47f, .55f);
            var origin = LabeledInput(panel, "ORIGEM", "", "Cidade, casa ou território", .36f, .44f);
            var birth = LabeledInput(panel, "ANO DE NASCIMENTO", "1935", "Ano", .25f, .33f);

            var factionIndex = 0;
            Button factionButton = null;
            factionButton = UiFactory.Button("Person Faction", panel,
                "ALIANÇA: " + project.factions[factionIndex].name.ToUpperInvariant(), PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                () =>
                {
                    factionIndex = (factionIndex + 1) % project.factions.Count;
                    UiFactory.SetButtonCaption(factionButton, "ALIANÇA: " + project.factions[factionIndex].name.ToUpperInvariant());
                }, new Vector2(.07f, .155f), new Vector2(.93f, .23f), Vector2.zero, Vector2.zero, 18);

            var error = UiFactory.Label("Person Error", panel, "", 17, PrinceTitanTheme.Danger, TextAnchor.MiddleCenter,
                new Vector2(.07f, .105f), new Vector2(.93f, .15f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Button("Cancel Person", panel, "CANCELAR", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                CloseModal, new Vector2(.07f, .025f), new Vector2(.44f, .095f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Create Person", panel, "REGISTRAR PESSOA", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                () =>
                {
                    if (string.IsNullOrWhiteSpace(name.text) || string.IsNullOrWhiteSpace(family.text))
                    {
                        error.text = "NOME E FAMÍLIA SÃO OBRIGATÓRIOS.";
                        return;
                    }
                    int birthYear;
                    if (!int.TryParse(birth.text, out birthYear)) birthYear = 1935;
                    var index = project.people.Count;
                    var column = index % 4;
                    var row = (index / 4) % 3;
                    var person = new PersonData
                    {
                        id = Guid.NewGuid().ToString("N"),
                        name = name.text.Trim(),
                        family = family.text.Trim(),
                        role = string.IsNullOrWhiteSpace(role.text) ? "Papel não definido" : role.text.Trim(),
                        origin = string.IsNullOrWhiteSpace(origin.text) ? "Origem não definida" : origin.text.Trim(),
                        factionId = project.factions[factionIndex].id,
                        birthYear = birthYear,
                        treePosition = new Vector2(.20f + column * .20f, .78f - row * .28f)
                    };
                    project.people.Add(person);
                    selectedPerson = person;
                    MarkDirty();
                    RebuildPeopleCards();
                    SelectPerson(person);
                    CloseModal();
                }, new Vector2(.46f, .025f), new Vector2(.93f, .095f), Vector2.zero, Vector2.zero, 18);
        }

        private void OpenPlaceCreator()
        {
            var panel = BeginModal("NOVO LUGAR", "Crie uma casa, mercado, companhia, aeródromo, porto ou fábrica no Mapa Vivo.");
            var name = LabeledInput(panel, "NOME", "", "Nome do lugar", .67f, .75f);
            var note = LabeledInput(panel, "O QUE ACONTECE AQUI", "", "Uma descrição curta e útil para escrever", .51f, .64f, true);

            var kinds = new[]
            {
                SiteKind.Market, SiteKind.Company, SiteKind.Estate, SiteKind.Airfield,
                SiteKind.RobotWorks, SiteKind.City, SiteKind.Port, SiteKind.Relay
            };
            var kindIndex = 0;
            Button kindButton = null;
            kindButton = UiFactory.Button("Place Kind", panel, "TIPO: " + SiteLabel(kinds[kindIndex]), PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, () =>
                {
                    kindIndex = (kindIndex + 1) % kinds.Length;
                    UiFactory.SetButtonCaption(kindButton, "TIPO: " + SiteLabel(kinds[kindIndex]));
                }, new Vector2(.07f, .39f), new Vector2(.93f, .48f), Vector2.zero, Vector2.zero, 18);

            var factionIndex = 0;
            Button factionButton = null;
            factionButton = UiFactory.Button("Place Faction", panel,
                "PODER: " + project.factions[factionIndex].name.ToUpperInvariant(), PrinceTitanTheme.InkRaised,
                PrinceTitanTheme.Ivory, () =>
                {
                    factionIndex = (factionIndex + 1) % project.factions.Count;
                    UiFactory.SetButtonCaption(factionButton, "PODER: " + project.factions[factionIndex].name.ToUpperInvariant());
                }, new Vector2(.07f, .28f), new Vector2(.93f, .37f), Vector2.zero, Vector2.zero, 18);

            var error = UiFactory.Label("Place Error", panel, "", 17, PrinceTitanTheme.Danger, TextAnchor.MiddleCenter,
                new Vector2(.07f, .17f), new Vector2(.93f, .25f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            UiFactory.Button("Cancel Place", panel, "CANCELAR", PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory,
                CloseModal, new Vector2(.07f, .055f), new Vector2(.44f, .145f), Vector2.zero, Vector2.zero, 18);
            UiFactory.Button("Create Place", panel, "COLOCAR NO MUNDO", PrinceTitanTheme.Magenta, PrinceTitanTheme.Ivory,
                () =>
                {
                    if (string.IsNullOrWhiteSpace(name.text))
                    {
                        error.text = "O LUGAR PRECISA DE UM NOME.";
                        return;
                    }
                    var faction = project.factions[factionIndex];
                    var angle = project.sites.Count * 1.87f;
                    var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (.08f + (project.sites.Count % 3) * .025f);
                    var position = faction.capital + offset;
                    position.x = Mathf.Clamp(position.x, .08f, .90f);
                    position.y = Mathf.Clamp(position.y, .10f, .88f);
                    var site = new SiteData
                    {
                        id = Guid.NewGuid().ToString("N"),
                        name = name.text.Trim(),
                        kind = kinds[kindIndex],
                        factionId = faction.id,
                        position = position,
                        note = string.IsNullOrWhiteSpace(note.text) ? "Ainda não há observações sobre este lugar." : note.text.Trim()
                    };
                    project.sites.Add(site);
                    if (site.kind == SiteKind.Market)
                        project.world.markets.Add(new MarketState { siteId = site.id, activity = 62f, phase = project.world.markets.Count * 1.17f });
                    selectedSite = site;
                    selectedEconomySite = site;
                    MarkDirty();
                    if (worldOverlay != null) worldOverlay.Configure(project);
                    BuildMapMarkers();
                    RebuildEconomyList();
                    SelectEconomySite(site);
                    CloseModal();
                }, new Vector2(.46f, .055f), new Vector2(.93f, .145f), Vector2.zero, Vector2.zero, 18);
        }

        private InputField LabeledInput(Transform parent, string label, string value, string placeholder, float minY, float maxY, bool multiline = false)
        {
            UiFactory.Label(label + " Label", parent, label, 16, PrinceTitanTheme.Brass, TextAnchor.MiddleLeft,
                new Vector2(.07f, maxY), new Vector2(.93f, maxY + .045f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            return UiFactory.Input(label, parent, value, placeholder, multiline ? 18 : 20,
                PrinceTitanTheme.InkRaised, PrinceTitanTheme.Ivory, new Vector2(.07f, minY), new Vector2(.93f, maxY),
                Vector2.zero, Vector2.zero, multiline);
        }
    }
}
