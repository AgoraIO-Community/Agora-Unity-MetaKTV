using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEditor.SceneManagement;
#endif
namespace Mkey
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Effects/SoftEffectsFont", 14)]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(Text))]
    public class SoftEffectsFont : SoftEffects
    {
        [SerializeField]
        private Font SourceFont;
        [SerializeField]
        private Font SoftFont;

        /// <summary>
        /// SoftEffects/objectName+SEFontSubFolderSufix + order/objectNAme + key
        /// </summary>
        private string SEFontSubFolderSufix = "_SEFont_0";
        private string key = "_se_rep";

#if UNITY_EDITOR
        public override FaceTarget Facetarget
        {
            get { return FaceTarget.Font; }
        }

        public override bool HaveSourceObject
        {
            get { return (SourceFont && SourceFont != null); }
        }

        public override string SourceObjectName
        {
            get
            {
                if (SourceFont && SourceFont != null) return SourceFont.name;
                return "No saved source";
            }
        }

        private void OnValidate()
        {

        }

        private void OnEnable()
        {

#if UNITY_STANDALONE_WIN
            if (SoftEffects.debuglog) Debug.Log("<<<<<<<<<<<<<<  SoftEffects enable - Unity Stand Alone Windows - >>>>>>>>>>>>");
#elif UNITY_IOS
        if(SoftEffects.debuglog)Debug.Log("<<<<<<<<<<<<<<  SoftEffects enable - Unity IOS - >>>>>>>>>>>>");
#elif UNITY_STANDALONE_OSX
        if(SoftEffects.debuglog)Debug.Log("<<<<<<<<<<<<<<  SoftEffects enable - Unity Stand Alone OSX - >>>>>>>>>>>>");
#elif UNITY_STANDALONE_LINUX
        if(SoftEffects.debuglog)Debug.Log("<<<<<<<<<<<<<<  SoftEffects enable - Unity Stand Alone Linux - >>>>>>>>>>>>");
#elif UNITY_ANDROID
        if(SoftEffects.debuglog)Debug.Log("<<<<<<<<<<<<<<  SoftEffects enable - Unity ANDROID - >>>>>>>>>>>>");
#endif
            cb = new CBuffers();
            if (faceOptions != null) faceOptions.IsCombinedDirty = true;
            AdjustTextureOnLine();
        }

        private void OnDisable()
        {
            if (SoftEffects.debuglog) Debug.Log("<<<<<<<<<<<<<<  SoftEffects disable  >>>>>>>>>>>>");
            ReleaseData();
        }

        void Update()
        {
            UpdateMaterial();
        }

        public override bool IsTargetObjectMissed(ref string missedError)
        {
            missedError = "";
            Text t = GetComponent<Text>();
            if (!t)
            {
                missedError = "Object must have <Text> component.";
                return true;
            }

            if (t && !t.font)
            {
                missedError = "Component <Text> must have font.";
                return true;
            }
            return false;
        }

        public override bool IsSoftObjectMissed(ref string missedError)
        {
            if (!SoftFont)
            {
                missedError = "Editable font missed. Rebuild or Create new.";
                return true;
            }
            return false;
        }

        public bool IsSoftFontCreated
        {
            get
            {
                //  return (SoftFont && SoftFont.material && SoftFont.material.mainTexture);
                return (SoftFont && SoftFont.material);
            }
        }

        private bool IsSourceFont(Font f)
        {
            if (!f) return false;
            bool isSource = true;
            if (f.name.Contains(key))
            {
                isSource = false;
            }
            return isSource;
        }

        public override void ApplyWorkMaterial(Material mat, bool disableComponent)
        {
            GetComponent<Text>().material = null;
            GetComponent<Text>().material = mat;

            EditorUtility.SetDirty(GetComponent<Text>());

            if (SoftFont)
            {
                SoftFont.material = mat;
                EditorUtility.SetDirty(SoftFont);
            }

            if (faceOptions.mainTexture)
            {
                if (File.Exists(AssetDatabase.GetAssetPath(faceOptions.mainTexture)))
                {
                    FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(faceOptions.mainTexture));
                }
            }

            if (SoftMaterial)
            {
                if (File.Exists(AssetDatabase.GetAssetPath(SoftMaterial)))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(SoftMaterial));
                    if (SoftEffects.debuglog) Debug.Log("Deleta  material");
                }
            }

            enabled = !disableComponent;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Check source Text component, create SoftEffects Font and set new Font to Text 
        /// </summary>
        public override void ApplySoftEffect(bool createNewFolder)
        {
            ReleaseData();
            Text text = GetComponent<Text>();
            Font f = (text) ? text.font : null;

#if UNITY_EDITOR

            if (!(EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (!f && !SourceFont) return;
                if (IsSourceFont(f))
                {
                    SourceFont = f;
                    SourceMaterial = text.material;
                }
                else
                {
                    if (!SourceFont) return;
                }

                CreateSEFolder(SourceFont.name, createNewFolder);

                // load compute shader
                FindComputeShader();

                if (!eShader) return;

                //create softfont and set it to text
                CreateSoftFont(SourceFont, createNewFolder);
            }

#endif
            if (SoftFont)
            {
                text.font = SoftFont;
                text.material = SoftFont.material;
                text.color = Color.white;
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create Soft Font, Soft Font Material, Soft Font Texture from SoftEffect Settings,
        /// </summary>
        /// <param name="font"></param>
        private void CreateSoftFont(Font font, bool createNewFolder) // http://answers.unity3d.com/questions/485695/truetypefontimportergenerateeditablefont-does-not.html
        {
            //  Mkey.Utils.Measure("<<<<<<<<<<<<Summary CreateSoftFontTexture>>>>>>>>>>>>>>>: ", () => {
            gpuWorker = new GPUWorker(eShader);
            string dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(font));

            //1) load source font asset
            string path = AssetDatabase.GetAssetPath(font);
            Font f = font;
            if (SoftEffects.debuglog) Debug.Log("Path to Source font: " + path);

            font = (Font)AssetDatabase.LoadMainAssetAtPath(path);
            if (f && !font)
            {
                Debug.LogError("Can't use embedded font : " + f.name);
                return;
            }

            //2) Remove old Editable font
            if (SoftFont && !createNewFolder)
            {
                if (SoftEffects.debuglog) Debug.Log("EditableFont folder: " + Path.GetDirectoryName(AssetDatabase.GetAssetPath(SoftFont)));
                if (SoftEffects.debuglog) Debug.Log("Remove old EditableFont: " + SoftFont.name);
                if (SoftFont.material)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(SoftFont.material.mainTexture));
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(SoftFont.material));
                }
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(SoftFont));
                AssetDatabase.Refresh();
            }

            //3) reimport source font as editable
            TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath(path) as TrueTypeFontImporter;

            //source settings
            int sourceSize = fontImporter.fontSize;
            int sourcePadding = fontImporter.characterPadding;
            int sourceSpacing = fontImporter.characterSpacing;

            FontTextureCase sourceCase = fontImporter.fontTextureCase;
            string chars = fontImporter.customCharacters;

            fontImporter.fontSize = GetComponent<Text>().fontSize;
            fontImporter.characterPadding = Mathf.Clamp(faceOptions.extPixels, 1, 100);
            fontImporter.characterSpacing = 0;

            // Mkey.Utils.Measure("Summary PreCreateFontTexture: ", () =>  {
            //     Mkey.Utils.Measure("Reimport font: ", () => {
            if (SoftFontTextureCase == FontTextureCase.CustomSet)
            {
                if (customCharacters.Length == 0 || customCharacters == " ")
                {
                    Debug.LogError("Custom Characters string is empty. Set default string.");
                    customCharacters = GetComponent<Text>().text;
                    if (customCharacters.Length == 0 || customCharacters == " ")
                    {
                        customCharacters = "New txt";
                    }
                }
                fontImporter.customCharacters = customCharacters;
            }
            else if (SoftFontTextureCase == FontTextureCase.Dynamic)
            {
                SoftFontTextureCase = FontTextureCase.ASCII;
            }
            fontImporter.fontTextureCase = SoftFontTextureCase;
            fontImporter.SaveAndReimport();
            // });

            //  Mkey.Utils.Measure("GenerateEditableFont: ", () =>  {
            SoftFont = fontImporter.GenerateEditableFont(path);
            // });
            int maxSize = Mathf.Max(font.material.mainTexture.width, font.material.mainTexture.height);

            // Mkey.Utils.Measure("RenameAsset: ", () =>    {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(SoftFont), font.name + key);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(SoftFont.material), font.name + key + "_edit");
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(SoftFont.material.mainTexture), font.name + key);
            AssetDatabase.Refresh();
            //  });

            Shader softShader = Shader.Find("SoftEffects/SoftEditShader");
            SoftFont.material.shader = softShader;
            SoftMaterial = SoftFont.material;

            if (SoftEffects.debuglog) Debug.Log("Editable texture size: " + SoftFont.material.mainTexture.width + " x " + SoftFont.material.mainTexture.height);
            // Mkey.Utils.Measure("Reimport texture: ", () =>  {
            //5) Reimport EditableFont texture as readable
            SoftFont.material.mainTexture.ReimportTexture(true, maxSize);
            if (SoftEffects.debuglog) Debug.Log("Editable texture size after reimport: " + SoftFont.material.mainTexture.width + " x " + SoftFont.material.mainTexture.height);
            // });

            // });

            //5) Generate new Texture for editable font
            // Mkey.Utils.Measure("faceOptions.RenderFontTexture: ", () =>  {
            faceOptions.RenderFontTexture(gpuWorker, SoftFont, cb);
            //});

            // Mkey.Utils.Measure("AfterCreateFontTexture: ", () =>  {
            faceOptions.CreateTextureFromRender_ARGB32(true, dirPath + "/" + font.name + key + "_edit" + ".png");

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(SoftFont.material.mainTexture));//  Remove old texture 

            Texture2D t = (Texture2D)AssetDatabase.LoadMainAssetAtPath(dirPath + "/" + font.name + key + "_edit" + ".png"); // load new texture asset
            t.ReimportTexture(true);

            //6 extend verts and uvs
            // SoftFont.ExtendVertsAndUvs(faceOptions.extPixels);
            faceOptions.mainTexture = t;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            // 9) remove editable font to unique folder
            string targetFolder = AssetDatabase.GUIDToAssetPath(FolderGUID);
            string fontPath = AssetDatabase.GetAssetPath(SoftFont);
            string materialPath = AssetDatabase.GetAssetPath(SoftFont.material);
            string texturePath = AssetDatabase.GetAssetPath(t);

            if (SoftEffects.debuglog) Debug.Log("Move file: " + fontPath + " to : " + targetFolder + "/" + Path.GetFileName(fontPath));
            AssetDatabase.MoveAsset(fontPath, targetFolder + "/" + Path.GetFileName(fontPath));// FileUtil.MoveFileOrDirectory(fontPath, targetFolder + "/"+  Path.GetFileName(fontPath));
            AssetDatabase.MoveAsset(materialPath, targetFolder + "/" + Path.GetFileName(materialPath));
            AssetDatabase.MoveAsset(texturePath, targetFolder + "/" + Path.GetFileName(texturePath));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            faceOptions.IsCombinedDirty = true;
            RenderNewTextures(gpuWorker, true);
            /**/
            EditorUtility.SetDirty(SoftFont); // problem font data fix v1.10

            EditorGUIUtility.PingObject(SoftFont);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            //revert source settings
            fontImporter = AssetImporter.GetAtPath(path) as TrueTypeFontImporter;
            fontImporter.fontSize = sourceSize;
            fontImporter.fontTextureCase = sourceCase;
            // fontImporter.characterPadding = sourcePadding;
            // fontImporter.characterSpacing = sourceSpacing;

            fontImporter.characterPadding = 1;
            fontImporter.characterSpacing = 0;

            if (sourceCase == FontTextureCase.CustomSet) fontImporter.customCharacters = chars;
            fontImporter.SaveAndReimport();
            // });
            // });
        }

        /// <summary>
        /// Create Soft Font, Soft Font Material, Soft Font Texture from SoftEffect Settings,
        /// </summary>
        /// <param name="font"></param>
        public override void AdjustTextureOnLine()
        {
            if (!eShader || eShader == null) FindComputeShader();
            if (!eShader || eShader == null) return;

            if (!SoftFont || !SoftFont.material)
            {
                if (SoftEffects.debuglog) Debug.LogError("SoftFont is missing. Create SoftFont bevore.");
                return;
            }

            if (!faceOptions.mainTexture || faceOptions.mainTexture == null)
            {
                if (SoftEffects.debuglog) Debug.LogError("Edit texture is missing. Create or rebuild.");
                return;
            }

            if (cb == null) cb = new CBuffers();
            if (gpuWorker == null) gpuWorker = new GPUWorker(eShader);

            RenderNewTextures(gpuWorker, false);

            //2 Set new gpu rendered texture to font
            GetComponent<Text>().material = null;
            GetComponent<Text>().material = SoftFont.material;
        }

        /// <summary>
        /// Create unique font folder for SoftEffects Instance or clean existing.
        /// </summary>
        private void CreateSEFolder(string fontName, bool createNew)
        {
            if (!createNew)
            {
                // check for folder existing for SoftEffects Font
                if (AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(FolderGUID)))
                {
                    // delete all assets from folder if SE.isDirty
                    if (SoftEffects.debuglog) Debug.Log("Folder for SeFont also exist: " + AssetDatabase.GUIDToAssetPath(FolderGUID));
                    if (SoftEffects.debuglog) Debug.Log("Delete files from existing folder: " + AssetDatabase.GUIDToAssetPath(FolderGUID));

                    ClassExtensions.DeleteFilesFromDir(AssetDatabase.GUIDToAssetPath(FolderGUID), key);

                    if (SoftFont && SoftFont.material && SoftFont.material.mainTexture)
                    {
                        if (File.Exists(AssetDatabase.GetAssetPath(SoftFont.material.mainTexture)))
                        {
                            if (SoftEffects.debuglog) Debug.Log("Texture File: " + AssetDatabase.GetAssetPath(SoftFont.material.mainTexture) + " - delete");
                            FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(SoftFont.material.mainTexture));
                        }
                    }

                    if (SoftFont && SoftFont.material)
                    {
                        if (File.Exists(AssetDatabase.GetAssetPath(SoftFont.material)))
                        {
                            if (SoftEffects.debuglog) Debug.Log("Material File: " + AssetDatabase.GetAssetPath(SoftFont.material) + " - delete");
                            FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(SoftFont.material));
                        }
                    }

                    if (SoftFont)
                    {
                        if (File.Exists(AssetDatabase.GetAssetPath(SoftFont)))
                        {
                            if (SoftEffects.debuglog) Debug.Log("Font File: " + AssetDatabase.GetAssetPath(SoftFont) + " - delete");
                            FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(SoftFont));
                        }
                    }
                    return;
                }
            }

            string seFolder = GetSEEditFolder();
            if (seFolder != "")
            {
                FolderGUID = AssetDatabase.CreateFolder(seFolder, fontName + SEFontSubFolderSufix);
                if (SoftEffects.debuglog) Debug.Log("Create new folder : " + AssetDatabase.GUIDToAssetPath(FolderGUID));
            }

        }


#endif
    }
}