using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;

using BoundingBox_Script;
using CalcCoord_Script;
using JsonData_Script;

namespace ReceiptTag_Script
{
    public class ReceiptTagDataScript
    {
        [Serializable]
        public class SegmentationInfoList
        {
            public List<SegmentationInfo> info_list = new List<SegmentationInfo>();
            public float camera_angle;
        };
        [Serializable]
        public class SegmentationInfo
        {
            public string image_filename;
            public List<Segment> segment_list = new List<Segment>();
        };
        [Serializable]
        public class Segment
        {
            public string name;
            public List<Words> words = new List<Words>();
        };
        [Serializable]
        public struct Quad
        {
            public Vector2 left_top;
            public Vector2 right_top;
            public Vector2 left_bottom;
            public Vector2 right_bottom;
        };
        [Serializable]
        public class Words
        {
            public string transcription;
            public List<Vector2> word_points;
        }
        public static Dictionary<string, List<Vector2>> LoadTaggingTextFile(string txt_path)
        {
            Dictionary<string, List<Vector2>> tag_data = new Dictionary<string, List<Vector2>>();
            string[] text_lines = System.IO.File.ReadAllLines(txt_path);
            foreach (string line in text_lines)
            {
                List<string> line_split = line.Split('\t').ToList();
                string text = line_split[0];
                if (tag_data.Keys.Contains(text))
                    //text = text + "dupl";
                    text = duple_naming(tag_data.Keys.ToList(), text);
                List<Vector2> bounds = ConvertStringCoordToVector2(line_split);
                tag_data.Add(text, bounds);
            }
            return tag_data;
        }
        public static string duple_naming(List<string> string_list, string text)
        {
            string rename = text;
            for(int i = 0; i<string_list.Count; i++)
            {
                if (string_list.Contains(text))
                {
                    rename = String.Format("{0}_{1}", rename, i);
                    continue;
                }
                else
                    break;
            }
            return rename;
                
        }
        public static List<Vector2> ConvertStringCoordToVector2(List<string> line_split)
        {
            List<float> bounds_parse = ConvertStringListTofloatList(line_split.GetRange(1, line_split.Count - 1)); //line_split.GetRange(1, line_split.Count - 1).Select(s => System.Int32.Parse(s)).ToList();  
            List<Vector2> bounds = new List<Vector2>() {
                new Vector2(bounds_parse[0],bounds_parse[1]),
                new Vector2(bounds_parse[2],bounds_parse[3]),
                new Vector2(bounds_parse[4],bounds_parse[5]),
                new Vector2(bounds_parse[6],bounds_parse[7])
            };
            return bounds;
        }
        public static List<float> ConvertStringListTofloatList(List<string> str_list)
        {
            List<float> num_list = new List<float>();
            foreach (string str in str_list)
            {
                float num = float.Parse(str);
                num_list.Add(num);          
            }
            return num_list;
        }
        public static SegmentationInfo CreateSeginfoAndAddObjName(SegmentationInfoList segmentation_info_list, List<string> tag_keys, string filename)
        {
            SegmentationInfo seg_info = new SegmentationInfo();
            segmentation_info_list.info_list.Add(seg_info);

            foreach (string tag_key in tag_keys)
            {
                Segment segment = new Segment();
                segment.name = tag_key;
                seg_info.segment_list.Add(segment);
            }
            seg_info = segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1];
            seg_info.image_filename = filename;
            return seg_info;
        }
        public static string LoadAndTransFormBoundingBoxData(string txt_path, string filename)
        {
            Dictionary<string, List<Vector2>> tag_data = LoadTaggingTextFile(txt_path); //coord in templete

            SegmentationInfoList segmentation_info_list = new SegmentationInfoList();
            SegmentationInfo seg_info = CreateSeginfoAndAddObjName(segmentation_info_list, tag_data.Keys.ToList(), filename);
            AddTagTextInfo(seg_info, tag_data);
            //segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1] = seg_info;
            segmentation_info_list.camera_angle = JsonCharacter.CalcRotateAngle();

            string json_seg_info_list = UnityEngine.JsonUtility.ToJson(segmentation_info_list, true);
            return json_seg_info_list;
        }
        public static void AddTagTextInfo(SegmentationInfo seg_info, Dictionary<string, List<Vector2>> tag_data)
        {
            for (int i = 0; i < seg_info.segment_list.Count; i++)
            {
                Segment segment = seg_info.segment_list[i];
                List<Words> words_list = ExtractTagWordInfo(tag_data);
                segment.words = words_list;
                seg_info.segment_list[i] = segment;
            }
        }
        public static List<Words> ExtractTagWordInfo(Dictionary<string, List<Vector2>> tag_data)
        {
            List<Words> words_list = new List<Words>();
            foreach (string tag_key in tag_data.Keys)
            {
                List<Vector2> value = tag_data[tag_key];
                List<Vector2> word_coord = CalcImageCoords(value);

                Words words = new Words();
                words.transcription = tag_key;
                words.word_points = word_coord;
                words_list.Add(words);
            }
            return words_list;
        }
        public static List<Vector2> CalcImageCoords(List<Vector2> value)
        {
            Vector2 tl = CalcCoord.CalcImageCoordsFromImageCoords(value[0]);
            Vector2 tr = CalcCoord.CalcImageCoordsFromImageCoords(value[1]);
            Vector2 br = CalcCoord.CalcImageCoordsFromImageCoords(value[2]);
            Vector2 bl = CalcCoord.CalcImageCoordsFromImageCoords(value[3]);
            return new List<Vector2>() { tl, tr, br, bl };
        }
    }
}

