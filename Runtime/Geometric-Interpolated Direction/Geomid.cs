using System;
using System.Collections.Generic;
using UnityEngine;

namespace JonathanBryant
{
        /// <summary>
        /// Contains methods for calculating geometric-interpolated directions.
        /// </summary>
        public static class Geomid
        {
                /// <summary>
                /// Returns an average direction from two points on a cuboid.
                /// </summary>
                /// <param name="start">The starting point of the direction.</param>
                /// <param name="end">The ending point of the direction.</param>
                /// <returns>The average direction from the two points on a cuboid.</returns>
                public static Vector3 Cubid(Vector3 start, Vector3 end)
                {
                        Vector3 primaryDirection = DirectionToPoint(start, end);
                        Vector3 convergenceDirection = new Vector3(Mathf.Sign(primaryDirection.x), Mathf.Sign(primaryDirection.y), Mathf.Sign(primaryDirection.z));
                        return AverageDirection(new Vector3[] { primaryDirection, convergenceDirection });
                }

                /// <summary>
                /// Returns an average direction from multiple vertices.
                /// </summary>
                /// <param name="start">The starting point of the direction.</param>
                /// <param name="end">The ending point of the direction.</param>
                /// <param name="surroundingDirections">An array of surrounding directions.</param>
                /// <returns>The average direction from the starting point to the ending point and the surrounding directions.</returns>
                public static Vector3 Vertid(Vector3 start, Vector3 end, Vector3[] surroundingDirections)
                {
                        Vector3 primaryDirection = DirectionToPoint(start, end);
                        Vector3 averageSurroundingDirection = AverageDirection(surroundingDirections);

                        return AverageDirection(new Vector3[] { primaryDirection, averageSurroundingDirection });
                }

                /// <summary>
                /// Calculates the direction from the starting point to the end point of a mesh, as well as the average vertex and face normals surrounding the end point.
                /// </summary>
                /// <param name="start">The starting point of the direction.</param>
                /// <param name="renderer">The Renderer component of the mesh.</param>
                /// <returns>A tuple containing the direction from the starting point to the end point of the mesh, as well as the average vertex and face normals surrounding the end point.</returns>
                public static (Vector3 direction, Vector3 averageVertexNormal, Vector3 averageFaceNormal) MeshDirectionToPoint(Vector3 start, Renderer renderer)
                {
                        Vector3 end = renderer.bounds.center;
                        Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                        Vector3[] vertices = mesh.vertices;
                        int[] triangles = mesh.triangles;
                        Vector3[] vertexNormals = mesh.normals;
                        Vector3[] faceNormals = new Vector3[triangles.Length / 3];

                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                                Vector3 v1 = vertices[triangles[i]];
                                Vector3 v2 = vertices[triangles[i + 1]];
                                Vector3 v3 = vertices[triangles[i + 2]];
                                Vector3 faceNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
                                faceNormals[i / 3] = faceNormal;
                        }

                        List<Vector3> surroundingVertexNormals = new List<Vector3>();
                        List<Vector3> surroundingFaceNormals = new List<Vector3>();

                        for (int i = 0; i < vertices.Length; i++)
                        {
                                Vector3 vertexNormal = vertexNormals[i];
                                Vector3 vertexPosition = renderer.transform.TransformPoint(vertices[i]);

                                if (Vector3.Distance(vertexPosition, end) < 0.1f)
                                {
                                        surroundingVertexNormals.Add(vertexNormal);
                                }
                        }

                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                                Vector3 faceNormal = faceNormals[i / 3];
                                Vector3 v1 = vertices[triangles[i]];
                                Vector3 v2 = vertices[triangles[i + 1]];
                                Vector3 v3 = vertices[triangles[i + 2]];
                                Vector3 faceCenter = (v1 + v2 + v3) / 3f;

                                if (Vector3.Distance(renderer.transform.TransformPoint(faceCenter), end) < 0.1f)
                                {
                                        surroundingFaceNormals.Add(faceNormal);
                                }
                        }

                        Vector3 averageVertexNormal = AverageDirection(surroundingVertexNormals.ToArray());
                        Vector3 averageFaceNormal = AverageDirection(surroundingFaceNormals.ToArray());

                        return (DirectionToPoint(start, end), averageVertexNormal, averageFaceNormal);
                }

                /// <summary>
                /// Calculates the direction from one point to another.
                /// </summary>
                /// <param name="start">The starting point of the direction.</param>
                /// <param name="end">The ending point of the direction.</param>
                /// <returns>The direction from the starting point to the ending point.</returns>
                public static Vector3 DirectionToPoint(Vector3 start, Vector3 end)
                {
                        return (end - start).normalized;
                }

                /// <summary>
                /// Calculates the average direction from a set of directions.
                /// </summary>
                /// <param name="directions">An array of directions.</param>
                /// <returns>The average direction from the set of directions.</returns>
                public static Vector3 AverageDirection(Vector3[] directions)
                {
                        Vector3 sum = Vector3.zero;
                        foreach (Vector3 direction in directions)
                        {
                                sum += direction;
                        }
                        return (sum / directions.Length).normalized;
                }
        }
}