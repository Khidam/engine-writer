using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrinceTitan
{
    public enum PowerKind { Empire, Government, Clan, Contractor }
    public enum SiteKind { City, Market, Company, Estate, Airfield, RobotWorks, Port, Relay }
    public enum MoverKind { Aircraft, Robot }

    [Serializable]
    public sealed class ChapterData
    {
        public string id;
        public string title;
        [TextArea(12, 40)] public string body;
        public long updatedUnix;
    }

    [Serializable]
    public sealed class FactionData
    {
        public string id;
        public string name;
        public string shortName;
        public PowerKind kind;
        public string motto;
        public string colorHex;
        public Vector2 capital;

        public Color Color
        {
            get
            {
                Color parsed;
                return ColorUtility.TryParseHtmlString(colorHex, out parsed) ? parsed : PrinceTitanTheme.Magenta;
            }
        }
    }

    [Serializable]
    public sealed class FactionState
    {
        public string factionId;
        [Range(0f, 100f)] public float influence;
    }

    [Serializable]
    public sealed class SiteData
    {
        public string id;
        public string name;
        public SiteKind kind;
        public string factionId;
        public Vector2 position;
        public string note;
    }

    [Serializable]
    public sealed class PersonData
    {
        public string id;
        public string name;
        public string family;
        public string role;
        public string origin;
        public string factionId;
        public string parentAId;
        public string parentBId;
        public int birthYear;
        public Vector2 treePosition;
    }

    [Serializable]
    public sealed class MarketState
    {
        public string siteId;
        [Range(0f, 100f)] public float activity;
        public float phase;
    }

    [Serializable]
    public sealed class MoverState
    {
        public string id;
        public MoverKind kind;
        public string factionId;
        public string fromSiteId;
        public string toSiteId;
        [Range(0f, 1f)] public float progress;
        public float speed;
        public bool forward = true;
    }

    [Serializable]
    public sealed class WorldState
    {
        public int day = 128;
        public float minuteOfDay = 510f;
        public bool paused;
        public float timeScale = 1f;
        public List<FactionState> factions = new List<FactionState>();
        public List<MarketState> markets = new List<MarketState>();
        public List<MoverState> movers = new List<MoverState>();
    }

    [Serializable]
    public sealed class ProjectData
    {
        public string schema = "prince-titan/2";
        public string projectName = "Príncipe dos Titãs";
        public string activeChapterId;
        public List<ChapterData> chapters = new List<ChapterData>();
        public List<FactionData> factions = new List<FactionData>();
        public List<SiteData> sites = new List<SiteData>();
        public List<PersonData> people = new List<PersonData>();
        public WorldState world = new WorldState();
    }

    public sealed class WorldEvent
    {
        public readonly string title;
        public readonly string detail;
        public readonly string factionId;

        public WorldEvent(string title, string detail, string factionId)
        {
            this.title = title;
            this.detail = detail;
            this.factionId = factionId;
        }
    }

    public static class WorldSeed
    {
        public static readonly List<FactionData> Factions = new List<FactionData>
        {
            new FactionData
            {
                id = "vesper", name = "Império Vesper", shortName = "VESPER", kind = PowerKind.Empire,
                motto = "A coroa recorda cada estrada.", colorHex = "#E22A82", capital = new Vector2(.20f, .72f)
            },
            new FactionData
            {
                id = "assembly", name = "Governo da Assembleia", shortName = "ASSEMBLEIA", kind = PowerKind.Government,
                motto = "A ordem pertence aos vivos.", colorHex = "#B8D8E7", capital = new Vector2(.48f, .80f)
            },
            new FactionData
            {
                id = "emberline", name = "Clã Emberline", shortName = "EMBERLINE", kind = PowerKind.Clan,
                motto = "O sangue atravessa a fronteira primeiro.", colorHex = "#E0A34D", capital = new Vector2(.41f, .34f)
            },
            new FactionData
            {
                id = "aurelia", name = "Aurelia Contratos", shortName = "AURELIA", kind = PowerKind.Contractor,
                motto = "Construímos o que a era exige.", colorHex = "#55C9C2", capital = new Vector2(.76f, .61f)
            }
        };

        public static readonly List<SiteData> Sites = new List<SiteData>
        {
            new SiteData { id="asterfall", name="Asterfall", kind=SiteKind.City, factionId="vesper", position=new Vector2(.18f,.73f), note="Capital imperial; ministérios sob telhados de pedra rosada." },
            new SiteData { id="ferrous", name="Pátio Ferrous", kind=SiteKind.RobotWorks, factionId="vesper", position=new Vector2(.30f,.55f), note="Estruturas de Titãs são reconstruídas junto às fundições ferroviárias." },
            new SiteData { id="mirador", name="Casa Mirador", kind=SiteKind.Estate, factionId="vesper", position=new Vector2(.18f,.34f), note="Propriedade da família Veyra e sede do seu arquivo privado." },
            new SiteData { id="helion", name="Aeródromo Helion", kind=SiteKind.Airfield, factionId="assembly", position=new Vector2(.46f,.82f), note="Asas de reconhecimento do Governo patrulham a linha do sol." },
            new SiteData { id="relay", name="Relé do Norte", kind=SiteKind.Relay, factionId="assembly", position=new Vector2(.61f,.85f), note="Estação de escuta entre ministérios e cidades de fronteira." },
            new SiteData { id="glassharbor", name="Porto de Vidro", kind=SiteKind.Port, factionId="assembly", position=new Vector2(.86f,.30f), note="Docas civis, alfândega e mercado de balsas do leste." },
            new SiteData { id="whitenoon", name="Meio-Dia Branco", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.47f,.55f), note="Mercado neutro onde os quatro poderes compram sob trégua." },
            new SiteData { id="saffron", name="Degraus de Açafrão", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.40f,.22f), note="Caravanas trocam remédios, rádios e notícias de família." },
            new SiteData { id="vale", name="Casas do Vale", kind=SiteKind.Estate, factionId="emberline", position=new Vector2(.56f,.36f), note="Bairro em terraços partilhado por três linhagens antigas." },
            new SiteData { id="aureliaworks", name="Robótica Aurelia", kind=SiteKind.RobotWorks, factionId="aurelia", position=new Vector2(.77f,.63f), note="Complexo que constrói máquinas de apoio Titan." },
            new SiteData { id="exchange", name="Bolsa de Contratos", kind=SiteKind.Company, factionId="aurelia", position=new Vector2(.68f,.34f), note="Corretores rastreiam vendas, origens, destinos e proprietários." },
            new SiteData { id="sunward", name="Companhia Sunward", kind=SiteKind.Company, factionId="aurelia", position=new Vector2(.82f,.77f), note="Aviação civil, seguro de carga e inteligência discreta." },
            new SiteData { id="lumen", name="Bairro Lumen", kind=SiteKind.City, factionId="assembly", position=new Vector2(.57f,.66f), note="O bairro residencial mais luminoso da bacia interior." },
            new SiteData { id="oldbridge", name="Mercado Ponte Velha", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.28f,.25f), note="Bancas familiares ocupam os dois lados de uma ponte-fortaleza." }
        };

        public static readonly List<PersonData> People = new List<PersonData>
        {
            new PersonData { id="lucien", name="Lucien Veyra", family="Veyra", role="Cartógrafo Imperial", origin="Asterfall", factionId="vesper", birthYear=1881, treePosition=new Vector2(.22f,.79f) },
            new PersonData { id="celine", name="Celine Veyra", family="Veyra", role="Guardiã da Propriedade", origin="Casa Mirador", factionId="vesper", birthYear=1887, treePosition=new Vector2(.40f,.79f) },
            new PersonData { id="orian", name="Orian Veyra", family="Veyra", role="Emissário dos Titãs", origin="Pátio Ferrous", factionId="vesper", parentAId="lucien", parentBId="celine", birthYear=1910, treePosition=new Vector2(.25f,.51f) },
            new PersonData { id="sabine", name="Sabine Veyra", family="Veyra", role="Inteligência Aérea", origin="Aeródromo Helion", factionId="assembly", parentAId="lucien", parentBId="celine", birthYear=1914, treePosition=new Vector2(.45f,.51f) },
            new PersonData { id="adara", name="Adara Ember", family="Ember", role="Tesoureira do Clã", origin="Degraus de Açafrão", factionId="emberline", birthYear=1889, treePosition=new Vector2(.63f,.79f) },
            new PersonData { id="matteo", name="Matteo Sol", family="Sol", role="Mediador de Mercado", origin="Meio-Dia Branco", factionId="emberline", birthYear=1885, treePosition=new Vector2(.80f,.79f) },
            new PersonData { id="ines", name="Ines Ember-Sol", family="Ember-Sol", role="Mensageira de Rádio", origin="Casas do Vale", factionId="emberline", parentAId="adara", parentBId="matteo", birthYear=1912, treePosition=new Vector2(.70f,.51f) },
            new PersonData { id="elias", name="Elias Veyra", family="Veyra", role="Auditor de Robótica", origin="Robótica Aurelia", factionId="aurelia", parentAId="orian", birthYear=1932, treePosition=new Vector2(.24f,.22f) },
            new PersonData { id="maris", name="Maris Veyra", family="Veyra", role="Piloto do Governo", origin="Bairro Lumen", factionId="assembly", parentAId="sabine", birthYear=1935, treePosition=new Vector2(.44f,.22f) },
            new PersonData { id="noa", name="Noa Ember", family="Ember-Sol", role="Negociador de Contratos", origin="Bolsa de Contratos", factionId="aurelia", parentAId="ines", birthYear=1936, treePosition=new Vector2(.68f,.22f) }
        };

        public static ProjectData CreateDefaultProject()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var project = new ProjectData();
            var chapter = new ChapterData
            {
                id = Guid.NewGuid().ToString("N"),
                title = "Capítulo 1 — O Sino da Fundição",
                body = "O sino acima do Pátio Ferrous tocou antes do amanhecer.\n\nNegotit ergueu os olhos da mão incompleta de uma máquina colossal. Pelo ritmo, soube que alguém acabara de abrir a arena.",
                updatedUnix = now
            };
            project.chapters.Add(chapter);
            project.activeChapterId = chapter.id;
            project.factions = CloneFactions();
            project.sites = CloneSites();
            project.people = ClonePeople();
            project.world.factions = new List<FactionState>
            {
                new FactionState { factionId="vesper", influence=78f },
                new FactionState { factionId="assembly", influence=69f },
                new FactionState { factionId="emberline", influence=57f },
                new FactionState { factionId="aurelia", influence=64f }
            };
            project.world.markets = project.sites.Where(s => s.kind == SiteKind.Market)
                .Select((s, i) => new MarketState { siteId=s.id, activity=58f + i * 8f, phase=i * 1.37f }).ToList();
            project.world.movers = new List<MoverState>
            {
                new MoverState { id="flight-lark", kind=MoverKind.Aircraft, factionId="assembly", fromSiteId="helion", toSiteId="glassharbor", progress=.22f, speed=.025f },
                new MoverState { id="flight-rose", kind=MoverKind.Aircraft, factionId="vesper", fromSiteId="asterfall", toSiteId="sunward", progress=.58f, speed=.018f },
                new MoverState { id="titan-hauler", kind=MoverKind.Robot, factionId="aurelia", fromSiteId="aureliaworks", toSiteId="ferrous", progress=.31f, speed=.008f }
            };
            return project;
        }

        public static List<FactionData> CloneFactions()
        {
            return Factions.Select(f => new FactionData
            {
                id=f.id, name=f.name, shortName=f.shortName, kind=f.kind, motto=f.motto,
                colorHex=f.colorHex, capital=f.capital
            }).ToList();
        }

        public static List<SiteData> CloneSites()
        {
            return Sites.Select(s => new SiteData
            {
                id=s.id, name=s.name, kind=s.kind, factionId=s.factionId, position=s.position, note=s.note
            }).ToList();
        }

        public static List<PersonData> ClonePeople()
        {
            return People.Select(p => new PersonData
            {
                id=p.id, name=p.name, family=p.family, role=p.role, origin=p.origin,
                factionId=p.factionId, parentAId=p.parentAId, parentBId=p.parentBId,
                birthYear=p.birthYear, treePosition=p.treePosition
            }).ToList();
        }

        public static FactionData Faction(ProjectData project, string id)
        {
            var factions = project != null && project.factions != null && project.factions.Count > 0 ? project.factions : Factions;
            return factions.FirstOrDefault(f => f.id == id) ?? factions[0];
        }

        public static SiteData Site(ProjectData project, string id)
        {
            var sites = project != null && project.sites != null && project.sites.Count > 0 ? project.sites : Sites;
            return sites.FirstOrDefault(s => s.id == id) ?? sites[0];
        }

        public static void ValidateOrThrow()
        {
            if (Factions.Count != 4) throw new InvalidOperationException("Prince Titan precisa de quatro poderes principais.");
            if (Factions.Select(f => f.id).Distinct().Count() != Factions.Count) throw new InvalidOperationException("Os códigos dos poderes precisam ser únicos.");
            if (Sites.Count < 12 || People.Count < 8) throw new InvalidOperationException("A base do Mapa Vivo está incompleta.");
            var peopleIds = new HashSet<string>(People.Select(p => p.id));
            foreach (var person in People)
            {
                if (!string.IsNullOrEmpty(person.parentAId) && !peopleIds.Contains(person.parentAId)) throw new InvalidOperationException("Ascendente desconhecido: " + person.parentAId);
                if (!string.IsNullOrEmpty(person.parentBId) && !peopleIds.Contains(person.parentBId)) throw new InvalidOperationException("Ascendente desconhecido: " + person.parentBId);
            }
        }
    }
}
