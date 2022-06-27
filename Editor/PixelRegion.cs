using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameframe.Pixels.Editor
{
    public class PixelRegion
    {
        private readonly List<Vector2Edge> _edges = new List<Vector2Edge>();

        public readonly List<Vector2Int> neighbors = new List<Vector2Int>();

        public void AddEdge(Vector2Edge edge)
        {
            //Check if this edge can be combined with any other edge
            var candidates = _edges.Where(x => x.v0 == edge.v0 || x.v1 == edge.v1 || x.v1 == edge.v0 || x.v0 == edge.v1);
            foreach (var candidate in candidates)
            {
                //Check if edges are the same direction
                if (!candidate.TryCombine(edge, out var newEdge)) continue;
                
                //Combined Edges
                _edges.Remove(candidate);
                AddEdge(newEdge);
                return;
            }
            //If unable to find any combinable edge just add it to our list of edges
            _edges.Add(edge);
        }

        public void GetOutline(IList<Vector2Int> outline)
        {
            var unused = new List<Vector2Edge>(_edges);
            
            outline.Clear();
            
            //Setup first edge
            var current = unused[0];
            unused.RemoveAt(0);
            
            outline.Add(current.v0);
            outline.Add(current.v1);
            var startPt = current.v0;
            var currentPt = current.v1;
            
            while (unused.Count > 0)
            {
                var edge = unused.First(x => x.v0 == currentPt || x.v1 == currentPt);
                
                //Add point on the edge that isn't already added and continue
                if (edge.v1 == currentPt)
                {
                    currentPt = edge.v0;
                }
                else
                {
                    currentPt = edge.v1;
                }

                if (currentPt == startPt)
                {
                    break;
                }
                
                outline.Add(currentPt);
                unused.Remove(edge);
            }

            var idx = GetMinPoint(outline);
            var a = outline[idx];
            var b = outline[idx > 0 ? idx - 1 : outline.Count-1];
            var c = outline[(idx + 1) % outline.Count];
            if (outline.Count >= 3 && !IsClockwise(a,b,c))
            {
                //Reverse Winding
                for (var i = 0; i < outline.Count / 2; i++)
                {
                    //This swaps the values
                    (outline[i], outline[outline.Count - i - 1]) = (outline[outline.Count - i - 1], outline[i]);
                }
            }
        }
        
        private static int GetMinPoint(IList<Vector2Int> outline)
        {
            var minY = int.MaxValue;
            var maxX = int.MaxValue;
            var idx = -1;
            for (var i = 0; i < outline.Count; i++)
            {
                if (minY > outline[i].y)
                {
                    idx = i;
                    minY = outline[i].y;
                    maxX = outline[i].x;
                }
                else if (minY == outline[i].y && maxX < outline[i].x)
                {
                    idx = i;
                    minY = outline[i].y;
                    maxX = outline[i].x;
                }
            }
            return idx;
        }
        
        private static bool IsClockwise(Vector2 mid, Vector2 prev, Vector2 next)
        {
            double detOrient = (prev.x * next.y + mid.x * prev.y + mid.y * next.x) - (mid.y * prev.x + prev.y * next.x + mid.x * next.y);
            return detOrient < 0;
        }
    }
}