using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class LineageConnectionsGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            raycastTarget = false;
            foreach (var child in WorldSeed.People)
            {
                DrawParent(vh, child.parentAId, child.treePosition);
                DrawParent(vh, child.parentBId, child.treePosition);
            }
        }

        private void DrawParent(VertexHelper vh, string parentId, Vector2 childPosition)
        {
            if (string.IsNullOrEmpty(parentId)) return;
            var parent = WorldSeed.People.Find(p => p.id == parentId);
            if (parent == null) return;
            var a = Position(parent.treePosition) + new Vector2(0f, -38f);
            var b = Position(childPosition) + new Vector2(0f, 38f);
            var middle = (a.y + b.y) * .5f;
            AddLine(vh, a, new Vector2(a.x, middle), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Brass, .66f));
            AddLine(vh, new Vector2(a.x, middle), new Vector2(b.x, middle), 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Brass, .66f));
            AddLine(vh, new Vector2(b.x, middle), b, 2f, PrinceTitanTheme.WithAlpha(PrinceTitanTheme.Brass, .66f));
            AddCircle(vh, b, 3f, PrinceTitanTheme.Magenta, 10);
        }

        private Vector2 Position(Vector2 normalized)
        {
            var r = GetPixelAdjustedRect();
            return new Vector2(r.xMin + r.width * normalized.x, r.yMin + r.height * normalized.y);
        }

        private static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            var direction = (b - a).normalized;
            if (direction.sqrMagnitude < .001f) return;
            var normal = new Vector2(-direction.y, direction.x) * width * .5f;
            var start = vh.currentVertCount;
            vh.AddVert(a - normal, color, Vector2.zero); vh.AddVert(a + normal, color, Vector2.up);
            vh.AddVert(b + normal, color, Vector2.one); vh.AddVert(b - normal, color, Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
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
    }
}
