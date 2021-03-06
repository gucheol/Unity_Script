using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;

using BoundingBox_Script;
using CalcCoord_Script;

namespace JsonData_Script
{
    public class JsonCharacter
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
            public List<Chars> chars = new List<Chars>();
        }
        [Serializable]
        public struct Chars
        {
            public string transcription;
            public List<Vector2> char_points;
        }
        public static SegmentationInfo CreateSeginfoAndAddObjName(SegmentationInfoList segmentation_info_list, List<string> feature_names, string filename)
        {
            SegmentationInfo seg_info = new SegmentationInfo();
            segmentation_info_list.info_list.Add(seg_info);

            foreach (string feature_name in feature_names)
            {
                Segment segment = new Segment();
                segment.name = feature_name;
                seg_info.segment_list.Add(segment);
            }
            seg_info = segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1];
            seg_info.image_filename = filename;
            return seg_info;
        }
        public static List<int> AddTextInfo(SegmentationInfo seg_info, float img_angle)
        {
            List<int> removeIndex = new List<int>();
            for (int i = 0; i < seg_info.segment_list.Count; i++)
            {
                Segment segment = seg_info.segment_list[i];
                string tmp_obj_name = segment.name;
                var text_info = BoundingBox.removeOutOfScreenCharacter(tmp_obj_name);
                if (text_info.Item1.Count == 0 || text_info.Item1[0].Count == 0)
                    continue;

                List<Words> words_list = ExtractWordInfo(text_info.Item1, tmp_obj_name, img_angle, true);
                segment.words = words_list;
                seg_info.segment_list[i] = segment;
            }
            removeIndex.Reverse();
            return removeIndex;
        }
        public static SegmentationInfo RemoveOutofScreenIndex(SegmentationInfo seg_info, List<int> removeIndex)
        {
            foreach (int index in removeIndex)
                seg_info.segment_list.RemoveAt(index);
            return seg_info;
        }
        public static string MakeBoundingBoxData(List<string> feature_names, string filename, bool isMergeWordsInLine = false)
        {
            // json 개별 저장을 위해
            SegmentationInfoList segmentation_info_list = new SegmentationInfoList();
            segmentation_info_list.camera_angle = CalcRotateAngle(); //카메라 앵글값 저장, 모델각도는 (0,0,0)으로 가정
            SegmentationInfo seg_info = CreateSeginfoAndAddObjName(segmentation_info_list, feature_names, filename); // 오브젝트 이름과 빈 리스트만 넣어줌
            AddTextInfo(seg_info, segmentation_info_list.camera_angle); // 텍스트의 좌표를 구함
            List<int> removeIndex = new List<int>();
            seg_info = RemoveOutofScreenIndex(seg_info, removeIndex); // 화면 밖에 나간 글자 제외시킴
            List<Segment> removed_empty_segments_list = DelEmptySegment(seg_info.segment_list); // 가끔 쓰레기값이 들어옴
            bool is_parts_exist = IsInPartsOfWord(removed_empty_segments_list); // _part1 의 이름을 가진 오브젝트들를 찾는다
            if (is_parts_exist)
                seg_info.segment_list = CombinePartsOfWord(seg_info.segment_list, segmentation_info_list.camera_angle); // _part{num} 의 값들을 하나로 합친다
            segmentation_info_list.info_list[segmentation_info_list.info_list.Count - 1] = seg_info;
            string json_seg_info_list = UnityEngine.JsonUtility.ToJson(segmentation_info_list, true);
            return json_seg_info_list;
        }
        public static float CalcRotateAngle()
        {
            GameObject model = DataNeeds.passport_obj;
            Bounds bounds = model.GetComponent<MeshFilter>().mesh.bounds;
            Vector3 model_tl = Camera.main.WorldToScreenPoint(new Vector3(0, 0, bounds.size.z));
            Vector3 model_tr = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
            float x_len = model_tr.x - model_tl.x;
            float y_len = model_tr.y - model_tl.y;
            float angle = (float)Math.Round(Mathf.Atan2(-y_len, x_len) / Math.PI * 180, 3);

            return angle;
        }
        public static bool IsInPartsOfWord(List<Segment> segment_list)
        {
            bool is_exist = true;
            List<string> tmp_string_list = GetSeperateWordList(segment_list);
            if (tmp_string_list.Count == 0)
                is_exist = false;
            return is_exist;
        }
        public static List<Segment> CombinePartsOfWord(List<Segment> segment_list, float img_angle)
        {
            List<string> sep_word_list = GetSeperateWordList(segment_list);
            List<int> must_del_index = new List<int>();
            List<Segment> combined_segments_list = CombinedSegment(sep_word_list, segment_list, must_del_index, img_angle);
            List<Segment> deleted_segments_list = RemovePartsElement(must_del_index, segment_list);
            List<Segment> added_segments_list = AddCombinedElement(deleted_segments_list, combined_segments_list);

            return added_segments_list;
        }
        public static List<Segment> DelEmptySegment(List<Segment> segment_list)
        {
            List<int> remove_indices = new List<int>();
            for(int i=0; i<segment_list.Count; i++)
            {
                Segment segment = segment_list[i];
                if (segment.words.Count == 0)
                    remove_indices.Add(i);
            }
            remove_indices.Sort();
            remove_indices.Reverse();

            foreach (int remove_index in remove_indices)
                segment_list.RemoveAt(remove_index);
            return segment_list;
        }
        public static List<Segment> AddCombinedElement(List<Segment> deleted_segments_list, List<Segment> combined_segments_list)
        {
            foreach (Segment combined_segment in combined_segments_list)
                deleted_segments_list.Add(combined_segment);
            return deleted_segments_list;
        }
        public static List<Segment> RemovePartsElement(List<int> must_del_index, List<Segment> segment_list)
        {
            must_del_index.Sort();
            must_del_index.Reverse();
            foreach (int del_index in must_del_index)
                segment_list.RemoveAt(del_index);
            return segment_list;
        }
        public static List<Segment> CombinedSegment(List<string> sep_word_list, List<Segment> segment_list, List<int> must_del_index, float img_angle)
        {
            List<Segment> combined_segments_list = new List<Segment>();
            foreach (string sep_word in sep_word_list)
            {
                var collect_result = CollectPartsOfTextAndChars(segment_list, sep_word);
                List<string> combined_text = collect_result.Item1;
                List<Chars> combined_chars = collect_result.Item2;
                List<int> del_indicies = collect_result.Item3;
                must_del_index.AddRange(del_indicies);

                Words combined_words = new Words();
                combined_words.transcription = string.Join("", combined_text);
                combined_words.word_points = MakeCharsMinimalBoundingBox(combined_chars);
                combined_words.chars = combined_chars;

                Segment combined_segment = new Segment();
                combined_segment.name = sep_word;
                combined_segment.words = new List<Words>() { combined_words };
                combined_segments_list.Add(combined_segment);
            }
            return combined_segments_list;
        }
        public static (List<string>, List<Chars>, List<int>) CollectPartsOfTextAndChars(List<Segment> segment_list, string sep_word)
        {
            List<string> combined_text = new List<string>();
            List<Chars> combined_chars = new List<Chars>();
            List<int> del_indicies = new List<int>();
            for (int i = 0; i < segment_list.Count; i++)
            {
                Segment segment = segment_list[i];
                string obj_name = segment.name;
                if (!(sep_word == obj_name.Split(new char[] { '_' })[0]))//!regex.IsMatch(obj_name)
                    continue;
                if (segment.words.Count == 0)
                {
                    del_indicies.Add(i);
                    continue;
                }
                Words segment_words = segment.words[0];
                string text_parts = segment_words.transcription;
                List<Chars> words_parts = segment_words.chars;
                combined_text.Add(text_parts);
                combined_chars.AddRange(words_parts);
                del_indicies.Add(i);
            }
            return (combined_text, combined_chars, del_indicies);
        }
        public static List<Vector2> MakeCombinedWordcoord(List<Chars> combined_char_info)
        {
            Vector2 left_top;
            Vector2 left_bottom;
            Vector2 right_top;
            Vector2 right_bottom;
            // 가로박스 세로박스 좌표 판단
            if (combined_char_info.Count > 1)
            {
                float horizon_distance = Vector2.Distance(combined_char_info[0].char_points[1], combined_char_info[1].char_points[0]);
                float vertical_distance = Vector2.Distance(combined_char_info[0].char_points[3], combined_char_info[1].char_points[0]);
                if (horizon_distance > vertical_distance)
                {
                    left_top = combined_char_info[0].char_points[0];
                    left_bottom = combined_char_info[combined_char_info.Count - 1].char_points[3];
                    right_top = combined_char_info[0].char_points[1];
                    right_bottom = combined_char_info[combined_char_info.Count - 1].char_points[2];
                    return new List<Vector2>() { left_top, right_top, right_bottom, left_bottom };
                }
            }
            left_top = combined_char_info[0].char_points[0];
            left_bottom = combined_char_info[0].char_points[3];
            right_top = combined_char_info[combined_char_info.Count - 1].char_points[1];
            right_bottom = combined_char_info[combined_char_info.Count - 1].char_points[2];
            return new List<Vector2>() { left_top, right_top, right_bottom, left_bottom };
        }
        public static List<string> GetSeperateWordList(List<Segment> segment_list)
        {
            // 조각난 단어 종류별로 이름 뽑기
            Regex regex = new Regex(@"_part*");
            List<string> sep_word_list = new List<string>();
            foreach (Segment word_piece in segment_list)
            {
                string obj_name = word_piece.name;
                if (!regex.IsMatch(obj_name))
                    continue;

                string text_group_name = obj_name.Split(new char[] { '_' })[0];
                if (!sep_word_list.Contains(text_group_name))
                    sep_word_list.Add(text_group_name);     
            }
            return sep_word_list;    
        }

        public static List<Chars> ExtractCharInfoFromWord(List<char> char_oneWord, string tmp_obj_name, List<List<Vector3>> char_coord, int char_count)
        {
            List<Chars> chars_list = new List<Chars>();

            int char_index;
            Vector2[] char_points;
            for (int k = 0; k < char_oneWord.Count; k++)
            {
                char_index = char_count + k;
                Chars chars = new Chars();
                char_points = CalcCoord.CalcQuadForFeature(tmp_obj_name, char_coord[char_index]);
                chars.transcription = char_oneWord[k].ToString();
                chars.char_points = char_points.ToList();
                chars_list.Add(chars);
            }
            return chars_list;
        }
        public static List<Words> ExtractWordInfo(List<List<TMPro.TMP_CharacterInfo>> charsInLine_list, string tmp_obj_name, float img_angle, bool isMergeLineWords = false)
        {
            List<Words> words_list = new List<Words>();
            
            if (isMergeLineWords)
            {
                var word_info = CalcCoord.ExtractMergeWorldAndCoord(charsInLine_list); // line_word_char, line_word, line_char_coord, line_word_coord
                return CreateMergeWordsInfo(word_info, tmp_obj_name, img_angle); ;
            }
            else
            {
                var word_info = CalcCoord.ExtractWorldAndCoord(charsInLine_list); // words_char, line_word, line_char_coord, line_word_coord
                return CreateWordsInfo(word_info, tmp_obj_name);
            }
                
        }
        public static List<Words> CreateMergeWordsInfo((List<List<List<char>>>, List<string>, List<List<Vector3>>, List<List<Vector3>>) word_info, string tmp_obj_name, float img_angle)
        {
            List<List<List<char>>> charInLines = word_info.Item1; // line-word-char
            List<string> word_text_list = word_info.Item2;
            List<List<Vector3>> char_boundingbox_list = word_info.Item3; // allchar-4coord
            List<List<Vector3>> word_boundingbox_list = word_info.Item4;

            int char_count_in_word = 0;
            List<Words> words_list = new List<Words>();
            for (int i = 0; i< charInLines.Count; i++)
            {
                List<Chars> chars_list = new List<Chars>();
                for (int j = 0; j < charInLines[i].Count; j++)
                {
                    List<Chars> oneWordChars = ExtractCharInfoFromWord(charInLines[i][j], tmp_obj_name, char_boundingbox_list, char_count_in_word);
                    chars_list.AddRange(oneWordChars);
                    char_count_in_word += charInLines[i][j].Count;
                }
                Words words = new Words();
                words.word_points = MakeCharsMinimalBoundingBox(chars_list);
                words.chars = chars_list;
                words.transcription = word_info.Item2[i];
                words_list.Add(words);
            }
            return words_list;
        }
        public static List<Vector2> MakeCharsMinimalBoundingBox(List<Chars> chars_list)
        {
            List<Vector2> char_points = ExtractCharCoords(chars_list);
            List<Vector2> bounding_box = MinimalBoundingBox.Calculate(char_points);
            return bounding_box;
        }
        public static List<Vector2> ExtractCharCoords(List<Chars> oneWordChars)
        {
            List<Vector2> allCharsCoord = new List<Vector2>();
            foreach(Chars chars in oneWordChars)
                allCharsCoord.AddRange(chars.char_points);

            return allCharsCoord;
        }
        public static List<Words> CreateWordsInfo((List<List<char>>, List<string>, List<List<Vector3>>, List<List<Vector3>>) word_info, string tmp_obj_name)
        {
            List<List<Char>> char_text_list = word_info.Item1;
            List<string> word_text_list = word_info.Item2;
            List<List<Vector3>> char_boundingbox_list = word_info.Item3;
            List<List<Vector3>> word_boundingbox_list = word_info.Item4;

            int char_count = 0;
            List<Words> words_list = new List<Words>();
            for (int j = 0; j < word_text_list.Count; j++)
            {
                Words words = new Words();
                Vector2[] word_points = CalcCoord.CalcQuadForFeature(tmp_obj_name, word_boundingbox_list[j]);
                List<Chars> chars_list = ExtractCharInfoFromWord(char_text_list[j], tmp_obj_name, char_boundingbox_list, char_count);

                words.word_points = word_points.ToList();
                words.chars = chars_list;
                words.transcription = word_info.Item2[j];
                words_list.Add(words);

                char_count += char_text_list[j].Count;
            }
            return words_list;
        }
    } 
}

