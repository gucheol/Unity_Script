using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageUtil_Script
{
    public class Util
    {
        public static void AntiAliasingFunc(bool on_off)
        {
            string rendertexture_camera_name = "Render Camera";
            string main_camera_name = "Main Camera";
            // render camera
            FilterMode filtermode;
            // main camera
            bool is_Set_MSAA;

            if (on_off == true)
            {
                filtermode = FilterMode.Trilinear;
                is_Set_MSAA = true;
            }
            else
            {
                filtermode = FilterMode.Point;
                is_Set_MSAA = false;
            }

            Camera rt_camera = GameObject.Find(rendertexture_camera_name).GetComponent<Camera>();
            RenderTexture rt = rt_camera.targetTexture;
            rt.filterMode = filtermode;

            Camera main_camera = GameObject.Find(main_camera_name).GetComponent<Camera>();
            main_camera.allowMSAA = is_Set_MSAA;
        }
        public static void LodingNextFrame()
        {
            ObjChildsColorToBlack("BackgroundManager");
            ObjColorToBlack("Inner");
            ObjColorToBlack("Cover");
        }
        public static void ObjChildsColorToBlack(string obj_name)
        {
            Color black = new Color(0, 0, 0, 0);
            GameObject obj = GameObject.Find(obj_name);
            if(obj != null)
            {
                MeshRenderer[] obj_childs_renderer = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer obj_child in obj_childs_renderer)
                {
                    if (obj_child.material != null)
                        obj_child.material.SetColor("_BaseColor", black);
                }
            }      
        }
        public static void ObjColorToBlack(string obj_name)
        {
            Color black = new Color(0, 0, 0, 0);
            GameObject obj = GameObject.Find(obj_name);
            if (obj != null)
            {
                MeshRenderer obj_renderer = obj.GetComponent<MeshRenderer>();
                obj_renderer.material.SetColor("_BaseColor", black);
            }
        }
        public static void LodeComplete()
        {
            Color white = new Color(1f, 1f, 1f, 1f);
            GameObject.Find("Background").GetComponent<MeshRenderer>().material.SetColor("_BaseColor", white);
        }

    }
    
}
