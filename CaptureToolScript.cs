using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureTool_Script
{
    public static class CaptureTool
    {
        public static void RandomizeCamera(string obj_name, float camera_min_dist, float camera_max_dist, float camera_max_angle, bool debug_mode=false)
        {
            if (!debug_mode)
            {
                //GameObject passport_obj = GameObject.Find("Passport");
                //GameObject inner_obj = passport_obj.transform.Find("Inner").gameObject;
                GameObject inner_obj = GameObject.Find(obj_name);
                MeshRenderer inner_renderer = inner_obj.GetComponent<MeshRenderer>();
                Bounds inner_bounds = inner_renderer.bounds;
                Bounds target_bounds = new Bounds(inner_bounds.center, inner_bounds.size * 0.5f);

                // (1) Target position
                Vector3 target_position = RandomPointInBounds(target_bounds);

                // (2) Camera position
                Vector3 camera_position = RandomPointInCone(target_position, Vector3.up, camera_max_angle, camera_min_dist, camera_max_dist);

                // (3) Up vector
                Vector3 camera_up = RandomUnitVectorPerpendicularTo(target_position - camera_position);

                // (4) Set
                Camera.main.transform.position = camera_position;
                Camera.main.transform.LookAt(target_position, camera_up);
            }
            
        }
        public static Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }
        public static Vector3 RandomPointInCone(Vector3 center, Vector3 dir, float max_angle, float min_radius, float max_radius)
        {
            Vector3 U = RandomDirectionInCone(dir, max_angle);

            float r = UnityEngine.Random.Range(min_radius, max_radius);
            Vector3 p = center + r * U;

            return p;
        }
        public static Vector3 RandomDirectionInCone(Vector3 dir, float max_angle)
        {
            Vector3 V = UnityEngine.Random.insideUnitSphere;
            Vector3 N = dir.normalized;

            Vector3 axis = Vector3.ProjectOnPlane(V, N);
            float angle = UnityEngine.Random.Range(0.0f, max_angle);

            Quaternion rot_quat = Quaternion.AngleAxis(angle, axis);
            Vector3 U = rot_quat * N;

            return U;
        }
        public static Vector3 RandomUnitVectorPerpendicularTo(Vector3 n)
        {
            Vector3 N = n.normalized;
            Vector3 V = UnityEngine.Random.insideUnitSphere;
            Vector3 P = Vector3.ProjectOnPlane(V, N).normalized;

            return P;
        }
        
    }
    
}

