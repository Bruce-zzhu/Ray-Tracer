using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RayTracer
{
  /// <summary>
  /// Add-on option C. You should implement your solution in this class template.
  /// </summary>
  public class ObjModel : SceneEntity
  {
    private Material material;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> vertNormals = new List<Vector3>();
    private List<Vector3> vertNormal0s = new List<Vector3>();
    private List<Vector3> vertNormal1s = new List<Vector3>();
    private List<Vector3> vertNormal2s = new List<Vector3>();
    private List<Triangle> faces = new List<Triangle>();
    private Sphere boundingSphere;   // create a sphere boundry for the obj to speed up the rendering process

    /// <summary>
    /// Construct a new OBJ model.
    /// </summary>
    /// <param name="objFilePath">File path of .obj</param>
    /// <param name="offset">Vector each vertex should be offset by</param>
    /// <param name="scale">Uniform scale applied to each vertex</param>
    /// <param name="material">Material applied to the model</param>
    public ObjModel(string objFilePath, Vector3 offset, double scale, Material material)
    {
      this.material = material;

      double maxX = Double.MinValue, minX = Double.MaxValue,
      maxY = Double.MinValue, minY = Double.MaxValue,
      maxZ = Double.MinValue, minZ = Double.MaxValue;

      try
      {
        // Here's some code to get you started reading the file...
        string[] lines = File.ReadAllLines(objFilePath);

        for (int l = 0; l < lines.Length; l++)
        {
          string[] line = lines[l].Split().Where(x => !string.IsNullOrEmpty(x)).ToArray(); ;
          if (line.Length == 0) continue;

          switch (line[0])
          {
            case "v":
              // vertex
              double x = Double.Parse(line[1]) * scale + offset.X;
              double y = Double.Parse(line[2]) * scale + offset.Y;
              double z = Double.Parse(line[3]) * scale + offset.Z;
              Vector3 vertex = new Vector3(x, y, z);
              vertices.Add(vertex);

              // Collect the sphere boundry values
              if (x > maxX) maxX = x;
              else if (x < minX) minX = x;
              if (y > maxY) maxY = y;
              else if (y < minY) minY = y;
              if (z > maxZ) maxZ = z;
              else if (z < minZ) minZ = z;

              break;
            case "vn":
              // vertex normal
              Vector3 n = new Vector3(Double.Parse(line[1]), Double.Parse(line[2]), Double.Parse(line[3]));
              vertNormals.Add(n);
              break;
            case "f":
              // face
              int v0Index, v1Index, v2Index;
              // index in obj file starts from 1
              v0Index = Int32.Parse(line[1].Split('/')[0]) - 1;
              v1Index = Int32.Parse(line[2].Split('/')[0]) - 1;
              v2Index = Int32.Parse(line[3].Split('/')[0]) - 1;

              Vector3 v0, v1, v2;
              v0 = vertices[v0Index];
              v1 = vertices[v1Index];
              v2 = vertices[v2Index];

              if (vertNormals.Count > v2Index)
              {
                vertNormal0s.Add(vertNormals[v0Index]);
                vertNormal1s.Add(vertNormals[v1Index]);
                vertNormal2s.Add(vertNormals[v2Index]);
              }


              faces.Add(new Triangle(v0, v1, v2, material));
              break;

            default:
              break;
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }



      double centerX = (maxX + minX) / 2;
      double centerY = (maxY + minY) / 2;
      double centerZ = (maxZ + minZ) / 2;

      double diffX = maxX - minX;
      double diffY = maxY - minY;
      double diffZ = maxZ - minZ;

      double radius = Math.Max(Math.Max(diffX, diffY), diffZ) / 2;
      Vector3 center = new Vector3(centerX, centerY, centerZ);

      this.boundingSphere = new Sphere(center, radius, material);


    }

    /// <summary>
    /// Given a ray, determine whether the ray hits the object
    /// and if so, return relevant hit data (otherwise null).
    /// </summary>
    /// <param name="ray">Ray data</param>
    /// <returns>Ray hit data, or null if no hit</returns>
    public RayHit Intersect(Ray ray)
    {
      RayHit closestHit = null;
      double distanceSq = Double.MaxValue;

      // if sphere not hit, obj not hit
      if (this.boundingSphere.Intersect(ray) == null) return null;

      int faceCount = 0;
      foreach (Triangle face in this.faces)
      {
        RayHit hit = null;
        if (vertNormals.Count == vertices.Count)
        {
          Vector3 n0 = vertNormal0s[faceCount];
          Vector3 n1 = vertNormal1s[faceCount];
          Vector3 n2 = vertNormal2s[faceCount];
          faceCount += 1;

          hit = face.Intersect(ray, n0, n1, n2);
        }
        else
        {
          hit = face.Intersect(ray);
        }

        if (hit != null)
        {
          // find the closeset hit
          double currDistanceSq = (hit.Position - ray.Origin).LengthSq();
          if (currDistanceSq < distanceSq && Math.Sqrt(currDistanceSq) > 1e-6)
          {
            distanceSq = currDistanceSq;
            closestHit = hit;
          }
        }
      }

      if (closestHit != null) return closestHit;



      return null;
    }

    /// <summary>
    /// The material attached to this object.
    /// </summary>
    public Material Material { get { return this.material; } }

    public List<Vector3> Vertices { get { return this.vertices; } }
    public List<Vector3> VertNormals { get { return this.vertNormals; } }
    public List<Triangle> Faces { get { return this.faces; } }
  }

}
