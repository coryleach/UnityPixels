using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEvent = UnityEngine.Event;

namespace Gameframe.Pixels.Editor
{
    [RequireSpriteDataProvider(typeof(ISpriteOutlineDataProvider), typeof(ITextureDataProvider))]
    public class PixelOutlineEditorModule : SpriteEditorModuleBase
    {
        private class PixelOutlineEditorModuleStyles
        {
            public readonly GUIContent generateOutlineLabel =
                EditorGUIUtility.TrTextContent("Generate", "Generate new outline for selected sprite.");
            public readonly GUIContent generateAllOutlineLabel =
                EditorGUIUtility.TrTextContent("Generate All", "Generate new outlines for all sprites.");
            public readonly Color spriteBorderColor = new Color(0.25f, 0.5f, 1f, 0.75f);
            public readonly Color outlineBorderColor = new Color(1f, 0f, 1f, 0.75f);
        }
        
        private const float OutlineWidth = 0.25f;

        private static readonly List<Vector2[]> Outlines = new List<Vector2[]>();
        private static readonly List<bool> SVisited = new();
        private static readonly List<Vector2Int> RegionOutline = new();

        [Serializable]
        public class SpriteOutline
        {
            [SerializeField] public List<Vector2> Path = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Path.Add(point);
            }

            public void Insert(int index, Vector2 point)
            {
                Path.Insert(index, point);
            }

            public void RemoveAt(int index)
            {
                Path.RemoveAt(index);
            }

            public Vector2 this[int index]
            {
                get => Path[index];
                set => Path[index] = value;
            }

            public int Count => Path.Count;

            public void AddRange(IEnumerable<Vector2> addRange)
            {
                Path.AddRange(addRange);
            }
        }

        private List<SpriteOutline> _generatedSpriteOutline;
        private ISpriteOutlineDataProvider _outlineDataProvider;
        private ITextureDataProvider _textureDataProvider;
        private PixelOutlineEditorModuleStyles _styles;
        private SpriteRect[] _spriteRects = null;
        private SpriteOutlineModel _outlineModel;
        
        private PixelOutlineEditorModuleStyles Styles
        {
            get
            {
                if (_styles == null)
                    _styles = new PixelOutlineEditorModuleStyles();
                return _styles;
            }
        }

        #region SpriteEditorModuleBase

        public override string moduleName => "Pixel Outline";

        public override bool CanBeActivated()
        {
            return true;
        }

        public override void DoMainGUI()
        {
            var evt = UnityEvent.current;
            var selected = spriteEditor.selectedSpriteRect;
            if (selected == null || !selected.rect.Contains(evt.mousePosition))
            {
                spriteEditor.HandleSpriteSelection();
            }
            DrawOutline();
            DrawGizmos();
        }

        public override void DoToolbarGUI(Rect drawArea)
        {
            var style = Styles;
            var generateButtonArea = new Rect(drawArea.x, drawArea.y,
                EditorStyles.toolbarButton.CalcSize(style.generateOutlineLabel).x, drawArea.height);
            if (GUI.Button(generateButtonArea, style.generateOutlineLabel, EditorStyles.toolbarButton))
            {
                GenerateOutlines();
            }
            var generateAllButtonArea = new Rect(drawArea.x + generateButtonArea.width, drawArea.y,
                EditorStyles.toolbarButton.CalcSize(style.generateAllOutlineLabel).x, drawArea.height);
            if (GUI.Button(generateAllButtonArea, style.generateAllOutlineLabel, EditorStyles.toolbarButton))
            {
                GenerateAllOutlines();
            }
        }

        public override void OnModuleActivate()
        {
            if (spriteEditor == null)
            {
                Debug.LogError("PixelOutlineEditorModule has no SpriteEditor");
                return;
            }

            _outlineDataProvider = spriteEditor.GetDataProvider<ISpriteOutlineDataProvider>();
            _textureDataProvider = spriteEditor.GetDataProvider<ITextureDataProvider>();
            _spriteRects = spriteEditor.GetDataProvider<ISpriteEditorDataProvider>().GetSpriteRects();
            LoadOutlines();
            Debug.Log($"{moduleName} Activated");
        }

        public override void OnModuleDeactivate()
        {
            _outlineDataProvider = null;
            _textureDataProvider = null;
            if (_outlineModel != null)
            {
                Object.DestroyImmediate(_outlineModel);
                _outlineModel = null;
            }
        }

        public override void DoPostGUI()
        {
        }

        public override bool ApplyRevert(bool apply)
        {
            if (_outlineModel != null)
            {
                if (apply)
                {
                    for (int i = 0; i < _outlineModel.Count; ++i)
                    {
                        _outlineDataProvider.SetOutlines(_outlineModel[i].spriteID, _outlineModel[i].ToListVector());
                        _outlineDataProvider.SetTessellationDetail(_outlineModel[i].spriteID, _outlineModel[i].TessellationDetail);
                    }
                }
                Object.DestroyImmediate(_outlineModel);
                _outlineModel = null;
            }
            return true;
        }

        #endregion

        protected virtual void LoadOutlines()
        {
            _outlineModel = ScriptableObject.CreateInstance<SpriteOutlineModel>();
            _outlineModel.hideFlags = HideFlags.HideAndDontSave;
            foreach (var rect in _spriteRects)
            {
                var outlines = _outlineDataProvider.GetOutlines(rect.spriteID);
                _outlineModel.AddListVector2(rect.spriteID, outlines);
                _outlineModel[_outlineModel.Count - 1].TessellationDetail = _outlineDataProvider.GetTessellationDetail(rect.spriteID);
            }
        }
        
        private void DrawGizmos()
        {
            var evt = UnityEvent.current;
            if (evt.type != EventType.Repaint) return;
            var selected = spriteEditor.selectedSpriteRect;
            if (selected == null) return;
            PixelOutlineEditorUtility.BeginLines(Styles.spriteBorderColor);
            PixelOutlineEditorUtility.DrawBox(selected.rect);
            PixelOutlineEditorUtility.EndLines();
        }
        
        private void DrawOutline()
        {
            var evt = UnityEvent.current;
            if (evt.type != EventType.Repaint)
            {
                return;
            }

            if (spriteEditor.selectedSpriteRect != null)
            {
                var outlines = _outlineModel[spriteEditor.selectedSpriteRect.spriteID].SpriteOutlines;
                foreach (var outline in outlines)
                {
                    DrawPath(outline.m_Path, spriteEditor.selectedSpriteRect.rect,OutlineWidth);
                }
            }
            else
            {
                if (_spriteRects == null)
                {
                    return;
                }

                foreach (var spriteRect in _spriteRects)
                {
                    var outlines = _outlineModel[spriteRect.spriteID].SpriteOutlines;
                    foreach (var outline in outlines)
                    {
                        DrawPath(outline.m_Path, spriteRect.rect, OutlineWidth);
                    }
                }
            }
        }

        private void DrawPath(IReadOnlyList<Vector2> path, Rect rect)
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            PixelOutlineEditorUtility.BeginLines(Styles.outlineBorderColor);
            for (var i = 0; i < path.Count; i++)
            {
                var idx0 = path[i];
                var idx1 = path[(i + 1) % path.Count];
                var center = rect.center;
                idx0 += center;
                idx1 += center;
                PixelOutlineEditorUtility.DrawLine(idx0, idx1);
            }
            PixelOutlineEditorUtility.EndLines();
        }
        
        private void DrawPath(IReadOnlyList<Vector2> path, Rect rect, float width)
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            PixelOutlineEditorUtility.BeginQuads(Styles.outlineBorderColor);
            for (var i = 0; i < path.Count; i++)
            {
                var idx0 = path[i];
                var idx1 = path[(i + 1) % path.Count];
                var center = rect.center;
                idx0 += center;
                idx1 += center;
                PixelOutlineEditorUtility.DrawQuadLine(idx0, idx1, width);
            }
            PixelOutlineEditorUtility.EndQuads();
        }

        private void GenerateOutlines()
        {
            var texture = _textureDataProvider.GetReadableTexture2D();
            _textureDataProvider.GetTextureActualWidthAndHeight(out var actualWidth, out var actualHeight);

            int count = 0;
            var sprite = spriteEditor.selectedSpriteRect;
            if (sprite != null)
            {
                var outlineList = GenerateSpriteRectOutline(sprite.rect, texture, actualWidth, actualHeight);
                _outlineModel[sprite.spriteID] = new SpriteOutlineList(sprite.spriteID,outlineList);
                count++;
            }
            else
            {
                foreach (var spriteRect in _spriteRects)
                {
                    var outlineList = GenerateSpriteRectOutline(spriteRect.rect, texture, actualWidth, actualHeight);
                    _outlineModel[spriteRect.spriteID] = new SpriteOutlineList(spriteRect.spriteID,outlineList);
                    count++;
                }
            }

            spriteEditor.SetDataModified();
        }

        private void GenerateAllOutlines()
        {
            var texture = _textureDataProvider.GetReadableTexture2D();
            _textureDataProvider.GetTextureActualWidthAndHeight(out var actualWidth, out var actualHeight);

            foreach (var spriteRect in _spriteRects)
            {
                var outlineList = GenerateSpriteRectOutline(spriteRect.rect, texture, actualWidth, actualHeight);
                _outlineModel[spriteRect.spriteID] = new SpriteOutlineList(spriteRect.spriteID,outlineList);
            }
            
            spriteEditor.SetDataModified();
        }

        private static List<Vector2[]> GenerateSpriteRectOutline(Rect rect, Texture2D texture, int actualWidth, int actualHeight)
        {
            Outlines.Clear();

            if (texture == null)
            {
                return Outlines;
            }

            // we might have a texture that is capped because of max size or NPOT.
            // in that case, we need to convert values from capped space to actual texture space and back.
            var cappedWidth = texture.width;
            var cappedHeight = texture.height;

            var scale = new Vector2(cappedWidth / (float) actualWidth, cappedHeight / (float) actualHeight);
            var spriteRect = rect;
            spriteRect.xMin *= scale.x;
            spriteRect.xMax *= scale.x;
            spriteRect.yMin *= scale.y;
            spriteRect.yMax *= scale.y;

            GenerateOutline(texture, spriteRect, out var paths);

            var capRect = new Rect
            {
                size = rect.size,
                center = Vector2.zero
            };

            for (var j = 0; j < paths.Length; ++j)
            {
                var path = new List<Vector2>();
                foreach (var v in paths[j])
                {
                    var scaledV = new Vector2(v.x / scale.x, v.y / scale.y);
                    var cappedV = CapPointToRect(scaledV, capRect);
                    path.Add(cappedV);
                }
                Outlines.Add(path.ToArray());
            }

            return Outlines;
        }

        private static void GenerateOutline(Texture2D texture, Rect spriteRect, out Vector2[][] paths)
        {
            paths = null;

            if (!texture.isReadable)
            {
                Debug.LogError("Texture is not readable");
                return;
            }

            var pixels = texture.GetPixels32();
            
            SVisited.Clear();
            if (SVisited.Capacity < pixels.Length)
            {
                SVisited.Capacity = pixels.Length;
            }
            for (var i = 0; i < pixels.Length; i++)
            {
                SVisited.Add(false);
            }
            
            var width = texture.width;

            var regions = new List<PixelRegion>();
            
            for (var y = 0; y < spriteRect.height; y++)
            {
                for (var x = 0; x < spriteRect.width; x++)
                {
                    int textureY = y + (int)spriteRect.y;
                    int textureX = x + (int) spriteRect.x;
                    int pixelIndex = (textureY * width) + textureX;
                    
                    //Skip any pixels we've already visited
                    if (SVisited[pixelIndex])
                    {
                        continue;
                    }

                    //If pixel is transparent mark it visited and continue
                    if (pixels[pixelIndex].a == 0)
                    {
                        SVisited[pixelIndex] = true;
                        continue;
                    }
                    
                    //Checks the given pixel and recursively checks neighbors to build a region
                    var region = new PixelRegion();
                    
                    AddPixels(region, SVisited, pixels, new Vector2Int(x,y), spriteRect, texture.width);

                    while (region.neighbors.Count > 0)
                    {
                        var pt = region.neighbors[0];
                        region.neighbors.RemoveAt(0);
                        AddPixels(region, SVisited, pixels, pt, spriteRect, texture.width);
                    }
                    
                    regions.Add(region);
                }
            }
            
            var halfSize = spriteRect.size * 0.5f;
            
            //Pixel coordinates
            paths = new Vector2[regions.Count][];
            for (var i = 0; i < regions.Count; i++)
            {
                regions[i].GetOutline(RegionOutline);
                paths[i] = new Vector2[RegionOutline.Count];
                for (var j = 0; j < RegionOutline.Count; j++)
                {
                    paths[i][j] = RegionOutline[j] - halfSize;
                }
            }
        }
        
        private static bool IsEdge(IReadOnlyList<Color32> pixels, Vector2Int pt, Rect spriteRect, int textureWidth)
        {
            var tx0 = new Vector2Int((int)spriteRect.x + pt.x, (int)spriteRect.y + pt.y);
            var idx = (tx0.y * textureWidth) + tx0.x;

            //Check if index is out of range
            if (idx < 0 || idx >= pixels.Count)
            {
                return true;
            }

            //Check that texture coordinate is in bounds
            if (!spriteRect.Contains(tx0))
            {
                return true;
            }
            
            //If pixel is transparent then this is an edge
            var color = pixels[idx];
            return color.a == 0;
        }

        private static void SetVisited(IList<bool> visited, Vector2Int pt, Rect spriteRect, int textureWidth)
        {
            var textureX = (int)spriteRect.x + pt.x;
            var textureY = (int)spriteRect.y + pt.y;
            var idx = (textureY * textureWidth) + textureX;
            
            //Check if index is out of range
            if (idx < 0 || idx >= visited.Count)
            {
                return;
            }

            //Check that texture coordinate is in bounds
            if (!spriteRect.Contains(new Vector2(textureX, textureY)))
            {
                return;
            }

            visited[idx] = true;
        }

        private static bool CanVisit(IList<bool> visited, Vector2Int pt, Rect spriteRect, int textureWidth)
        {
            var textureX = (int)spriteRect.x + pt.x;
            var textureY = (int)spriteRect.y + pt.y;
            var idx = (textureY * textureWidth) + textureX;

            //Check if index is out of range
            if (idx < 0 || idx >= visited.Count)
            {
                return false;
            }

            //Check that texture coordinate is in bounds
            if (!spriteRect.Contains(new Vector2(textureX, textureY)))
            {
                return false;
            }

            //Check if pixel has already been visited
            if (visited[idx])
            {
                return false;
            }

            return true;
        }
        
        private static void AddPixels(PixelRegion region, IList<bool> visited, IReadOnlyList<Color32> pixels, Vector2Int pt, Rect spriteRect, int textureWidth)
        {
            var textureX = (int)spriteRect.x + pt.x;
            var textureY = (int)spriteRect.y + pt.y;
            var idx = (textureY * textureWidth) + textureX;

            //Check if index is out of range
            if (idx < 0 || idx >= pixels.Count)
            {
                return;
            }

            //Check that texture coordinate is in bounds
            if (!spriteRect.Contains(new Vector2(textureX, textureY)))
            {
                return;
            }

            //Check if pixel has already been visited
            if (visited[idx])
            {
                return;
            }
            
            visited[idx] = true;
            
            //If pixel is transparent then we're done after marking pixel as visited
            var color = pixels[idx];
            if (color.a == 0)
            {
                return;
            }
            
            var left = new Vector2Int(pt.x-1, pt.y);
            var right = new Vector2Int(pt.x+1, pt.y);
            var top = new Vector2Int(pt.x, pt.y+1);
            var bottom = new Vector2Int(pt.x, pt.y-1);
            var topRight = new Vector2Int(pt.x+1, pt.y+1);

            Vector2Int[] sidePts = {left, right, top, bottom};
            Vector2Edge[] edges =
            {
                new() { v0 = pt, v1 = top, }, //Left Edge
                new() { v0 = right, v1 = topRight, }, //Right Edge
                new() { v0 = top, v1 = topRight, }, //Top Edge
                new() { v0 = pt, v1 = right, }, //Bottom Edge
            };

            for (var i = 0; i < 4; i++)
            {
                var sidePt = sidePts[i];
                if (IsEdge(pixels, sidePts[i], spriteRect, textureWidth))
                {
                    //Add Edge
                    SetVisited(visited, sidePt, spriteRect, textureWidth);
                    region.AddEdge(edges[i]);
                }
                else
                {
                    if (CanVisit(visited, sidePt, spriteRect, textureWidth))
                    {
                        region.neighbors.Add(sidePt);
                    }
                }
            }
        }
        
        public static Vector2 CapPointToRect(Vector2 so, Rect r)
        {
            so.x = Mathf.Min(r.xMax, so.x);
            so.x = Mathf.Max(r.xMin, so.x);
            so.y = Mathf.Min(r.yMax, so.y);
            so.y = Mathf.Max(r.yMin, so.y);
            return so;
        }
    }
}