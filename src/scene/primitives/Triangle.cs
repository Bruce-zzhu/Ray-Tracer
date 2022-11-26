using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            
            Vector3 normal = (this.v1 - this.v0).Cross(this.v2 - this.v0).Normalized();
            Vector3 rayDirection = ray.Direction.Normalized();

            // check if the ray is parallel
            double dotRayDir = normal.Dot(rayDirection);
            if (Math.Abs(dotRayDir) < 1e-6) return null;    // parallel, no intersection

            double dis = -normal.Dot(this.v0);
            double t = -((normal.Dot(ray.Origin) + dis) / dotRayDir);

            // check if the triangle is in behind the ray
            if (t < 1e-6) return null;

            Vector3 point = ray.Origin + t * rayDirection;

            // inside-out test
            Vector3 testVec;

            Vector3 edge0 = v1 - v0;
            Vector3 vp0 = point - this.v0;
            testVec = edge0.Cross(vp0);
            if (normal.Dot(testVec) < 0) return null;

            Vector3 edge1 = v2 - v1;
            Vector3 vp1 = point - this.v1;
            testVec = edge1.Cross(vp1);
            if (normal.Dot(testVec) < 0) return null;

            Vector3 edge2 = v0 - v2;
            Vector3 vp2 = point - this.v2;
            testVec = edge2.Cross(vp2);
            if (normal.Dot(testVec) < 0) return null;


            return new RayHit(point, normal, rayDirection, this.material);
        }


        public RayHit Intersect(Ray ray, Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Vector3 v01 = this.v1 - this.v0;
            Vector3 v02 = this.v2 - this.v0;
            Vector3 vp = ray.Direction.Cross(v02);
            double deno = v01.Dot(vp);

            if (Math.Abs(deno) < 1e-6) return null;

            double invDeno = 1 / deno;

            Vector3 vt = ray.Origin - v0;
            double u = vt.Dot(vp) * invDeno;

            if (u < 0 || u > 1) return null;

            Vector3 vq = vt.Cross(v01);
            double v = ray.Direction.Dot(vq) * invDeno;
            if (v < 0 || u + v > 1) return null;

            double t = v02.Dot(vq) * invDeno;

            if (t > 1e-6)
            {
                Vector3 hitPoint = ray.Origin + t * ray.Direction;
                Vector3 normal = ((1 - u - v) * n0 + u * n1 + v * n2).Normalized();

                return new RayHit(hitPoint, normal, ray.Direction.Normalized(), this.material);
            }

            
            return null;
        }


        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }

        public Vector3 V0 { get { return this.v0; } }
        public Vector3 V1 { get { return this.v1; } }
        public Vector3 V2 { get { return this.v2; } }

    }

}
