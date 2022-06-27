using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameframe.Pixels
{
    public static class SpriteExtruderUtility
    {
        /// <summary>
        /// This is the amount of padding added to the UVs that extrude part of the sprite.
        /// This helps stretch the color of the vertex across the mesh's top, bottom, left, and right.
        /// </summary>
        private const float UVPadding = 0.0001f;

        /// <summary>
        /// Construct a 3D mesh from a Sprite.
        /// Note: This requires that the outline of the sprite be a pixel perfect exact outline of the sprite.
        /// </summary>
        /// <param name="sprite">Sprite for which the mesh will be built</param>
        /// <param name="mesh">Mesh object that will receive the verts, tris, UVs, and normals</param>
        /// <param name="thickness">Thickness of the mesh in the Z direction</param>
        /// <param name="anchor">0 builds the mesh with the pivot on the same plane as the front, 0.5 is the middle, and 1 is the back.</param>
        /// <param name="generateFaces">Faces to be generated. Defaults to SpriteFaces.All.</param>
        /// <param name="uvPadding">Amount of padding to be added to the UVs that extrude from the front and back.</param>
        public static void BuildMesh(Sprite sprite, Mesh mesh, float thickness = 1,
            float anchor = 0, SpriteFaces generateFaces = SpriteFaces.All, float uvPadding = UVPadding)
        {
            if (sprite == null)
            {
                return;
            }

            var depth = Vector3.forward * thickness;
            var frontZ = -depth.z * anchor;
            var edges = new List<Vector3Edge>();

            for (var i = 0; i < sprite.triangles.Length; i += 3)
            {
                var idx0 = sprite.triangles[i];
                var idx1 = sprite.triangles[i + 1];
                var idx2 = sprite.triangles[i + 2];

                var v0 = (Vector3) sprite.vertices[idx0];
                var v1 = (Vector3) sprite.vertices[idx1];
                var v2 = (Vector3) sprite.vertices[idx2];

                v0.z = frontZ;
                v1.z = frontZ;
                v2.z = frontZ;

                var uv0 = sprite.uv[idx0];
                var uv1 = sprite.uv[idx1];
                var uv2 = sprite.uv[idx2];

                edges.Add(new Vector3Edge(i, v0, v1, uv0, uv1));
                edges.Add(new Vector3Edge(i + 1, v1, v2, uv1, uv2));
                edges.Add(new Vector3Edge(i + 2, v2, v0, uv2, uv0));
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            if (generateFaces.Check(SpriteFaces.Front))
            {
                vertices.AddRange(sprite.vertices.Select(x => new Vector3(x.x, x.y, frontZ)));
                triangles.AddRange(sprite.triangles.Select(x => (int) x));
                uvs.AddRange(sprite.uv);
                normals.AddRange(vertices.Select(x => Vector3.back));
            }

            if (generateFaces.Check(SpriteFaces.Back) && thickness != 0)
            {
                var count = sprite.triangles.Length;
                for (var i = 0; i < count; i += 3)
                {
                    var idx0 = sprite.triangles[i];
                    var idx1 = sprite.triangles[i + 1];
                    var idx2 = sprite.triangles[i + 2];

                    var v0 = sprite.vertices[idx0];
                    var v1 = sprite.vertices[idx1];
                    var v2 = sprite.vertices[idx2];

                    var uv0 = sprite.uv[idx0];
                    var uv1 = sprite.uv[idx1];
                    var uv2 = sprite.uv[idx2];

                    //Append Back Vertices
                    var idx = vertices.Count;
                    vertices.Add(new Vector3(v0.x, v0.y, frontZ) + depth);
                    vertices.Add(new Vector3(v1.x, v1.y, frontZ) + depth);
                    vertices.Add(new Vector3(v2.x, v2.y, frontZ) + depth);

                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);

                    triangles.Add(idx + 2);
                    triangles.Add(idx + 1);
                    triangles.Add(idx);

                    uvs.Add(uv0);
                    uvs.Add(uv1);
                    uvs.Add(uv2);
                }
            }

            if (thickness != 0)
            {
                //Get Unique Edges
                var uniqueEdges = edges.Where(x => !edges.Exists(x.IsSameButDifferentEdge)).ToList();

                //Create a quad in the z plane for each unique edge
                foreach (var edge in uniqueEdges)
                {
                    var v0 = edge.v0;
                    var v1 = edge.v1;
                    var v2 = v0 + depth;
                    var v3 = v1 + depth;

                    var dir = v1 - v0;
                    var normalizedDir = (Vector2) dir.normalized;

                    Vector2 padding;
                    Vector3 normal;
                    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    {
                        //Horizontal
                        if (dir.x > 0)
                        {
                            if (!generateFaces.Check(SpriteFaces.Top))
                            {
                                continue;
                            }

                            //Right
                            padding = Vector2.down * uvPadding;
                            normal = Vector3.up;
                        }
                        else
                        {
                            if (!generateFaces.Check(SpriteFaces.Bottom))
                            {
                                continue;
                            }

                            //Left
                            padding = Vector2.up * uvPadding;
                            normal = Vector3.down;
                        }
                    }
                    else
                    {
                        //Vertical
                        if (dir.y > 0)
                        {
                            if (!generateFaces.Check(SpriteFaces.Left))
                            {
                                continue;
                            }

                            //Up
                            padding = Vector2.right * uvPadding;
                            normal = Vector3.left;
                        }
                        else
                        {
                            if (!generateFaces.Check(SpriteFaces.Right))
                            {
                                continue;
                            }

                            //Down
                            padding = Vector2.left * uvPadding;
                            normal = Vector3.right;
                        }
                    }

                    var uv0 = edge.uv0;
                    var uv1 = edge.uv1;
                    var uv2 = edge.uv0 + padding + (normalizedDir * uvPadding);
                    var uv3 = edge.uv1 + padding - (normalizedDir * uvPadding);

                    // V0 ----- V2
                    // |      /  |
                    // |  /      |
                    // V1 ----- V3

                    //Two Triangles for a quad
                    var idx0 = vertices.Count;

                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);

                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(uv0);
                    uvs.Add(uv1);
                    uvs.Add(uv2);
                    uvs.Add(uv3);

                    //Triangle 1
                    triangles.Add((idx0 + 2));
                    triangles.Add((idx0 + 1));
                    triangles.Add(idx0);

                    //Triangle 2
                    triangles.Add(idx0 + 3);
                    triangles.Add(idx0 + 1);
                    triangles.Add(idx0 + 2);
                }
            }

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();
        }
        
        /// <summary>
        /// Test if set of vertices is ordered clockwise.
        /// </summary>
        /// <param name="v0">First Vertex</param>
        /// <param name="v1">Second Vertex</param>
        /// <param name="v2">Thrid Vertex</param>
        /// <returns>True if vertices are in clockwise order</returns>
        public static bool IsClockwise(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            float sum = 0;
            sum += (v1.x - v0.x) * (v1.y - v0.y);
            sum += (v2.x - v1.x) * (v2.y - v1.y);
            sum += (v0.x - v2.x) * (v0.y - v2.y);
            return sum > 0;
        }
    }
}