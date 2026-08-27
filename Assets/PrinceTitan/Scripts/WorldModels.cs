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
                return ColorUtility.TryParseHtmlString(colorHex, out parsed) ? parsed : Color.magenta;
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
        public string schema = "prince-titan/1";
        public string projectName = "Prince of Titans";
        public string activeChapterId;
        public List<ChapterData> chapters = new List<ChapterData>();
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
                id = "vesper", name = "Vesper Imperium", shortName = "VESPER", kind = PowerKind.Empire,
                motto = "The crown remembers every road.", colorHex = "#D82B78", capital = new Vector2(.20f, .72f)
            },
            new FactionData
            {
                id = "assembly", name = "Crownless Assembly", shortName = "ASSEMBLY", kind = PowerKind.Government,
                motto = "Order belongs to the living.", colorHex = "#82A9BD", capital = new Vector2(.48f, .80f)
            },
            new FactionData
            {
                id = "emberline", name = "Emberline Clan", shortName = "EMBERLINE", kind = PowerKind.Clan,
                motto = "Kin crosses borders first.", colorHex = "#C89245", capital = new Vector2(.41f, .34f)
            },
            new FactionData
            {
                id = "aurelia", name = "Aurelia Works", shortName = "AURELIA", kind = PowerKind.Contractor,
                motto = "What the age demands, we build.", colorHex = "#6DBFB8", capital = new Vector2(.76f, .61f)
            }
        };

        public static readonly List<SiteData> Sites = new List<SiteData>
        {
            new SiteData { id="asterfall", name="Asterfall", kind=SiteKind.City, factionId="vesper", position=new Vector2(.18f,.73f), note="Imperial capital; ministries beneath rose-stone roofs." },
            new SiteData { id="ferrous", name="Ferrous Yard", kind=SiteKind.RobotWorks, factionId="vesper", position=new Vector2(.30f,.55f), note="Old Titan frames are repaired beside the rail foundries." },
            new SiteData { id="mirador", name="Mirador House", kind=SiteKind.Estate, factionId="vesper", position=new Vector2(.18f,.34f), note="The Veyra family estate and its private archive." },
            new SiteData { id="helion", name="Helion Airfield", kind=SiteKind.Airfield, factionId="assembly", position=new Vector2(.46f,.82f), note="Government reconnaissance wings patrol the northern sunline." },
            new SiteData { id="relay", name="Northern Relay", kind=SiteKind.Relay, factionId="assembly", position=new Vector2(.61f,.85f), note="A listening station connecting ministries and frontier towns." },
            new SiteData { id="glassharbor", name="Glass Harbor", kind=SiteKind.Port, factionId="assembly", position=new Vector2(.86f,.30f), note="Civil docks, customs halls and the eastern ferry market." },
            new SiteData { id="whitenoon", name="White Noon", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.47f,.55f), note="A neutral market where every faction buys under truce." },
            new SiteData { id="saffron", name="Saffron Steps", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.40f,.22f), note="Clan caravans trade medicine, radio parts and family news." },
            new SiteData { id="vale", name="Vale Houses", kind=SiteKind.Estate, factionId="emberline", position=new Vector2(.56f,.36f), note="A terraced neighborhood shared by three old lineages." },
            new SiteData { id="aureliaworks", name="Aurelia Robotics", kind=SiteKind.RobotWorks, factionId="aurelia", position=new Vector2(.77f,.63f), note="Contractor campus building modern Titan support machines." },
            new SiteData { id="exchange", name="Contract Exchange", kind=SiteKind.Company, factionId="aurelia", position=new Vector2(.68f,.34f), note="Brokers track sales, origins, destinations and current owners." },
            new SiteData { id="sunward", name="Sunward Company", kind=SiteKind.Company, factionId="aurelia", position=new Vector2(.82f,.77f), note="Civil aviation, freight insurance and quiet intelligence work." },
            new SiteData { id="lumen", name="Lumen Quarter", kind=SiteKind.City, factionId="assembly", position=new Vector2(.57f,.66f), note="The brightest residential quarter in the interior basin." },
            new SiteData { id="oldbridge", name="Old Bridge Bazaar", kind=SiteKind.Market, factionId="emberline", position=new Vector2(.28f,.25f), note="Family stalls occupy both sides of an abandoned fort bridge." }
        };

        public static readonly List<PersonData> People = new List<PersonData>
        {
            new PersonData { id="lucien", name="Lucien Veyra", family="Veyra", role="Imperial Cartographer", origin="Asterfall", factionId="vesper", birthYear=1881, treePosition=new Vector2(.19f,.82f) },
            new PersonData { id="celine", name="Celine Veyra", family="Veyra", role="Estate Keeper", origin="Mirador House", factionId="vesper", birthYear=1887, treePosition=new Vector2(.39f,.82f) },
            new PersonData { id="orian", name="Orian Veyra", family="Veyra", role="Titan Liaison", origin="Ferrous Yard", factionId="vesper", parentAId="lucien", parentBId="celine", birthYear=1910, treePosition=new Vector2(.25f,.54f) },
            new PersonData { id="sabine", name="Sabine Veyra", family="Veyra", role="Air Intelligence", origin="Helion Airfield", factionId="assembly", parentAId="lucien", parentBId="celine", birthYear=1914, treePosition=new Vector2(.45f,.54f) },
            new PersonData { id="adara", name="Adara Ember", family="Ember", role="Clan Treasurer", origin="Saffron Steps", factionId="emberline", birthYear=1889, treePosition=new Vector2(.63f,.82f) },
            new PersonData { id="matteo", name="Matteo Sol", family="Sol", role="Market Mediator", origin="White Noon", factionId="emberline", birthYear=1885, treePosition=new Vector2(.82f,.82f) },
            new PersonData { id="ines", name="Ines Ember-Sol", family="Ember-Sol", role="Radio Courier", origin="Vale Houses", factionId="emberline", parentAId="adara", parentBId="matteo", birthYear=1912, treePosition=new Vector2(.72f,.54f) },
            new PersonData { id="elias", name="Elias Veyra", family="Veyra", role="Robotics Auditor", origin="Aurelia Works", factionId="aurelia", parentAId="orian", birthYear=1932, treePosition=new Vector2(.19f,.22f) },
            new PersonData { id="maris", name="Maris Veyra", family="Veyra", role="Government Pilot", origin="Lumen Quarter", factionId="assembly", parentAId="sabine", birthYear=1935, treePosition=new Vector2(.42f,.22f) },
            new PersonData { id="noa", name="Noa Ember", family="Ember-Sol", role="Contract Negotiator", origin="Contract Exchange", factionId="aurelia", parentAId="ines", birthYear=1936, treePosition=new Vector2(.69f,.22f) }
        };

        public static ProjectData CreateDefaultProject()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var project = new ProjectData();
            var chapter = new ChapterData
            {
                id = Guid.NewGuid().ToString("N"),
                title = "Chapter 1 — The Foundry Bell",
                body = "The bell above Ferrous Yard rang before sunrise.\n\nNegotit looked up from the half-built hand of a war machine and knew, from the rhythm alone, that somebody had opened the arena.",
                updatedUnix = now
            };
            project.chapters.Add(chapter);
            project.activeChapterId = chapter.id;
            project.world.factions = new List<FactionState>
            {
                new FactionState { factionId="vesper", influence=78f },
                new FactionState { factionId="assembly", influence=69f },
                new FactionState { factionId="emberline", influence=57f },
                new FactionState { factionId="aurelia", influence=64f }
            };
            project.world.markets = Sites.Where(s => s.kind == SiteKind.Market)
                .Select((s, i) => new MarketState { siteId=s.id, activity=58f + i * 8f, phase=i * 1.37f }).ToList();
            project.world.movers = new List<MoverState>
            {
                new MoverState { id="flight-lark", kind=MoverKind.Aircraft, factionId="assembly", fromSiteId="helion", toSiteId="glassharbor", progress=.22f, speed=.025f },
                new MoverState { id="flight-rose", kind=MoverKind.Aircraft, factionId="vesper", fromSiteId="asterfall", toSiteId="sunward", progress=.58f, speed=.018f },
                new MoverState { id="titan-hauler", kind=MoverKind.Robot, factionId="aurelia", fromSiteId="aureliaworks", toSiteId="ferrous", progress=.31f, speed=.008f }
            };
            return project;
        }

        public static FactionData Faction(string id)
        {
            return Factions.FirstOrDefault(f => f.id == id) ?? Factions[0];
        }

        public static SiteData Site(string id)
        {
            return Sites.FirstOrDefault(s => s.id == id) ?? Sites[0];
        }

        public static void ValidateOrThrow()
        {
            if (Factions.Count != 4) throw new InvalidOperationException("Prince Titan requires exactly four map powers.");
            if (Factions.Select(f => f.id).Distinct().Count() != Factions.Count) throw new InvalidOperationException("Faction IDs must be unique.");
            if (Sites.Count < 12 || People.Count < 8) throw new InvalidOperationException("The atlas seed is incomplete.");
            var peopleIds = new HashSet<string>(People.Select(p => p.id));
            foreach (var person in People)
            {
                if (!string.IsNullOrEmpty(person.parentAId) && !peopleIds.Contains(person.parentAId)) throw new InvalidOperationException("Unknown parent: " + person.parentAId);
                if (!string.IsNullOrEmpty(person.parentBId) && !peopleIds.Contains(person.parentBId)) throw new InvalidOperationException("Unknown parent: " + person.parentBId);
            }
        }
    }
}
