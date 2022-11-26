using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            double t0 = 0;
            double t1 = 0;
            double t = 0;

            Vector3 rayDir = ray.Direction.Normalized();

            Vector3 voc = this.center - ray.Origin;
            double tca = voc.Dot(rayDir);
            if (tca < 0) return null;
            double d2 = voc.Dot(voc) - tca * tca;
            if (d2 > this.radius * this.radius) return null; // outside the sphere
            double thc = Math.Sqrt(this.radius * this.radius - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            // Vector3 vco = ray.Origin - this.center;
            // double a = rayDir.Dot(rayDir);
            // double b = 2 * rayDir.Dot(vco);
            // double c = vco.Dot(vco) - this.radius * this.radius;
            // if (!solveQuadratic(a, b, c, ref t0, ref t1)) return null;

            

            t = Math.Min(t0, t1);
            if (t < 1e-6) {
                t = Math.Max(t0, t1);
                if (t < 1e-6) {
                    return null;
                }
            }

            Vector3 point = ray.Origin + t * rayDir;
            Vector3 normal = (point - this.center).Normalized();

            return new RayHit(point, normal, rayDir, this.material);
            
        }

        private bool solveQuadratic(double a, double b, double c, ref double t0, ref double t1) {
            double delta = b * b - 4 * a * c;
            if (delta < 0) {
                // miss the ball
                return false;
            } else if (delta == 0) {
                // only one point
                t0 = t1 = -0.5 * b / a;
            } else {
                // two points
                double q;
                if (b > 0) {
                    q = -0.5 * (b + Math.Sqrt(delta));
                } else {
                    q = -0.5 * (b - Math.Sqrt(delta));
                }
                t0 = q / a;
                t1 = c / q;

            }
            
            return true;
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
