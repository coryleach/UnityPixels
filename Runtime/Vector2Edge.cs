using UnityEngine;

namespace Gameframe.Pixels
{
    public struct Vector2Edge
    {
        public Vector2Int v0;
        public Vector2Int v1;

        private bool IsHorizontal => v0.y == v1.y;
        private bool IsVertical => v0.x == v1.x;

        public bool TryCombine(Vector2Edge other, out Vector2Edge newEdge)
        {
            if ((IsHorizontal && other.IsHorizontal) || (IsVertical && other.IsVertical))
            {
                if (v0 == other.v0)
                {
                    newEdge = new Vector2Edge()
                    {
                        v0 = v1,
                        v1 = other.v1
                    };
                    return true;
                }
                
                if (v0 == other.v1)
                {
                    newEdge = new Vector2Edge()
                    {
                        v0 = v1,
                        v1 = other.v0
                    };
                    return true;
                }
                
                if (v1 == other.v1)
                {
                    newEdge = new Vector2Edge()
                    {
                        v0 = v0,
                        v1 = other.v0
                    };
                    return true;
                }
                
                if (v1 == other.v0)
                {
                    newEdge = new Vector2Edge()
                    {
                        v0 = v0,
                        v1 = other.v1
                    };
                    return true;
                }
            }
            newEdge = new Vector2Edge();
            return false;
        }
        
    }
}