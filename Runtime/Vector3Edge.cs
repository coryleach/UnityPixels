using UnityEngine;

namespace Gameframe.Pixels
{
    public struct Vector3Edge
    {
        public Vector3Edge(int idx, Vector3 v0, Vector3 v1, Vector2 uv0, Vector2 uv1)
        {
            _idx = idx;
            this.v0 = v0;
            this.v1 = v1;
            this.uv0 = uv0;
            this.uv1 = uv1;
        }

        private readonly int _idx;

        public Vector3 v0;
        public Vector3 v1;

        public Vector2 uv0;
        public Vector2 uv1;

        public bool IsSameButDifferentEdge(Vector3Edge other)
        {
            return (_idx != other._idx) && ((v0 == other.v0 && v1 == other.v1) || (v0 == other.v1 && v1 == other.v0));
        }
    }
}

