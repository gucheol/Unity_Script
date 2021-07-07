using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Segmentation_Script
{
    public static class PresetColor
    {
        //Preset Color
        public static float[] white = new float[] { 1f, 1f, 1f, 1f };
        public static float[] black = new float[] { 0f, 0f, 0f, 1f };
        public static float[] gray = new float[] { 0.5f, 0.5f, 0.5f, 1f };
        public static float[] red = new float[] { 1f, 0f, 0f, 1f };
        public static float[] blue = new float[] { 0f, 0f, 1f, 1f };
        public static float[] green = new float[] { 0f, 1f, 0f, 1f };
        public static float[] cyan = new float[] { 0f, 0f, 1f, 1f };
        public static float[] magenta = new float[] { 1f, 0f, 1f, 1f };
        public static float[] yellow = new float[] { 1f, 0.92f, 0.016f, 1f };
        public static float[] brown = new float[] { 0.403f, 0.239f, 0.050f, 1f };
        public static float[] navy = new float[] { 0.054f, 0.156f, 0.356f, 1f };
        public static float[] violet = new float[] { 0.462f, 0.027f, 0.682f, 1f };
        public static float[] purple = new float[] { 0.780f, 0f, 0.874f, 1f };
        public static float[] orange = new float[] { 0.921f, 0.454f, 0.047f, 1f };
        public static float[] pink = new float[] { 0.988f, 0.631f, 0.901f, 1f };
        public static Color kor_red = new Color(0.9803922f, 0.345098f, 0.2862745f, 1f);
        public static Color kor_yellow = new Color(0.9843137f, 0.8156863f, 0.4431372f, 1f);
        public static Color kor_brown = new Color(0.3764706f, 0.2666667f, 0.1137255f, 1f);
        //Segmenation Color
        public static float[] person_image = new float[] {0.0f, 0.0f, 1.0f, 1.0f };
        public static float[] object_background = new float[] { 0.0f, 1.0f, 0.0f, 1.0f };
        public static float[] kor = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        public static float[] stamp = new float[] { 0.40000f, 0.18823f, 0.08627f, 1.0f }; //0.086274512112140656f
        public static float[] mr = new float[] { 1.0f, 0.92156f, 0.01568f, 1.0f };
    }
    public static class Seg
    {
        public static float z_child_offset = -0.02f;
        public static float z_parnet_offest = -0.02f;
        public static List<Vector3> CalcTMPBounds(string TMPObjectName)
        {
            GameObject TMPObject = GameObject.Find(TMPObjectName);
            TMP_Text textComponent = TMPObject.GetComponent<TMP_Text>();

            float z_offset = z_child_offset;
            if (TMPObject.transform.parent != null)
            {           
                if (TMPObject.transform.parent.parent != null)
                    z_offset = z_child_offset + z_parnet_offest * 2;
                else
                    z_offset = z_child_offset + z_parnet_offest;
            }

            Vector3 passportScale;
            if (textComponent.transform.parent != null)
            {
                if (textComponent.transform.parent.parent != null)
                    passportScale = textComponent.transform.parent.localScale + textComponent.transform.parent.parent.localScale;
                else
                    passportScale = textComponent.transform.parent.localScale;
            }
            else
                passportScale = new Vector3(1, 1, 1);


            Vector3 textScale = textComponent.rectTransform.localScale;
            Vector3 scale = new Vector3(passportScale.x * textScale.x, passportScale.y * textScale.y, passportScale.z * textScale.z);

            textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = textComponent.textInfo;
            TMP_CharacterInfo[] charInfos = textComponent.textInfo.characterInfo;

            float max_y = 0;
            float min_y = 0;
            float min_x = charInfos[0].vertex_BL.position.x;
            float max_x = charInfos[textInfo.characterCount - 1].vertex_BR.position.x;

            foreach (TMP_CharacterInfo charinfo in charInfos)
            {
                if (min_y > charinfo.vertex_BL.position.y)
                    min_y = charinfo.vertex_BL.position.y;
                if (max_y < charinfo.vertex_TL.position.y)
                    max_y = charinfo.vertex_TL.position.y;
            }

            Vector3 bot_left = new Vector3(min_x * scale.x, min_y * scale.y, z_offset) + textComponent.transform.position;
            Vector3 top_left = new Vector3(min_x * scale.x, max_y * scale.y, z_offset) + textComponent.transform.position;
            Vector3 top_right = new Vector3(max_x * scale.x, max_y * scale.y, z_offset) + textComponent.transform.position;
            Vector3 bot_right = new Vector3(max_x * scale.x, min_y * scale.y, z_offset) + textComponent.transform.position;

            List<Vector3> tmp_bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right };
            return tmp_bounds;
        }
        public static List<Vector3> CalcRectObjectBounds(string targetName)
        {
            GameObject rect = GameObject.Find(targetName);
            MeshFilter rectMesh = rect.GetComponent<MeshFilter>();

            float z_offset = z_child_offset;
            if (rect.transform.parent != null)
            {
                if (rect.transform.parent.parent != null)
                    z_offset = z_child_offset + z_parnet_offest * 2;
                else
                    z_offset = z_child_offset + z_parnet_offest;
            }

            Vector3 rectScale = rect.transform.localScale;
            Vector3 parentScale;
            if (rect.transform.parent != null)
            {
                if (rect.transform.parent.parent != null)
                    parentScale = rect.transform.parent.localScale + rect.transform.parent.parent.localScale;
                else
                    parentScale = rect.transform.parent.localScale;
            }
            else
                parentScale = new Vector3(1, 1, 1);

            Vector3[] rectVertices = rectMesh.mesh.vertices;
            Vector3 scale = new Vector3(rectScale.x * parentScale.x, rectScale.y * parentScale.y, 1);

            Vector3 bot_left = new Vector3(rectVertices[0].x * scale.x, rectVertices[0].y * scale.y, z_offset) + rect.transform.position;
            Vector3 bot_right = new Vector3(rectVertices[1].x * scale.x, rectVertices[1].y * scale.y, z_offset) + rect.transform.position;
            Vector3 top_left = new Vector3(rectVertices[2].x * scale.x, rectVertices[2].y * scale.y, z_offset) + rect.transform.position;
            Vector3 top_right = new Vector3(rectVertices[3].x * scale.x, rectVertices[3].y * scale.y, z_offset) + rect.transform.position;

            List<Vector3> Bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right };
            return Bounds;
        }
        public static void CreateMesh(GameObject attachObject, Vector3[] objectVerts)
        {
            MeshFilter meshFilter = attachObject.GetComponent<MeshFilter>();

            // Mesh 만들기
            Mesh customMesh = new Mesh();
            meshFilter.mesh = customMesh;
            customMesh.name = "CustomMesh";

            Vector3[] vertices = new Vector3[4]
            {
            objectVerts[0],objectVerts[1], objectVerts[2], objectVerts[3]
            };
            // Mesh 그리는 데 필요한 요소 - 삼각형들로 하나의 Mesh를 만듬
            int[] triangles = new int[6]
            {
            1,0,3,3,0,2
            };
            // Mesh 방향 설정
            Vector3[] normals = new Vector3[4]
            {
            -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward
            };

            customMesh.vertices = vertices;
            customMesh.normals = normals;
            customMesh.triangles = triangles;
        }
        public static void CreateMaterial(GameObject attachObject, float[] color_rgba)
        {
            MeshRenderer meshRenderer = attachObject.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            float r = color_rgba[0];
            float g = color_rgba[1];
            float b = color_rgba[2];
            float a = color_rgba[3];
            
            Color color = new Color(r, g, b, a);
            meshRenderer.material.SetColor("_BaseColor", color);
            
        }
        public static void CreateAttachQuad(string targetName, float[] color_rgba)
        {
            GameObject attachObject = GameObject.Find(targetName + "_attachQuad");
            if (attachObject != null)
                Object.Destroy(attachObject);
            attachObject = new GameObject(targetName + "_attachQuad", typeof(MeshFilter), typeof(MeshRenderer));
            Vector3[] Bounds = null;

            if (GameObject.Find(targetName).GetComponent<TMP_Text>() == null) 
                Bounds = CalcRectObjectBounds(targetName).ToArray(); // for picture
            else
                Bounds = CalcTMPBounds(targetName).ToArray(); // for text

            CreateMesh(attachObject, Bounds);
            CreateMaterial(attachObject, color_rgba);
        }
        public static void BackgroundToBlack()
        {
            GameObject background = GameObject.Find("BackgroundManager").transform.Find("Background").gameObject;
            if (background.activeSelf == true)
                background.SetActive(false);
        }
        public static void BackgroundToNomal()
        {
            GameObject background = GameObject.Find("BackgroundManager").transform.Find("Background").gameObject;
            if (background.activeSelf == false)
                background.SetActive(true);
        }
        public static void TempleteFilm_On()
        {
            GameObject.Find("TempleteFilm").GetComponent<MeshRenderer>().enabled = true;
        }
        public static void TempleteFilm_Off()
        {
            GameObject.Find("TempleteFilm").GetComponent<MeshRenderer>().enabled = false;
        }

        public static void SegLightOff()
        {
            GameObject point_light = GameObject.Find("ReflectionLight");
            GameObject directional_light = GameObject.Find("World SunLight");
            GameObject segmentation_light = GameObject.Find("Studio SunLight");
            point_light.GetComponent<Light>().intensity = 1;
            directional_light.GetComponent<Light>().intensity = 1;
            segmentation_light.GetComponent<Light>().intensity = 0;
        }
        public static void SegLightOn()
        {
            GameObject point_light = GameObject.Find("ReflectionLight");
            GameObject directional_light = GameObject.Find("World SunLight");
            GameObject segmentation_light = GameObject.Find("Studio SunLight");
            point_light.GetComponent<Light>().intensity = 0;
            directional_light.GetComponent<Light>().intensity = 0;
            segmentation_light.GetComponent<Light>().intensity = 1;
        }
        public static void Destroy_All_AttachQuad(List<string> feature_names)
        {
            foreach (string feature_name in feature_names)
            {
                Destroy_AttachQuad(feature_name);
            }
        }
        public static void Destroy_AttachQuad(string feature_name)
        {
            string attachQuad_name = feature_name + "_attachQuad";
            GameObject attachObject = GameObject.Find(attachQuad_name);
            if (attachObject != null)
                UnityEngine.Object.Destroy(attachObject);
        }
    }
}
