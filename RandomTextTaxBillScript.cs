using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TaxBillTextGen_Script
{
    public class RandomTextTaxBill
    {
        public static void Gen_BookID(string obj_name)
        {
            int num = UnityEngine.Random.Range(0, 100);
            string text = string.Format("{0}", num);
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_SN(string obj_name)
        {
            int num = UnityEngine.Random.Range(10000, 100000);
            string text = string.Format("{0}", num);
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_CompanyID(string obj_name)
        {
            int num0 = UnityEngine.Random.Range(0, 1000);
            int num1 = UnityEngine.Random.Range(0, 100);
            int num2 = UnityEngine.Random.Range(0, 100000);
            string text = string.Format("{0:000}-{1:00}-{2:00000}", num0, num1, num2);
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_CompanyName(string obj_name, List<string> name_list)
        {
            int index = UnityEngine.Random.Range(0, name_list.Count);
            string text = name_list[index];
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_PersonName(string obj_name, List<string> name_list)
        {
            int index = UnityEngine.Random.Range(0, name_list.Count);
            string text = name_list[index];
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_CompanyAddress(string obj_name, List<string> address_list1, List<string> address_list2, List<string> address_list3)
        {
            int index1 = UnityEngine.Random.Range(0, address_list1.Count);
            int index2 = UnityEngine.Random.Range(0, address_list2.Count);
            int index3 = UnityEngine.Random.Range(0, address_list3.Count);
            string text = string.Format("{0} {1} {2}", address_list1[index1], address_list2[index2], address_list3[index3]);
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_Category(string obj_name, List<string> name_list)
        {
            int index = UnityEngine.Random.Range(0, name_list.Count);
            string text = name_list[index];
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_type(string obj_name, List<string> name_list)
        {
            int index = UnityEngine.Random.Range(0, name_list.Count);
            string text = name_list[index];
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_Date(string year_obj_name, string month_obj_name, string day_obj_name)
        {
            int year = UnityEngine.Random.Range(2014, 2022);
            int month = UnityEngine.Random.Range(0, 13);
            int day;
            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string year_text = string.Format("{0}", year);
            GameObject.Find(year_obj_name).GetComponent<TMP_Text>().SetText(year_text);

            string month_text = string.Format("{0}", month);
            GameObject.Find(month_obj_name).GetComponent<TMP_Text>().SetText(month_text);

            string day_text = string.Format("{0}", day);
            GameObject.Find(day_obj_name).GetComponent<TMP_Text>().SetText(day_text);
        }
        public static void Gen_SupplyPrice(string black_obj_name, string price_obj_name, string tax_obj_name, string total_obj_name)
        {
            // 공급가액
            int price = UnityEngine.Random.Range(0, 1000000000);
            string price_text = string.Format("{0}00", price);
            GameObject.Find(price_obj_name).GetComponent<TMP_Text>().SetText(price_text);
            // 세금
            string tax_text = string.Format("{0}0", price);
            GameObject.Find(tax_obj_name).GetComponent<TMP_Text>().SetText(tax_text);
            // 공란수
            int blank_num = string.Format("{0}", 1000000000).Length - string.Format("{0}", price).Length-1;
            string blank_text = string.Format("{0}", blank_num);
            GameObject.Find(black_obj_name).GetComponent<TMP_Text>().SetText(blank_text);
            //if (blank_num == 0)
            //{
            //    GameObject blank_obj = GameObject.Find(black_obj_name);
            //    GameObject price_obj = GameObject.Find(price_obj_name);
            //    blank_obj.name = "공급가액-금액-내용_part1";
            //    price_obj.name = "공급가액-금액-내용-part2";
            //}

            int total = price * 10 + price;
            string total_text = string.Format("{0}0", total);
            GameObject.Find(total_obj_name).GetComponent<TMP_Text>().SetText(total_text);
        }
    }

}
