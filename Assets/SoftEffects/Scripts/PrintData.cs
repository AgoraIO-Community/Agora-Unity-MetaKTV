using System;
using UnityEngine;

namespace Mkey
{
    public class PrintData
    {
        #region ComputeBuffer 
        /// <summary>
        ///  Print to debug consol float comp buffer
        /// </summary>
        public static void BufTostring(ComputeBuffer c, int width, int height)
        {
            UnityEngine.Debug.Log("compute  buffer");
            float[] a = new float[width * height];
            c.GetData(a);

            BufTostring(a, width, height);
        }

        /// <summary>
        ///  Print to debug consol float comp buffer
        /// </summary>
        public static void BufTostring(ComputeBuffer c, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("compute  buffer");
            float[] a = new float[width * height];
            c.GetData(a);

            BufTostring(a, width, height, row, column);
        }

        /// <summary>
        ///  Print to debug consol float comp buffer
        /// </summary>
        public static void BufTostring(ComputeBuffer c, int width, int height, int row, int column, bool numbers)
        {
            UnityEngine.Debug.Log("compute  buffer");
            float[] a = new float[width * height];
            c.GetData(a);

            BufTostring(a, width, height, row, column, numbers);
        }
        #endregion ComputeBuffer

        #region double
        /// <summary>
        ///  Print to debug consol int array in to 2 dim
        /// </summary>
        public static void BufTostring(double[] a, int width, int height)
        {
            UnityEngine.Debug.Log("int[] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s +=((long)a[i * width + j]).ToString() + " ";
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }
        #endregion double

        #region int 
        /// <summary>
        ///  Print to debug consol 2D int [,] array in to 2 dim
        /// </summary>
        public static void BufTostring(int[,] a, int width, int height)
        {
            UnityEngine.Debug.Log("int[,] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[i, j].ToString() + " ";
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol int array in to 2 dim
        /// </summary>
        public static void BufTostring(int[] a, int width, int height)
        {
            UnityEngine.Debug.Log("int[] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[i * width + j].ToString();
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }
        #endregion int

        #region byte
        /// <summary>
        ///  Print to debug consol 2D int [,] array in to 2 dim
        /// </summary>
        public static void BufTostring(byte[,] a, int width, int height)
        {
            UnityEngine.Debug.Log("int[,] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[i, j].ToString() + " ";
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        #endregion byte

        #region float
        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(float[] a, int width, int height)
        {
            UnityEngine.Debug.Log("float [] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += System.String.Format("{0:0.000 } ", a[i * width + j]); 
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(float[] a, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("float [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += System.String.Format("{0:0.000 } ", a[row * width + j]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    s += System.String.Format("{0:0.000 } ", a[i * width + column]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(float[] a, int width, int height, int row, int column, bool numbers)
        {
            UnityEngine.Debug.Log("float [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    if (numbers) { s += (j.ToString() +")" ); }
                    s += System.String.Format("{0:0.000 } ", a[row * width + j]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    if (numbers) { s += (i.ToString() + ")"); }
                    s += System.String.Format("{0:0.000 } ", a[i * width + column]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }


        /// <summary>
        ///  Print to debug consol 2D float [,] array in to 2 dim
        /// </summary>
        public static void BufTostring(float[,] a, int width, int height)
        {
            UnityEngine.Debug.Log("float [,] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += j.ToString() + ") " + System.String.Format("{0:0.000 } ", a[i, j]); ;
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(float[,] a, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("float [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += System.String.Format("{0:0.000 } ", a[row, j]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    s += System.String.Format("{0:0.000 } ", a[i, column]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

        }
        #endregion float

        #region Vector2
        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(Vector2[] a, int width, int height)
        {
            UnityEngine.Debug.Log("Vector2 [] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += System.String.Format("{0:0.000 } ", a[i * width + j]);
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(Vector2[] a, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("float [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += System.String.Format("{0:0.00 } ", a[row * width + j]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    s += System.String.Format("{0:0.00 } ", a[i * width + column]);
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }
        #endregion Vector2

        #region long
        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(long[] a, int width, int height)
        {
            UnityEngine.Debug.Log("long [] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s +=  a[i * width + j].ToString() +" ";
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(long[] a, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("long [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[row * width + j].ToString() +" ";
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    s +=  a[i * width + column].ToString() + " ";
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 2D float [,] array in to 2 dim
        /// </summary>
        public static void BufTostring(long[,] a, int width, int height)
        {
            UnityEngine.Debug.Log("long [,] buffer");
            for (int i = 0; i < height; i++)
            {
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[i, j].ToString() + " ";
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol 1D float [] array in to 2 dim
        /// </summary>
        public static void BufTostring(long[,] a, int width, int height, int row, int column)
        {
            UnityEngine.Debug.Log("long [] buffer");
            if (row >= 0 && row < height)
            {
                UnityEngine.Debug.Log("buffer row: " + row);
                string s = "";
                for (int j = 0; j < width; j++)
                {
                    s += a[row, j].ToString() + " ";
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < width)
            {
                UnityEngine.Debug.Log("buffer column: " + column);
                string s = "";
                for (int i = 0; i < height; i++)
                {
                    s += a[i, column].ToString() + " ";
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

        }
        #endregion long

        #region texture

        /// <summary>
        ///  Print to debug consol Texture2D color in to 2 dim, for component (r = 0, g = 1, b = 2, a = 3)
        /// </summary>
        public static void Texture2DToString(Texture2D a, int component)
        {
            UnityEngine.Debug.Log("texture component plot");
            for (int i = 0; i < a.height; i++)
            {
                string s = "";
                for (int j = 0; j < a.width; j++)
                {
                    if (component == 3)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, i).a);
                    }
                    else if (component == 2)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, i).b);
                    }
                    else if (component == 1)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, i).g);
                    }
                    else if (component == 0)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, i).r);
                    }
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol Texture2D color in to 2 dim
        /// </summary>
        public static void Texture2DToString(Texture2D a)
        {
            UnityEngine.Debug.Log("texture plot");
            for (int i = 0; i < a.height; i++)
            {
                string s = "";
                for (int j = 0; j < a.width; j++)
                {
                    s += a.GetPixel(j, i).ToString();
                }
                if (s != "")
                    UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol Texture2D color in to 2 dim, for component (r = 0, g = 1, b = 2, a = 3)
        /// </summary>
        public static void Texture2DToString(Texture2D a, int component, int row, int column)
        {
            UnityEngine.Debug.Log("texture component plot");

            if (row >= 0 && row < a.height)
            {
                UnityEngine.Debug.Log("texture row: " + row);
                string s = "";
                for (int j = 0; j < a.width; j++)
                {
                    if (component == 3)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, row).a);
                    }
                    else if (component == 2)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, row).b);
                    }
                    else if (component == 1)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, row).g);
                    }
                    else if (component == 0)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(j, row).r);
                    }
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < a.width)
            {
                UnityEngine.Debug.Log("texture column: " + column);
                string s = "";
                for (int i = 0; i < a.height; i++)
                {
                    if (component == 3)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(column, i).a);
                    }
                    else if (component == 2)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(column, i).b);
                    }
                    else if (component == 1)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(column, i).g);
                    }
                    else if (component == 0)
                    {
                        s += System.String.Format("{0:0.000 } ", a.GetPixel(column, i).r);
                    }
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }

        /// <summary>
        ///  Print to debug consol Texture2D color in to 2 dim
        /// </summary>
        public static void Texture2DToString(Texture2D a, int row , int column)
        {
            UnityEngine.Debug.Log("texture component plot");

            if (row >= 0 && row < a.height)
            {
                UnityEngine.Debug.Log("texture row: " + row);
                string s = "";
                for (int j = 0; j < a.width; j++)
                {
                        s +=  a.GetPixel(j, row).ToString();
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }

            if (column >= 0 && column < a.width)
            {
                UnityEngine.Debug.Log("texture column: " + column);
                string s = "";
                for (int i = 0; i < a.height; i++)
                {
                        s +=  a.GetPixel(column, i).ToString();
                }
                if (s != "") UnityEngine.Debug.Log(s);
            }
        }
        #endregion texture

        /// <summary>
        ///  Print 1D char array to string
        /// </summary>
        public static string CharsToString(char[] chars, char[] divider, char endChar)
        {
            string res = "";

            if (chars != null && chars.Length > 0)
            {
                int length = chars.Length;
                if (divider != null && divider.Length > 0)
                {
                    for (int ic = 0; ic < length; ic++)
                    {
                        res += chars[ic];
                        for (int id = 0; id < divider.Length; id++)
                        {
                            if (ic == length - 1) { res += endChar; }
                            else { res += divider[id]; }
                        }
                    }
                }
                else
                {
                    for (int ic = 0; ic < length; ic++)
                    {
                        res += chars[ic];
                    }
                    res += endChar;
                }
            }
            return res;
        }

        /// <summary>
        ///  Print 1D char array to string
        /// </summary>
        public static string FloatsToString(float[] floats, char[] divider, char endChar)
        {
            string res = "";

            if (floats != null && floats.Length > 0)
            {
                int length = floats.Length;
                if (divider != null && divider.Length > 0)
                {
                    for (int ic = 0; ic < length; ic++)
                    {
                        res += floats[ic].ToString();
                        res += "f";
                        for (int id = 0; id < divider.Length; id++)
                        {
                            if (ic == length - 1) { res += endChar; }
                            else { res += divider[id]; }
                        }
                    }
                }
                else
                {
                    for (int ic = 0; ic < length; ic++)
                    {
                        res += floats[ic];
                    }
                    res += endChar;
                }
            }
            return res;
        }
    }
}
