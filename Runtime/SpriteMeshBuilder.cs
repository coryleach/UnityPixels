using System;
using UnityEngine;

namespace Gameframe.Pixels
{
    /// <summary>
    /// This component takes a sprite reference and builds a 3D Mesh for it.
    /// </summary>
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class SpriteMeshBuilder : MonoBehaviour
    {
        [SerializeField] private Sprite Sprite;
        [SerializeField] private MeshFilter MeshFilter;
        [SerializeField] private MeshRenderer MeshRenderer;
        
        /// <summary>
        /// Thickness of mesh that will be generated in the Z direction.
        /// </summary>
        [SerializeField] private float Thickness = 1f;
        
        /// <summary>
        /// Faces that should be generated.
        /// By default should be all but the front face for SpriteExtruder component.
        /// </summary>
        [SerializeField] private SpriteFaces GenerateFaces = SpriteFaces.All;
        
        /// <summary>
        /// The location of the pivot within the Z plane of the mesh.
        /// Anchor value of 0 will be the front face's plane. Anchor value of 1 will be the back face's plane.
        /// </summary>
        [SerializeField, Range(0, 1f)] private float Anchor = 0.5f;
        
        /// <summary>
        /// If true mesh.optimize() will be run after building the mesh.
        /// </summary>
        [SerializeField] private bool OptimizeAfterBuild = true;
        
        private Mesh _mesh;

        #region Unity Event Methods

        private void OnEnable()
        {
            BuildMesh();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += _OnValidate;
        }

        private void _OnValidate()
        {
            if (MeshFilter == null)
            {
                MeshFilter = GetComponent<MeshFilter>();
            }

            if (MeshRenderer == null)
            {
                MeshRenderer = GetComponent<MeshRenderer>();
            }

            if (!Application.isPlaying)
            {
                BuildMesh();
            }
        }
#endif

        private void OnDidApplyAnimationProperties()
        {
            BuildMesh();
        }
        
        #endregion

        public void SetSprite(Sprite sprite)
        {
            Sprite = sprite;
            BuildMesh();
        }

        public Sprite GetSprite() => Sprite;

        private void BuildMesh()
        {
            if (Sprite == null)
            {
                if (_mesh != null)
                {
                    _mesh.Clear();
                }
                return;
            }

            if (_mesh == null)
            {
                _mesh = new Mesh()
                {
                    name = name
                };
            }

            try
            {
                SpriteExtruderUtility.BuildMesh(
                    Sprite,
                    _mesh,
                    Thickness,
                    Anchor,
                    GenerateFaces);

                if (OptimizeAfterBuild)
                {
                    _mesh.Optimize();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            MeshFilter.sharedMesh = _mesh;
            MeshRenderer.sharedMaterial.mainTexture = Sprite.texture;
        }

    }
}