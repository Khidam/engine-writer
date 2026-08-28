using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class WorldOverlayGraphic : MaskableGraphic
    {
        private ProjectData project;
        private string activeFactionId;

        public void Configure(ProjectData value)
        {
            project = value;
            SetVerticesDirty();
        }

        public void SetFilter(string factionId)
        {
            activeFactionId = factionId;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            raycastTarget = false;
            if (project == null || project.sites == null) return;

            var rect = GetPixelAdjustedRect();
            foreach (var faction in project.factions)
            {
                var state = project.world.factions.Find(f => f.factionId == faction.id);
                var influence = state == null ? 50f : state.influence;
                var alpha = string.IsNullOrEmpty(activeFactionId) || activeFactionId == faction.id ? .16f : .035f;
                AddCircle(vh, Position(rect, faction.capital), 36f + influence * .38f,
                    PrinceTitanTheme.WithAlpha(faction.Color, alpha), 48);
            }

            foreach (var mover in project.world.movers)
            {
                var from = FindSite(mover.fromSiteId);
                var to = FindSite(mover.toSiteId);
                if (from == null || to == null) continue;
                var faction = WorldSeed.Faction(project, mover.factionId);
                var alpha = string.IsNullOrEmpty(activeFactionId) || activeFactionId == mover.factionId ? .72f : .12f;
                AddDashedLine(vh, Position(rect, from.position), Position(rect, to.position), 2.5f,
                    PrinceTitanTheme.WithAlpha(faction.Color, alpha));
            }

            foreach (var site in project.sites)
            {
                var faction = WorldSeed.Faction(project, site.factionId);
                var visible = string.IsNullOrEmpty(activeFactionId) || activeFactionId == site.factionId;
                AddRing(vh, Position(rect, site.position), visible ? 14f : 9f, visible ? 3f : 1.5f,
                    PrinceTitanTheme.WithAlpha(faction.Color, visible ? .90f : .20f), 28);
            }
        }

        private SiteData FindSite(string id)
        {
            return project.sites.Find(s => s.id == id);
        }

        private static Vector2 Position(Rect rect, Vector2 normalized)
        {
            return new Vector2(rect.xMin + rect.width * normalized.x, rect.yMin + rect.height * normalized.y);
        }

        private static void AddDashedLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            var distance = Vector2.Distance(a, b);
            if (distance < 1f) return;
            var direction = (b - a) / distance;
            const float dash = 12f;
            const float gap = 8f;
            for (var cursor = 0f; cursor < distance; cursor += dash + gap)
            {
                var end = Mathf.Min(distance, cursor + dash);
                AddLine(vh, a + direction * cursor, a + direction * end, width, color);
            }
        }

        private static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            var direction = (b - a).normalized;
            if (direction.sqrMagnitude < .001f) return;
            var normal = new Vector2(-direction.y, direction.x) * width * .5f;
            var start = vh.currentVertCount;
            vh.AddVert(a - normal, color, Vector2.zero);
            vh.AddVert(a + normal, color, Vector2.up);
            vh.AddVert(b + normal, color, Vector2.one);
            vh.AddVert(b - normal, color, Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddCircle(VertexHelper vh, Vector2 center, float radius, Color color, int segments)
        {
            var centerIndex = vh.currentVertCount;
            vh.AddVert(center, color, Vector2.zero);
            for (var i = 0; i <= segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }

        private static void AddRing(VertexHelper vh, Vector2 center, float radius, float width, Color color, int segments)
        {
            for (var i = 0; i < segments; i++)
            {
                var a0 = Mathf.PI * 2f * i / segments;
                var a1 = Mathf.PI * 2f * (i + 1) / segments;
                AddLine(vh, center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius,
                    center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius, width, color);
            }
        }
    }

    public enum MapIconKind { Site, Aircraft, Robot }

    public sealed class MapIconGraphic : MaskableGraphic
    {
        public MapIconKind iconKind;
        public SiteKind siteKind;
        public Color tint = Color.white;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = GetPixelAdjustedRect();
            var center = rect.center;
            var size = Mathf.Min(rect.width, rect.height);
            var outer = PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ink, .92f);
            AddCircle(vh, center, size * .48f, outer, 28);
            AddRing(vh, center, size * .43f, Mathf.Max(2f, size * .07f), tint, 28);

            if (iconKind == MapIconKind.Aircraft)
            {
                var points = new[]
                {
                    center + new Vector2(0f, size*.29f), center + new Vector2(size*.07f, size*.02f),
                    center + new Vector2(size*.27f, -size*.06f), center + new Vector2(size*.27f, -size*.14f),
                    center + new Vector2(size*.06f, -size*.10f), center + new Vector2(size*.04f, -size*.29f),
                    center + new Vector2(-size*.04f, -size*.29f), center + new Vector2(-size*.06f, -size*.10f),
                    center + new Vector2(-size*.27f, -size*.14f), center + new Vector2(-size*.27f, -size*.06f),
                    center + new Vector2(-size*.07f, size*.02f)
                };
                AddPolygon(vh, points, PrinceTitanTheme.Ivory);
            }
            else if (iconKind == MapIconKind.Robot)
            {
                AddQuad(vh, new Rect(center.x-size*.20f, center.y-size*.16f, size*.40f, size*.31f), tint);
                AddQuad(vh, new Rect(center.x-size*.14f, center.y+size*.14f, size*.28f, size*.18f), PrinceTitanTheme.Ivory);
                AddCircle(vh, center + new Vector2(-size*.07f, size*.23f), size*.025f, PrinceTitanTheme.Ink, 10);
                AddCircle(vh, center + new Vector2(size*.07f, size*.23f), size*.025f, PrinceTitanTheme.Ink, 10);
                AddQuad(vh, new Rect(center.x-size*.27f, center.y-size*.10f, size*.07f, size*.22f), tint);
                AddQuad(vh, new Rect(center.x+size*.20f, center.y-size*.10f, size*.07f, size*.22f), tint);
            }
            else
            {
                DrawSite(vh, center, size);
            }
        }

        private void DrawSite(VertexHelper vh, Vector2 center, float size)
        {
            switch (siteKind)
            {
                case SiteKind.Airfield:
                    var old = iconKind;
                    iconKind = MapIconKind.Aircraft;
                    AddTriangle(vh, center + new Vector2(0f,size*.27f), center + new Vector2(size*.23f,-size*.19f),
                        center + new Vector2(0f,-size*.08f), PrinceTitanTheme.Ivory);
                    AddTriangle(vh, center + new Vector2(0f,size*.27f), center + new Vector2(-size*.23f,-size*.19f),
                        center + new Vector2(0f,-size*.08f), PrinceTitanTheme.Ivory);
                    iconKind = old;
                    break;
                case SiteKind.RobotWorks:
                    AddQuad(vh, new Rect(center.x-size*.20f, center.y-size*.19f, size*.40f, size*.34f), tint);
                    AddQuad(vh, new Rect(center.x-size*.12f, center.y+size*.13f, size*.24f, size*.13f), PrinceTitanTheme.Ivory);
                    break;
                case SiteKind.Market:
                    AddTriangle(vh, center + new Vector2(-size*.25f,size*.05f), center + new Vector2(size*.25f,size*.05f),
                        center + new Vector2(0f,size*.28f), tint);
                    AddQuad(vh, new Rect(center.x-size*.19f, center.y-size*.22f, size*.38f, size*.25f), PrinceTitanTheme.Ivory);
                    break;
                case SiteKind.Company:
                    AddQuad(vh, new Rect(center.x-size*.21f, center.y-size*.22f, size*.42f, size*.43f), tint);
                    AddQuad(vh, new Rect(center.x-size*.08f, center.y-size*.14f, size*.16f, size*.27f), PrinceTitanTheme.Ink);
                    break;
                case SiteKind.Estate:
                    AddTriangle(vh, center + new Vector2(-size*.25f,size*.02f), center + new Vector2(size*.25f,size*.02f),
                        center + new Vector2(0f,size*.27f), tint);
                    AddQuad(vh, new Rect(center.x-size*.20f, center.y-size*.22f, size*.40f, size*.23f), PrinceTitanTheme.Ivory);
                    break;
                case SiteKind.Port:
                    AddCircle(vh, center + new Vector2(0f,size*.06f), size*.16f, tint, 18);
                    AddLine(vh, center + new Vector2(0f,size*.24f), center + new Vector2(0f,-size*.22f), size*.06f, PrinceTitanTheme.Ivory);
                    AddLine(vh, center + new Vector2(-size*.20f,-size*.05f), center + new Vector2(size*.20f,-size*.05f), size*.05f, PrinceTitanTheme.Ivory);
                    break;
                case SiteKind.Relay:
                    AddLine(vh, center + new Vector2(0f,-size*.25f), center + new Vector2(0f,size*.24f), size*.06f, tint);
                    AddLine(vh, center + new Vector2(-size*.18f,-size*.22f), center + new Vector2(0f,size*.14f), size*.05f, PrinceTitanTheme.Ivory);
                    AddLine(vh, center + new Vector2(size*.18f,-size*.22f), center + new Vector2(0f,size*.14f), size*.05f, PrinceTitanTheme.Ivory);
                    break;
                default:
                    AddCircle(vh, center, size*.22f, tint, 22);
                    AddCircle(vh, center, size*.09f, PrinceTitanTheme.Ivory, 16);
                    break;
            }
        }

        private static void AddCircle(VertexHelper vh, Vector2 center, float radius, Color color, int segments)
        {
            var start = vh.currentVertCount;
            vh.AddVert(center, color, Vector2.zero);
            for (var i=0; i<=segments; i++)
            {
                var a=Mathf.PI*2f*i/segments;
                vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);
                if(i>0) vh.AddTriangle(start,start+i,start+i+1);
            }
        }

        private static void AddRing(VertexHelper vh, Vector2 center, float radius, float width, Color color, int segments)
        {
            for(var i=0;i<segments;i++)
            {
                var a0=Mathf.PI*2f*i/segments; var a1=Mathf.PI*2f*(i+1)/segments;
                AddLine(vh,center+new Vector2(Mathf.Cos(a0),Mathf.Sin(a0))*radius,
                    center+new Vector2(Mathf.Cos(a1),Mathf.Sin(a1))*radius,width,color);
            }
        }

        private static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            var direction=(b-a).normalized; if(direction.sqrMagnitude<.001f)return;
            var normal=new Vector2(-direction.y,direction.x)*width*.5f; var start=vh.currentVertCount;
            vh.AddVert(a-normal,color,Vector2.zero); vh.AddVert(a+normal,color,Vector2.up);
            vh.AddVert(b+normal,color,Vector2.one); vh.AddVert(b-normal,color,Vector2.right);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var start=vh.currentVertCount; vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero);
            vh.AddVert(c,color,Vector2.zero); vh.AddTriangle(start,start+1,start+2);
        }

        private static void AddQuad(VertexHelper vh, Rect r, Color color)
        {
            var start=vh.currentVertCount;
            vh.AddVert(new Vector2(r.xMin,r.yMin),color,Vector2.zero); vh.AddVert(new Vector2(r.xMin,r.yMax),color,Vector2.up);
            vh.AddVert(new Vector2(r.xMax,r.yMax),color,Vector2.one); vh.AddVert(new Vector2(r.xMax,r.yMin),color,Vector2.right);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }

        private static void AddPolygon(VertexHelper vh, IList<Vector2> points, Color color)
        {
            if(points==null||points.Count<3)return; var center=Vector2.zero;
            foreach(var p in points) center+=p; center/=points.Count; var start=vh.currentVertCount;
            vh.AddVert(center,color,Vector2.zero); foreach(var p in points) vh.AddVert(p,color,Vector2.zero);
            vh.AddVert(points[0],color,Vector2.zero);
            for(var i=0;i<points.Count;i++) vh.AddTriangle(start,start+i+1,start+i+2);
        }
    }

    public sealed class MapPanZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
    {
        public RectTransform viewport;
        public RectTransform target;
        public float minZoom = 1f;
        public float maxZoom = 3.2f;
        private float zoom = 1f;

        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData)
        {
            if (target == null || viewport == null) return;
            target.anchoredPosition += eventData.delta;
            Clamp();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (target == null || viewport == null) return;
            var factor = eventData.scrollDelta.y > 0f ? 1.14f : .88f;
            zoom = Mathf.Clamp(zoom * factor, minZoom, maxZoom);
            target.localScale = new Vector3(zoom, zoom, 1f);
            Clamp();
        }

        public void ResetView()
        {
            zoom = 1f;
            if (target == null) return;
            target.localScale = Vector3.one;
            target.anchoredPosition = Vector2.zero;
        }

        public void Focus(Vector2 normalized, float targetZoom)
        {
            if (target == null || viewport == null) return;
            zoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            target.localScale = new Vector3(zoom, zoom, 1f);
            var delta = new Vector2(.5f - normalized.x, .5f - normalized.y);
            target.anchoredPosition = new Vector2(delta.x * viewport.rect.width * zoom, delta.y * viewport.rect.height * zoom);
            Clamp();
        }

        private void Clamp()
        {
            if (target == null || viewport == null) return;
            var maxX = Mathf.Max(0f, (target.rect.width * zoom - viewport.rect.width) * .5f);
            var maxY = Mathf.Max(0f, (target.rect.height * zoom - viewport.rect.height) * .5f);
            var p = target.anchoredPosition;
            p.x = Mathf.Clamp(p.x, -maxX, maxX);
            p.y = Mathf.Clamp(p.y, -maxY, maxY);
            target.anchoredPosition = p;
        }
    }
}
