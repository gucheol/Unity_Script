using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using BoundingBox_Script;


namespace CalcCoord_Script
{
    public class CalcCoord
    {
        public static Vector2 CalcImageCoordsInPassport(Vector3 world_coords)
        {
            GameObject passport_inner_object = DataNeeds.passport_inner_object;
            Bounds passport_cover_bounds = passport_inner_object.GetComponent<MeshRenderer>().bounds;
            float passport_left = passport_cover_bounds.min.x;
            float passport_right = passport_cover_bounds.max.x;
            float passport_bottom = passport_cover_bounds.min.y;
            float passport_top = passport_cover_bounds.max.y;

            float passport_width = passport_right - passport_left;
            float passport_height = passport_top - passport_bottom;

            Vector2 image_size = DataNeeds.image_size;
            float image_width = image_size.x;
            float image_height = image_size.y;

            float scale_x = image_width / passport_width;
            float scale_y = image_height / passport_height;

            float world_x = world_coords.x;
            float world_y = world_coords.y;

            float image_x = (world_x - passport_left) * scale_x;
            float image_y = (passport_top - world_y) * scale_y;

            return new Vector2(image_x, image_y);
        }

        public static List<Vector2> CalcImageCoordsInPassport(List<Vector3> world_coords_list)
        {
            List<Vector2> image_coords_list = new List<Vector2>();

            foreach (Vector2 world_coords in world_coords_list)
            {
                Vector2 image_coords = CalcImageCoordsInPassport(world_coords);
                image_coords_list.Add(image_coords);
            }

            return image_coords_list;
        }

        public static Vector2 CalcImageCoordsFromImageCoords(Vector2 input_image_coords)
        {
            Vector2 screen_coords = CalcScreenCoordsFromImageCoords(input_image_coords);
            Vector2 output_image_coords = CalcImageCoordsFromScreenCoords(screen_coords);

            return output_image_coords;
        }
        public static Vector2 CalcImageCoordsFromScreenCoords(Vector2 screen_coords)
        {
            Vector2 image_coords = new Vector2(screen_coords.x, Camera.main.pixelHeight - screen_coords.y);
            return image_coords;
        }

        public static Vector2 CalcScreenCoordsFromImageCoords(Vector2 image_coords)
        {
            Vector2 tex_coords = CalcTextureCoordsFromImageCoords(image_coords);
            Vector3 model_coords = CalcModelCoordsFromTextureCoords(tex_coords);
            Vector3 world_coords = CalcWorldCoordsFromModelCoords(model_coords);
            Vector2 screen_coords = CalcScreenCoordsFromWorldCoords(world_coords);

            return screen_coords;
        }
        public static Vector2 CalcTextureCoordsFromImageCoords(Vector2 image_coords)
        {
            Vector2 image_size = DataNeeds.image_size;
            float tu = image_coords.x / image_size.x;
            float tv = image_coords.y / image_size.y;

            //        return new Vector2(tu, tv);
            return new Vector2(tv, 1 - tu);
        }
        public static Vector3 CalcModelCoordsFromTextureCoords(Vector2 texture_coords)
        {
            Vector2Int mesh_tess = DataNeeds.mesh_tess;
            float du = 1.0f / (float)mesh_tess.x;
            float dv = 1.0f / (float)mesh_tess.y;

            float tu = texture_coords.x;
            float tv = texture_coords.y;

            int i0 = (int)(tu / du);
            int j0 = (int)(tv / dv);

            if (i0 > mesh_tess.x - 1) i0 = mesh_tess.x - 1;
            if (j0 > mesh_tess.y - 1) j0 = mesh_tess.y - 1;

            int i1 = (i0 < mesh_tess.x - 1 ? i0 + 1 : i0);
            int j1 = (j0 < mesh_tess.y - 1 ? j0 + 1 : j0);

            //
            Vector3 p_O = GetVertex(i0, j0);
            Vector3 p_U = GetVertex(i1, j0);
            Vector3 p_V = GetVertex(i0, j1);

            //
            float u = (tu - i0 * du) / du;
            float v = (tv - j0 * dv) / dv;

            Vector3 p_u = Vector3.Lerp(p_O, p_U, u);
            Vector3 p_v = Vector3.Lerp(p_O, p_V, v);

            //
            Vector3 p_M = p_O + (p_u - p_O) + (p_v - p_O);
            return p_M;
        }
        public static Vector3 CalcWorldCoordsFromModelCoords(Vector3 model_coords)
        {
            return DataNeeds.passport_obj.transform.TransformPoint(model_coords);
        }
        public static Vector2 CalcScreenCoordsFromWorldCoords(Vector3 world_coords)
        {
            Vector3 screen_coords = Camera.main.WorldToScreenPoint(world_coords);
            return new Vector2(screen_coords.x, screen_coords.y);
        }
        public static Vector3 GetVertex(int i, int j)
        {
            int index = GetVertexIndex(i, j);
            return GetVertex(index);
        }
        public static int GetVertexIndex(int i, int j)
        {
            Vector2Int mesh_tess = DataNeeds.mesh_tess;
            return i * (mesh_tess.y + 1) + j;
        }
        public static Vector3 GetVertex(int index)
        {
            List<Vector3> inner_vertices = DataNeeds.inner_vertices;
            if (inner_vertices != null)
            {
                return inner_vertices[index];
            }
            else
            {
                return Vector3.zero;
            }
        }

        public static Vector2[] CalcQuadForFeature(string feature_name)
        {
            List<Vector2> feature_points = RecalcFeaturePoints(feature_name);

            Vector2 left_top = CalcImageCoordsFromImageCoords(feature_points[0]);
            Vector2 right_top = CalcImageCoordsFromImageCoords(feature_points[1]);
            Vector2 left_bottom = CalcImageCoordsFromImageCoords(feature_points[2]);
            Vector2 right_bottom = CalcImageCoordsFromImageCoords(feature_points[3]);
            //middle mr
            Vector2 middle_top = CalcImageCoordsFromImageCoords(feature_points[4]);
            Vector2 middle_bot = CalcImageCoordsFromImageCoords(feature_points[5]);

            return new Vector2[] { left_top, right_top, right_bottom, left_bottom, middle_top, middle_bot };
        }

        public static List<Vector2> RecalcFeaturePoints(string feature_name)
        {
            List<Vector3> world_features = BoundingBox.CalcTMPTextBounds(feature_name);
            List<Vector2> image_features = CalcImageCoordsInPassport(world_features);
            //for mr middle
            return image_features;
        }

        public static Vector2[] CalcQuadForFeature(string feature_name, List<Vector3> world_coord)
        {
            var scale_offset = CalcScaleAndOffset(feature_name);
            List<Vector3> recalc_world = ApplyScaleAndOffset(world_coord, scale_offset.Item1, scale_offset.Item2);
            List<Vector2> feature_points = CalcImageCoordsInPassport(recalc_world);

            Vector2 left_top = CalcImageCoordsFromImageCoords(feature_points[0]);
            Vector2 right_top = CalcImageCoordsFromImageCoords(feature_points[1]);
            Vector2 left_bottom = CalcImageCoordsFromImageCoords(feature_points[2]);
            Vector2 right_bottom = CalcImageCoordsFromImageCoords(feature_points[3]);

            return new Vector2[] { left_top, right_top, right_bottom, left_bottom };
        }
        public static (List<List<char>>, List<string>, List<List<Vector3>>, List<List<Vector3>>) ExtractWorldAndCoord(List<List<TMP_CharacterInfo>> origin_info_list)
        {
            List<List<TMP_CharacterInfo>> char_info_line = origin_info_list;

            List<List<char>> chars_delSpace_list = new List<List<char>>();
            List<List<Vector3>> chars_coord = new List<List<Vector3>>();
            List<string> worlds = new List<string>();
            List<List<Vector3>> words_coord_list = new List<List<Vector3>>();

            foreach (List<TMP_CharacterInfo> char_info in char_info_line)
            {
                // 글자 좌표 정보 추출
                var charAndSpaceInfo = ExtractCharAndSpaceInfo(char_info);
                List<List<Vector3>> charCoord_inWord = charAndSpaceInfo.Item1;
                chars_coord.AddRange(charCoord_inWord);
                // 글자 내용 추출
                List<char> chars = charAndSpaceInfo.Item2;
                List<int> word_firstIndicies = charAndSpaceInfo.Item3;
                List<int> word_lastIndicies = charAndSpaceInfo.Item4;
                List<List<char>> chars_delSpace = CombineChar_UseWordLength(word_firstIndicies, word_lastIndicies, chars);
                chars_delSpace_list.AddRange(chars_delSpace);
                // 글자 내용 단어 내용으로 만들기
                string text = new string(chars.ToArray());
                List<string> oneLine_worlds = text.Split(' ').ToList();
                worlds.AddRange(oneLine_worlds);
                // 글자 좌표 정보 만들기
                List<List<Vector3>> word_coord = MakeWordBoundingBoxData(word_firstIndicies, word_lastIndicies, chars_delSpace, chars_coord, char_info);
                words_coord_list.AddRange(word_coord);
            }

            return (chars_delSpace_list, worlds, chars_coord, words_coord_list);
        }   
        public static (Vector3, Vector3) CalcScaleAndOffset(string feature_name)
        {
            TMP_Text textComponent = GameObject.Find(feature_name).GetComponent<TMP_Text>();

            Vector3 passportScale;
            if (textComponent.transform.parent != null)
            {
                if (textComponent.transform.parent.parent != null)
                {
                    Vector3 granfa_scale = textComponent.transform.parent.parent.localScale;
                    Vector3 fa_scale = textComponent.transform.parent.localScale;
                    passportScale = new Vector3(granfa_scale.x * fa_scale.x, granfa_scale.y * fa_scale.y, granfa_scale.z * fa_scale.z);
                }
                else
                    passportScale = textComponent.transform.parent.localScale;
            }
            else
                passportScale = new Vector3(1, 1, 1);

            Vector3 textScale = textComponent.rectTransform.localScale;
            Vector3 scale = new Vector3(passportScale.x * textScale.x, passportScale.y * textScale.y, passportScale.z * textScale.z);
            Vector3 offset = textComponent.transform.position;

            return (scale, offset);
        }
        public static List<Vector3> ApplyScaleAndOffset(List<Vector3> world_position, Vector3 scale, Vector3 offset)
        {
            List<Vector3> new_position = new List<Vector3>();
            foreach (Vector3 position in world_position)
            {
                Vector3 recalc_position = new Vector3(position.x * scale.x, position.y * scale.y, position.z * scale.z) + offset;
                new_position.Add(recalc_position);
            }
            return new_position;
        }
        public static Vector3 WorldCoordTextureToModel(Vector3 texture_coord)
        {
            Vector2 image_features = CalcCoord.CalcImageCoordsInPassport(texture_coord);
            Vector2 tex_coords = CalcCoord.CalcTextureCoordsFromImageCoords(image_features);
            Vector3 model_coords = CalcCoord.CalcModelCoordsFromTextureCoords(tex_coords);
            Vector3 world_coords = CalcCoord.CalcWorldCoordsFromModelCoords(model_coords);
            return world_coords;
        }
        public static List<Vector3> FeatureWorldCoordInModel(string feature_name)
        {
            List<Vector3> texture_coord = BoundingBox.CalcTMPBounds(feature_name);
            List<Vector3> model_coord = new List<Vector3>();
            for (int i = 0; i < 4; i++)
                model_coord.Add(WorldCoordTextureToModel(texture_coord[i]));
            return model_coord;
        }
        public static (List<List<Vector3>>, List<char>, List<int>, List<int>) ExtractCharAndSpaceInfo(List<TMP_CharacterInfo> char_info)
        {
            List<List<Vector3>> charCoord_inWord = new List<List<Vector3>>();
            List<char> chars = new List<char>();
            List<int> world_firstIndicies = new List<int>() { 0 };
            List<int> world_lastIndicies = new List<int>() { char_info.Count - 1 };
            List<Vector3> coord;
            // 캐릭터 정보 추출
            for (int i = 0; i < char_info.Count; i++)
            {
                // 스페이스의 인덱스 저장
                if (char_info[i].character == ' ')
                {
                    world_firstIndicies.Add(i + 1);
                    world_lastIndicies.Add(i - 1);
                }
                // 캐릭터 정보 저장
                else
                {
                    coord = new List<Vector3>(){char_info[i].topLeft, char_info[i].topRight,
                    char_info[i].bottomLeft, char_info[i].bottomRight};
                    charCoord_inWord.Add(coord);
                }
                chars.Add(char_info[i].character);
            }
            world_firstIndicies.Sort();
            world_lastIndicies.Sort();

            return (charCoord_inWord, chars, world_firstIndicies, world_lastIndicies);
        }
        public static List<List<char>> CombineChar_UseWordLength(List<int> word_firstIndicies, List<int> word_lastIndicies, List<char> chars)
        {
            List<List<char>> chars_delSpace = new List<List<char>>();
            // 단어별 캐릭터 묶음
            for (int i = 0; i < word_firstIndicies.Count; i++)
            {
                int first_index = word_firstIndicies[i];
                int last_index = word_lastIndicies[i];
                int word_length = last_index - first_index + 1;
                List<char> char_oneWord = chars.GetRange(first_index, word_length);
                chars_delSpace.Add(char_oneWord);
            }
            return chars_delSpace;
        }
        public static List<List<Vector3>> MakeWordBoundingBoxData(List<int> world_firstIndicies, List<int> world_lastIndicies, List<List<char>> chars_delSpace_list, List<List<Vector3>> char_coord, List<TMP_CharacterInfo> char_info)
        {
            int char_count = 0;
            List<List<Vector3>> word_coord = new List<List<Vector3>>();
            for (int i = 0; i < world_firstIndicies.Count; i++)
            {
                float max = 0;
                float min = 100000;
                for (int j = 0; j < chars_delSpace_list[i].Count; j++)
                {
                    Vector3 current_topleft = char_coord[char_count][0];
                    Vector3 current_bottomleft = char_coord[char_count][2];
                    if (max < current_topleft.y)
                        max = current_topleft.y;
                    if (min > current_bottomleft.y)
                        min = current_bottomleft.y;
                    char_count++;
                }

                var first_char = char_info[world_firstIndicies[i]];
                var last_char = char_info[world_lastIndicies[i]];

                Vector3 topLeft = new Vector3(first_char.topLeft.x, max, first_char.topLeft.z);
                Vector3 topRight = new Vector3(last_char.topRight.x, max, last_char.topRight.z);
                Vector3 bottomLeft = new Vector3(first_char.bottomLeft.x, min, first_char.bottomLeft.z);
                Vector3 bottomRight = new Vector3(last_char.bottomRight.x, min, last_char.bottomRight.z);

                List<Vector3> coord = new List<Vector3>() { topLeft, topRight, bottomLeft, bottomRight };
                word_coord.Add(coord);
            }
            return word_coord;
        }
    }
    public class DataNeeds
    {
        public static Vector2 image_size;
        public static GameObject passport_inner_object;
        public static GameObject passport_obj;
        public static Vector2Int mesh_tess;
        public static List<Vector3> inner_vertices;
    }
}