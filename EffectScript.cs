using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object_Script;
using CaptureTool_Script;
using BoundingBox_Script;
using CalcCoord_Script;

namespace Effect_Script
{
    public static class Effect_Env
    {
        public static void TempleteBrightness()
        {
            float light_distance_max = 20f;
            float light_distance_min = 3f;
            float light_distance = UnityEngine.Random.Range(light_distance_min, light_distance_max);

            Transform light_transform = GameObject.Find("SpaceBrightness").transform;
            Vector3 light_pos = light_transform.position;
            Vector3 new_light_pos = new Vector3(light_pos.x, light_distance, light_pos.z);
            light_transform.position = new_light_pos;
        }
        public static void TempleteReflectLight_Point(List<string> feature_name_list)
        {
            float light_height = 0.29f;
            float light_intensity = 0.3f;

            int index = UnityEngine.Random.Range(0, feature_name_list.Count);
            List<Vector3> feature_model_coord = CalcCoord.FeatureWorldCoordInModel(feature_name_list[index]);
            Vector3 bot_left = feature_model_coord[0];
            Vector3 top_right = feature_model_coord[3];

            float target_pos_x = UnityEngine.Random.Range(bot_left.x, top_right.x);
            float target_pos_y = light_height;
            float target_pos_z = UnityEngine.Random.Range(top_right.z, bot_left.z);
            Vector3 target_point = new Vector3(target_pos_x, target_pos_y, target_pos_z);

            GameObject reflectionLight = GameObject.Find("ReflectionLight");
            if(reflectionLight != null)
            {
                reflectionLight.transform.position = target_point;
                reflectionLight.GetComponent<Light>().intensity = light_intensity;
            }
        }
        public static void EachChar_TempleteReflectLight_Point_On(List<string> feature_name_list)
        {
            GameObject effect_obj = GameObject.Find("ReflectionLight");
            effect_obj.GetComponent<Light>().intensity = 0.5f;
            // instance
            int i = 0;
            foreach(string feature_name in feature_name_list)
            {
                List<Vector3> feature_model_coord = CalcCoord.FeatureWorldCoordInModel(feature_name);
                Vector3 bot_left = feature_model_coord[0];
                Vector3 top_right = feature_model_coord[3];

                float obj_pos_x = UnityEngine.Random.Range(bot_left.x, top_right.x);
                float obj_pos_y = UnityEngine.Random.Range(0.35f, 0.45f);
                float obj_pos_z = UnityEngine.Random.Range(bot_left.z, top_right.z);

                Vector3 obj_pos = new Vector3(obj_pos_x, obj_pos_y, obj_pos_z);
                Quaternion obj_rot = effect_obj.transform.rotation;
                GameObject instance = UnityEngine.Object.Instantiate(effect_obj, obj_pos, obj_rot);
                instance.name = string.Format("ReflectionLight_{0}", i);
                i++;
            }
        }
        public static void EachChar_TempleteReflectRight_Point_Off(List<string> feature_name_list)
        {
            for(int i = 0; i < feature_name_list.Count; i++)
            {
                string instance_obj_name = string.Format("ReflectionLight_{0}", i);
                GameObject instance = GameObject.Find(instance_obj_name);
                UnityEngine.Object.Destroy(instance);
            }
            
        }
        
        public static void CharShadowOrLight_On(List<string> feature_name_list)
        {
            // shadow or light 
            int shadow_or_light = UnityEngine.Random.Range(0, 2);
            GameObject char_effect_obj;
            if (shadow_or_light == 0)
                char_effect_obj = GameObject.Find("CharShadow");
            else
                char_effect_obj = GameObject.Find("CharLight");
            //range
            float effect_size = UnityEngine.Random.Range(1.0f, 4.0f);
            //transform
            float x_offset = 2.0f;
            float z_offset = 3.0f;
            Bounds inner_bounds = GameObject.Find("Inner").GetComponent<MeshRenderer>().bounds;
            float obj_pos_x = UnityEngine.Random.Range(inner_bounds.min.x - x_offset, inner_bounds.max.x + x_offset);
            float obj_pos_y = UnityEngine.Random.Range(0.5f, 10.0f);
            float obj_pos_z = UnityEngine.Random.Range(inner_bounds.min.z - z_offset, inner_bounds.max.z + z_offset);         
            Vector3 obj_position = new Vector3(obj_pos_x, obj_pos_y, obj_pos_z);
            // choose one in feature list
            int index = UnityEngine.Random.Range(0, feature_name_list.Count);
            List<Vector3> feature_model_coord = CalcCoord.FeatureWorldCoordInModel(feature_name_list[index]);
            Vector3 bot_left = feature_model_coord[0];
            Vector3 top_right = feature_model_coord[3];
            //target position
            float target_pos_x = UnityEngine.Random.Range(bot_left.x, top_right.x);
            float target_pos_y = UnityEngine.Random.Range(bot_left.y, top_right.y); //strength
            float target_pos_z = UnityEngine.Random.Range(top_right.z, bot_left.z);
            Vector3 target_point = new Vector3(target_pos_x, target_pos_y, target_pos_z);

            if(char_effect_obj != null)
            {
                char_effect_obj.GetComponent<Projector>().enabled = true;
                char_effect_obj.GetComponent<Projector>().orthographicSize = effect_size;
                char_effect_obj.transform.position = obj_position;
                char_effect_obj.transform.LookAt(target_point);
            }
        }
        public static void CharShadowOrLight_Off()
        {
            GameObject.Find("CharShadow").GetComponent<Projector>().enabled = false;
            GameObject.Find("CharLight").GetComponent<Projector>().enabled = false;
        }
        public static void StudioBrightness_On()
        {
            GameObject spaceBrightness1 = GameObject.Find("SpaceBrightness1");
            GameObject spaceBrightness2 = GameObject.Find("SpaceBrightness2");
            if (spaceBrightness1 != null)
                GameObject.Find("SpaceBrightness1").GetComponent<Projector>().enabled = true;
            if(spaceBrightness2 != null)
                GameObject.Find("SpaceBrightness2").GetComponent<Projector>().enabled = true;
        }
        public static void StudioBrightness_Off()
        {
            GameObject spaceBrightness1 = GameObject.Find("SpaceBrightness1");
            GameObject spaceBrightness2 = GameObject.Find("SpaceBrightness2");
            if (spaceBrightness1 != null)
                GameObject.Find("SpaceBrightness1").GetComponent<Projector>().enabled = false;
            if (spaceBrightness2 != null)
                GameObject.Find("SpaceBrightness2").GetComponent<Projector>().enabled = false;
        }
    }
}

