using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrinceTitan
{
    public enum OrganizationKind { Empire, Government, Clan, Contractor }
    public enum SiteKind { Capital, Settlement, Estate, Airfield, RobotWorks, Arena, Port, Relay, Rift, Forest, Depot, Academy }
    public enum RealmLayer { RealWorld, BrokenDimension }
    public enum UnitKind { ReconFighter, RadialFighter, DiveAircraft, CargoRobot, ArenaRobot, GiantRobot, Titan }
    public enum MissionKind { Reconnaissance, Transfer, Interception, Recovery, Sabotage, Assault, Patrol, DimensionalReturn }
    public enum MissionStatus { Planned, EnRoute, Interrupted, Arrived, Missing, Completed }

    [Serializable]
    public sealed class ChapterData
    {
        public string id;
        public string title;
        [TextArea(12, 40)] public string body;
        public long updatedUnix;
        public string locationId;
        public string pointOfView;
        public string machineId;
        public string classification = "RELATO DE OPERAÇÃO";
    }

    [Serializable]
    public sealed class OrganizationData
    {
        public string id;
        public string name;
        public string shortName;
        public OrganizationKind kind;
        public string doctrine;
        public string territory;
        public string resources;
        public string technology;
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
    public sealed class SiteData
    {
        public string id;
        public string name;
        public SiteKind kind;
        public string organizationId;
        public Vector2 position;
        public string note;
        public string operationalState;
        public RealmLayer realm;
    }

    [Serializable]
    public sealed class PersonData
    {
        public string id;
        public string name;
        public string family;
        public string role;
        public string teamRole;
        public string ability;
        public string technique;
        public string origin;
        public string organizationId;
        public string parentAId;
        public string parentBId;
        public int birthYear;
        public int progressionPhase = 1;
        public Vector2 treePosition;
    }

    [Serializable]
    public sealed class MachineData
    {
        public string id;
        public string name;
        public string model;
        public UnitKind kind;
        public string organizationId;
        public string homeSiteId;
        public float heightMeters;
        public float weightTons;
        public string crew;
        public string controllers;
        public string systems;
        public string currentState;
        public string recordingNote;
        public float headIntegrity = 100f;
        public float torsoIntegrity = 100f;
        public float leftArmIntegrity = 100f;
        public float rightArmIntegrity = 100f;
        public float legsIntegrity = 100f;
        public float coolingIntegrity = 100f;
        public RealmLayer realm;
    }

    [Serializable]
    public sealed class MissionData
    {
        public string id;
        public string title;
        public string callsign;
        public MissionKind kind;
        public MissionStatus status;
        public string unitId;
        public string originSiteId;
        public string destinationSiteId;
        public int departureDay;
        public float departureMinute;
        public float durationMinutes;
        public float elapsedMinutes;
        public float altitudeMeters;
        public RealmLayer realm;
        public string objective;
        public string cargo;
        public string context;
        public string consequence;
        public bool pinned = true;

        public float Progress { get { return durationMinutes <= .01f ? 1f : Mathf.Clamp01(elapsedMinutes / durationMinutes); } }
    }

    [Serializable]
    public sealed class IntelEventData
    {
        public string id;
        public string title;
        public string detail;
        public string missionId;
        public string siteId;
        public int day;
        public float minuteOfDay;
        public RealmLayer realm;
    }

    [Serializable]
    public sealed class RecordingData
    {
        public string id;
        public string machineId;
        public string title;
        public string location;
        public string battle;
        public string recoveredBy;
        public string summary;
        public int day;
    }

    [Serializable]
    public sealed class WorldState
    {
        public int day = 133;
        public float minuteOfDay = 423f;
        public bool paused;
        public float timeScale = 1f;
        public RealmLayer visibleRealm = RealmLayer.RealWorld;
        public List<MissionData> missions = new List<MissionData>();
        public List<IntelEventData> eventHistory = new List<IntelEventData>();
        public List<RecordingData> recordings = new List<RecordingData>();
    }

    [Serializable]
    public sealed class ProjectData
    {
        public string schema = "prince-titan/3";
        public string projectName = "Prince of Titans";
        public string activeChapterId;
        public List<ChapterData> chapters = new List<ChapterData>();
        public List<OrganizationData> organizations = new List<OrganizationData>();
        public List<SiteData> sites = new List<SiteData>();
        public List<PersonData> people = new List<PersonData>();
        public List<MachineData> machines = new List<MachineData>();
        public WorldState world = new WorldState();
    }

    public sealed class WorldEvent
    {
        public readonly string title;
        public readonly string detail;
        public readonly string missionId;
        public readonly string siteId;
        public readonly RealmLayer realm;

        public WorldEvent(string title, string detail, string missionId, string siteId, RealmLayer realm)
        {
            this.title = title;
            this.detail = detail;
            this.missionId = missionId;
            this.siteId = siteId;
            this.realm = realm;
        }
    }

    public static class WorldSeed
    {
        public static ProjectData CreateDefaultProject()
        {
            var project = new ProjectData();
            var chapter = new ChapterData
            {
                id = Guid.NewGuid().ToString("N"),
                title = "Capítulo 1 — O ringue dos soldados",
                body = "O metal batia no outro lado da parede da forja. Cada pancada vinha do ringue onde os soldados apostavam nos robôs de duas pernas.\n\nO garoto continuou martelando a peça, mas já havia decorado o ritmo das engrenagens.",
                updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), locationId = "forge-district",
                pointOfView = "Príncipe dos Titãs", machineId = "arena-half-ton", classification = "GRAVAÇÃO DE ORIGEM"
            };
            project.chapters.Add(chapter);
            project.activeChapterId = chapter.id;
            project.organizations = CreateOrganizations();
            project.sites = CreateSites();
            project.people = CreatePeople();
            project.machines = CreateMachines();
            project.world.missions = CreateMissions();
            project.world.recordings = CreateRecordings();
            project.world.eventHistory = new List<IntelEventData>();
            return project;
        }

        public static List<OrganizationData> CreateOrganizations()
        {
            return new List<OrganizationData>
            {
                new OrganizationData { id="bellica-government", name="Nação Bélica — Governo", shortName="NAÇÃO BÉLICA", kind=OrganizationKind.Government, doctrine="Quantidade, disciplina de regimento, engenharia pública e retirada dimensional quando a batalha é impossível.", territory="Centros industriais, quartéis, aeródromos e oficinas de protótipos.", resources="Soldados treinados, radares, peças padronizadas, mísseis de curto alcance e os melhores engenheiros.", technology="Robôs de vários controladores, gravação interna e protótipos deliberadamente escondidos.", colorHex="#DDE7E8", capital=new Vector2(.30f,.65f) },
                new OrganizationData { id="unnamed-empire", name="Império — nome a definir", shortName="IMPÉRIO", kind=OrganizationKind.Empire, doctrine="Mantém uma nação por regimentos, linhagem política e máquinas de guerra próprias.", territory="Capital murada e corredores ferroviários do oeste.", resources="Tesouro em prata, aviação de reconhecimento e grandes hangares.", technology="Blindagem pesada, cabines abdominais e sistemas de apoio contra mísseis.", colorHex="#E52B86", capital=new Vector2(.17f,.74f) },
                new OrganizationData { id="unnamed-clan", name="Clã abastado — nome a definir", shortName="CLÃ", kind=OrganizationKind.Clan, doctrine="Treina crianças desde cedo, domina artes raras e evita depender de tecnologia pública.", territory="Academias, propriedades familiares e montanhas do norte.", resources="Fortunas familiares, técnicas secretas, professores e herdeiros despertos.", technology="Compra carcaças e plantas antigas enquanto aperfeiçoa habilidades pessoais.", colorHex="#D8AA62", capital=new Vector2(.61f,.78f) },
                new OrganizationData { id="unnamed-contractor", name="Empreiteira — nome a definir", shortName="EMPREITEIRA", kind=OrganizationKind.Contractor, doctrine="Vende transporte, manutenção, informação e máquinas para qualquer lado capaz de pagar.", territory="Portos, depósitos de carga e oficinas móveis do sudeste.", resources="Contratos, robôs de carga, rotas comerciais e acesso a carcaças.", technology="Reaproveitamento rápido, radiadores modulares e peças intercambiáveis.", colorHex="#5EC8C1", capital=new Vector2(.76f,.34f) },
                new OrganizationData { id="titan-organization", name="Organização do Príncipe", shortName="ORGANIZAÇÃO", kind=OrganizationKind.Contractor, doctrine="Invadir, proteger o grupo dentro do Titã, roubar plantas e sair antes que a nação reorganize as defesas.", territory="Nenhum território fixo; usa o Titã como fortaleza ambulante.", resources="Nanomáquinas, escuta, gelo vivo, fogo-magma, pólvora, latão e eletricidade.", technology="Formação Torre de Troia, besouros-rádio, sabotagem interna e fuga pela Dimensão Quebrada.", colorHex="#8BE0B3", capital=new Vector2(.46f,.43f) }
            };
        }

        public static List<SiteData> CreateSites()
        {
            return new List<SiteData>
            {
                Site("forge-district", "Distrito das Forjas", SiteKind.RobotWorks, "bellica-government", .23f,.58f, "O protagonista fabrica peças e estuda plantas roubadas enquanto robôs são montados para o regimento.", "FORJA ATIVA", RealmLayer.RealWorld),
                Site("soldier-ring", "Ringue dos Soldados", SiteKind.Arena, "bellica-government", .31f,.53f, "Robôs de 2,3 metros e meia tonelada lutam diante dos soldados que apostam toda semana.", "COMBATE DE TESTE", RealmLayer.RealWorld),
                Site("bellica-airfield", "Aeródromo da Nação Bélica", SiteKind.Airfield, "bellica-government", .36f,.76f, "Caças e aeronaves de mergulho de 1944 patrulham as rotas de mísseis e as fraturas.", "ESQUADRILHA EM ALERTA", RealmLayer.RealWorld),
                Site("government-prototype", "Oficina de Protótipos do Governo", SiteKind.RobotWorks, "bellica-government", .43f,.66f, "Os melhores engenheiros escondem modelos atuais e permitem que compradores levem apenas carcaças antigas.", "ACESSO RESTRITO", RealmLayer.RealWorld),
                Site("clan-academy", "Academia dos Clãs", SiteKind.Academy, "unnamed-clan", .63f,.78f, "Filhos de famílias ricas treinam suas habilidades e técnicas desde cedo.", "TREINAMENTO", RealmLayer.RealWorld),
                Site("empire-capital", "Capital Imperial", SiteKind.Capital, "unnamed-empire", .14f,.77f, "Centro de comando de uma nação governada pelo Império.", "TOQUE DE RECOLHER", RealmLayer.RealWorld),
                Site("contractor-depot", "Depósito de Carga", SiteKind.Depot, "unnamed-contractor", .74f,.35f, "Robôs comuns descarregam materiais, sucata e placas compradas de várias nações.", "CARGA EM TRÂNSITO", RealmLayer.RealWorld),
                Site("eastern-port", "Porto das Empreiteiras", SiteKind.Port, "unnamed-contractor", .84f,.27f, "Contratos e tecnologia atravessam o continente em caixas sem identificação.", "COMBOIO AGUARDANDO", RealmLayer.RealWorld),
                Site("missile-field", "Campo de Interceptação", SiteKind.Relay, "bellica-government", .58f,.52f, "Radares e portadores de vento interceptam mísseis antes que alcancem os robôs.", "RADAR OPERACIONAL", RealmLayer.RealWorld),
                Site("rift-east", "Fratura Dimensional Leste", SiteKind.Rift, "titan-organization", .72f,.62f, "Uma quebra instável por onde máquinas e pessoas podem fugir ou retornar ao mundo real.", "JANELA INSTÁVEL", RealmLayer.RealWorld),
                Site("titan-route", "Rota da Fortaleza Ambulante", SiteKind.Settlement, "titan-organization", .48f,.42f, "Trajeto atual do Titã que transporta a organização em formação Torre de Troia.", "MOVIMENTO OCULTO", RealmLayer.RealWorld),
                Site("broken-forest", "Floresta Reclamada", SiteKind.Forest, "titan-organization", .24f,.31f, "Na Dimensão Quebrada, estradas humanas voltaram a ser floresta e alongaram o retorno por meses ou anos.", "ROTA PERDIDA", RealmLayer.BrokenDimension),
                Site("broken-rift", "Quebra de Retorno", SiteKind.Rift, "titan-organization", .79f,.69f, "A única ruptura conhecida nesta região que devolve viajantes ao mundo real.", "SINAL FRACO", RealmLayer.BrokenDimension)
            };
        }

        public static List<PersonData> CreatePeople()
        {
            return new List<PersonData>
            {
                Person("prince", "Príncipe dos Titãs", "Sem família conhecida", "Nanomante e engenheiro", "ENGINEER", "Nanomáquinas verde-escuras", "Invade sistemas, copia plantas, cria chips e melhora o Titã.", "Distrito das Forjas", "titan-organization", .18f,.72f),
                Person("whisper", "Sussurro Fantasma", "Linhagem desconhecida", "Líder e inteligência", "SPY", "Controle absoluto do som", "Ecolocalização, boom sônico, sabotagem e rede de besouros por rádio.", "Origem não registrada", "titan-organization", .40f,.72f),
                Person("brass", "Imperador Latão", "Casa do Latão", "Tesoureiro e combatente", "DEMOLIDOR", "Círculos e vigas de latão", "Cria pontos de teleporte e enfrenta vários inimigos protegendo o grupo.", "Império", "titan-organization", .62f,.72f),
                Person("electric", "Imperatriz Bandida Elétrica", "Casa do Latão", "Ladra de recursos", "BATEDORA", "Eletricidade e teleporte pelo latão", "Rouba prata, plantas e recursos através dos círculos do Imperador.", "Império", "titan-organization", .82f,.72f),
                Person("infernal", "Rei Dragão Infernal", "Linhagem do Dragão", "Artilharia de longa distância", "SOLDADO", "Fogo com terra derretida", "Ataca coordenadas marcadas pela Sussurro com fogo semelhante a magma.", "Nação não definida", "titan-organization", .24f,.42f),
                Person("powder", "Fiel Escudeiro", "Linhagem da Pólvora", "Atirador e escudeiro", "SNIPER", "Criação e controle de pólvora", "Forma um dragão de pólvora que explode no instante marcado pela chama.", "Nação não definida", "titan-organization", .43f,.42f),
                Person("ice", "Rainha de Gelo", "Linhagem de Gelo", "Controle de perímetro", "PYRO", "Esculturas e tempestades de gelo", "Cria tropas, projéteis e seres voadores de gelo.", "Clã não definido", "titan-organization", .63f,.42f),
                Person("animator", "Companheira da Rainha", "Linhagem não definida", "Suporte e criação", "MEDIC", "Vida em objetos inanimados", "Dá vida às esculturas da Rainha e mantém o exército funcionando.", "Clã não definido", "titan-organization", .82f,.42f),
                Person("living-titan", "Titã da Organização", "Nação Titã oculta", "Fortaleza ambulante", "HEAVY", "Corpo verdadeiro de Titã", "Protege todos no interior e enfrenta robôs gigantes de igual altura.", "Nação Titã", "titan-organization", .50f,.15f)
            };
        }

        public static List<MachineData> CreateMachines()
        {
            return new List<MachineData>
            {
                Machine("arena-half-ton", "Robô de Arena", "R-23 meia-tonelada", UnitKind.ArenaRobot, "bellica-government", "soldier-ring", 2.3f,.5f, "Dois jovens controladores", "Braços, pernas e equilíbrio disputados por controladores separados", "Placas rebitadas, vapor, gancho curto e gravação mecânica", "PRONTO PARA O RINGUE", "Registra apostas, golpes e falhas de montagem."),
                Machine("cargo-loader", "Robô de Carga", "Carregador C-4", UnitKind.CargoRobot, "unnamed-contractor", "contractor-depot", 3.4f,2.8f, "Operador de depósito", "Controle único por alavancas", "Braços hidráulicos, radiador exposto e rodas auxiliares", "TRANSPORTANDO SUCATA", "Grava apenas rotas e peso de carga."),
                Machine("government-giant", "Robô Lutador Gigante", "Protótipo abdominal G-44", UnitKind.GiantRobot, "bellica-government", "government-prototype", 52f,860f, "Regimento inteiro em cabines abdominais", "Comando, pernas, braços, resfriamento, radar e apoio divididos", "Cabeça removível, cabine no estômago, placas descartáveis, vapor, gancho e defesa de mísseis", "TRANSFERÊNCIA SOB SIGILO", "Suas gravações podem revelar a história escondida do Governo."),
                Machine("organization-titan", "Titã da Organização", "Fortaleza Torre de Troia", UnitKind.Titan, "titan-organization", "titan-route", 55f,0f, "Toda a organização protegida no interior", "Movimento do Titã com coordenação da Sussurro e melhorias do Príncipe", "Costelas de artilharia, pele dourada por latão, tropas de gelo e rede de besouros", "EM MARCHA", "Seus modelos de gravação preservam rotas e contatos com as nações Titãs escondidas."),
                Machine("falcon-109r", "Caça de Reconhecimento", "Falcão 109-R", UnitKind.ReconFighter, "bellica-government", "bellica-airfield", 0f,3.1f, "Piloto e operador de rádio", "Comando duplo", "Hélice, rádio, câmera ventral e tanque auxiliar", "PATRULHA", "Fotografias das fraturas e rotas de robôs."),
                Machine("wolf-190a", "Caça Radial", "Lobo 190-A", UnitKind.RadialFighter, "unnamed-empire", "empire-capital", 0f,4f, "Piloto", "Controle único", "Motor radial, asas curtas e rádio de interceptação", "ESCOLTA", "Grava comunicações interceptadas durante o voo."),
                Machine("raven-87s", "Aeronave de Mergulho", "Corvo 87-S", UnitKind.DiveAircraft, "unnamed-clan", "clan-academy", 0f,4.3f, "Piloto e observador", "Comando duplo", "Freios de mergulho, sirene mecânica removida e suporte de carga", "RECONHECIMENTO", "Observa treinamento e transporte de protótipos.")
            };
        }

        public static List<MissionData> CreateMissions()
        {
            return new List<MissionData>
            {
                Mission("mission-falcon", "Reconhecimento da Fratura", "FANTASMA-04", MissionKind.Reconnaissance, "falcon-109r", "bellica-airfield", "rift-east", 133,390f,310f,61f,920f,RealmLayer.RealWorld, "Fotografar a ruptura e localizar sinais de retorno.", "Câmera ventral e rádio cifrado", "O Governo suspeita que uma máquina desaparecida tenta voltar.", "As imagens serão enviadas à Oficina de Protótipos."),
                Mission("mission-wolf", "Escolta do Protótipo", "LOBO-12", MissionKind.Transfer, "wolf-190a", "empire-capital", "government-prototype", 133,360f,420f,146f,760f,RealmLayer.RealWorld, "Acompanhar o transporte do robô gigante sem pousar.", "Filmes de reconhecimento", "O Império quer descobrir qual modelo o Governo realmente esconde.", "A rota ficará registrada nos arquivos do caça."),
                Mission("mission-raven", "Vigilância da Academia", "CORVO-07", MissionKind.Patrol, "raven-87s", "clan-academy", "missile-field", 133,410f,265f,42f,680f,RealmLayer.RealWorld, "Observar quais alunos conseguem interceptar mísseis.", "Observador e câmera oblíqua", "Os clãs investem em habilidade enquanto compram tecnologia antiga.", "O relatório pode revelar novas técnicas de vento e raio."),
                Mission("mission-cargo", "Comboio de Carcaças", "CARGA-3", MissionKind.Transfer, "cargo-loader", "contractor-depot", "forge-district", 133,330f,540f,205f,0f,RealmLayer.RealWorld, "Levar placas descartadas e radiadores à forja.", "Sucata, juntas e tubos de resfriamento", "O Príncipe poderá estudar peças que o Governo vendeu como obsoletas.", "A carga entra no estoque da forja."),
                Mission("mission-giant", "Transferência do Lutador", "GIGANTE-01", MissionKind.Transfer, "government-giant", "government-prototype", "soldier-ring", 133,300f,720f,188f,0f,RealmLayer.RealWorld, "Mover o protótipo para uma luta fechada.", "Regimento de controladores e placas extras", "A cabeça é o alvo óbvio, mas o comando real está no abdômen.", "O ringue será isolado quando o robô chegar."),
                Mission("mission-titan", "Marcha da Torre de Troia", "TITÃ-PRÍNCIPE", MissionKind.Assault, "organization-titan", "titan-route", "government-prototype", 133,270f,980f,228f,0f,RealmLayer.RealWorld, "Romper as defesas e alcançar as plantas do robô lutador.", "Organização inteira no interior", "A Sussurro coordena alvos; o Príncipe invade sistemas; as costelas abrigam artilharia e gelo vivo.", "A organização tentará sair pela fratura antes da reação nacional."),
                Mission("mission-return", "Retorno pela Dimensão Quebrada", "ECO-PERDIDO", MissionKind.DimensionalReturn, "arena-half-ton", "broken-forest", "broken-rift", 119,180f,21600f,12940f,0f,RealmLayer.BrokenDimension, "Cruzar novamente todo o mapa e encontrar a quebra de retorno.", "Gravações antigas de arena", "As estradas voltaram a ser floresta; o trajeto pode levar dias, meses ou anos.", "Se alcançar a ruptura, o robô reaparece no mundo real com suas gravações.")
            };
        }

        public static List<RecordingData> CreateRecordings()
        {
            return new List<RecordingData>
            {
                new RecordingData { id="rec-ring", machineId="arena-half-ton", title="Primeira luta observada", location="Ringue dos Soldados", battle="Robô contra robô de meia tonelada", recoveredBy="Príncipe dos Titãs", summary="Mostra o funcionamento das juntas e o instante em que o despertar alcançou o garoto.", day=1 },
                new RecordingData { id="rec-giant", machineId="government-giant", title="Cabeça arrancada, comando preservado", location="Nação não identificada", battle="Robô gigante contra invasores", recoveredBy="Arquivo do Governo", summary="Confirma que a cabeça é sacrificável e que o regimento opera dentro do abdômen.", day=87 },
                new RecordingData { id="rec-titan", machineId="organization-titan", title="Formação Torre de Troia", location="Fortaleza ambulante", battle="Invasão de oficina nacional", recoveredBy="Sussurro Fantasma", summary="Registra comunicação, sabotagem, fogo-magma, tropas de gelo e rotas de fuga.", day=132 }
            };
        }

        public static OrganizationData Organization(ProjectData project, string id) { return project != null && project.organizations != null ? project.organizations.FirstOrDefault(value => value.id == id) : null; }
        public static SiteData Site(ProjectData project, string id) { return project != null && project.sites != null ? project.sites.FirstOrDefault(value => value.id == id) : null; }
        public static MachineData Machine(ProjectData project, string id) { return project != null && project.machines != null ? project.machines.FirstOrDefault(value => value.id == id) : null; }
        public static MissionData Mission(ProjectData project, string id) { return project != null && project.world != null && project.world.missions != null ? project.world.missions.FirstOrDefault(value => value.id == id) : null; }

        public static void ValidateOrThrow()
        {
            var project = CreateDefaultProject();
            if (project.organizations.Count < 5) throw new InvalidOperationException("O mundo precisa aceitar governos, impérios, clãs, empreiteiras e organizações móveis.");
            if (!project.sites.Any(site => site.realm == RealmLayer.BrokenDimension)) throw new InvalidOperationException("A Dimensão Quebrada precisa existir no mapa.");
            if (!project.machines.Any(machine => machine.kind == UnitKind.Titan)) throw new InvalidOperationException("O Titã da organização está ausente.");
            var siteIds = new HashSet<string>(project.sites.Select(site => site.id));
            foreach (var mission in project.world.missions)
                if (!siteIds.Contains(mission.originSiteId) || !siteIds.Contains(mission.destinationSiteId)) throw new InvalidOperationException("Rota inválida: " + mission.id);
        }

        private static SiteData Site(string id, string name, SiteKind kind, string organizationId, float x, float y, string note, string state, RealmLayer realm) { return new SiteData { id=id, name=name, kind=kind, organizationId=organizationId, position=new Vector2(x,y), note=note, operationalState=state, realm=realm }; }
        private static PersonData Person(string id, string name, string family, string role, string teamRole, string ability, string technique, string origin, string organizationId, float x, float y) { return new PersonData { id=id, name=name, family=family, role=role, teamRole=teamRole, ability=ability, technique=technique, origin=origin, organizationId=organizationId, birthYear=0, progressionPhase=1, treePosition=new Vector2(x,y) }; }
        private static MachineData Machine(string id, string name, string model, UnitKind kind, string organizationId, string homeSiteId, float height, float weight, string crew, string controllers, string systems, string state, string recording) { return new MachineData { id=id, name=name, model=model, kind=kind, organizationId=organizationId, homeSiteId=homeSiteId, heightMeters=height, weightTons=weight, crew=crew, controllers=controllers, systems=systems, currentState=state, recordingNote=recording, realm=RealmLayer.RealWorld }; }
        private static MissionData Mission(string id, string title, string callsign, MissionKind kind, string unitId, string origin, string destination, int day, float departure, float duration, float elapsed, float altitude, RealmLayer realm, string objective, string cargo, string context, string consequence) { return new MissionData { id=id, title=title, callsign=callsign, kind=kind, status=MissionStatus.EnRoute, unitId=unitId, originSiteId=origin, destinationSiteId=destination, departureDay=day, departureMinute=departure, durationMinutes=duration, elapsedMinutes=elapsed, altitudeMeters=altitude, realm=realm, objective=objective, cargo=cargo, context=context, consequence=consequence, pinned=true }; }
    }
}
