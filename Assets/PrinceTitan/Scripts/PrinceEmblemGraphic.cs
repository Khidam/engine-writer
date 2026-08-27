using UnityEngine;
using UnityEngine.UI;

namespace PrinceTitan
{
    public sealed class PrinceEmblemGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = GetPixelAdjustedRect();
            var c = r.center;
            AddTriangle(vh, c + new Vector2(-18f,-15f), c + new Vector2(18f,-15f), c + new Vector2(0f,18f), PrinceTitanTheme.Magenta);
            AddTriangle(vh, c + new Vector2(-13f,8f), c + new Vector2(-3f,8f), c + new Vector2(-8f,20f), PrinceTitanTheme.Brass);
            AddTriangle(vh, c + new Vector2(-4f,8f), c + new Vector2(4f,8f), c + new Vector2(0f,23f), PrinceTitanTheme.Brass);
            AddTriangle(vh, c + new Vector2(3f,8f), c + new Vector2(13f,8f), c + new Vector2(8f,20f), PrinceTitanTheme.Brass);
            AddQuad(vh, new Rect(c.x-14f,c.y+5f,28f,5f), PrinceTitanTheme.Brass);
            raycastTarget = false;
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var start=vh.currentVertCount; vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero); vh.AddVert(c,color,Vector2.zero); vh.AddTriangle(start,start+1,start+2);
        }
        private static void AddQuad(VertexHelper vh, Rect r, Color color)
        {
            var start=vh.currentVertCount; vh.AddVert(new Vector2(r.xMin,r.yMin),color,Vector2.zero); vh.AddVert(new Vector2(r.xMin,r.yMax),color,Vector2.zero);
            vh.AddVert(new Vector2(r.xMax,r.yMax),color,Vector2.zero); vh.AddVert(new Vector2(r.xMax,r.yMin),color,Vector2.zero); vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }
    }
}
