using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class AtlasGraphic : MaskableGraphic
    {
        private WorldState world;
        private PowerKind? filter;

        private readonly Vector2[][] districts =
        {
            new [] { new Vector2(.07f,.60f), new Vector2(.17f,.91f), new Vector2(.38f,.88f), new Vector2(.44f,.61f), new Vector2(.28f,.46f) },
            new [] { new Vector2(.38f,.88f), new Vector2(.71f,.91f), new Vector2(.91f,.72f), new Vector2(.75f,.54f), new Vector2(.44f,.61f) },
            new [] { new Vector2(.08f,.22f), new Vector2(.28f,.46f), new Vector2(.44f,.61f), new Vector2(.61f,.38f), new Vector2(.46f,.10f), new Vector2(.18f,.10f) },
            new [] { new Vector2(.44f,.61f), new Vector2(.75f,.54f), new Vector2(.94f,.34f), new Vector2(.82f,.10f), new Vector2(.46f,.10f), new Vector2(.61f,.38f) }
        };

        private readonly string[][] roads =
        {
            new [] { "asterfall", "ferrous", "whitenoon", "lumen", "aureliaworks", "sunward" },
            new [] { "mirador", "oldbridge", "saffron", "vale", "exchange", "glassharbor" },
            new [] { "helion", "relay", "sunward" },
            new [] { "ferrous", "oldbridge", "saffron" },
            new [] { "whitenoon", "vale", "exchange", "aureliaworks" }
        };

        public void Bind(WorldState value)
        {
            world = value;
            raycastTarget = false;
            SetVerticesDirty();
        }

        public void SetFilter(PowerKind? value)
        {
            filter = value;
            SetVerticesDirty();
        }

        public Vector2 CanvasPosition(Vector2 normalized)
        {
            var r = GetPixelAdjustedRect();
            return new Vector2(r.xMin + normalized.x * r.width, r.yMin + normalized.y * r.height);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = GetPixelAdjustedRect();
            AddQuad(vh, r, PrinceTitanTheme.Paper);

            for (var i = 0; i < 12; i++)
            {
                var y = r.yMin + r.height * (i + .5f) / 12f;
                AddLine(vh, new Vector2(r.xMin, y), new Vector2(r.xMax, y), 1f, new Color(.43f, .28f, .18f, i % 2 == 0 ? .035f : .018f));
            }

            AddCircle(vh, CanvasPosition(new Vector2(.88f, .85f)), Mathf.Min(r.width, r.height) * .16f,
                PrinceTitanTheme.WithAlpha(new Color(1f, .76f, .33f), .08f), 40);
            AddCircle(vh, CanvasPosition(new Vector2(.88f, .85f)), Mathf.Min(r.width, r.height) * .08f,
                PrinceTitanTheme.WithAlpha(new Color(1f, .89f, .57f), .12f), 32);

            DrawDistricts(vh);
            DrawInfluence(vh);
            DrawWater(vh);
            DrawRoads(vh);
            DrawSites(vh);
            DrawMovers(vh);
            DrawFootprints(vh);

            AddLine(vh, new Vector2(r.xMin + 6f, r.yMin + 6f), new Vector2(r.xMax - 6f, r.yMin + 6f), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .72f));
            AddLine(vh, new Vector2(r.xMin + 6f, r.yMax - 6f), new Vector2(r.xMax - 6f, r.yMax - 6f), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .72f));
            AddLine(vh, new Vector2(r.xMin + 6f, r.yMin + 6f), new Vector2(r.xMin + 6f, r.yMax - 6f), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .72f));
            AddLine(vh, new Vector2(r.xMax - 6f, r.yMin + 6f), new Vector2(r.xMax - 6f, r.yMax - 6f), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .72f));
        }

        private void DrawDistricts(VertexHelper vh)
        {
            var colors = new[]
            {
                new Color(.51f,.28f,.39f,.11f), new Color(.35f,.48f,.52f,.10f),
                new Color(.57f,.42f,.22f,.10f), new Color(.28f,.53f,.49f,.09f)
            };
            for (var i = 0; i < districts.Length; i++)
            {
                var points = districts[i].Select(CanvasPosition).ToArray();
                AddPolygon(vh, points, colors[i]);
                for (var j = 0; j < points.Length; j++)
                    AddLine(vh, points[j], points[(j + 1) % points.Length], 1.2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .34f));
            }
        }

        private void DrawInfluence(VertexHelper vh)
        {
            if (world == null || world.factions == null) return;
            foreach (var state in world.factions)
            {
                var faction = WorldSeed.Faction(state.factionId);
                if (filter.HasValue && faction.kind != filter.Value) continue;
                var radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * Mathf.Lerp(.10f, .21f, state.influence / 100f);
                var center = CanvasPosition(faction.capital);
                for (var ring = 4; ring >= 1; ring--)
                {
                    var alpha = .018f + (.065f * (5 - ring) / 4f);
                    AddCircle(vh, center, radius * ring / 4f, PrinceTitanTheme.WithAlpha(faction.Color, alpha), 28);
                }
            }
        }

        private void DrawWater(VertexHelper vh)
        {
            var river = new[]
            {
                new Vector2(.02f,.48f), new Vector2(.16f,.50f), new Vector2(.31f,.44f), new Vector2(.46f,.47f),
                new Vector2(.59f,.42f), new Vector2(.71f,.45f), new Vector2(.83f,.39f), new Vector2(1.01f,.41f)
            };
            for (var i = 0; i < river.Length - 1; i++)
            {
                AddLine(vh, CanvasPosition(river[i]), CanvasPosition(river[i + 1]), 6f, new Color(.26f,.46f,.52f,.22f));
                AddLine(vh, CanvasPosition(river[i]), CanvasPosition(river[i + 1]), 1.5f, new Color(.25f,.40f,.44f,.48f));
            }
        }

        private void DrawRoads(VertexHelper vh)
        {
            foreach (var route in roads)
            {
                for (var i = 0; i < route.Length - 1; i++)
                {
                    var a = CanvasPosition(WorldSeed.Site(route[i]).position);
                    var b = CanvasPosition(WorldSeed.Site(route[i + 1]).position);
                    AddDashedLine(vh, a, b, 1.3f, 9f, 7f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .42f));
                }
            }
        }

        private void DrawSites(VertexHelper vh)
        {
            foreach (var site in WorldSeed.Sites)
            {
                var faction = WorldSeed.Faction(site.factionId);
                if (filter.HasValue && faction.kind != filter.Value) continue;
                var p = CanvasPosition(site.position);
                AddCircle(vh, p, 10.5f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Ivory, .82f), 18);
                AddCircle(vh, p, 8f, PrinceTitanTheme.WithAlpha(faction.Color, .92f), 18);
                DrawSiteIcon(vh, p, site.kind, PrinceTitanTheme.Ink);
            }
        }

        private void DrawMovers(VertexHelper vh)
        {
            if (world == null || world.movers == null) return;
            foreach (var mover in world.movers)
            {
                var faction = WorldSeed.Faction(mover.factionId);
                if (filter.HasValue && faction.kind != filter.Value) continue;
                var a = CanvasPosition(WorldSeed.Site(mover.fromSiteId).position);
                var b = CanvasPosition(WorldSeed.Site(mover.toSiteId).position);
                var p = Vector2.Lerp(a, b, mover.progress);
                var direction = (mover.forward ? b - a : a - b).normalized;
                if (mover.kind == MoverKind.Aircraft) DrawAircraft(vh, p, direction, faction.Color);
                else DrawRobot(vh, p, faction.Color);
            }
        }

        private void DrawFootprints(VertexHelper vh)
        {
            var a = CanvasPosition(new Vector2(.12f,.17f));
            var b = CanvasPosition(new Vector2(.34f,.31f));
            for (var i = 0; i < 9; i++)
            {
                var t = (i + Mathf.Repeat(Time.unscaledTime * .06f, 1f)) / 9f;
                var p = Vector2.Lerp(a, b, t);
                var side = i % 2 == 0 ? -1f : 1f;
                var perpendicular = new Vector2(-(b-a).y, (b-a).x).normalized;
                AddCircle(vh, p + perpendicular * side * 3f, 2.2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.PaperInk, .48f), 8);
            }
        }

        private static void DrawSiteIcon(VertexHelper vh, Vector2 p, SiteKind kind, Color ink)
        {
            switch (kind)
            {
                case SiteKind.Airfield:
                    DrawAircraft(vh, p, Vector2.up, ink, .64f); break;
                case SiteKind.RobotWorks:
                    DrawRobot(vh, p, ink, .58f); break;
                case SiteKind.Market:
                    AddTriangle(vh, p + new Vector2(-5f,1f), p + new Vector2(5f,1f), p + new Vector2(0f,6f), ink);
                    AddQuad(vh, new Rect(p.x-4f,p.y-5f,8f,6f), ink); break;
                case SiteKind.Estate:
                    AddTriangle(vh, p + new Vector2(-5f,0f), p + new Vector2(5f,0f), p + new Vector2(0f,5f), ink);
                    AddQuad(vh, new Rect(p.x-4f,p.y-5f,8f,5f), ink); break;
                case SiteKind.Port:
                    AddLine(vh, p + new Vector2(0f,-5f), p + new Vector2(0f,5f), 2f, ink);
                    AddLine(vh, p + new Vector2(-5f,-1f), p + new Vector2(5f,-1f), 2f, ink);
                    AddLine(vh, p + new Vector2(-5f,-1f), p + new Vector2(-2f,-5f), 2f, ink);
                    AddLine(vh, p + new Vector2(5f,-1f), p + new Vector2(2f,-5f), 2f, ink); break;
                case SiteKind.Relay:
                    AddLine(vh, p + new Vector2(0f,-5f), p + new Vector2(0f,5f), 1.7f, ink);
                    AddLine(vh, p + new Vector2(-4f,-5f), p + new Vector2(0f,5f), 1.7f, ink);
                    AddLine(vh, p + new Vector2(4f,-5f), p + new Vector2(0f,5f), 1.7f, ink); break;
                default:
                    AddQuad(vh, new Rect(p.x-4f,p.y-5f,8f,10f), ink);
                    AddQuad(vh, new Rect(p.x-6f,p.y+2f,12f,2f), ink); break;
            }
        }

        private static void DrawAircraft(VertexHelper vh, Vector2 center, Vector2 direction, Color color, float scale = 1f)
        {
            var right = new Vector2(direction.y, -direction.x);
            AddTriangle(vh, center + direction * 12f * scale, center - direction * 7f * scale + right * 5f * scale,
                center - direction * 4f * scale, color);
            AddTriangle(vh, center + direction * 12f * scale, center - direction * 7f * scale - right * 5f * scale,
                center - direction * 4f * scale, color);
            AddLine(vh, center - direction * 1f * scale - right * 9f * scale, center - direction * 1f * scale + right * 9f * scale,
                2.8f * scale, color);
        }

        private static void DrawRobot(VertexHelper vh, Vector2 center, Color color, float scale = 1f)
        {
            AddQuad(vh, new Rect(center.x-6f*scale, center.y-6f*scale, 12f*scale, 12f*scale), color);
            AddQuad(vh, new Rect(center.x-4f*scale, center.y+6f*scale, 8f*scale, 4f*scale), color);
            AddLine(vh, center + new Vector2(-7f,-8f)*scale, center + new Vector2(-3f,-5f)*scale, 2.5f*scale, color);
            AddLine(vh, center + new Vector2(7f,-8f)*scale, center + new Vector2(3f,-5f)*scale, 2.5f*scale, color);
        }

        private static void AddQuad(VertexHelper vh, Rect rect, Color color)
        {
            var start = vh.currentVertCount;
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.up);
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color, Vector2.one);
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color, Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        private static void AddCircle(VertexHelper vh, Vector2 center, float radius, Color color, int segments)
        {
            var centerIndex = vh.currentVertCount;
            vh.AddVert(center, color, new Vector2(.5f,.5f));
            for (var i = 0; i <= segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }

        private static void AddPolygon(VertexHelper vh, IList<Vector2> points, Color color)
        {
            if (points == null || points.Count < 3) return;
            var center = Vector2.zero;
            foreach (var point in points) center += point;
            center /= points.Count;
            for (var i = 0; i < points.Count; i++) AddTriangle(vh, center, points[i], points[(i + 1) % points.Count], color);
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

        private static void AddDashedLine(VertexHelper vh, Vector2 a, Vector2 b, float width, float dash, float gap, Color color)
        {
            var distance = Vector2.Distance(a, b);
            if (distance < .01f) return;
            var direction = (b - a) / distance;
            for (var start = 0f; start < distance; start += dash + gap)
                AddLine(vh, a + direction * start, a + direction * Mathf.Min(start + dash, distance), width, color);
        }
    }
}
