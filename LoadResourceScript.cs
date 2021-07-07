using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace LoadResource_Script
{
    public static class LoadResouce
    {
        public static List<string> TextFile(string fileName)
        {
            TextAsset dataSet = Resources.Load<TextAsset>($"TextDataSet/{fileName}");
            StringReader type_sr = new StringReader(dataSet.text);
            string line = type_sr.ReadLine();
            List<string> textData = new List<string>();
            while (line != null)
            {
                textData.Add(line);
                line = type_sr.ReadLine();
            }
            type_sr.Close();
            return textData;
        }

        public static List<string> CollectFaceTextureFilenames()
        {
            List<string> face_filenames = new List<string>();
            string face_path = Application.dataPath + "/Resources/Faces/";
            string[] face_paths = Directory.GetFiles(face_path);
            foreach (string path in face_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".png")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    face_filenames.Add(filename);
                }
            }
            return face_filenames;
        }

        public static List<string> CollectHoloMeshFilenames()
        {
            List<string> holo_filenames = new List<string>();
            string holo_mesh_path = Application.dataPath + "/Resources/Meshes/holo_shape/";
            string[] holo_mesh_paths = Directory.GetFiles(holo_mesh_path);
            foreach (string path in holo_mesh_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".fbx")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    holo_filenames.Add(filename);
                }
            }
            return holo_filenames;
        }
        public static List<string> CollectCoverMeshFilenames()
        {
            List<string> cover_filenames = new List<string>();
            string cover_path = Application.dataPath + "/Resources/Meshes/cover_pages/";
            string[] cover_paths = Directory.GetFiles(cover_path);
            foreach (string path in cover_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".obj")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    cover_filenames.Add(filename);
                }
            }
            return cover_filenames;
        }
        public static List<string> CollectInnerMeshFilenames()
        {
            List<string> inner_filenames = new List<string>();
            string inner_path = Application.dataPath + "/Resources/Meshes/inner_pages/";
            string[] inner_paths = Directory.GetFiles(inner_path);
            foreach (string path in inner_paths)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".obj")
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    inner_filenames.Add(filename);
                }
            }
            return inner_filenames;
        }
    }
}

