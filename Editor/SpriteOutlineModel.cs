using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameframe.Pixels.Editor
{
    public class SpriteOutlineModel : ScriptableObject
    {
        [SerializeField] List<SpriteOutlineList> m_SpriteOutlineList = new List<SpriteOutlineList>();

        private SpriteOutlineModel()
        {
        }

        public SpriteOutlineList this[int index]
        {
            get { return IsValidIndex(index) ? m_SpriteOutlineList[index] : null; }
            set
            {
                if (IsValidIndex(index))
                    m_SpriteOutlineList[index] = value;
            }
        }

        public SpriteOutlineList this[GUID guid]
        {
            get { return m_SpriteOutlineList.FirstOrDefault(x => x.spriteID == guid); }
            set
            {
                var index = m_SpriteOutlineList.FindIndex(x => x.spriteID == guid);
                if (index != -1)
                    m_SpriteOutlineList[index] = value;
            }
        }

        public void AddListVector2(GUID guid, List<Vector2[]> outline)
        {
            m_SpriteOutlineList.Add(new SpriteOutlineList(guid, outline));
        }

        public int Count
        {
            get { return m_SpriteOutlineList.Count; }
        }

        bool IsValidIndex(int index)
        {
            return index >= 0 && index < Count;
        }
    }
}