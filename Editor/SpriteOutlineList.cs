using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameframe.Pixels.Editor
{
    [Serializable]
    public class SpriteOutlineList
    {
        [SerializeField] List<SpriteOutline> _SpriteOutlines = new List<SpriteOutline>();
        [SerializeField] float _TessellationDetail = 0;

        public List<SpriteOutline> SpriteOutlines
        {
            get => _SpriteOutlines;
            set => _SpriteOutlines = value;
        }

        public GUID spriteID { get; private set; }

        public float TessellationDetail
        {
            get => _TessellationDetail;
            set
            {
                _TessellationDetail = value;
                _TessellationDetail = Mathf.Min(1, _TessellationDetail);
                _TessellationDetail = Mathf.Max(0, _TessellationDetail);
            }
        }

        public SpriteOutlineList(GUID guid)
        {
            spriteID = guid;
            _SpriteOutlines = new List<SpriteOutline>();
        }

        public SpriteOutlineList(GUID guid, List<Vector2[]> list)
        {
            spriteID = guid;

            _SpriteOutlines = new List<SpriteOutline>(list.Count);
            for (var i = 0; i < list.Count; ++i)
            {
                var newList = new SpriteOutline();
                newList.m_Path.AddRange(list[i]);
                _SpriteOutlines.Add(newList);
            }
        }

        public SpriteOutlineList(GUID guid, List<SpriteOutline> list)
        {
            spriteID = guid;
            _SpriteOutlines = list;
        }

        public List<Vector2[]> ToListVector()
        {
            var value = new List<Vector2[]>(_SpriteOutlines.Count);
            foreach (var s in _SpriteOutlines)
            {
                value.Add(s.m_Path.ToArray());
            }

            return value;
        }

        public List<Vector2[]> ToListVectorCapped(Rect rect)
        {
            var value = ToListVector();
            rect.center = Vector2.zero;
            foreach (var path in value)
            {
                for (int i = 0; i < path.Length; ++i)
                {
                    var point = path[i];
                    path[i] = PixelOutlineEditorModule.CapPointToRect(point, rect);
                    ;
                }
            }

            return value;
        }

        public SpriteOutline this[int index]
        {
            get => IsValidIndex(index) ? _SpriteOutlines[index] : null;
            set
            {
                if (IsValidIndex(index))
                {
                    _SpriteOutlines[index] = value;
                }
            }
        }

        public static implicit operator List<SpriteOutline>(SpriteOutlineList list)
        {
            return list?._SpriteOutlines;
        }

        public int Count => _SpriteOutlines.Count;

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < Count;
        }
    }
}