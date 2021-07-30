using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using CalcCoord_Script;

namespace BoundingBox_Script
{
    public class BoundingBox
    {
        public static List<Vector3> CalcTMPCharVertex(string TMPObjectName, TMP_CharacterInfo charInfo, float min_y, float max_y)
        {
            TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();

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

            Vector3 bot_left = new Vector3(charInfo.bottomLeft.x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;
            Vector3 bot_right = new Vector3(charInfo.bottomRight.x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_left = new Vector3(charInfo.topLeft.x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_right = new Vector3(charInfo.topRight.x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;

            List<Vector3> text3DVertex = new List<Vector3>() { bot_left, bot_right, top_left, top_right };

            return text3DVertex;
        }

        public static List<Vector3> CalcRemakeTMPBounds(string TMPObjectName, List<TMP_CharacterInfo> remake_CharInfos, int num_deleted)
        {
            TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();

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
            List<TMP_CharacterInfo> charInfos = remake_CharInfos;

            float max_y = 0;
            float min_y = 0;
            float min_x = 0;
            float max_x = 0;

            int final_index = textInfo.characterCount - num_deleted - 1;
            if (final_index > -1)
            {
                min_x = charInfos[0].bottomLeft.x;
                float test;
                test = charInfos[final_index].bottomRight.x;
                max_x = charInfos[final_index].bottomRight.x;

                foreach (TMP_CharacterInfo charinfo in charInfos)
                {
                    if (min_y > charinfo.bottomLeft.y)
                        min_y = charinfo.bottomLeft.y;
                    if (max_y < charinfo.topLeft.y)
                        max_y = charinfo.topLeft.y;
                }
            }

            Vector3 bot_left = new Vector3(min_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_left = new Vector3(min_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_right = new Vector3(max_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
            Vector3 bot_right = new Vector3(max_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;

            //mr middle height
            Vector3 middle_top = new Vector3(0,0,0);
            Vector3 middle_bot = new Vector3(0,0,0);

            if(final_index > -1)
            {
                int middle_index = 0;
                if(final_index != 0)
                    middle_index = final_index/2;
                TMP_CharacterInfo middle_info = charInfos[middle_index];
                float middle_x = middle_info.topLeft.x;
                float top_height = middle_info.topLeft.y;
                float bot_height = middle_info.bottomLeft.y;
                middle_top = new Vector3(middle_x * scale.x, top_height * scale.y, 0) + textComponent.transform.position;
                middle_bot = new Vector3(middle_x * scale.x, bot_height * scale.y, 0) + textComponent.transform.position;
            }

            //List<Vector3> tmp_bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right };
            List<Vector3> tmp_bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right, middle_top, middle_bot }; // include mr middle
            return tmp_bounds;
        }

        public static List<Vector3> CalcTMPTextBounds(string TMPObjectName)
        {
            var remakeTextInfo = removeOutOfScreenCharacter(TMPObjectName);
            List<TMP_CharacterInfo> remake_CharInfos = remakeTextInfo.Item1[0];
            int num_deleted_list = remakeTextInfo.Item2[0];

            List<Vector3> tmp_bounds = CalcRemakeTMPBounds(TMPObjectName, remake_CharInfos, num_deleted_list);

            return tmp_bounds;
        }
        public static List<Vector3> CalcTMPBounds(string TMPObjectName)
        {
            TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();

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
            TMP_CharacterInfo[] charInfos = textInfo.characterInfo;

            float max_y = 0;
            float min_y = 0;
            float min_x = 0;
            float max_x = 0;

            int final_index = textInfo.characterCount - 1;
            if (final_index > -1)
            {
                min_x = charInfos[0].bottomLeft.x;
                max_x = charInfos[final_index].bottomRight.x;

                foreach (TMP_CharacterInfo charinfo in charInfos)
                {
                    if (min_y > charinfo.bottomLeft.y)
                        min_y = charinfo.bottomLeft.y;
                    if (max_y < charinfo.topLeft.y)
                        max_y = charinfo.topLeft.y;
                }
            }

            Vector3 bot_left = new Vector3(min_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_left = new Vector3(min_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
            Vector3 top_right = new Vector3(max_x * scale.x, max_y * scale.y, 0) + textComponent.transform.position;
            Vector3 bot_right = new Vector3(max_x * scale.x, min_y * scale.y, 0) + textComponent.transform.position;

            List<Vector3> tmp_bounds = new List<Vector3> { bot_left, bot_right, top_left, top_right };
            return tmp_bounds;
        }

        public static (List<List<TMP_CharacterInfo>>, List<int>) removeOutOfScreenCharacter(string TMPObjectName)
        {
            TMP_Text textComponent = GameObject.Find(TMPObjectName).GetComponent<TMP_Text>();
            TMP_TextInfo textInfo = textComponent.textInfo;

            List<List<TMP_CharacterInfo>> charinfo_splitLine = Split_LineCharacter(textInfo);

            var screenCharInfos = FindOutScreenCharInfo(charinfo_splitLine, TMPObjectName);
            List<List<TMP_CharacterInfo>> screenCharInfos_list = screenCharInfos.Item1;
            List<int> num_deleted_list = screenCharInfos.Item2;

            screenCharInfos_list = DelEmptyLine(screenCharInfos_list);
            screenCharInfos_list = Remove_FirstLastCharSpace(screenCharInfos_list);

            return (screenCharInfos_list, num_deleted_list);
        }
        public static List<List<TMP_CharacterInfo>> Split_LineCharacter(TMP_TextInfo textInfo)
        {
            List<TMP_LineInfo> lineInfo_list = textInfo.lineInfo.ToList();
            List<TMP_CharacterInfo> charInfosList = textInfo.characterInfo.ToList();
            List<List<TMP_CharacterInfo>> charinfo_splitLine = new List<List<TMP_CharacterInfo>>();

            foreach (TMP_LineInfo line in lineInfo_list)
            {
                int firstIndex = line.firstCharacterIndex;
                int line_length = line.characterCount;
                var line_char = charInfosList.GetRange(firstIndex, line_length);
                charinfo_splitLine.Add(line_char);
            }
            if (GameObject.Find("공급자-업태-내용").GetComponent<TMP_Text>().textInfo == textInfo)
                lineInfo_list = null;
            return charinfo_splitLine;
        }
        public static (float, float) Calc_CharacterHeight_MinMax(List<TMP_CharacterInfo> charInfo_oneLine)
        {
            float min_y = 10000;
            float max_y = 0;
            foreach (TMP_CharacterInfo charinfo in charInfo_oneLine)
            {
                if (min_y > charinfo.bottomLeft.y)
                    min_y = charinfo.bottomLeft.y;
                if (max_y < charinfo.topLeft.y)
                    max_y = charinfo.topLeft.y;
            }
            return (min_y, max_y);
        }
        public static List<int> Calc_RemoveIndex(List<TMP_CharacterInfo> charInfo_oneLine, string TMPObjectName, float min_y, float max_y)
        {
            List<int> shouldDel_indexList = new List<int>();
            foreach (TMP_CharacterInfo charInfo in charInfo_oneLine)
            {
                int index = charInfo_oneLine.IndexOf(charInfo);
                List<Vector3> char3DVertex = CalcTMPCharVertex(TMPObjectName, charInfo, min_y, max_y);
                List<Vector2> char2DVertex = CalcCoord.CalcImageCoordsInPassport(char3DVertex);

                bool shoudDel = Is_OutOfScreen_Vertices(char2DVertex);
                if (shoudDel == true)
                    shouldDel_indexList.Add(index);
            }

            // 라인 구분시 처음과 마지막이 공백일 수 있으니 삭제
            if (charInfo_oneLine.Count != 0)
            {
                if (charInfo_oneLine[charInfo_oneLine.Count - 1].character == ' ' && !shouldDel_indexList.Contains(charInfo_oneLine.Count - 1))
                    shouldDel_indexList.Add(charInfo_oneLine.Count - 1);

                if (charInfo_oneLine[0].character == ' ' && !shouldDel_indexList.Contains(0))
                    shouldDel_indexList.Add(0);
            }

            shouldDel_indexList.Sort();
            shouldDel_indexList.Reverse();
            return shouldDel_indexList;
        }
        public static bool Is_OutOfScreen_Vertices(List<Vector2> char2DVertex)
        {
            bool shoudDel = false;
            foreach (Vector2 vertex in char2DVertex)
            {
                Vector2 screenVertex = CalcCoord.CalcImageCoordsFromImageCoords(vertex);
                if ((screenVertex.x < 0 || screenVertex.x > Screen.width || screenVertex.y < 0
                    || screenVertex.y > Screen.height))
                {
                    shoudDel = true;
                }
            }
            return shoudDel;
        }
        public static (List<List<TMP_CharacterInfo>>, List<int>) FindOutScreenCharInfo(List<List<TMP_CharacterInfo>> charinfo_splitLine, string TMPObjectName)
        {
            List<List<TMP_CharacterInfo>> remake_charInfos_list = new List<List<TMP_CharacterInfo>>();
            List<int> num_deleted_list = new List<int>();

            foreach (List<TMP_CharacterInfo> charInfo_oneLine in charinfo_splitLine)
            {
                var y_min_max = Calc_CharacterHeight_MinMax(charInfo_oneLine);
                float min_y = y_min_max.Item1;
                float max_y = y_min_max.Item2;

                List<int> shouldDel_indexList = Calc_RemoveIndex(charInfo_oneLine, TMPObjectName, min_y, max_y);

                foreach (int index in shouldDel_indexList)
                    charInfo_oneLine.RemoveAt(index);

                List<TMP_CharacterInfo> remake_charInfos_oneLine = charInfo_oneLine;
                remake_charInfos_list.Add(remake_charInfos_oneLine);

                int num_deleted = shouldDel_indexList.Count;
                num_deleted_list.Add(num_deleted);
            }
            return (remake_charInfos_list, num_deleted_list);
        }
        public static List<List<TMP_CharacterInfo>> DelEmptyLine(List<List<TMP_CharacterInfo>> screenCharInfos_list)
        {
            List<List<TMP_CharacterInfo>> delEmptyLine_charInfos_list = new List<List<TMP_CharacterInfo>>();
            foreach (List<TMP_CharacterInfo> screenCharInfo in screenCharInfos_list)
            {
                if (screenCharInfo.Count != 0)
                    delEmptyLine_charInfos_list.Add(screenCharInfo);
            }
            return delEmptyLine_charInfos_list;
        }
        public static List<List<TMP_CharacterInfo>> Remove_FirstLastCharSpace(List<List<TMP_CharacterInfo>> screenCharInfos_list)
        {
            List<List<TMP_CharacterInfo>> delSpace_screenCharInfos_list = screenCharInfos_list;

            for (int i = 0; i < screenCharInfos_list.Count; i++)
            {
                List<TMP_CharacterInfo> screenCharInfos_oneLine = screenCharInfos_list[i];
                if (screenCharInfos_oneLine[0].character == ' ')
                {
                    delSpace_screenCharInfos_list[i].RemoveAt(0);
                    if (delSpace_screenCharInfos_list[i].Count == 0)
                        continue;
                }
                if (screenCharInfos_oneLine[screenCharInfos_oneLine.Count-1].character == ' ')
                {
                    delSpace_screenCharInfos_list[i].RemoveAt(screenCharInfos_oneLine.Count - 1);
                }
            }

            return delSpace_screenCharInfos_list;
        }
    }
    public static class MinimalBoundingBox
    {
        /// <summary>
		/// Calculates the minimum bounding box.
		/// </summary>
		/// <param name="points">Bounding Box.</param>
        public static List<Vector2> Calculate(List<Vector2> points)
        {
            //calculate the convex hull
            List<Vector2> hullPoints = ConvexHull(points);

            //check if no bounding box available
            if (hullPoints.Count <= 1)
                return hullPoints;

            Rectangle2d minBox = null;
            float minAngle = 0;

            //foreach edge of the convex hull
            for (int i = 0; i < hullPoints.Count; i++)
            {
                int nextIndex = i + 1;
                Vector2 current = hullPoints[i];
                Vector2 next = hullPoints[nextIndex % hullPoints.Count];
                Segment2d segment = new Segment2d(current, next);

                //min / max points
                float top = float.MinValue;
                float bottom = float.MaxValue;
                float left = float.MaxValue;
                float right = float.MinValue;

                //get angle of segment to x axis
                float angle = AngleToXAxis(segment);

                //rotate every point and get min and max values for each direction
                foreach (Vector2 p in hullPoints)
                {
                    Vector2 rotatedPoint = RotateToXAxis(p, angle);

                    top = Mathf.Max(top, rotatedPoint.y);
                    bottom = Mathf.Min(bottom, rotatedPoint.y);
                    left = Mathf.Min(left, rotatedPoint.x);
                    right = Mathf.Max(right, rotatedPoint.x);
                }

                //create axis aligned bounding box
                Rectangle2d box = new Rectangle2d(new Vector2(left, bottom), new Vector2(right, top));

                if (minBox == null || minBox.Area() > box.Area())
                {
                    minBox = box;
                    minAngle = angle;
                }
            }

            //rotate axis algined box back
            List<Vector2> minimalBoundingBox = minBox.Points.Select(p => RotateToXAxis(p, -minAngle)).ToList();

            return minimalBoundingBox;
        }
        public static List<Vector2> ConvexHull(List<Vector2> points)
        {
            //sort vectors
            points.Sort((a,b) => a.x.CompareTo(b.x));
            //List<Vector2> hullPoints = new List<Vector2>();
            Vector2[] hullPoints = new Vector2[2 * points.Count];

            //break if only one point as input
            if (points.Count <= 1)
                return points;

            int pointLength = points.Count;
            int counter = 0;

            //iterate for lowerHull
            for (int i = 0; i < pointLength; ++i)
            {
                while (counter >= 2 && Cross(hullPoints[counter - 2], hullPoints[counter - 1], points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //iterate for upperHull
            for (int i = pointLength - 2, j = counter + 1; i >= 0; i--)
            {
                while (counter >= j && Cross(hullPoints[counter - 2],
                           hullPoints[counter - 1],
                           points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //remove duplicate start points
            //List<Vector2> result = hullPoints.GetRange(0, counter - 1);
            List<Vector2> result = hullPoints.ToList().GetRange(0, counter - 1);
            return result;
        }

        static float AngleToXAxis(Segment2d s)
        {
            Vector2 delta = s.A - s.B;
            return -Mathf.Atan(delta.y / delta.x);
        }
        static Vector2 RotateToXAxis(Vector2 v, float angle)
        {
            float newX = v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle);
            float newY = v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle);

            return new Vector2(newX, newY);
        }

        /// <summary>
		/// Cross the specified o, a and b.
		/// Zero if collinear
		/// Positive if counter-clockwise
		/// Negative if clockwise
		/// </summary>
		/// <param name="o">O.</param>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
        public static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }
    }
    public class Rectangle2d
    {
        public Vector2 location { get; set; }
        public Vector2 size { get; set; }
        public Rectangle2d()
        {
        }
        public Rectangle2d(Vector2 a, Vector2 c) : this()
        {
            location = a;
            size = c - a;
        }
        public float Area()
        {
            return size.x * size.y;
        }
        public List<Vector2> Points
        {
            get
            {
                return new List<Vector2>{
                    new Vector2 (location.x, location.y),
                    new Vector2 (location.x + size.x, location.y),
                    new Vector2 (location.x + size.x, location.y + size.y),
                    new Vector2 (location.x, location.y + size.y)
                };
            }
        }
    }
    public class Segment2d : Line2d
    {
        /// <summary>
        /// Second Point of Line.
        /// </summary>
        /// <value>The b.</value>
        public Vector2 B { get; set; }

        public Segment2d(Vector2 point1, Vector2 point2) :
            base(point1, point2 - point1)
        {
            B = point2;
        }

        public float Length()
        {
            return Mathf.Abs(Vector2.Distance(A, B));
        }
    }
    public class Line2d
    {
        public Vector2 A { get; set; }
        public Vector2 R { get; set; }
        public Line2d(Vector2 position, Vector2 direction)
        {
            A = position;
            R = direction;
        }
    }
}
