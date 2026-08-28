using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class ReliefSimulationView : MonoBehaviour
    {
        private sealed class UnitVisual
        {
            public MissionData mission;
            public MachineData machine;
            public Transform root;
            public Transform propeller;
            public Transform marker;
            public Transform head;
            public Transform cooling;
            public Transform leftArm;
            public Transform rightArm;
            public Transform leftLeg;
            public Transform rightLeg;
            public Vector3 coolingBaseScale;
            public Vector3 lastPosition;
        }

        private ProjectData project;
        private Camera mapCamera;
        private RenderTexture targetTexture;
        private Transform realRoot;
        private Transform brokenRoot;
        private readonly Dictionary<string, Transform> siteAnchors = new Dictionary<string, Transform>();
        private readonly Dictionary<string, UnitVisual> units = new Dictionary<string, UnitVisual>();
        private readonly List<UnityEngine.Object> generatedResources = new List<UnityEngine.Object>();
        private RealmLayer realm;
        private Vector3 focus = Vector3.zero;
        private float yaw = -18f;
        private float pitch = 52f;
        private float distance = 48f;

        public event Action<SiteData> SiteSelected;
        public event Action<MissionData> MissionSelected;

        public RenderTexture Configure(ProjectData value, int width = 1600, int height = 900)
        {
            project = value;
            if (targetTexture != null) return targetTexture;

            targetTexture = new RenderTexture(Mathf.Max(960, width), Mathf.Max(540, height), 24, RenderTextureFormat.ARGB32)
            {
                name = "Prince Titan Relief Simulation",
                antiAliasing = 4,
                useMipMap = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            targetTexture.Create();

            var cameraObject = new GameObject("Relief Intelligence Camera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            mapCamera = cameraObject.GetComponent<Camera>();
            mapCamera.targetTexture = targetTexture;
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = new Color(.025f, .028f, .024f, 1f);
            mapCamera.fieldOfView = 38f;
            mapCamera.nearClipPlane = .1f;
            mapCamera.farClipPlane = 180f;
            mapCamera.allowHDR = true;
            mapCamera.allowMSAA = true;
            mapCamera.depth = -20f;

            BuildLighting();
            realRoot = BuildRealm(RealmLayer.RealWorld);
            brokenRoot = BuildRealm(RealmLayer.BrokenDimension);
            SetRealm(project.world.visibleRealm, false);
            ResetView();
            return targetTexture;
        }

        public void SyncVisuals(float deltaTime)
        {
            if (project == null || project.world == null || project.world.missions == null) return;
            foreach (var mission in project.world.missions)
            {
                UnitVisual visual;
                if (!units.TryGetValue(mission.id, out visual) || visual.root == null) continue;
                var origin = WorldSeed.Site(project, mission.originSiteId);
                var destination = WorldSeed.Site(project, mission.destinationSiteId);
                if (origin == null || destination == null) continue;

                var progress = mission.Progress;
                var a = MapPoint(origin.position, mission.realm);
                var b = MapPoint(destination.position, mission.realm);
                var position = Vector3.Lerp(a, b, progress);
                var aircraft = IsAircraft(WorldSeed.Machine(project, mission.unitId));
                if (aircraft)
                    position.y += 2.4f + Mathf.Clamp(mission.altitudeMeters / 420f, .8f, 3.2f) + Mathf.Sin(progress * Mathf.PI) * 1.2f;
                else
                    position.y += .22f + Mathf.Sin(Time.unscaledTime * 4f + progress * 12f) * .05f;

                var lookProgress = Mathf.Clamp01(progress + .006f);
                var look = Vector3.Lerp(a, b, lookProgress);
                if (aircraft) look.y = position.y;
                var direction = look - position;
                if (direction.sqrMagnitude > .0001f)
                {
                    var desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    if (aircraft) desired *= Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * .9f + progress * 10f) * 7f);
                    visual.root.rotation = Quaternion.Slerp(visual.root.rotation, desired, deltaTime * 5f);
                }
                visual.root.position = position;
                visual.lastPosition = position;
                if (visual.propeller != null) visual.propeller.Rotate(0f, 0f, deltaTime * 1200f, Space.Self);
                if (visual.marker != null)
                {
                    var pulse = 1f + Mathf.Sin(Time.unscaledTime * 4.5f + progress * 9f) * .12f;
                    visual.marker.localScale = new Vector3(pulse, pulse, pulse);
                }
                ApplyMachineDamage(visual);
            }
            ApplyCamera();
        }

        public void SetRealm(RealmLayer value, bool focusActive = true)
        {
            realm = value;
            if (project != null && project.world != null) project.world.visibleRealm = value;
            if (realRoot != null) realRoot.gameObject.SetActive(value == RealmLayer.RealWorld);
            if (brokenRoot != null) brokenRoot.gameObject.SetActive(value == RealmLayer.BrokenDimension);
            if (focusActive)
            {
                var active = project.world.missions.FirstOrDefault(item => item.realm == value && item.status == MissionStatus.EnRoute);
                if (active != null) FocusMission(active);
                else ResetView();
            }
        }

        public void Orbit(Vector2 screenDelta)
        {
            yaw += screenDelta.x * .14f;
            pitch = Mathf.Clamp(pitch - screenDelta.y * .11f, 28f, 72f);
        }

        public void Pan(Vector2 screenDelta)
        {
            if (mapCamera == null) return;
            var scale = distance * .0022f;
            var right = mapCamera.transform.right;
            var forward = Vector3.ProjectOnPlane(mapCamera.transform.forward, Vector3.up).normalized;
            focus += (-right * screenDelta.x - forward * screenDelta.y) * scale;
            focus.x = Mathf.Clamp(focus.x, -27f, 27f);
            focus.z = Mathf.Clamp(focus.z, -15f, 15f);
        }

        public void Zoom(float wheelDelta)
        {
            distance = Mathf.Clamp(distance * (wheelDelta > 0f ? .87f : 1.15f), 16f, 70f);
        }

        public void ResetView()
        {
            focus = realm == RealmLayer.RealWorld ? new Vector3(0f, 1.3f, 0f) : new Vector3(0f, 1.7f, 0f);
            yaw = realm == RealmLayer.RealWorld ? -18f : 20f;
            pitch = 52f;
            distance = 48f;
            ApplyCamera();
        }

        public void FocusMission(MissionData mission)
        {
            if (mission == null) return;
            if (realm != mission.realm) SetRealm(mission.realm, false);
            var origin = WorldSeed.Site(project, mission.originSiteId);
            var destination = WorldSeed.Site(project, mission.destinationSiteId);
            if (origin == null || destination == null) return;
            focus = Vector3.Lerp(MapPoint(origin.position, mission.realm), MapPoint(destination.position, mission.realm), mission.Progress);
            focus.y = Mathf.Max(1.2f, focus.y);
            distance = IsAircraft(WorldSeed.Machine(project, mission.unitId)) ? 23f : 20f;
            pitch = 47f;
            ApplyCamera();
        }

        public void FocusSite(SiteData site)
        {
            if (site == null) return;
            if (realm != site.realm) SetRealm(site.realm, false);
            focus = MapPoint(site.position, site.realm) + Vector3.up * .8f;
            distance = 20f;
            pitch = 45f;
            ApplyCamera();
        }

        public void Pick(RectTransform viewport, Vector2 screenPosition, Camera uiCamera)
        {
            if (mapCamera == null || viewport == null) return;
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, screenPosition, uiCamera, out local)) return;
            var rect = viewport.rect;
            var uv = new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
            if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f) return;
            RaycastHit hit;
            if (!Physics.Raycast(mapCamera.ViewportPointToRay(new Vector3(uv.x, uv.y, 0f)), out hit, 200f)) return;
            var selectable = hit.collider.GetComponentInParent<ReliefSelectable>();
            if (selectable == null) return;
            if (!string.IsNullOrEmpty(selectable.missionId))
            {
                var mission = WorldSeed.Mission(project, selectable.missionId);
                if (mission != null)
                {
                    FocusMission(mission);
                    var handler = MissionSelected;
                    if (handler != null) handler(mission);
                }
                return;
            }
            if (!string.IsNullOrEmpty(selectable.siteId))
            {
                var site = WorldSeed.Site(project, selectable.siteId);
                if (site != null)
                {
                    FocusSite(site);
                    var handler = SiteSelected;
                    if (handler != null) handler(site);
                }
            }
        }

        private void BuildLighting()
        {
            var keyObject = new GameObject("Dawn Directional Light", typeof(Light));
            keyObject.transform.SetParent(transform, false);
            keyObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
            var key = keyObject.GetComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, .82f, .60f);
            key.intensity = 1.15f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = .72f;

            var fillObject = new GameObject("Map Fill Light", typeof(Light));
            fillObject.transform.SetParent(transform, false);
            fillObject.transform.position = new Vector3(-12f, 18f, -8f);
            var fill = fillObject.GetComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 70f;
            fill.intensity = .42f;
            fill.color = new Color(.58f, .72f, .74f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.25f, .27f, .24f);
            RenderSettings.ambientEquatorColor = new Color(.14f, .15f, .13f);
            RenderSettings.ambientGroundColor = new Color(.055f, .05f, .045f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = .006f;
            RenderSettings.fogColor = new Color(.16f, .17f, .15f);
        }

        private Transform BuildRealm(RealmLayer layer)
        {
            var root = new GameObject(layer == RealmLayer.RealWorld ? "REAL WORLD RELIEF" : "BROKEN DIMENSION RELIEF").transform;
            root.SetParent(transform, false);
            BuildTerrain(root, layer);
            BuildWater(root, layer);
            foreach (var site in project.sites.Where(value => value.realm == layer)) BuildSite(root, site);
            foreach (var mission in project.world.missions.Where(value => value.realm == layer))
            {
                BuildRoute(root, mission);
                BuildUnit(root, mission);
            }
            return root;
        }

        private void BuildTerrain(Transform parent, RealmLayer layer)
        {
            const int columns = 129;
            const int rows = 73;
            const float width = 60f;
            const float depth = 34f;
            var vertices = new Vector3[columns * rows];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(columns - 1) * (rows - 1) * 6];
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < columns; x++)
                {
                    var nx = x / (float)(columns - 1);
                    var ny = y / (float)(rows - 1);
                    var wx = (nx - .5f) * width;
                    var wz = (ny - .5f) * depth;
                    vertices[y * columns + x] = new Vector3(wx, HeightAt(nx, ny, layer), wz);
                    uv[y * columns + x] = new Vector2(nx, ny);
                }
            }
            var cursor = 0;
            for (var y = 0; y < rows - 1; y++)
            {
                for (var x = 0; x < columns - 1; x++)
                {
                    var a = y * columns + x;
                    var b = a + 1;
                    var c = a + columns;
                    var d = c + 1;
                    triangles[cursor++] = a; triangles[cursor++] = c; triangles[cursor++] = b;
                    triangles[cursor++] = b; triangles[cursor++] = c; triangles[cursor++] = d;
                }
            }
            var mesh = new Mesh { name = layer + " sculpted terrain", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            generatedResources.Add(mesh);
            var go = new GameObject("Sculpted Terrain", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var material = NewMaterial(layer == RealmLayer.RealWorld ? new Color(.43f,.40f,.29f) : new Color(.20f,.30f,.22f), .08f, .08f);
            var texture = BuildTerrainTexture(layer, 512, 288);
            material.mainTexture = texture;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            go.GetComponent<MeshRenderer>().receiveShadows = true;
        }

        private Texture2D BuildTerrainTexture(RealmLayer layer, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGB24, true, false)
            {
                name = layer + " relief material",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4
            };
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var nx = x / (float)(width - 1);
                    var ny = y / (float)(height - 1);
                    var h = HeightAt(nx, ny, layer);
                    var grain = Mathf.PerlinNoise(nx * 51f + 4.2f, ny * 51f + 8.4f) * .10f - .05f;
                    Color color;
                    if (layer == RealmLayer.RealWorld)
                    {
                        if (h < .28f) color = new Color(.23f,.29f,.25f);
                        else if (h < 1.2f) color = new Color(.35f,.39f,.27f);
                        else if (h < 2.8f) color = new Color(.43f,.38f,.27f);
                        else color = new Color(.58f,.57f,.51f);
                    }
                    else
                    {
                        if (h < .35f) color = new Color(.10f,.23f,.22f);
                        else if (h < 1.8f) color = new Color(.12f,.30f,.18f);
                        else if (h < 3.4f) color = new Color(.26f,.20f,.27f);
                        else color = new Color(.52f,.43f,.54f);
                    }
                    var contour = Mathf.Abs(Mathf.Repeat(h * 2.1f, 1f) - .5f);
                    if (contour > .475f) color *= .78f;
                    color += new Color(grain, grain, grain, 0f);
                    pixels[y * width + x] = color;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(true, false);
            generatedResources.Add(texture);
            return texture;
        }

        private void BuildWater(Transform parent, RealmLayer layer)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = layer == RealmLayer.RealWorld ? "Rivers and lowland water" : "Dimensional mist basin";
            water.transform.SetParent(parent, false);
            water.transform.localPosition = new Vector3(0f, .08f, 0f);
            water.transform.localScale = new Vector3(6.2f, 1f, 3.6f);
            var collider = water.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = water.GetComponent<Renderer>();
            var color = layer == RealmLayer.RealWorld ? new Color(.12f,.24f,.27f,.78f) : new Color(.17f,.08f,.22f,.64f);
            renderer.sharedMaterial = NewMaterial(color, .52f, .38f, true);
            renderer.receiveShadows = true;
        }

        private void BuildSite(Transform parent, SiteData site)
        {
            var root = new GameObject(site.name).transform;
            root.SetParent(parent, false);
            root.position = MapPoint(site.position, site.realm) + Vector3.up * .08f;
            var organization = WorldSeed.Organization(project, site.organizationId);
            var accent = organization == null ? PrinceTitanTheme.Magenta : organization.Color;
            var wall = NewMaterial(new Color(.28f,.27f,.23f), .18f, .12f);
            var roof = NewMaterial(new Color(.16f,.17f,.15f), .36f, .18f);
            var metal = NewMaterial(new Color(.24f,.26f,.25f), .62f, .25f);

            switch (site.kind)
            {
                case SiteKind.Airfield:
                    Part(PrimitiveType.Cube, root, "Runway", new Vector3(0f,.06f,0f), new Vector3(3.4f,.08f,.55f), roof);
                    Part(PrimitiveType.Cube, root, "Hangar", new Vector3(-.75f,.32f,.62f), new Vector3(1.25f,.55f,.72f), wall);
                    CreateStaticAircraft(root, new Vector3(.55f,.22f,-.10f), .28f, metal);
                    break;
                case SiteKind.RobotWorks:
                    Part(PrimitiveType.Cube, root, "Factory", new Vector3(0f,.42f,0f), new Vector3(1.5f,.78f,1.05f), wall);
                    Part(PrimitiveType.Cylinder, root, "Chimney", new Vector3(-.48f,1.18f,.18f), new Vector3(.20f,.82f,.20f), metal);
                    Part(PrimitiveType.Cylinder, root, "Chimney", new Vector3(.48f,1.02f,.18f), new Vector3(.17f,.65f,.17f), metal);
                    break;
                case SiteKind.Arena:
                    Part(PrimitiveType.Cylinder, root, "Arena", new Vector3(0f,.18f,0f), new Vector3(1.25f,.18f,1.25f), wall);
                    Part(PrimitiveType.Cylinder, root, "Ring", new Vector3(0f,.40f,0f), new Vector3(.85f,.04f,.85f), NewMaterial(accent,.36f,.15f));
                    break;
                case SiteKind.Rift:
                    CreateRift(root, accent, site.realm);
                    break;
                case SiteKind.Forest:
                    for (var i=0;i<10;i++) CreateTree(root, new Vector3(Mathf.Sin(i*2.1f)*1.2f,0f,Mathf.Cos(i*1.7f)*.8f), .55f + (i%3)*.12f);
                    break;
                case SiteKind.Port:
                    Part(PrimitiveType.Cube, root, "Pier", new Vector3(0f,.14f,0f), new Vector3(2.4f,.16f,.55f), wall);
                    Part(PrimitiveType.Cube, root, "Warehouse", new Vector3(-.55f,.42f,.65f), new Vector3(1.25f,.65f,.75f), wall);
                    Part(PrimitiveType.Cylinder, root, "Crane", new Vector3(.82f,.85f,.42f), new Vector3(.08f,.85f,.08f), metal);
                    break;
                case SiteKind.Relay:
                    Part(PrimitiveType.Cylinder, root, "Radar Mast", new Vector3(0f,.82f,0f), new Vector3(.10f,.82f,.10f), metal);
                    var dish = Part(PrimitiveType.Sphere, root, "Radar Dish", new Vector3(0f,1.58f,0f), new Vector3(.62f,.15f,.62f), NewMaterial(accent,.52f,.16f));
                    dish.localRotation = Quaternion.Euler(25f,0f,0f);
                    break;
                case SiteKind.Depot:
                    Part(PrimitiveType.Cube, root, "Depot", new Vector3(0f,.35f,0f), new Vector3(1.65f,.62f,.85f), wall);
                    for (var i=0;i<4;i++) Part(PrimitiveType.Cube, root, "Cargo", new Vector3(-.7f+i*.46f,.20f,-.67f), new Vector3(.34f,.34f,.34f), metal);
                    break;
                case SiteKind.Academy:
                    CreateHouse(root, Vector3.zero, new Vector3(1.8f,.8f,1.1f), wall, roof);
                    Part(PrimitiveType.Cylinder, root, "Training Tower", new Vector3(.95f,.75f,.18f), new Vector3(.30f,.75f,.30f), metal);
                    break;
                case SiteKind.Capital:
                    CreateHouse(root, new Vector3(-.45f,0f,0f), new Vector3(1.25f,.75f,.9f), wall, roof);
                    CreateHouse(root, new Vector3(.65f,0f,.18f), new Vector3(.82f,.58f,.65f), wall, roof);
                    Part(PrimitiveType.Cylinder, root, "Tower", new Vector3(0f,.72f,.65f), new Vector3(.24f,.72f,.24f), metal);
                    break;
                default:
                    CreateHouse(root, Vector3.zero, new Vector3(1.25f,.65f,.85f), wall, roof);
                    break;
            }

            var pin = Part(PrimitiveType.Sphere, root, "Location Pin", new Vector3(0f,2.15f,0f), new Vector3(.16f,.16f,.16f), NewEmissive(accent, 1.8f));
            pin.gameObject.AddComponent<ReliefPulse>();
            var selectable = root.gameObject.AddComponent<ReliefSelectable>();
            selectable.siteId = site.id;
            var box = root.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f,.85f,0f);
            box.size = new Vector3(3f,2.5f,2.4f);
            siteAnchors[site.id] = root;
        }

        private void BuildRoute(Transform parent, MissionData mission)
        {
            var origin = WorldSeed.Site(project, mission.originSiteId);
            var destination = WorldSeed.Site(project, mission.destinationSiteId);
            if (origin == null || destination == null) return;
            var lineObject = new GameObject("Route " + mission.callsign, typeof(LineRenderer));
            lineObject.transform.SetParent(parent, false);
            var line = lineObject.GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 48;
            line.widthMultiplier = IsAircraft(WorldSeed.Machine(project, mission.unitId)) ? .12f : .16f;
            line.numCapVertices = 5;
            line.numCornerVertices = 4;
            line.textureMode = LineTextureMode.Tile;
            line.sharedMaterial = NewLineMaterial(mission.realm == RealmLayer.RealWorld ? PrinceTitanTheme.Magenta : new Color(.72f,.28f,.88f));
            var a = MapPoint(origin.position, mission.realm);
            var b = MapPoint(destination.position, mission.realm);
            var aircraft = IsAircraft(WorldSeed.Machine(project, mission.unitId));
            for (var i=0;i<line.positionCount;i++)
            {
                var t = i/(float)(line.positionCount-1);
                var p = Vector3.Lerp(a,b,t);
                p.y += aircraft ? 1.1f + Mathf.Sin(t*Mathf.PI)*2.1f : .22f;
                line.SetPosition(i,p);
            }
        }

        private void BuildUnit(Transform parent, MissionData mission)
        {
            var machine = WorldSeed.Machine(project, mission.unitId);
            if (machine == null) return;
            var root = new GameObject(mission.callsign).transform;
            root.SetParent(parent, false);
            Transform propeller = null;
            if (IsAircraft(machine)) propeller = CreateAircraft(root, machine.kind);
            else CreateRobot(root, machine.kind);
            var markerHeight=machine.kind==UnitKind.Titan?3.28f:IsAircraft(machine)?1.48f:2.45f;
            var marker = Part(PrimitiveType.Sphere, root, "Moving Intelligence Pin", new Vector3(0f,markerHeight,0f), new Vector3(.18f,.18f,.18f), NewEmissive(PrinceTitanTheme.Magenta,2.2f));
            var selectable = root.gameObject.AddComponent<ReliefSelectable>();
            selectable.missionId = mission.id;
            var collider = root.gameObject.AddComponent<BoxCollider>();
            collider.center = machine.kind==UnitKind.Titan?new Vector3(0f,1.25f,0f):new Vector3(0f,.7f,0f);
            collider.size = IsAircraft(machine) ? new Vector3(3.8f,1.8f,3.8f) : machine.kind==UnitKind.Titan?new Vector3(2.5f,4.1f,2.2f):new Vector3(2.2f,3.2f,2.2f);
            var visual = new UnitVisual
            {
                mission=mission, machine=machine, root=root, propeller=propeller, marker=marker,
                head=root.Find("Removable Head"), cooling=root.Find("Cooling Radiator"),
                leftArm=root.Find("Left Riveted Arm"), rightArm=root.Find("Right Riveted Arm"),
                leftLeg=root.Find("Left Piston Leg"), rightLeg=root.Find("Right Piston Leg")
            };
            if (visual.cooling != null) visual.coolingBaseScale = visual.cooling.localScale;
            units[mission.id] = visual;
        }

        private static void ApplyMachineDamage(UnitVisual visual)
        {
            if (visual == null || visual.machine == null || visual.root == null) return;
            if (visual.head != null) visual.head.gameObject.SetActive(visual.machine.headIntegrity > .5f);
            if (visual.cooling != null)
            {
                var scale = visual.coolingBaseScale;
                scale.y *= Mathf.Lerp(.18f, 1f, visual.machine.coolingIntegrity / 100f);
                visual.cooling.localScale = scale;
                visual.cooling.gameObject.SetActive(visual.machine.coolingIntegrity > .5f);
            }
            if (visual.leftArm != null) visual.leftArm.gameObject.SetActive(visual.machine.leftArmIntegrity > .5f);
            if (visual.rightArm != null) visual.rightArm.gameObject.SetActive(visual.machine.rightArmIntegrity > .5f);
            if (visual.leftLeg != null) visual.leftLeg.gameObject.SetActive(visual.machine.legsIntegrity > .5f);
            if (visual.rightLeg != null) visual.rightLeg.gameObject.SetActive(visual.machine.legsIntegrity > .5f);
            if (visual.machine.torsoIntegrity <= .5f) visual.root.rotation = Quaternion.Euler(0f, visual.root.eulerAngles.y, 82f);
        }

        private Transform CreateAircraft(Transform root, UnitKind kind)
        {
            var bodyColor = kind == UnitKind.RadialFighter ? new Color(.25f,.28f,.22f) : kind == UnitKind.DiveAircraft ? new Color(.31f,.29f,.23f) : new Color(.24f,.27f,.26f);
            var metal = NewMaterial(bodyColor,.55f,.26f);
            var underside = NewMaterial(new Color(.55f,.55f,.48f),.42f,.18f);
            var scale = kind == UnitKind.DiveAircraft ? 1.10f : 1f;
            root.localScale = Vector3.one * .62f;
            var body = Part(PrimitiveType.Cylinder, root, "Riveted Fuselage", new Vector3(0f,.18f,0f), new Vector3(.38f,1.65f,.38f)*scale, metal);
            body.localRotation = Quaternion.Euler(90f,0f,0f);
            Part(PrimitiveType.Sphere, root, "Engine Cowling", new Vector3(0f,.18f,1.58f*scale), new Vector3(.52f,.46f,.70f), metal);
            Part(PrimitiveType.Cube, root, "Left Tapered Wing", new Vector3(-1.15f,.18f,.12f), new Vector3(2.05f,.10f,.78f)*scale, underside);
            Part(PrimitiveType.Cube, root, "Right Tapered Wing", new Vector3(1.15f,.18f,.12f), new Vector3(2.05f,.10f,.78f)*scale, underside);
            if (kind == UnitKind.DiveAircraft)
            {
                Part(PrimitiveType.Cube, root, "Bent Left Wing", new Vector3(-1.52f,-.02f,.10f), new Vector3(1.15f,.09f,.62f), metal).localRotation=Quaternion.Euler(0f,0f,-8f);
                Part(PrimitiveType.Cube, root, "Bent Right Wing", new Vector3(1.52f,-.02f,.10f), new Vector3(1.15f,.09f,.62f), metal).localRotation=Quaternion.Euler(0f,0f,8f);
            }
            Part(PrimitiveType.Cube, root, "Tail Plane", new Vector3(0f,.24f,-1.42f*scale), new Vector3(1.45f,.08f,.46f), underside);
            Part(PrimitiveType.Cube, root, "Tail Fin", new Vector3(0f,.65f,-1.40f*scale), new Vector3(.10f,.78f,.55f), metal).localRotation=Quaternion.Euler(-12f,0f,0f);
            Part(PrimitiveType.Sphere, root, "Canopy", new Vector3(0f,.62f,.34f), new Vector3(.42f,.34f,.72f), NewMaterial(new Color(.12f,.20f,.19f,.82f),.15f,.78f,true));
            var propellerRoot = new GameObject("Mechanical Propeller").transform;
            propellerRoot.SetParent(root,false);
            propellerRoot.localPosition = new Vector3(0f,.18f,2.18f*scale);
            Part(PrimitiveType.Cube, propellerRoot, "Blade A", Vector3.zero, new Vector3(.10f,1.55f,.06f), metal);
            Part(PrimitiveType.Cube, propellerRoot, "Blade B", Vector3.zero, new Vector3(1.55f,.10f,.06f), metal);
            Part(PrimitiveType.Sphere, propellerRoot, "Hub", Vector3.zero, new Vector3(.22f,.22f,.18f), metal);
            return propellerRoot;
        }

        private void CreateStaticAircraft(Transform root, Vector3 position, float scale, Material material)
        {
            var plane = new GameObject("Parked 1944 Aircraft").transform;
            plane.SetParent(root,false);
            plane.localPosition=position;
            plane.localRotation=Quaternion.Euler(0f,25f,0f);
            plane.localScale=Vector3.one*scale;
            var body=Part(PrimitiveType.Cylinder,plane,"Fuselage",Vector3.zero,new Vector3(.3f,1.3f,.3f),material);
            body.localRotation=Quaternion.Euler(90f,0f,0f);
            Part(PrimitiveType.Cube,plane,"Wings",new Vector3(0f,0f,.1f),new Vector3(2.8f,.08f,.7f),material);
            Part(PrimitiveType.Cube,plane,"Tail",new Vector3(0f,.15f,-1.1f),new Vector3(1.1f,.08f,.35f),material);
        }

        private void CreateRobot(Transform root, UnitKind kind)
        {
            if(kind==UnitKind.Titan)
            {
                CreateLivingTitan(root);
                return;
            }
            var giant = kind == UnitKind.GiantRobot || kind == UnitKind.Titan;
            var cargo = kind == UnitKind.CargoRobot;
            root.localScale = Vector3.one * (kind == UnitKind.Titan ? 1.05f : giant ? .88f : .58f);
            var steel = NewMaterial(kind == UnitKind.Titan ? new Color(.38f,.31f,.18f) : new Color(.30f,.31f,.29f),.72f,.28f);
            var dark = NewMaterial(new Color(.12f,.13f,.12f),.58f,.18f);
            var accent = NewEmissive(kind == UnitKind.Titan ? new Color(.22f,.70f,.40f) : PrinceTitanTheme.Magenta,1.4f);
            Part(PrimitiveType.Cube, root, "Armored Torso", new Vector3(0f,1.12f,0f), new Vector3(cargo?1.25f:1f,.95f,.70f), steel);
            Part(PrimitiveType.Cube, root, "Abdominal Command Cabin", new Vector3(0f,.77f,.38f), new Vector3(.64f,.38f,.16f), accent);
            Part(PrimitiveType.Sphere, root, "Removable Head", new Vector3(0f,1.88f,0f), new Vector3(.50f,.45f,.48f), steel);
            Part(PrimitiveType.Cube, root, "Optics", new Vector3(0f,1.90f,.45f), new Vector3(.48f,.12f,.08f), accent);
            for (var side=-1;side<=1;side+=2)
            {
                var arm=Part(PrimitiveType.Cylinder,root,side<0?"Left Riveted Arm":"Right Riveted Arm",new Vector3(side*.78f,1.08f,0f),new Vector3(.25f,.72f,.25f),steel);
                arm.localRotation=Quaternion.Euler(0f,0f,side*8f);
                Part(PrimitiveType.Sphere,root,"Shoulder",new Vector3(side*.68f,1.52f,0f),new Vector3(.37f,.37f,.37f),steel);
                Part(PrimitiveType.Cube,root,"Foot",new Vector3(side*.30f,.10f,.08f),new Vector3(.48f,.20f,.68f),dark);
                var leg=Part(PrimitiveType.Cylinder,root,side<0?"Left Piston Leg":"Right Piston Leg",new Vector3(side*.30f,.42f,0f),new Vector3(.24f,.45f,.24f),steel);
                leg.localRotation=Quaternion.Euler(0f,0f,side*3f);
            }
            Part(PrimitiveType.Cylinder, root, "Cooling Radiator", new Vector3(0f,1.18f,-.48f), new Vector3(.34f,.64f,.18f), dark);
        }

        private void CreateLivingTitan(Transform root)
        {
            root.localScale=Vector3.one*1.08f;
            var skin=NewMaterial(new Color(.34f,.25f,.19f),.08f,.28f);
            var gold=NewMaterial(new Color(.53f,.40f,.18f),.58f,.35f);
            var shadow=NewMaterial(new Color(.10f,.08f,.07f),.04f,.18f);
            var signal=NewEmissive(new Color(.18f,.76f,.44f),1.8f);
            var ice=NewEmissive(new Color(.32f,.72f,.92f),1.2f);
            var fire=NewEmissive(new Color(.95f,.32f,.10f),1.4f);

            Part(PrimitiveType.Capsule,root,"Living Torso",new Vector3(0f,1.42f,0f),new Vector3(.82f,.92f,.52f),skin);
            Part(PrimitiveType.Sphere,root,"Removable Head",new Vector3(0f,2.52f,0f),new Vector3(.48f,.57f,.46f),skin);
            Part(PrimitiveType.Cylinder,root,"Neck",new Vector3(0f,2.12f,0f),new Vector3(.26f,.30f,.26f),shadow);
            Part(PrimitiveType.Sphere,root,"Abdominal Command Cabin",new Vector3(0f,1.10f,.46f),new Vector3(.48f,.38f,.16f),signal);
            for(var rib=0;rib<4;rib++)
            {
                var y=1.43f+rib*.18f;
                Part(PrimitiveType.Cube,root,"Left Rib Station "+rib,new Vector3(-.34f,y,.45f),new Vector3(.32f,.08f,.10f),rib%2==0?ice:gold);
                Part(PrimitiveType.Cube,root,"Right Rib Station "+rib,new Vector3(.34f,y,.45f),new Vector3(.32f,.08f,.10f),rib%2==0?fire:gold);
            }
            for(var side=-1;side<=1;side+=2)
            {
                Part(PrimitiveType.Sphere,root,"Living Shoulder",new Vector3(side*.76f,1.86f,0f),new Vector3(.38f,.42f,.40f),gold);
                var arm=Part(PrimitiveType.Capsule,root,side<0?"Left Riveted Arm":"Right Riveted Arm",new Vector3(side*.90f,1.22f,0f),new Vector3(.29f,.78f,.29f),skin);
                arm.localRotation=Quaternion.Euler(0f,0f,side*5f);
                Part(PrimitiveType.Sphere,root,"Living Hand",new Vector3(side*.98f,.57f,.02f),new Vector3(.33f,.36f,.28f),skin);
                var leg=Part(PrimitiveType.Capsule,root,side<0?"Left Piston Leg":"Right Piston Leg",new Vector3(side*.34f,.48f,0f),new Vector3(.34f,.62f,.36f),skin);
                leg.localRotation=Quaternion.Euler(0f,0f,side*2f);
                Part(PrimitiveType.Cube,root,"Living Foot",new Vector3(side*.34f,.06f,.17f),new Vector3(.52f,.18f,.78f),shadow);
            }
            Part(PrimitiveType.Capsule,root,"Golden Nanomachine Mantle",new Vector3(0f,1.55f,-.44f),new Vector3(.54f,.72f,.16f),gold);
        }

        private void CreateHouse(Transform root, Vector3 position, Vector3 scale, Material wall, Material roofMaterial)
        {
            Part(PrimitiveType.Cube,root,"1940s Masonry House",position+new Vector3(0f,scale.y*.42f,0f),scale,wall);
            var roof = new GameObject("Pitched Roof",typeof(MeshFilter),typeof(MeshRenderer)).transform;
            roof.SetParent(root,false);
            roof.localPosition=position+new Vector3(0f,scale.y*.98f,0f);
            roof.localScale=new Vector3(scale.x,scale.y*.62f,scale.z);
            var mesh=new Mesh { name="Pitched miniature roof" };
            mesh.vertices=new[] { new Vector3(-.5f,0f,-.5f),new Vector3(.5f,0f,-.5f),new Vector3(-.5f,0f,.5f),new Vector3(.5f,0f,.5f),new Vector3(0f,.5f,-.5f),new Vector3(0f,.5f,.5f) };
            mesh.triangles=new[] {0,4,1,2,3,5,0,2,5,0,5,4,1,4,5,1,5,3,0,1,3,0,3,2};
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            roof.GetComponent<MeshFilter>().sharedMesh=mesh;
            roof.GetComponent<MeshRenderer>().sharedMaterial=roofMaterial;
            generatedResources.Add(mesh);
        }

        private void CreateRift(Transform root, Color accent, RealmLayer layer)
        {
            var emissive=NewEmissive(layer==RealmLayer.RealWorld?PrinceTitanTheme.Magenta:new Color(.62f,.20f,.86f),2.8f);
            for(var i=0;i<12;i++)
            {
                var angle=i*Mathf.PI*2f/12f;
                var crystal=Part(PrimitiveType.Cube,root,"Fracture Shard",new Vector3(Mathf.Cos(angle)*.82f,.58f+Mathf.Sin(i*1.7f)*.12f,Mathf.Sin(angle)*.82f),new Vector3(.14f,.95f,.14f),emissive);
                crystal.localRotation=Quaternion.Euler(Mathf.Sin(angle)*22f,-angle*Mathf.Rad2Deg,Mathf.Cos(angle)*18f);
            }
            var core=Part(PrimitiveType.Sphere,root,"Dimensional Core",new Vector3(0f,.65f,0f),new Vector3(.72f,.72f,.72f),emissive);
            core.gameObject.AddComponent<ReliefPulse>();
        }

        private void CreateTree(Transform root, Vector3 position, float scale)
        {
            var trunk=NewMaterial(new Color(.18f,.12f,.08f),.08f,.05f);
            var leaf=NewMaterial(new Color(.08f,.22f,.12f),.04f,.06f);
            Part(PrimitiveType.Cylinder,root,"Trunk",position+new Vector3(0f,.35f*scale,0f),new Vector3(.10f*scale,.36f*scale,.10f*scale),trunk);
            Part(PrimitiveType.Capsule,root,"Canopy",position+new Vector3(0f,.92f*scale,0f),new Vector3(.50f*scale,.68f*scale,.50f*scale),leaf);
        }

        private Transform Part(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go=GameObject.CreatePrimitive(type);
            go.name=name;
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            go.transform.localScale=scale;
            var renderer=go.GetComponent<Renderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.On;
            renderer.receiveShadows=true;
            var collider=go.GetComponent<Collider>();
            if(collider!=null) Destroy(collider);
            return go.transform;
        }

        private Material NewMaterial(Color color, float metallic, float smoothness, bool transparent = false)
        {
            var shader=Shader.Find("Standard");
            if(shader==null) shader=Shader.Find("Diffuse");
            var material=new Material(shader) { name="Prince Titan material" };
            material.color=color;
            if(material.HasProperty("_Metallic")) material.SetFloat("_Metallic",metallic);
            if(material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness",smoothness);
            if(transparent)
            {
                material.SetFloat("_Mode",3f);
                material.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite",0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue=3000;
            }
            generatedResources.Add(material);
            return material;
        }

        private Material NewEmissive(Color color, float intensity)
        {
            var material=NewMaterial(color,.20f,.45f);
            if(material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",color*intensity);
            }
            return material;
        }

        private Material NewLineMaterial(Color color)
        {
            var shader=Shader.Find("Unlit/Color");
            if(shader==null) shader=Shader.Find("Sprites/Default");
            var material=new Material(shader) { name="Mission route" };
            material.color=color;
            generatedResources.Add(material);
            return material;
        }

        private Vector3 MapPoint(Vector2 normalized, RealmLayer layer)
        {
            return new Vector3((normalized.x-.5f)*60f,HeightAt(normalized.x,normalized.y,layer),(normalized.y-.5f)*34f);
        }

        private static float HeightAt(float x, float y, RealmLayer layer)
        {
            var edge=Mathf.Clamp01(Mathf.Min(Mathf.Min(x,y),Mathf.Min(1f-x,1f-y))*6.2f);
            var broad=Mathf.PerlinNoise(x*3.1f+(layer==RealmLayer.RealWorld?2.3f:14.1f),y*3.1f+4.7f);
            var detail=Mathf.PerlinNoise(x*9.7f+8.2f,y*9.7f+1.4f);
            var ridge=1f-Mathf.Abs(Mathf.PerlinNoise(x*5.3f+20f,y*5.3f+11f)*2f-1f);
            var height=(broad*.62f+detail*.24f+ridge*.30f-.40f)*edge;
            if(layer==RealmLayer.BrokenDimension)
            {
                var fracture=Mathf.Abs(Mathf.Sin((x*1.45f+y*.72f)*Mathf.PI*4f));
                height+=fracture*.48f+Mathf.PerlinNoise(x*17f,y*17f)*.18f;
            }
            return Mathf.Max(.02f,height*(layer==RealmLayer.RealWorld?5.4f:6.4f));
        }

        private static bool IsAircraft(MachineData machine)
        {
            return machine!=null&&(machine.kind==UnitKind.ReconFighter||machine.kind==UnitKind.RadialFighter||machine.kind==UnitKind.DiveAircraft);
        }

        private void ApplyCamera()
        {
            if(mapCamera==null)return;
            var rotation=Quaternion.Euler(pitch,yaw,0f);
            mapCamera.transform.position=focus-rotation*Vector3.forward*distance;
            mapCamera.transform.rotation=rotation;
        }

        private void OnDestroy()
        {
            if(targetTexture!=null){targetTexture.Release();Destroy(targetTexture);}
            foreach(var resource in generatedResources) if(resource!=null) Destroy(resource);
            generatedResources.Clear();
        }
    }

    public sealed class ReliefMapInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerClickHandler
    {
        public ReliefSimulationView view;
        private Vector2 pressPosition;
        private bool dragged;

        public void OnBeginDrag(PointerEventData eventData) { pressPosition=eventData.position; dragged=false; }
        public void OnDrag(PointerEventData eventData)
        {
            if(view==null)return;
            dragged|=(eventData.position-pressPosition).sqrMagnitude>36f;
            if(eventData.button==PointerEventData.InputButton.Right||eventData.button==PointerEventData.InputButton.Middle) view.Pan(eventData.delta);
            else view.Orbit(eventData.delta);
        }
        public void OnEndDrag(PointerEventData eventData) { }
        public void OnScroll(PointerEventData eventData) { if(view!=null&&Mathf.Abs(eventData.scrollDelta.y)>.01f)view.Zoom(eventData.scrollDelta.y); }
        public void OnPointerClick(PointerEventData eventData) { if(view!=null&&!dragged)view.Pick((RectTransform)transform,eventData.position,eventData.pressEventCamera); }
    }

    public sealed class ReliefSelectable : MonoBehaviour
    {
        public string siteId;
        public string missionId;
    }

    public sealed class ReliefPulse : MonoBehaviour
    {
        private Vector3 baseScale;
        private float phase;
        private void Awake(){baseScale=transform.localScale;phase=UnityEngine.Random.value*5f;}
        private void Update(){var pulse=1f+Mathf.Sin(Time.unscaledTime*3.8f+phase)*.12f;transform.localScale=baseScale*pulse;transform.Rotate(0f,Time.unscaledDeltaTime*28f,0f,Space.Self);}
    }
}
