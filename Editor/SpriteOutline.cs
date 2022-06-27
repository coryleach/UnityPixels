using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframe.Pixels.Editor
{
    [Serializable]
    public class SpriteOutline
    {
        [SerializeField] 
        public List<Vector2> m_Path = new List<Vector2>();

        public void Add(Vector2 point)
        {
            m_Path.Add(point);
        }

        public void Insert(int index, Vector2 point)
        {
            m_Path.Insert(index, point);
        }

        public void RemoveAt(int index)
        {
            m_Path.RemoveAt(index);
        }

        public Vector2 this[int index]
        {
            get => m_Path[index];
            set => m_Path[index] = value;
        }

        public int Count => m_Path.Count;

        public void AddRange(IEnumerable<Vector2> addRange)
        {
            m_Path.AddRange(addRange);
        }
    }
}