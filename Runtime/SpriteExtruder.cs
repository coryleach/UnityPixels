using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameframe.Pixels
{
    /// <summary>
    /// This component uses a SpriteRenderer to render additional mesh adding depth and a back face to the sprite
    /// </summary>
    [ExecuteAlways]
    public class SpriteExtruder : MonoBehaviour
    {
        /// <summary>
        /// SpriteRenderer that is rendering the target sprite.
        /// By default the component will look for it on the game object it is attached to.
        /// </summary>
        [SerializeField] private SpriteRenderer SpriteRenderer;

        /// <summary>
        /// Thickness of mesh that will be generated in the Z direction.
        /// </summary>
        [SerializeField] private float Thickness = 1f;

        /// <summary>
        /// Controls the mesh renderer's ShadowCastingMode property since sprites don't cast shadows this is probably off.
        /// </summary>
        [SerializeField] private ShadowCastingMode Shadows = ShadowCastingMode.Off;

        /// <summary>
        /// Faces that should be generated.
        /// By default should be all but the front face for SpriteExtruder component.
        /// </summary>
        [SerializeField] private SpriteFaces GenerateSpriteFaces = SpriteFaces.Back | SpriteFaces.Left | SpriteFaces.Right | SpriteFaces.Bottom | SpriteFaces.Top;

        /// <summary>
        /// MeshRenderer used for additional geometry.
        /// Hidden in Hierarchy and Inspector.
        /// Serialized so that references are correctly handled during copy-paste
        /// </summary>
        [SerializeField, HideInInspector] private MeshRenderer MeshRenderer;
        /// <summary>
        /// MeshFilter used for additional geometry.
        /// Hidden in Hierarchy and Inspector.
        /// Serialized so that references are correctly handled during copy-paste
        /// </summary>
        [SerializeField, HideInInspector] private MeshFilter MeshFilter;
        /// <summary>
        /// GameObject used for mesh components.
        /// Hidden in Hierarchy and Inspector.
        /// Serialized so that references are correctly handled during copy-paste
        /// </summary>
        [SerializeField, HideInInspector] private GameObject MeshGameObject;

        private bool _isRegistered = false;
        private Mesh _mesh;

        #region Unity Event Methods

        private void Awake()
        {
            CreateMeshObjects();
        }

        private void OnEnable()
        {
            if (SpriteRenderer == null)
            {
                SpriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (SpriteRenderer.isVisible)
            {
                RegisterCallbacks();
            }

            CreateMeshObjects();
            UpdateMesh();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void OnDestroy()
        {
            DestroyMeshObjects();
        }

        private void OnBecameVisible()
        {
            RegisterCallbacks();
            UpdateMesh();
        }

        private void OnBecameInvisible()
        {
            UnregisterCallbacks();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += _OnValidate;
        }

        private void _OnValidate()
        {
            if (SpriteRenderer == null)
            {
                SpriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (!Application.isPlaying)
            {
                UpdateMesh();
            }
        }
#endif

        #endregion

        private void UpdateMesh()
        {
            if (SpriteRenderer == null || _mesh == null)
            {
                return;
            }

            if (SpriteRenderer.sprite == null)
            {
                _mesh.Clear();
                return;
            }

            try
            {
                SpriteExtruderUtility.BuildMesh(
                    SpriteRenderer.sprite,
                    _mesh,
                    Thickness,
                    0f,
                    GenerateSpriteFaces);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }

            MeshRenderer.sharedMaterial = SpriteRenderer.sharedMaterial;
            MeshRenderer.sharedMaterial.mainTexture = SpriteRenderer.sprite.texture;
            MeshRenderer.shadowCastingMode = Shadows;
            MeshFilter.sharedMesh = _mesh;
        }

        private void CreateMeshObjects()
        {
            if (MeshGameObject == null)
            {
                MeshGameObject = new GameObject("SpriteMesh");
                MeshGameObject.transform.SetParent(transform, worldPositionStays: false);
                MeshGameObject.transform.localPosition = Vector3.zero;
                MeshGameObject.hideFlags = HideFlags.HideAndDontSave;
                return;
            }

            if (MeshRenderer == null)
            {
                MeshRenderer = MeshGameObject.GetComponent<MeshRenderer>();
                if (MeshRenderer == null)
                {
                    MeshRenderer = MeshGameObject.AddComponent<MeshRenderer>();
                }
            }

            if (MeshFilter == null)
            {
                MeshFilter = MeshGameObject.GetComponent<MeshFilter>();
                if (MeshFilter == null)
                {
                    MeshFilter = MeshGameObject.AddComponent<MeshFilter>();
                }
            }

            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = name
                };
            }

            MeshRenderer.sharedMaterial = SpriteRenderer.sharedMaterial;
            MeshFilter.sharedMesh = _mesh;
        }

        private void DestroyMeshObjects()
        {
            if (MeshGameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(MeshGameObject);
            }
            else
            {
                DestroyImmediate(MeshGameObject);
            }
        }

        private void RegisterCallbacks()
        {
            if (_isRegistered)
            {
                return;
            }

            _isRegistered = true;
            if (SpriteRenderer != null)
            {
                SpriteRenderer.RegisterSpriteChangeCallback(OnSpriteChanged);
            }
        }

        private void UnregisterCallbacks()
        {
            if (!_isRegistered)
            {
                return;
            }

            _isRegistered = false;
            if (SpriteRenderer != null)
            {
                SpriteRenderer.UnregisterSpriteChangeCallback(OnSpriteChanged);
            }
        }

        private void OnSpriteChanged(SpriteRenderer spriteRenderer)
        {
            UpdateMesh();
        }
    }
}