using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ColorSetAlgorithm
{
    class Program
    {
        private static string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static void Main(string[] args)
        {
            List<string> colorList = new List<string>() { "#FFFF0000", "#FF00FF00", "#FF0000FF" };
            try
            {
                string[] colorArr = System.IO.File.ReadAllLines(path + "\\colors.txt");
                string reg = "^#FF[0-9A-F]{6}$";
                if (colorArr != null && colorArr.Length > 0)
                {
                    List<string> tmpColor = new List<string>();
                    foreach (var tmp in colorArr)
                    {
                        if (!String.IsNullOrWhiteSpace(tmp))
                        {
                            if (Regex.IsMatch(tmp.Trim().ToUpper(), reg))
                            {
                                tmpColor.Add(tmp.Trim().ToUpper());
                            }
                        }
                    }
                    if (tmpColor.Count > 0)
                    {
                        colorList.Clear();
                        colorList.AddRange(tmpColor);
                    }
                }
            }
            catch (Exception e)
            {
                //Color File Not Exist, Use default color
            }
            List<List<string>> rawKeysLL = GenerateRawKeysLL();
            try
            {
                string[] keyArr = System.IO.File.ReadAllLines(path + "\\keys.txt");
                string reg = "^[0-9A-Z]{1,4}$";
                if (keyArr != null && keyArr.Length > 0)
                {
                    List<string> tmpColor = new List<string>();
                    List<List<string>> tmpLL = new List<List<string>>();
                    foreach (var tmp in keyArr)
                    {
                        if (!String.IsNullOrWhiteSpace(tmp))
                        {
                            string[] keys = tmp.Split(new char[] { ' ' });
                            List<string> keyL = new List<string>();
                            foreach (var k in keys)
                            {
                                if (!String.IsNullOrWhiteSpace(k))
                                {
                                    if (Regex.IsMatch(k.Trim().ToUpper(), reg))
                                    {
                                        keyL.Add(k.Trim().ToUpper());
                                    }
                                }
                            }
                            if (keyL.Count > 0)
                            {
                                tmpLL.Add(keyL);
                            }
                        }
                    }
                    if (tmpLL.Count > 0)
                    {
                        foreach (var t in rawKeysLL)
                        {
                            t.Clear();
                        }
                        rawKeysLL.Clear();
                        rawKeysLL = tmpLL;
                    }
                }
            }
            catch (Exception e)
            {
                //Key file not exist, use random generated keys list
            }
            Dictionary<string, string> dic = ComputeColorDic(rawKeysLL, colorList);
            CreateImage(rawKeysLL, dic);
        }

        private static List<List<string>> GenerateRawKeysLL()
        {
            Random rnd = new Random();
            List<List<string>> rkll = new List<List<string>>();

            for (int i = 0; i < 18; i++)
            {
                int k = rnd.Next(1, 9);
                List<string> lst = new List<string>();
                for (int j = 0; j < k; j++)
                {
                    lst.Add(rnd.Next(1001, 1020).ToString());
                }
                rkll.Add(lst);
            }

            return rkll;
        }

        public static Dictionary<string, string> ComputeColorDic(List<List<string>> param, List<string> colorList)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var tmpList in param)
            {
                foreach (var tmpKey in tmpList)
                {
                    if (!dic.ContainsKey(tmpKey))
                    {
                        List<string> connected = ComputeConnected(tmpKey, param);
                        if (!lst.Exists(r => r.Key == tmpKey))
                        {
                            if (connected.Count < 1)
                            {
                                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(tmpKey, "");
                                lst.Add(kv);
                            }
                            else
                            {
                                foreach (var tmpConnected in connected)
                                {
                                    KeyValuePair<string, string> kv = new KeyValuePair<string, string>(tmpKey, tmpConnected);
                                    lst.Add(kv);
                                }
                            }
                        }
                        dic.Add(tmpKey, null);
                    }
                }
            }
            Dictionary<string, string> res = ComputeColorDic(lst, colorList);
            return res;
        }

        private static Dictionary<string, string> ComputeColorDic(List<KeyValuePair<string, string>> lst, List<string> colorList)
        {
            List<string> keys = lst.Select(r => r.Key).Distinct().OrderBy(r => r).ToList();
            Dictionary<string, string> colorDic = new Dictionary<string, string>();
            List<string> pop = new List<string>();
            //<key,color> is stored here, if color is set black due to lack of color data,<key,""> will be stored
            Dictionary<string, string> alreadySetKeys = new Dictionary<string, string>();
            foreach (var tmpKey in keys)
            {
                List<string> connected = lst.Where(r => r.Key == tmpKey).Select(r => r.Value).OrderBy(r => r).ToList();
                SetColorDic(tmpKey, connected, pop, colorList, lst, alreadySetKeys);
            }
            foreach (var tmp in alreadySetKeys)
            {
                if (String.IsNullOrEmpty(tmp.Value))
                {
                    colorDic.Add(tmp.Key, "#FF000000");
                }
                else
                {
                    colorDic.Add(tmp.Key, tmp.Value);
                }
            }
            return colorDic;
        }

        private static void SetColorDic(string tmpKey, List<string> connected, List<string> pop, List<string> colorList, List<KeyValuePair<string, string>> lst, Dictionary<string, string> alreadySetKeys)
        {
            if (!alreadySetKeys.ContainsKey(tmpKey))
            {
                if (pop.Count == 0)
                {
                    pop.AddRange(colorList);
                }
                else if (pop.Count < colorList.Count)
                {
                    int index = colorList.IndexOf(pop[pop.Count - 1]);
                    //pop's elements is added as loop
                    for (int i = index; i < colorList.Count; i++)
                    {
                        if (!pop.Contains(colorList[i]))
                        {
                            pop.Add(colorList[i]);
                        }
                    }
                    foreach (var tmp in colorList)
                    {
                        if (!pop.Contains(tmp))
                        {
                            pop.Add(tmp);
                        }
                    }
                }

                if (connected != null)
                {
                    //connected color be removed here
                    foreach (var tmpCon in connected)
                    {
                        if (alreadySetKeys.ContainsKey(tmpCon))
                        {
                            if (pop.Contains(alreadySetKeys[tmpCon]))
                            {
                                pop.Remove(alreadySetKeys[tmpCon]);
                            }
                        }
                    }
                }

                if (pop.Count > 0)
                {
                    //pop list's first color , because of pop has the same order as colorList
                    alreadySetKeys.Add(tmpKey, pop[0]);
                    pop.Remove(pop[0]);
                }
                else
                {
                    alreadySetKeys.Add(tmpKey, "");
                }
            }

            //connected key color set
            for (int i = 0; i < connected.Count; i++)
            {
                SetColorDicForConnected(tmpKey, connected[i], pop, colorList, lst, alreadySetKeys);
            }
        }

        private static void SetColorDicForConnected(string tmpKey, string v, List<string> pop, List<string> colorList, List<KeyValuePair<string, string>> lst, Dictionary<string, string> alreadySetKeys)
        {
            if (!alreadySetKeys.ContainsKey(v) && !string.IsNullOrWhiteSpace(v))
            {
                if (pop.Count == 0)
                {
                    pop.AddRange(colorList);
                }
                else if (pop.Count < colorList.Count)
                {
                    int index = colorList.IndexOf(pop[pop.Count - 1]);
                    //pop's elements is added as loop
                    for (int i = index; i < colorList.Count; i++)
                    {
                        if (!pop.Contains(colorList[i]))
                        {
                            pop.Add(colorList[i]);
                        }
                    }
                    foreach (var tmp in colorList)
                    {
                        if (!pop.Contains(tmp))
                        {
                            pop.Add(tmp);
                        }
                    }
                }
                if (pop.Contains(alreadySetKeys[tmpKey]))
                {
                    pop.Remove(alreadySetKeys[tmpKey]);
                }
                if (pop.Count > 0)
                {
                    string toRemove = null;
                    //loop all pop, find the color which is not used
                    //or which is used but not connected with this one
                    for (int j = 0; j < pop.Count; j++)
                    {
                        List<string> sameColorkeys = new List<string>();
                        foreach (var tmp in alreadySetKeys)
                        {
                            if (tmp.Value == pop[j])
                                sameColorkeys.Add(tmp.Key);
                        }
                        bool isColorDuplicate = false;
                        foreach (var tmpkey in sameColorkeys)
                        {
                            //all elements in lst should be checked
                            if (lst.Exists(r => r.Key == tmpkey && r.Value == v))
                            {
                                //this color has been set somewhere else
                                isColorDuplicate = true;
                            }
                        }
                        if (isColorDuplicate == false)
                        {
                            alreadySetKeys.Add(v, pop[j]);
                            toRemove = pop[j];
                            break;
                        }
                    }
                    //no color found, set color null
                    if (toRemove == null)
                        alreadySetKeys.Add(v, "");
                    else
                        pop.Remove(toRemove);
                }
                else
                {
                    alreadySetKeys.Add(v, "");
                }
            }
        }

        private static List<string> ComputeConnected(string key, List<List<string>> param)
        {
            List<string> res = new List<string>();
            foreach (var tmpList in param)
            {
                for (int i = 0; i < tmpList.Count; i++)
                {
                    if (tmpList[i] == key)
                    {
                        if (i - 1 >= 0)
                        {
                            if (tmpList[i - 1] != key && !res.Contains(tmpList[i - 1]))
                            {
                                res.Add(tmpList[i - 1]);
                            }
                        }
                        if (i + 1 < tmpList.Count)
                        {
                            if (tmpList[i + 1] != key && !res.Contains(tmpList[i + 1]))
                            {
                                res.Add(tmpList[i + 1]);
                            }
                        }
                    }
                }
            }
            return res;
        }

        public static void CreateImage(List<List<string>> keysLL, Dictionary<string, string> colorDic)
        {
            if (keysLL == null || keysLL.Count < 1 || colorDic == null || colorDic.Count < 1)
            {
                return;
            }
            Bitmap bm = new Bitmap(1024, 768);
            Graphics g = Graphics.FromImage(bm);
            g.Clear(Color.AliceBlue);
            ColorConverter con = new ColorConverter();

            int y = 100;
            foreach (var tmp in keysLL)
            {
                int x = 50;
                foreach (var k in tmp)
                {
                    Pen pen = new Pen((Color)con.ConvertFromString(colorDic[k]), 10);
                    Point point1 = new Point(x, y);
                    Point point2 = new Point(x + 50, y);
                    g.DrawLine(pen, point1, point2);
                    DrawText(g, k, x, y);
                    x += 80;
                    pen.Dispose();
                }
                y += 30;
            }
            string filename = path + "\\" + DateTime.Now.ToString("MM月dd日HHmmss") + ".png";
            bm.Save(filename, ImageFormat.Png);
            g.Dispose();
            bm.Dispose();
        }

        private static void DrawText(Graphics g, string text, int p, int q)
        {
            Font drawFont = new Font("Arial", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            float x = p - 4;
            float y = q - 20;
            StringFormat drawFormat = new StringFormat();
            g.DrawString(text, drawFont, drawBrush, x, y, drawFormat);
            drawFont.Dispose();
            drawBrush.Dispose();
        }

    }
}
