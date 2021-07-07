using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RandomText_Script
{
    public static class RandomText_Card
    {
        public static void Random_AlienName(List<string> surName, List<string> givenName, TMP_Text textObject)
        {
            int random_index_surName = UnityEngine.Random.Range(0, surName.Count);
            int random_index_givenName = UnityEngine.Random.Range(0, givenName.Count);
            string surName_string = surName[random_index_surName].ToUpper();
            string givenName_string = givenName[random_index_givenName].ToUpper();
            string name = surName_string + " " + givenName_string;
            textObject.SetText(name);
        }
        public static void Random_KorName(List<string> korName, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, korName.Count);
            string korName_string = korName[random_index];
            textObject.SetText(korName_string);
        }
        public static void Random_AlienType(List<string> type, TMP_Text textObject)
        {
            int random_num = UnityEngine.Random.Range(0, type.Count);
            string type_string = type[random_num];
            textObject.SetText(type_string);
        }
        public static void Random_DriverType(List<string> Driver_type, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, Driver_type.Count);
            string type_string = Driver_type[random_index];
            textObject.SetText(type_string);
        }
        public static void Random_Sex(TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, 2);
            string sex;
            if (random_index == 1)
                sex = "F";
            else
                sex = "M";
            textObject.SetText(sex);
        }
        public static void Random_Nation(List<string> nation, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, nation.Count);
            string nation_string = nation[random_index].ToUpper();
            textObject.SetText(nation_string);
        }
        public static void Random_Number(TMP_Text textObject, TMP_Text waterMarkObject)
        {
            int random_num1 = UnityEngine.Random.Range(0, 1000000);
            int random_num2 = UnityEngine.Random.Range(0, 10000000);
            string number_string = string.Format("{0:000000}-{1:0000000}", random_num1, random_num2);
            textObject.SetText(number_string);
            waterMarkObject.SetText(number_string);
        }

        public static void Random_AlienDate(TMP_Text textObject)
        {
            int year = UnityEngine.Random.Range(0, 20);
            int month = UnityEngine.Random.Range(1, 13);
            int day;
            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            textObject.SetText("20{0:00}.{1}.{2}", year, month, day);
        }
        public static void Random_OfficeKor(List<string> officeKor, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, officeKor.Count);
            string officeKorToken_string = officeKor[random_index];
            string officeKor_string = string.Format("{0}출입국관리사무소장", officeKorToken_string);
            textObject.SetText(officeKor_string);
        }
        public static void Random_OfficeEng(List<string> officeEng, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, officeEng.Count);
            string officeEngToken_string = officeEng[random_index].ToUpper();
            string officeEng_string = string.Format("CHIEF, {0} IMMIGRATION OFFICE", officeEngToken_string);
            textObject.SetText(officeEng_string);
        }
        public static void Random_PersonNumber(TMP_Text textObject)
        {
            int random_num1 = UnityEngine.Random.Range(0, 999999);
            int random_num2 = UnityEngine.Random.Range(0, 9999999);
            string personNumber_string = string.Format("{0:000000}-{1:0000000}", random_num1, random_num2);
            textObject.SetText(personNumber_string);
        }
        public static void Random_DriverAddress(List<string> address1, List<string> address2, List<string> address3, TMP_Text textObject)
        {
            int random_index1 = UnityEngine.Random.Range(0, address1.Count);
            int random_index2 = UnityEngine.Random.Range(0, address2.Count);
            int random_index3 = UnityEngine.Random.Range(0, address3.Count);

            string address = string.Format("{0} {1} {2}", address1[random_index1], address2[random_index2], address3[random_index3]);
            textObject.SetText(address);
        }
        public static void Random_IDAddress(List<string> address1, List<string> address2, List<string> address3, TMP_Text Address, TMP_Text Admin)
        {
            int random_index1 = UnityEngine.Random.Range(0, address1.Count);
            int random_index2 = UnityEngine.Random.Range(0, address2.Count);
            int random_index3 = UnityEngine.Random.Range(0, address3.Count);

            string address_string = string.Format("{0} {1} {2}", address1[random_index1], address2[random_index2], address3[random_index3]);
            Address.SetText(address_string);
            string admin_string = string.Format("{0} {1}장", address1[random_index1], address2[random_index2]);
            Admin.SetText(admin_string);
        }
        public static void Random_DriverDate(TMP_Text Date1, TMP_Text Date2, TMP_Text PublishDate)
        {
            int year = UnityEngine.Random.Range(0, 20);
            int month = UnityEngine.Random.Range(1, 13);
            int day;

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            //Date1
            string tmp_string1 = string.Format("20{0:00}.{1:00}.{2:00}.", year + 10, 1, 1);
            //Date2
            string tmp_string2 = string.Format("20{0:00}.{1:00}.{2:00}.", year + 10, 12, 31);
            //Date3
            string tmp_string3 = string.Format("20{0:00}.{1:00}.{2:00}.", year, month, day);

            Date1.SetText(tmp_string1);
            Date2.SetText(tmp_string2);
            PublishDate.SetText(tmp_string3);
        }
        public static void Random_IDDate(TMP_Text textObject)
        {
            int year = UnityEngine.Random.Range(0, 20);
            int month = UnityEngine.Random.Range(1, 13);
            int day;

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string tmp_string3 = string.Format("20{0:00}.{1:00}.{2:00}", year, month, day);

            textObject.SetText(tmp_string3);
        }
        public static void Random_CardNumber(TMP_Text textObject)
        {
            int random_num1 = UnityEngine.Random.Range(11, 29);  //admin num
            int random_num2 = UnityEngine.Random.Range(0, 100);  // year of first get license
            int random_num3 = UnityEngine.Random.Range(1, 1000000);
            int random_num4 = UnityEngine.Random.Range(1, 100);

            string cardNumber_string = string.Format("{0:00}-{1:00}-{2:000000}-{3:00}", random_num1, random_num2, random_num3, random_num4);
            textObject.SetText(cardNumber_string);
        }
        public static void Random_Require(TMP_Text textObject)
        {
            string require_Pool = "ABCDEFGHIJ";
            int random_index = UnityEngine.Random.Range(0, require_Pool.Length);
            string require_string = require_Pool[random_index].ToString();
            textObject.SetText(require_string);
        }
        public static void Random_SerialNumber(TMP_Text textObject)
        {
            string ran_alpabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int[] indexs = new int[6];

            for (int i = 0; i < 6; i++)
            {
                int random_index = UnityEngine.Random.Range(0, ran_alpabet.Length);
                indexs[i] = random_index;
            }

            string serialNumber_string = string.Format("{0}{1}{2}{3}{4}{5}", ran_alpabet[indexs[0]], ran_alpabet[indexs[1]], ran_alpabet[indexs[2]], ran_alpabet[indexs[3]], ran_alpabet[indexs[4]], ran_alpabet[indexs[5]]);
            textObject.SetText(serialNumber_string);
        }
        public static void Random_Admin(List<string> Driver_Admin, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, Driver_Admin.Count);
            string admin_string = Driver_Admin[random_index];
            textObject.SetText(admin_string);
        }
    }
    public static class RandomText_Passport
    {
        public static (string, string) Random_FullName(List<string> surname_dataSet, List<string> givenname_dataSet, TMP_Text textObject)
        {
            int surname_index = UnityEngine.Random.Range(0, surname_dataSet.Count);
            int givenname_index = UnityEngine.Random.Range(0, givenname_dataSet.Count);
            string surname = surname_dataSet[surname_index].ToUpper().Replace(" ", "");
            string givenname = givenname_dataSet[givenname_index].ToUpper().Replace(" ", "");
            string full_name = string.Format("{0} {1}", givenname, surname);
            textObject.SetText(full_name);
            return (surname, givenname);
        }
        public static string Random_Name(List<string> dataSet, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, dataSet.Count);
            string name_string = dataSet[random_index].ToUpper().Replace(" ", "");
            textObject.SetText(name_string);
            return name_string;
        }
        public static string Random_Birth(TMP_Text textObject) //birth
        {
            int year = UnityEngine.Random.Range(40, 100);
            int month = UnityEngine.Random.Range(1, 13);
            int day;
            string[] month_DataSet = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string birth_string = string.Format("{0:00}. {1}. {2:0000}", day, month_DataSet[month - 1], 1900 + year);
            textObject.SetText(birth_string);

            birth_string = string.Format("{0:00}{1:00}{2:00}", year, month, day);
            return birth_string;
        }
        public static string Random_Sex(TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, 2);
            string sex;
            if (random_index == 1)
                sex = "F";
            else
                sex = "M";
            textObject.SetText(sex);
            return sex;
        }
        public static string Random_Issue_And_Expiry(TMP_Text textObject1, TMP_Text textObject2) //issue, expiry
        {
            int year = UnityEngine.Random.Range(0, 20);
            int month = UnityEngine.Random.Range(1, 13);
            int day;
            string[] month_DataSet = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            if (month == 2)
                day = UnityEngine.Random.Range(1, 30);
            else if (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)
                day = UnityEngine.Random.Range(1, 31);
            else
                day = UnityEngine.Random.Range(1, 32);

            string issue_string = string.Format("{0:00}. {1}. {2:00}", day, month_DataSet[month - 1], 2000 + year);
            textObject1.SetText(issue_string);
            year += 10;
            string expiry_string = string.Format("{0:00}. {1}. {2:00}", day, month_DataSet[month - 1], 2000 + year);
            textObject2.SetText(expiry_string);

            expiry_string = string.Format("{0:00}{1:00}{2:00}", year, month, day);
            return expiry_string;
        }
        public static string Random_Type(List<string> Texts, List<string> types, TMP_Text textObject)
        {
            int random_index = UnityEngine.Random.Range(0, Texts.Count);
            string type = types[random_index];
            textObject.SetText(type);
            return type;
        }
        public static string Random_PassportNo(string type, TMP_Text textObject)
        {
            int random_num = UnityEngine.Random.Range(0, 100000000);
            string passportNo_string = string.Format("{0}{1:00000000}", type.Substring(1), random_num);
            textObject.SetText(passportNo_string);
            return passportNo_string;
        }
        public static string Random_PersonalNo(TMP_Text textObject)
        {
            int random_num = UnityEngine.Random.Range(0, 9999999);
            string personalNo_string = string.Format("{0:0000000}", random_num);
            textObject.SetText(personalNo_string);
            return personalNo_string;
        }
        public static void Random_Mr1(string type, string surname, string given, string nation, TMP_Text mr1)
        {
            string name = surname + "<<" + given;
            name = name.Replace("-", "<");
            if (name.Length > 40)
            {
                name = name.Substring(0, 40);
            }
            else
            {
                for (int i = name.Length; i < 39; i++)
                {
                    name = name.Insert(name.Length, "<");
                }
            }
            string tmp_string = string.Format("{0:<<}{1:<<<}{2:<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<}", type, nation, name);
            mr1.SetText(tmp_string);
        }
        public static void Random_Mr2(string Passport_no, string nation, string birth, string sex, string expiry, string Personal_no, TMP_Text mr2)
        {
            string tmp_string = string.Format("{0:<<<<<<<<<}{1}{2:<<<}{3:<<<<<<}{4}{5}{6:<<<<<<}{7}{8:<<<<<<<<}{9}{10:000000}{11}{12}",
                Passport_no, UnityEngine.Random.Range(0, 10), nation, birth, UnityEngine.Random.Range(0, 10), sex, expiry, UnityEngine.Random.Range(0, 10), Personal_no, "V", UnityEngine.Random.Range(0, 1000000), UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
            mr2.SetText(tmp_string);
        }
        public static void All_Random_Mr(TMP_Text mr1, TMP_Text mr2)
        {
            string alpha_pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<0123456789";
            System.Text.StringBuilder str1 = new System.Text.StringBuilder();
            System.Text.StringBuilder str2 = new System.Text.StringBuilder();
            int index;
            for (int i = 0; i < 44; i++)
            {
                index = UnityEngine.Random.Range(0, alpha_pool.Length);
                str1.Append(alpha_pool[index]);
            }
            for (int i = 0; i < 44; i++)
            {
                index = UnityEngine.Random.Range(0, alpha_pool.Length);
                str2.Append(alpha_pool[index]);
            }
            mr1.SetText(str1);
            mr2.SetText(str2);
        }
        public static string Extract_NationCode(string nation)
        {
            Dictionary<string, string> nationCode_dict = new Dictionary<string, string>()
            {
                {"Australia", "AUS"}, {"Bangladesh", "BGD"}, {"Bhutan", "BTN"}, {"Cambodia", "KHM"}, {"China", "CHN"},
                {"Egypt", "EGY"}, {"Ethiopia", "ETH"}, {"France", "FRA"}, {"India", "IND"}, {"Indonesia", "IDN"},
                {"Kazakhstan", "KAZ"}, {"Korea", "KOR"}, {"Kyrgyzstan", "KGZ"}, {"Libya", "LBY"}, {"Malaysia", "MYS"},
                {"Mali", "MLI"}, {"Mongolia", "MNG"}, {"Myanmar", "MMR"}, {"Nepal", "NPL"}, {"Newzealand", "NZL"},
                {"Pakistan", "PAK"}, {"Philippines", "PHL"}, {"Russia", "RUS"}, {"Southafrica", "ZAF"}, {"Sri Lanka", "LKA"},
                {"Tajikistan", "TJK"}, {"Thailand", "THA"}, {"Taiwan", "TWN"}, {"Ukraine", "UKR"}, {"USA", "USA"},
                {"Uzbekistan", "UZB"}, {"Vietnam", "VNM"}, { "Japan", "JPN"}
            };
            string nationCode = nationCode_dict[nation];
            return nationCode;
        }
    }
}

