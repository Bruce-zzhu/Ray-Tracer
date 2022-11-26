using System;
using System.Collections.Generic;

namespace RayTracer
{
  /// <summary>
  /// Class to represent a ray traced scene, including the objects,
  /// light sources, and associated rendering logic.
  /// </summary>
  public class Scene
  {
    private SceneOptions options;
    private ISet<SceneEntity> entities;
    private ISet<PointLight> lights;

    private double bias = 0.00001;

    /// <summary>
    /// Construct a new scene with provided options.
    /// </summary>
    /// <param name="options">Options data</param>
    public Scene(SceneOptions options = new SceneOptions())
    {
      this.options = options;
      this.entities = new HashSet<SceneEntity>();
      this.lights = new HashSet<PointLight>();
    }

    /// <summary>
    /// Add an entity to the scene that should be rendered.
    /// </summary>
    /// <param name="entity">Entity object</param>
    public void AddEntity(SceneEntity entity)
    {
      this.entities.Add(entity);
    }

    /// <summary>
    /// Add a point light to the scene that should be computed.
    /// </summary>
    /// <param name="light">Light structure</param>
    public void AddPointLight(PointLight light)
    {
      this.lights.Add(light);
    }

    /// <summary>
    /// Render the scene to an output image. This is where the bulk
    /// of your ray tracing logic should go... though you may wish to
    /// break it down into multiple functions as it gets more complex!
    /// </summary>
    /// <param name="outputImage">Image to store render output</param>
    public void Render(Image outputImage)
    {
      int width = outputImage.Width;
      int height = outputImage.Height;

      int numRays = this.options.AAMultiplier;
      int depth = 4;

      for (int h = 0; h < height; h++)
      {
        for (int w = 0; w < width; w++)
        {
          Color sumColor = new Color(0, 0, 0);
          for (int i = 0; i < numRays; i++)
          {
            for (int j = 0; j < numRays; j++)
            {
              Ray ray = new Ray(new Vector3(0, 0, 0), getPixelVector(w, h, j, i, width, height, numRays));
              SceneEntity entity = getClosestHit(ray);
              if (entity == null) continue;  // no entity hit

              RayHit hit = entity.Intersect(ray);
              sumColor += castRay(entity, hit, depth);
            }

          }

          Color color = sumColor / (numRays * numRays);

          outputImage.SetPixel(w, h, color);
        }
      }



    }


    // convert a pixel into Vector3
    private Vector3 getPixelVector(int x, int y, int x2, int y2, double width, double height, int numRays)
    {
      double fov = Math.PI / 3;
      double aspectRatio = height / width;

      double pixelX = (x + (double)x2 / numRays + 0.5 / numRays) / width;
      double pixelY = (y + (double)y2 / numRays + 0.5 / numRays) / height;
      // Console.WriteLine(pixelX);

      double xPos = pixelX * 2 - 1;
      double yPos = 1 - pixelY * 2;
      xPos = xPos * Math.Tan(fov / 2);
      yPos = yPos * (Math.Tan(fov / 2) / aspectRatio);

      return new Vector3(xPos, yPos, 1);
    }

    private SceneEntity getClosestHit(Ray ray)
    {
      SceneEntity closestEntity = null;
      double distanceSq = Double.MaxValue;
      foreach (SceneEntity entity in this.entities)
      {
        RayHit hit = entity.Intersect(ray);
        if (hit == null) continue;  // entity not hit

        double curDistanceSq = (hit.Position - ray.Origin).LengthSq();
        // check if this is the entity closest to the origin
        if (curDistanceSq < distanceSq && Math.Sqrt(curDistanceSq) > 1e-6)
        {
          distanceSq = curDistanceSq;
          closestEntity = entity;
        }
      }

      return closestEntity;

    }



    private Color castRay(SceneEntity entity, RayHit hit, int depth)
    {
      // the last color which is the material color of the entity
      if (depth == 0) return entity.Material.Color;

      Color color = new Color(0, 0, 0);

      switch (entity.Material.Type)
      {
        case Material.MaterialType.Diffuse:
          // apply diffuse material colors
          color = applyDiffuseShadow(hit, entity);
          break;        
        case Material.MaterialType.Reflective:
          Vector3 reflectVec = calcReflectVec(hit);
          Ray reflectRay = new Ray(hit.Position, reflectVec);
          SceneEntity nextEntity = getClosestHit(reflectRay);

          // hit an entity
          if (nextEntity != null)
          {
            RayHit nextHit = nextEntity.Intersect(reflectRay);
            color += castRay(nextEntity, nextHit, depth - 1);
          }

          break;

        case Material.MaterialType.Refractive:

          Color refleColor = new Color(0, 0, 0);
          Color refraColor = new Color(0, 0, 0);

          double k = fresnel(entity, hit);

          if (k < 1)
          {
            Vector3 refractDir = getRefractionDir(entity, hit);
            if (refractDir.ToString() != hit.Position.ToString())
            {
              Ray refractRay = new Ray(hit.Position, refractDir);
              SceneEntity nextEnt = getClosestHit(refractRay);
              if (nextEnt != null)
              {
                RayHit nextHit = nextEnt.Intersect(refractRay);
                // implement Beer's law
                double distance = (nextHit.Position - hit.Position).Length();
                var (r, g, b) = (entity.Material.Color.R, entity.Material.Color.G, entity.Material.Color.B);
                Color absorb = new Color(Math.Exp(-r * distance), Math.Exp(-g * distance), Math.Exp(-b * distance));
                refraColor = castRay(nextEnt, nextHit, depth - 1) * absorb;
              }
            }
          }

          Vector3 reflectDir = calcReflectVec(hit);
          Ray reflecRay = new Ray(hit.Position, reflectDir);
          SceneEntity hitEntity = getClosestHit(reflecRay);

          // hit an entity
          if (hitEntity != null)
          {
            RayHit nextHit = hitEntity.Intersect(reflecRay);
            refleColor = castRay(hitEntity, nextHit, depth - 1);
          }

          color += refleColor * k + refraColor * (1 - k);


          break;

        default:
          color = entity.Material.Color;
          break;
      }

      return color;

    }


    // apply shadow and color to diffuse materials
    private Color applyDiffuseShadow(RayHit hit, SceneEntity entity)
    {
      Color color = new Color(0, 0, 0);
      foreach (PointLight light in this.lights)
      {
        bool shadow = false;
        // direction from the hit point to the light source
        Vector3 lightDirection = (light.Position - hit.Position).Normalized();
        // check if there is a shadow by firing a back ray from the hit point
        Ray backRay = new Ray(hit.Position, lightDirection);
        // check if the back ray would hit any entity
        foreach (SceneEntity ent in this.entities)
        {
          RayHit backHit = ent.Intersect(backRay);
          if (backHit != null)
          {
            // make sure the hit is not itself
            double backHitDistSq = (backHit.Position - hit.Position).LengthSq();
            double lightDistSq = (light.Position - hit.Position).LengthSq();

            if (backHitDistSq < lightDistSq && Math.Sqrt(backHitDistSq) > 1e-6)
            {
              // there is a shadow here
              shadow = true;
              break;
            }
          }
        }

        // only the light source making no shadow contributes to the color
        if (!shadow)
        {
          // make sure the entity is in front of the light
          double deno = hit.Normal.Dot(lightDirection);
          if (deno > 1e-6)
          {
            color += entity.Material.Color * light.Color * hit.Normal.Dot(lightDirection);
          }
        }
      }

      return color;
    }


    private Vector3 calcReflectVec(RayHit hit)
    {
      return hit.Incident - 2 * hit.Normal.Dot(hit.Incident) * hit.Normal;
    }

    private Vector3 getRefractionDir(SceneEntity entity, RayHit hit)
    {
      double inInx = 1;
      double outInx = entity.Material.RefractiveIndex;
      Vector3 normal = hit.Normal;
      double cos = Math.Max(-1, Math.Min(1, normal.Dot(hit.Incident)));

      double dotNorI = hit.Normal.Dot(hit.Incident);
      if (dotNorI < 0)
      {
        // outside the surface, cos needs to be positive
        dotNorI = -dotNorI;
      }
      else
      {
        // inside the surface, cos is positive but need to revert the normal
        normal = -normal;
        // swap refraction indices
        double temp = inInx;
        inInx = outInx;
        outInx = temp;
      }

      double rate = inInx / outInx;
      double k = 1 - rate * rate * (1 - cos * cos);

      if (k < 0)
      {
        // total internal reflection, no refraction
        return hit.Position;
      }
      return rate * hit.Incident + (rate * dotNorI - Math.Sqrt(k)) * normal;

    }

    private double fresnel(SceneEntity entity, RayHit hit)
    {
      double inInx = 1;
      double outInx = entity.Material.RefractiveIndex;
      double cos = hit.Normal.Dot(hit.Incident);

      if (cos > 0) (inInx, outInx) = (outInx, inInx);

      double sint = inInx / outInx * Math.Sqrt(Math.Max(0, 1 - cos * cos));

      double k = 0;

      if (sint >= 1)
      {
        // total internal reflection
        k = 1;
      }
      else
      {
        double cost = Math.Sqrt(Math.Max(0, 1 - sint * sint));
        cos = Math.Abs(cos);
        double rs = (outInx * cos - inInx * cost) / (outInx * cos + inInx * cost);
        double rp = (inInx * cos - outInx * cost) / (inInx * cos + outInx * cost);
        k = (rs * rs + rp * rp) / 2;

      }

      return k;
    }



  }
}
