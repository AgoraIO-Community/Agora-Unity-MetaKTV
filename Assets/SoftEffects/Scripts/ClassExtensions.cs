using System;
using System.Collections.Generic;
//using System.Threading.Tasks;

using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
#if UNITY_EDITOR
    public static class ClassExtensions
    {
        /// <summary>
        ///  Copy source texture pixels to target texture pixels, if textures has equal size
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        private static void CopyTo(this Texture2D source, ref Texture2D target)
        {
            if (target.width != source.width || target.height != source.height) return;
            for (int y = 0; y < target.height; y++)
            {
                for (int x = 0; x < target.width; x++)
                {
                    target.SetPixel(x, y, source.GetPixel(x, y));
                }
            }
            target.Apply();
        }

        /// <summary>
        ///  Copy source texture pixels to target texture pixels, if textures has equal size
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void CreateAlpha8FromInputAlphaChannelRGBA(this Texture2D inputText, ref Texture2D outText2D, bool save, string path)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            ReimportTexture(inputText, true);
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.Alpha8, false);

            for (int y = 0; y < outText2D.height; y++)
            {
                for (int x = 0; x < outText2D.width; x++)
                {
                    Color inColor = inputText.GetPixel(x, y);
                    outText2D.SetPixel(x, y, new Color(inColor.a, inColor.a, inColor.a, inColor.a));
                }
            }
            outText2D.Apply();
            if (save && path != "")
            {
                SaveTextureToPng(outText2D, path);
                UnityEngine.Object.DestroyImmediate(outText2D);
                outText2D = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
                ReimportTexture(path, false, outText2D.height);
            }
        }

        /// <summary>
        /// Create Texture2D ARGB32  from render texture  ARGB32 and save it to file. Path like - "Assets/SoftEffects/Fonts/gpu.png"
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public static void CreateTextureFromRender_ARGB32(this RenderTexture inputText, ref Texture2D outText2D, bool save, string path)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.ARGB32, false);
            //  Debug.Log("Read texture, inputTex.format: " + inputText.format + " ;outText.format: " + outText2D.format);
            RenderTexture.active = inputText;
            outText2D.ReadPixels(new Rect(0, 0, inputText.width, inputText.height), 0, 0);
            RenderTexture.active = null;
            outText2D.Apply();

            if (save && path != "")
            {
                SaveTextureToPng(outText2D, path);
                UnityEngine.Object.DestroyImmediate(outText2D);
                outText2D = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
                ReimportTexture(path, false, GetMaxSize(inputText.width, inputText.height));
                // ReimportFontTexture(path, false, 4096);
            }
        }

        /// <summary>
        /// Create Texture2D ARGB32  from render texture  ARGB32 and save it to file. Path like - "Assets/SoftEffects/Fonts/gpu.png"
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public static void CreateTextureFromRender_A8(this RenderTexture inputText, ref Texture2D outText2D, bool save, string path)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.Alpha8, false);
            Debug.Log("Read texture, inputTex.format: " + inputText.format + " ;outText.format: " + outText2D.format);
            RenderTexture.active = inputText;
            // outText2D.ReadPixels(new Rect(0, 0, inputText.width, inputText.height), 0, 0);
            RenderTexture.active = null;
            outText2D.Apply();

            if (save && path != "")
            {
                SaveTextureToPng(outText2D, path);
                UnityEngine.Object.DestroyImmediate(outText2D);
                outText2D = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
                ReimportTexture(path, false, outText2D.height);
            }
        }

        /// <summary>
        /// Create RFloat Texture2D  from render texture RFloat
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public static void CreateTextureFromRender_Speed_R32(this RenderTexture inputText, ref Texture2D outText2D)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.RFloat, false);
            Graphics.CopyTexture(inputText, outText2D);
            Debug.Log("Copy, inputTex.format: " + inputText.format + " ;outText.format: " + outText2D.format);
        }

        /// <summary>
        /// Create R16 Texture2D  from render texture RHalf
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public static void CreateTextureFromRender_Speed_R16(this RenderTexture inputText, ref Texture2D outText2D)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.R16, false);
            Graphics.CopyTexture(inputText, outText2D);
            Debug.Log("Copy, inputTex.format: " + inputText.format + " ;outText.format: " + outText2D.format);
        }

        /// <summary>
        /// Create Texture2D ARGB32 from render texture ARGB32"
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public static void CreateTextureFromRender_Speed_ARGB32(this RenderTexture inputText, ref Texture2D outText2D)
        {
            if (inputText == null)
            {
                Debug.Log("Can't create Texture2D - input is null ");
                return;
            }
            outText2D = new Texture2D(inputText.width, inputText.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(inputText, outText2D);
            Debug.Log("Copy, inputTex.format: " + inputText.format + " ;outText.format: " + outText2D.format);
        }

        /// <summary>
        /// Save texture to png file at path like -"Assets/SoftEffects/Fonts/texture.png"
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="filePath"></param>
        public static void SaveTextureToPng(this Texture2D texture, string filePath)
        {
            byte[] bytes = texture.EncodeToPNG();
            FileStream stream = new FileStream(filePath, System.IO.FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < bytes.Length; i++)
            {
                writer.Write(bytes[i]);
            }
            writer.Close();
            stream.Close();
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();
        }

        public static void ReimportTexture(this Texture t, bool readable, ref bool prefReadable)
        {
            string path = AssetDatabase.GetAssetPath(t);
            int maxSize = (t.width > t.height) ? t.width : t.height;
            ReimportTexture(path, readable, maxSize, ref prefReadable);
        }

        public static void ReimportTexture(this Texture t, bool readable)
        {
            string path = AssetDatabase.GetAssetPath(t);
            int maxSize = (t.width > t.height) ? t.width : t.height;
            ReimportTexture(path, readable, maxSize);
        }

        public static void ReimportTexture(this Texture t, float pixelsPerUnit, bool readable)//https://forum.unity3d.com/threads/how-to-use-textureimporter-to-change-textures-format-and-re-import-again.86177/
        {
            string path = AssetDatabase.GetAssetPath(t);
            int maxSize = (t.width > t.height) ? t.width : t.height;
            ReimportTexture(path, readable, maxSize, pixelsPerUnit);
        }

        public static void ReimportTexture(this Texture t, bool readable, int maxSize)//https://forum.unity3d.com/threads/how-to-use-textureimporter-to-change-textures-format-and-re-import-again.86177/
        {
            string path = AssetDatabase.GetAssetPath(t);
            ReimportTexture(path, readable, maxSize);
        }

        public static void ReimportTexture(string path, bool readable, int maxSize)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer)
            {
                importer.isReadable = readable;
                importer.filterMode = FilterMode.Trilinear;
                importer.maxTextureSize = maxSize;
                importer.mipmapEnabled = false;

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.anisoLevel = 0;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.Log("no importer");
            }
        }

        public static void ReimportTexture(string path, bool readable, int maxSize, ref bool prevReadable)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer)
            {
                prevReadable = importer.isReadable;
                importer.isReadable = readable;
                importer.filterMode = FilterMode.Trilinear;
                importer.maxTextureSize = maxSize;
                importer.mipmapEnabled = false;

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.anisoLevel = 0;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.Log("no importer");
            }
        }

        public static void ReimportTexture(string path, bool readable, int maxSize, float pixelsPerUnit)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer)
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.isReadable = readable;
                importer.filterMode = FilterMode.Trilinear;
                importer.maxTextureSize = maxSize;
                importer.mipmapEnabled = false;

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.anisoLevel = 0;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.Log("no importer");
            }
        }

        public static void ReimportTexture(string path, bool readable)
        {
            AssetDatabase.LoadMainAssetAtPath(path); // load new texture asset

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (importer)
            {
                importer.isReadable = readable;
                importer.filterMode = FilterMode.Trilinear;
                importer.mipmapEnabled = false;

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.anisoLevel = 0;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.Log("no importer");
            }
        }

        public static void ReimportTextureAsSprite(string path, bool readable)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            TextureImporterSettings tis = new TextureImporterSettings();

            if (importer)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Single;
            }
            importer.ReadTextureSettings(tis);
            tis.spriteMeshType = SpriteMeshType.FullRect;
            tis.textureType = TextureImporterType.Sprite;
            tis.readable = readable;
            tis.mipmapEnabled = false;
            tis.npotScale = TextureImporterNPOTScale.None;
            tis.filterMode = FilterMode.Trilinear;
            tis.alphaIsTransparency = true;
            tis.spriteExtrude = 1;
            tis.spritePixelsPerUnit = 100;
            tis.alphaSource = TextureImporterAlphaSource.FromInput;
            tis.sRGBTexture = true;
            tis.spriteMode = 1;
            importer.SetTextureSettings(tis);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void ReimportTextureAsSprite_1(string path, float pixelsPerUnit, bool readable)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            TextureImporterSettings tis = new TextureImporterSettings();
            if (importer)
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.isReadable = readable;
                importer.mipmapEnabled = false;
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.npotScale = TextureImporterNPOTScale.None;
            }
            importer.ReadTextureSettings(tis);
            tis.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(tis);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Load png image in to texture2D at the path like - "Assets/SoftEffects/Fonts/texture.png"
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }
            return tex;
        }

        /// <summary>
        /// Get bytes array from png file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] PNG2Bytes(string filePath)
        {
            byte[] fileData = new byte[1];
            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
            }
            return fileData;
        }

        public static void DeleteFilesFromDir(string path, string file_key)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles("*.*");
            foreach (FileInfo f in info)
            {
                if (f.Name.Contains(file_key)) f.Delete();
            }
        }

        public static void PrintFontCharacters(this Font f)
        {
            CharacterInfo[] ch = f.characterInfo;
            char[] chars = new char[ch.Length];
            for (int i = 0; i < ch.Length; i++)
            {
                chars[i] = Convert.ToChar(ch[i].index);
            }
            // Debug.Log(Mkey.PrintData.CharsToString(chars, new char[] { }, '.'));
        }

        /// <summary>
        /// Add additional space between symbols on texture and create nev UVs  for font
        /// </summary>
        /// <param name="font"></param>
        public static void ExtendVertsAndUvs(this Font font, int extPixels)
        {
            CharacterInfo[] ch = font.characterInfo;
            float dx;
            float dy;
            for (int i = 0; i < ch.Length; i++)
            {
                dx = ch[i].maxX - ch[i].minX;
                dy = ch[i].maxY - ch[i].minY;

                ch[i].maxX += extPixels;
                ch[i].maxY += extPixels;
                ch[i].minX -= extPixels;
                ch[i].minY -= extPixels;

                float dxn = ch[i].maxX - ch[i].minX;
                float dyn = ch[i].maxY - ch[i].minY;

                float kX = dxn / dx;
                float kY = dyn / dy;

                Vector2 duv = ch[i].uvTopRight - ch[i].uvBottomLeft;
                Vector2 duvn = new Vector2(duv.x * kX, duv.y * kY);

                float duvX = (duvn - duv).x / 4.0f;
                float duvY = (duvn - duv).y / 4.0f;

                ch[i].uvBottomLeft += new Vector2(-duvX, -duvY);
                ch[i].uvBottomRight += new Vector2(duvX, -duvY);
                ch[i].uvTopLeft += new Vector2(-duvX, duvY);
                ch[i].uvTopRight += new Vector2(duvX, duvY);
            }
            font.characterInfo = ch;
        }

        //https://stackoverflow.com/questions/5943850/fastest-way-to-fill-an-array-with-a-single-value?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        public static void Fill<T>(this T[] destinationArray, params T[] value)
        {
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }

            if (value.Length >= destinationArray.Length)
            {
                throw new ArgumentException("Length of value array must be less than length of destination");
            }

            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }

        public static void FillNC<T>(this T[] destinationArray, params T[] value)
        {
            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }

        public static int GetMaxSize(int width, int height)
        {
            int s = Mathf.Max(width, height);

            int maxSize = 8192;
            int[] maxSizes = new int[] { 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 0 };

            for (int i = 0; i < maxSizes.Length - 1; i++)
            {
                if (s <= maxSizes[i] && s > maxSizes[i + 1])
                {
                    //  Debug.Log("maxSizes[i]: " + maxSizes[i]);
                    return maxSizes[i];
                }
            }
            return maxSize;
        }

        public static void DebugDrawCircle(Transform t, Vector2 center, float radius, Color color)
        {
            int count = 20;
            float da = 2 * Mathf.PI / count;
            Vector2[] pos = new Vector2[count + 1];
            for (int i = 0; i < count; i++)
            {
                float ida = i * da;
                pos[i] = t.TransformPoint( center + new Vector2(Mathf.Cos(ida) * radius, Mathf.Sin(ida) * radius));
            }
            pos[count] = pos[0];
            for (int i = 0; i < count; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }

        public static void DebugDrawCircle(Transform t, Vector2 center, float radius, int prec, Color color)
        {
            int count = prec;
            float da = 2 * Mathf.PI / count;
            Vector2[] pos = new Vector2[count + 1];
            for (int i = 0; i < count; i++)
            {
                float ida = i * da;
                pos[i] = t.TransformPoint(center + new Vector2(Mathf.Cos(ida) * radius, Mathf.Sin(ida) * radius));
            }
            pos[count] = pos[0];
            for (int i = 0; i < count; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }
    }

    [Serializable]
    public class Packer
    {
        SymbImage[] items;
        int oldOrder = 0;
        int maxSymbWidth = 0;
        int sumWidth = 0;
        int sumHeight = 0;
        int[] p2sizes = { 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        int minH = 500;
        int minW = 500;

        public void AddItem(SymbImage img, Data dat)
        {
            img.d = dat;
            img.oldOrder = oldOrder;
            maxSymbWidth = (img.size0 > maxSymbWidth) ? img.size0 : maxSymbWidth;
            sumWidth += img.size0;
            sumHeight += img.size1;  // Debug.Log(oldOrder+ " : " + items.Length);
            items[oldOrder] = img;
            oldOrder++;
            if (minW > img.size0) minW = img.size0;
            if (minH > img.size1) minH = img.size1;
        }

        public Packer(int count)
        {
            items = new SymbImage[count];
        }

        public SymbImage[] ResolveExtendBool(ref int image_width, ref int image_height, int ext)
        {
            int ext2 = ext + ext;
            image_width = ((maxSymbWidth + ext2) < image_width) ? image_width : maxSymbWidth + ext2;
            int count = items.Length;
            int averW = sumWidth / count + ext2;
            int averH = sumHeight / count + ext2;

            int averRowCount = (int)Mathf.Sqrt(count);
            int averRowWidth = averRowCount * averW;
            image_width = (averRowWidth > image_width) ? averRowWidth : image_width;
            image_width = GetP2Size(image_width);


            SymbImageCanvas masq = new SymbImageCanvas(image_width, image_height);
            int maxY = 0;
            Array.Sort(items);

            for (int i = 0; i < items.Length; i++)
            {
                int idx = items[i].size0 + ext2;
                int idy = items[i].size1 + ext2;
                int tty = 0;

                bool found = false;

                for (int ty = 0; ((ty < 8192) && !found); ty += 4) // 8192 - max resolved size; ty+=4 speed x4
                {
                    tty = ty + idy;
                    if (tty > masq.size1)
                    {
                        masq.Resize(image_width, ty + idy);
                    }
                    for (int tx = 0; ((tx <= (image_width - idx)) && !found); tx++)
                    {
                        bool valid = !masq.dat[ty][tx] && !masq.dat[tty - 1][tx] && !masq.dat[ty][tx + idx - 1] && !masq.dat[tty - 1][tx + idx - 1];

                        if (valid) // check full quad space for validating
                        {
                            int py;
                            for (int ity = 0; ity < idy; ity += 2)
                            {
                                py = ty + ity;
                                for (int itx = 0; itx < idx; itx += 2)
                                {
                                    if (masq.dat[py][tx + itx])
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                                if (!valid) break;
                            }
                        }
                        /*                    */
                        if (valid)
                        {
                            masq.Fill(tx, ty, tx + idx, tty, true);

                            items[i].d.sx = tx + ext;
                            items[i].d.sy = ty + ext;

                            items[i].d.ex = tx + idx - ext;
                            items[i].d.ey = tty - ext;

                            found = true;
                            maxY = tty;
                        }
                    }
                }
            }

            image_height = (maxY % 2 == 0) ? maxY + 2 : maxY + 3;
            image_height = GetP2Size(image_height);
            image_width = masq.size0;
            return items;
        }

        public SymbImage[] ResolveExtendBool(ref int image_width, ref int image_height)
        {
            image_width = ((maxSymbWidth) < image_width) ? image_width : maxSymbWidth;
            int count = items.Length;
            int averW = sumWidth / count;
            int averH = sumHeight / count;

            int averRowCount = (int)Mathf.Sqrt(count);
            int averRowWidth = averRowCount * averW;
            image_width = (averRowWidth > image_width) ? averRowWidth : image_width;
            image_width = GetP2Size(image_width);

            SymbImageCanvas masq = new SymbImageCanvas(image_width, image_height);
            int maxY = 0;
            Array.Sort(items);
            SymbImage sImage;
            int idx, idy, tty, ttx, tty1, ttx1, imwIDX, itemsLength;
            bool found, valid;
            int minHeight = (minH < 2) ? 2 : minH;
            int minWidth = (minW < 2) ? 2 : minW;
            itemsLength = items.Length;
            bool[][] canvas = masq.dat;

            for (int i = 0; i < itemsLength; i++)
            {
                sImage = items[i];
                idx = sImage.size0;
                idy = sImage.size1;
                tty = 0;
                imwIDX = image_width - idx + 1;
                found = false;

                for (int ty = 0; ((ty < 8192) && !found); ty += 4) // 8192 - max resolved size; ty+=4 speed x4
                {
                    tty = ty + idy;
                    tty1 = tty - 1;
                    if (tty > masq.size1)
                    {
                        masq.Resize(image_width, tty);
                    }
                    for (int tx = 0; ((tx < imwIDX) && !found);)
                    {
                        ttx = tx + idx;
                        ttx1 = ttx - 1;
                        valid = !canvas[ty][tx] && !canvas[tty1][tx] && !canvas[ty][ttx1] && !canvas[tty1][ttx1];

                        if (valid) // check full quad space for validating
                        {
                            for (int ity = ty; ity < tty; ity += minHeight)
                            {
                                for (int itx = tx; itx < ttx; itx += minWidth)
                                {
                                    if (canvas[ity][itx])
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                                if (!valid) break;
                            }
                        }

                        if (valid)
                        {
                            masq.Fill(tx, ty, ttx, tty, true);

                            sImage.d.sx = tx;
                            sImage.d.sy = ty;

                            sImage.d.ex = ttx;
                            sImage.d.ey = tty;

                            found = true;
                            maxY = tty;
                            tx = ttx;
                        }
                        else
                        {
                            tx += 2; // 2x faster but low packing
                        }
                    }
                }
            }

            image_height = (maxY % 2 == 0) ? maxY + 2 : maxY + 3;
            image_height = GetP2Size(image_height);
            image_width = masq.size0;
            return items;
        }

        /// <summary>
        /// return power 2 size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int GetP2Size(int size)
        {
            int res = p2sizes[0];
            for (int i = 0; i < p2sizes.Length; i++)
            {
                if (p2sizes[i] >= size) return p2sizes[i];
                else res = p2sizes[i];
            }
            return res;
        }
    }

    [Serializable]
    public struct Data
    {
        /// <summary>
        ///  item ID
        /// </summary>
        // public int id;

        /// <summary>
        /// dimensions of its spot in the world - start x
        /// </summary>
        public int sx;

        /// <summary>
        /// dimensions of its spot in the world - start y
        /// </summary>
        public int sy;

        /// <summary>
        /// dimensions of its spot in the world - end x
        /// </summary>
        public int ex;

        /// <summary>
        /// dimensions of its spot in the world - end y
        /// </summary>
        public int ey;

        /// <summary>
        /// offset from the origin X
        /// </summary>
        // public float ox;

        /// <summary>
        /// offset from the origin Y
        /// </summary>
        // public float oy;

        /// <summary>
        /// distance to move the origin forward
        /// </summary>
        // public float wx1;
    }

    [Serializable]
    public class SymbImage : IEquatable<SymbImage>, IComparable<SymbImage>
    {
        public int size0;
        public int size1;
        public Data d;
        private int fullSize;

        public int oldOrder;

        public SymbImage()
        {
            size0 = 1;
            size1 = 1;
            fullSize = 1;
        }

        public SymbImage(int x, int y)
        {
            size0 = x;
            size1 = y;
            fullSize = size0 * size1;
        }

        public bool Equals(SymbImage other)
        {
            return fullSize == other.fullSize;
        }

        public int CompareTo(SymbImage other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return this.fullSize.CompareTo(other.fullSize);
        }

        public static bool operator <(SymbImage lhs, SymbImage rhs)
        {
            return lhs.fullSize > rhs.fullSize;
        }

        public static bool operator >(SymbImage lhs, SymbImage rhs)
        {
            return lhs.fullSize < rhs.fullSize;
        }

    }


    [Serializable]
    public class SymbImageCanvas
    {
        public bool[][] dat;
        public int size0;
        public int size1;

        public SymbImageCanvas()
        {
            dat = new bool[1][];
            dat[0] = new bool[1];
            size0 = 1;
            size1 = 1;
        }

        public SymbImageCanvas(int x, int y)
        {
            dat = new bool[y][];
            for (int i = 0; i < y; i++)
            {
                dat[i] = new bool[x];
            }
            size0 = x;
            size1 = y;
        }

        public void Resize(int x, int y)
        {
            bool[][] dat_t = new bool[y][];
            for (int i = 0; i < y; i++)
            {
                dat_t[i] = new bool[x];
            }
            int minS0 = Mathf.Min(size0, x);
            int minS1 = Mathf.Min(size1, y);
            for (int ty = 0; ty < minS1; ty++)
            {
                for (int tx = 0; tx < minS0; tx++)
                {
                    dat_t[ty][tx] = dat[ty][tx];
                }
            }

            dat = dat_t;
            size0 = x;
            size1 = y;
            //  Debug.Log("size 0 1 : " + size0 + " :" + size1);
        }

        public void CopyFrom(SymbImageCanvas img, int ox, int oy)
        {
            for (int y = 0; y < img.size1; y++)
            {
                for (int x = 0; x < img.size0; x++)
                {
                    dat[y + oy][x + ox] = img.dat[y][x];
                }
            }
        }

        int length;
        int sy1;
        public void Fill(int sx, int sy, int ex, int ey, bool fil)
        {
            for (int x = sx; x < ex; x++)
                dat[sy][x] = true;
            length = ex - sx;
            sy1 = sy + 1;
            for (int y = sy1; y < ey; y++)
            {
                Array.Copy(dat[sy], sx, dat[y], sx, length);
            }
        }

        public void FillPar(int sx, int sy, int ex, int ey, bool fil)
        {
            for (int y = sy; y < ey; y++)
            {
                // Parallel.For(sx, ex, (x) => { dat[y][x] = fil;});
            }
        }

    }

    [Serializable]
    public class CBuffers
    {
        public Vector4[] gradientsArrayStroke; // container fo face gradient 0
        public Vector4[] gradientsArrayIGlow; // container fo face gradient 1
        public Vector4[] gradientsArrayFace; // container fo face gradient 2
        public Vector4[] gradientsArrayOGlow; // container fo face gradient 3

        public ComputeBuffer CBEDTSigned;

        public ComputeBuffer CBEDTAASigned;

        public ComputeBuffer uvMap;

        public ComputeBuffer gradients;

        public void Release()
        {
            ReleaseBuffer(CBEDTSigned);
            ReleaseBuffer(CBEDTAASigned);
            ReleaseBuffer(uvMap);
            ReleaseBuffer(gradients);
            gradientsArrayStroke = null;
            gradientsArrayIGlow = null;
            gradientsArrayFace = null;
            gradientsArrayOGlow = null;
        }

        private void ReleaseBuffer(ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }

        /// <summary>
        /// uvs - uvBottomLeft uvBottomRight uvTopRight
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="uvs"></param>
        internal void CreateUvMap(int width, int height, int2[] uvs, int extPixels)
        {
            ReleaseBuffer(uvMap);
            int size = width * height;
            uvMap = new ComputeBuffer(size, 8);
            Vector2[] uvMapArr = new Vector2[size];
            int pos = 0;
            int2 uvBottomLeft, uvTopRight, uvBottomLeftInt, uvTopRightInt, dXY;
            int s, i, j;
            float yi, xi, xr, yr;
            for (s = 0; s < uvs.Length; s += 3)
            {
                uvBottomLeft = uvs[s];
                uvTopRight = uvs[s + 2];

                uvBottomLeftInt.X = uvBottomLeft.X - extPixels; uvBottomLeftInt.Y = uvBottomLeft.Y - extPixels;
                uvTopRightInt.X = uvTopRight.X + extPixels; uvTopRightInt.Y = uvTopRight.Y + extPixels;
                dXY = uvTopRightInt - uvBottomLeftInt;
                xr = 1.0f / (float)dXY.X;
                yr = 1.0f / (float)dXY.Y;

                yi = 0;
                xi = 0;
                for (i = uvBottomLeftInt.Y; i < uvTopRightInt.Y; i++)
                {
                    for (j = uvBottomLeftInt.X; j < uvTopRightInt.X; j++)
                    {
                        pos = i * width + j;
                        if (pos > 0 && pos < size)
                        {
                            uvMapArr[pos].x = xi * xr;
                            uvMapArr[pos].y = yi * yr;
                        }
                        xi++;
                    }
                    yi++;
                    xi = 0;
                }
            }
            uvMap.SetData(uvMapArr);//  Mkey.PrintData.BufTostring(uvMapArr, width, height, 500, -1);
        }

        /// <summary>
        /// uvs - uvBottomLeft uvBottomRight uvTopRight
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="uvs"></param>
        internal void CreateUvMap(int width, int height, int2[] uvs)
        {
            ReleaseBuffer(uvMap);
            int size = width * height;
            uvMap = new ComputeBuffer(size, 8);
            Vector2[] uvMapArr = new Vector2[size];
            int pos = 0;
            int2 uvBottomLeftInt, uvTopRightInt, dXY;
            float yi, xi, xr, yr;
            int s, i, j;
            for (s = 0; s < uvs.Length; s += 3)
            {
                uvBottomLeftInt = uvs[s];
                uvTopRightInt = uvs[s + 2];
                dXY = uvTopRightInt - uvBottomLeftInt;
                xr = 1.0f / (float)dXY.X;
                yr = 1.0f / (float)dXY.Y;

                yi = 0;
                xi = 0;

                for (i = uvBottomLeftInt.Y; i < uvTopRightInt.Y; i++)
                {
                    for (j = uvBottomLeftInt.X; j < uvTopRightInt.X; j++)
                    {
                        pos = i * width + j;
                        if (pos > 0 && pos < size)
                        {
                            uvMapArr[pos].x = xi * xr;
                            uvMapArr[pos].y = yi * yr;
                        }
                        xi++;
                    }
                    yi++;
                    xi = 0;
                }
            }
            uvMap.SetData(uvMapArr); //  Mkey.PrintData.BufTostring(uvMapArr, width, height, 500, -1);
        }

        Vector4[] gradientsArray;
        internal void CreateGradientBuffer()
        {
            ReleaseBuffer(gradients);
            if (gradientsArray == null || gradientsArray.Length != 1024) gradientsArray = new Vector4[1024];
            int length = Options.gradientWidth;
            if (gradientsArrayStroke != null && gradientsArrayStroke.Length == length) Array.Copy(gradientsArrayStroke, gradientsArray, length);
            if (gradientsArrayIGlow != null && gradientsArrayIGlow.Length == length) Array.Copy(gradientsArrayIGlow, 0, gradientsArray, length, length);
            if (gradientsArrayFace != null && gradientsArrayFace.Length == length) Array.Copy(gradientsArrayFace, 0, gradientsArray, length + length, length);
            if (gradientsArrayOGlow != null && gradientsArrayOGlow.Length == length) Array.Copy(gradientsArrayOGlow, 0, gradientsArray, length + length + length, length);
            gradients = new ComputeBuffer(1024, 16);
            gradients.SetData(gradientsArray);
        }

        public void CreateGradientArray(Gradient g, int pos)
        {
            if (g == null) g = new Gradient();
            float k = 1.0f / ((float)Options.gradientWidth - 1.0f);

            Vector4[] tempArray = null;
            switch (pos)
            {
                case 0:
                    if (gradientsArrayStroke == null)
                    {
                        gradientsArrayStroke = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayStroke;
                    break;
                case 1:
                    if (gradientsArrayIGlow == null)
                    {
                        gradientsArrayIGlow = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayIGlow;
                    break;
                case 2:
                    if (gradientsArrayFace == null)
                    {
                        gradientsArrayFace = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayFace;
                    break;
                case 3:
                    if (gradientsArrayOGlow == null)
                    {
                        gradientsArrayOGlow = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayOGlow;
                    break;
            }
            for (int i = 0; i < Options.gradientWidth; i++)
            {
                tempArray[i] = g.Evaluate((float)i * k);
            }
            if (SoftEffects.debuglog) Debug.Log("Created Gradient Array");
        }

        public void CreateGradientArray(Gradient g, int pos, float scale)
        {
            if (g == null) g = new Gradient();

            int d = (int)((float)Options.gradientWidth * scale);
            int i0 = (Options.gradientWidth - d) / 2;
            float k = 1.0f / ((float)d - 1.0f);
            // int offset = pos * Options.gradientWidth;
            Vector4[] tempArray = null;
            switch (pos)
            {
                case 0:
                    if (gradientsArrayStroke == null)
                    {
                        gradientsArrayStroke = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayStroke;
                    break;
                case 1:
                    if (gradientsArrayIGlow == null)
                    {
                        gradientsArrayIGlow = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayIGlow;
                    break;
                case 2:
                    if (gradientsArrayFace == null)
                    {
                        gradientsArrayFace = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayFace;
                    break;
                case 3:
                    if (gradientsArrayOGlow == null)
                    {
                        gradientsArrayOGlow = new Vector4[Options.gradientWidth];
                    }
                    tempArray = gradientsArrayOGlow;
                    break;
            }
            for (int i = 0; i < Options.gradientWidth; i++)
            {
                tempArray[i] = g.Evaluate((float)(i - i0) * k);
            }
            if (SoftEffects.debuglog) Debug.Log("Created Gradient Array");
        }
    }

    // correcting vertex on texture positions
    // Bottom Left corner - the some
    // Top Righ -  + {-1;-1}
    public struct VertsAsset
    {
        public Vector2 BL;
        public Vector2 BR;
        public Vector2 TR;

        public int2 V0;
        public int2 V1;
        public int2 V2;

        float minu;
        float minv;
        float maxu;
        float maxv;

        Vector2 blV;
        Vector2 brV;
        Vector2 trV;
        Vector2 tlV;

        int2 corr;
        public void CalcPositions(float w, float h)
        {
            V0.X = Mathf.RoundToInt(BL.x * w); V0.Y = Mathf.RoundToInt(BL.y * h);
            V1.X = Mathf.RoundToInt(BR.x * w); V1.Y = Mathf.RoundToInt(BR.y * h);
            V2.X = Mathf.RoundToInt(TR.x * w); V2.Y = Mathf.RoundToInt(TR.y * h);

            minu = minU();
            minv = minV();
            maxu = maxU();
            maxv = maxV();

            blV.x = minu; blV.y = minv;
            brV.x = maxu; brV.y = minv;
            tlV.x = minu; tlV.y = maxv;
            trV.x = maxu; trV.y = maxv;

            V0 += Correct(BL);
            V1 += Correct(BR);
            V2 += Correct(TR);
        }

        float minU()
        {
            return Mathf.Min(BL.x, BR.x, TR.x);
        }

        float minV()
        {
            return Mathf.Min(BL.y, BR.y, TR.y);
        }

        float maxU()
        {
            return Mathf.Max(BL.x, BR.x, TR.x);
        }

        float maxV()
        {
            return Mathf.Max(BL.y, BR.y, TR.y);
        }

        int2 Correct(Vector2 vert)
        {
            corr.X = 0; corr.Y = 0;
            if (vert == brV)
            {
                corr.X = -1; corr.Y = 0;
            }
            else if (vert == tlV)
            {
                corr.X = 0; corr.Y = -1;
            }
            else if (vert == trV)
            {
                corr.X = -1; corr.Y = -1;
            }
            return corr;
        }
    }
#endif
}