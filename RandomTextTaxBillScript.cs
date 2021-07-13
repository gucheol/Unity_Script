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
            int probability_assistant = UnityEngine.Random.Range(1, 5);
            int price = UnityEngine.Random.Range(0, 1000000/ (int)(Mathf.Pow(10, probability_assistant)));
            string price_text = string.Format("{0}0000", price);
            GameObject.Find(price_obj_name).GetComponent<TMP_Text>().SetText(price_text);
            // 세금
            string tax_text = string.Format("{0}000", price);
            GameObject.Find(tax_obj_name).GetComponent<TMP_Text>().SetText(tax_text);
            // 공란수
            int blank_num = string.Format("{0}", 1000000000).Length - string.Format("{0}", price).Length-3;
            string blank_text = string.Format("{0}", blank_num);
            GameObject.Find(black_obj_name).GetComponent<TMP_Text>().SetText(blank_text);

            int total = price*10 + price;
            string total_text = string.Format("{0:#,0},000", total);
            GameObject.Find(total_obj_name).GetComponent<TMP_Text>().SetText(total_text);
        }
        public static void Gen_ListDate(string month_obj_name, string day_obj_name)
        {
            int month = UnityEngine.Random.Range(0, 13);
            int day;
            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string month_text = string.Format("{0}", month);
            GameObject.Find(month_obj_name).GetComponent<TMP_Text>().SetText(month_text);

            string day_text = string.Format("{0}", day);
            GameObject.Find(day_obj_name).GetComponent<TMP_Text>().SetText(day_text);
        }
        public static void Gen_ListGoodsName(string obj_name, List<string> goods_list)
        {
            int index = UnityEngine.Random.Range(0, goods_list.Count);
            string text = goods_list[index];
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_ListGoodsNum(string obj_name)
        {
            int num = UnityEngine.Random.Range(0, 100);
            string text = string.Format("{0}", num);
            GameObject.Find(obj_name).GetComponent<TMP_Text>().SetText(text);
        }
        public static void Gen_ListPrice(string supply_obj_name, string tax_obj_name, string total_obj_name)
        {
            int supply_price = UnityEngine.Random.Range(100, 100000000);
            int tax_price = supply_price / 10;
            int total_price = supply_price + tax_price;

            string supply_text = string.Format("{0:#,0}", supply_price);
            string tax_text = string.Format("{0:#,0}", tax_price);
            string total_text = string.Format("{0:#,0}", total_price);

            GameObject.Find(supply_obj_name).GetComponent<TMP_Text>().SetText(supply_text);
            GameObject.Find(tax_obj_name).GetComponent<TMP_Text>().SetText(tax_text);
            GameObject.Find(total_obj_name).GetComponent<TMP_Text>().SetText(total_text);
        }
        public static void Gen_ListContents(List<string> goods_list)
        {
            int gen_list_num = UnityEngine.Random.Range(1, 5); //목록 리스트가 4개
            // 텍스트 생성
            for(int i=1; i < gen_list_num + 1; i++)
            {
                string month_obj_name = string.Format("목록-월-내용{0}", i);
                string day_obj_name = string.Format("목록-일-내용{0}", i);
                string goods_name_obj_name = string.Format("목록-품목-내용{0}", i);
                string goods_num_obj_name = string.Format("목록-수량-내용{0}", i);
                string supply_obj_name = string.Format("목록-공급가액-내용{0}", i);
                string tax_obj_name = string.Format("목록-세액-내용{0}", i);
                string total_obj_name = string.Format("목록-단가-내용{0}", i);

                Gen_ListDate(month_obj_name, day_obj_name);
                Gen_ListGoodsName(goods_name_obj_name, goods_list);
                Gen_ListGoodsNum(goods_num_obj_name);
                Gen_ListPrice(supply_obj_name, tax_obj_name, total_obj_name);
            }

            for (int i = 4; gen_list_num < i; i--)
            {
                string month_obj_name = string.Format("목록-월-내용{0}", i);
                string day_obj_name = string.Format("목록-일-내용{0}", i);
                string goods_name_obj_name = string.Format("목록-품목-내용{0}", i);
                string goods_num_obj_name = string.Format("목록-수량-내용{0}", i);
                string supply_obj_name = string.Format("목록-공급가액-내용{0}", i);
                string tax_obj_name = string.Format("목록-세액-내용{0}", i);
                string total_obj_name = string.Format("목록-단가-내용{0}", i);

                GameObject.Find(month_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(day_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(goods_name_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(goods_num_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(supply_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(tax_obj_name).GetComponent<TMP_Text>().SetText(" ");
                GameObject.Find(total_obj_name).GetComponent<TMP_Text>().SetText(" ");
            }
        }
    }

}
