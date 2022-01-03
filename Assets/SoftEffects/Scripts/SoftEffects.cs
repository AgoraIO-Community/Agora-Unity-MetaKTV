using UnityEngine;
//using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEditor.SceneManagement;
#endif

namespace Mkey
{
    public enum FaceTarget { Sprite, Font };

    public class SoftEffects : MonoBehaviour
    {
        public static bool debuglog = false;

#if UNITY_EDITOR
        internal string AssetsFolder = "Assets";
        internal string AssetName = "SoftEffects";
        internal string EditFolderName = "EditFolder";
        internal string ComputeShaderName = "SoftEffectsCompute";

        public FontTextureCase SoftFontTextureCase = FontTextureCase.ASCII;
        public string customCharacters;

        [SerializeField]
        public ComputeShader eShader;

        [SerializeField]
        public Material SoftMaterial;

        [SerializeField]
        public FaceOptions faceOptions;

        [SerializeField]
        public StrokeOptions strokeOptions;

        [SerializeField]
        public OuterGlowOptions outerGlowOptions;

        [SerializeField]
        public CloseShadowOptions closeShadowOptions;

        [SerializeField]
        public CloseShadowOptions closeShadowOptions_1;

        [SerializeField]
        public ShadowOptions shadowOptions;

        [SerializeField]
        public BevelOptions bevelOptions;

        [SerializeField]
        public InnerGlowOptions innerGlowOptions;

        [SerializeField]
        public InnerShadowOptions innerShadowOptions;

        [SerializeField]
        public FaceGradientOptions faceGradientOptions;

        [SerializeField]
        internal Material SourceMaterial; // Save Text SourceFont Material Reference

        //  [SerializeField]
        internal CBuffers cb;
        internal GPUWorker gpuWorker;

        [SerializeField]
        public string FolderGUID;

        /// <summary>
        /// Return folder with editable objects - Assets/SoftEffects/EditFolder/editobjectfolder
        /// </summary>
        public string EditObjectFolder
        {
            get
            {
                string f = AssetDatabase.GUIDToAssetPath(FolderGUID);
                if (AssetDatabase.IsValidFolder(f))
                {
                    return f;
                }
                return " no valid folder";
            }
        }

        public bool HaveEditFolder
        {
            get { return AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(FolderGUID)); }
        }

        public virtual bool HaveSourceObject
        {
            get { return false; }
        }

        public virtual string SourceObjectName
        {
            get { return ""; }
        }

        /// <summary>
        /// return true if object Text or Text.font missed, or SpriteRenderer or SpriteRenderer.sprite
        /// </summary>
        /// <param name="missedError"></param>
        /// <returns></returns>
        public virtual bool IsTargetObjectMissed(ref string missedError)
        {

            return false;
        }

        public virtual bool IsSoftObjectMissed(ref string missedError)
        {
            return false;
        }

        public bool IsEditTextureMissed(ref string missedError)
        {

            if (!faceOptions.mainTexture || faceOptions.mainTexture == null)
            {
                missedError = "Edit texture is missed. Rebuild edit texures.";
                return true;
            }
            return false;
        }

        public bool IsEditMaterialMissed(ref string missedError)
        {
            if (!SoftMaterial)
            {
                missedError = "Edit material is missed. Rebuild edit material.";
                return true;
            }
            return false;
        }

        [SerializeField]
        public virtual FaceTarget Facetarget
        {
            get { return FaceTarget.Font; }
        }


        internal void RenderNewTextures(GPUWorker gpuWorker, bool rebuild)
        {
            //  Mkey.Utils.Measure("RenderNewTextures measure: ", ()=>  {
            //0a) Create close shadow 
            if (closeShadowOptions.use && (closeShadowOptions.IsDirty || rebuild || closeShadowOptions.NeedRebuild))
            {
                closeShadowOptions.RenderTexture_AR8(gpuWorker, faceOptions.mainTexture);
            }
            else
            {
                if (debuglog) Debug.Log("Close Shadow texture not need rendering");
                closeShadowOptions.IsDirty = false;
            }

            //1a) Create close shadow 
            if (closeShadowOptions_1.use && (closeShadowOptions_1.IsDirty || rebuild || closeShadowOptions_1.NeedRebuild))
            {
                closeShadowOptions_1.RenderTexture_AR8(gpuWorker, faceOptions.mainTexture);
            }
            else
            {
                if (debuglog) Debug.Log("Close Shadow(1) texture not need rendering");
                closeShadowOptions_1.IsDirty = false;
            }

            //2a) Create blured texture 
            if (shadowOptions.use && (shadowOptions.IsDirty || rebuild || shadowOptions.NeedRebuild))
            {
                shadowOptions.RenderTexture_AR8(gpuWorker, faceOptions.mainTexture);
            }
            else
            {
                if (debuglog) Debug.Log("Shadow texture not need rendering");
                shadowOptions.IsDirty = false;
            }

            //b) Create antialiazed stroke texture from buffer
            if (strokeOptions.use && (strokeOptions.IsDirty || rebuild || strokeOptions.NeedRebuild))
            {
                strokeOptions.RenderTextureFromAAFEDTBuffer_AR8(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Stroke texture not need rendering");
                strokeOptions.IsDirty = false;
            }

            //c) Create outer glow  dist map texture 
            if (outerGlowOptions.use && (outerGlowOptions.IsDirty || rebuild || outerGlowOptions.NeedRebuild))
            {
                outerGlowOptions.RenderTextureFromAAFEDT_AR8(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Outer Glow texture not need rendering");
                outerGlowOptions.IsDirty = false;
            }

            //d) Create bevel texture  
            if (bevelOptions.use && (bevelOptions.IsDirty || rebuild || bevelOptions.NeedRebuild))
            {
                bevelOptions.RenderTexture(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Bevel texture not need rendering");
                bevelOptions.IsDirty = false;
            }

            //e) Create inner glow  dist map texture 
            if (innerGlowOptions.use && (innerGlowOptions.IsDirty || rebuild || innerGlowOptions.NeedRebuild))
            {
                innerGlowOptions.RenderTextureFromAAFEDT_AR8(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Inner Glow texture not need rendering");
                innerGlowOptions.IsDirty = false;
            }

            //f) Create inner shadow 
            if (innerShadowOptions.use && (innerShadowOptions.IsDirty || rebuild || innerShadowOptions.NeedRebuild))
            {
                innerShadowOptions.RenderTextureFromAAFEDT_AR8(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Inner Shadow texture not need rendering");
                innerShadowOptions.IsDirty = false;
            }

            //g) Create face gradient 
            if (faceGradientOptions.use && (faceGradientOptions.IsDirty || rebuild || faceGradientOptions.NeedRebuild))
            {
                faceGradientOptions.RenderTextureFromAAFEDTBuffer_AR8(gpuWorker, faceOptions.mainTexture, cb, rebuild);
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Face gradient texture not need rendering");
                faceGradientOptions.IsDirty = false;
            }

            faceOptions.SetCloseShadowOptions(closeShadowOptions);
            faceOptions.SetCloseShadowOptions_1(closeShadowOptions_1);
            faceOptions.SetOuterGlowOptions(outerGlowOptions, cb);
            faceOptions.SetStrokeOptions(strokeOptions, cb);
            faceOptions.SetInnerGlowOptions(innerGlowOptions, cb);
            faceOptions.SetBevelOptions(bevelOptions);
            faceOptions.SetInnerShadowOptions(innerShadowOptions);
            faceOptions.SetFaceGradientOptions(faceGradientOptions, cb);
            faceOptions.SetFaceOptions();


            if (faceOptions.IsCombinedDirty)
            {
                //  Mkey.Utils.Measure("Render full face texture", () =>  {
                faceOptions.RenderCombineTexture(gpuWorker, cb);
                // });
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Face  not need rendering");
            }

            // });// end measure

            SetShadowMaterialPoperty();
        }

        public void SetShadowMaterialPoperty()
        {
            if (SoftMaterial)
            {
                shadowOptions.SetMaterialPoperty(SoftMaterial);
                EditorUtility.SetDirty(SoftMaterial);
            }
        }

        public virtual void ApplySoftEffect(bool createNewFolder)
        {

        }

        public virtual void AdjustTextureOnLine()
        {

        }

        public virtual void ApplyWorkMaterial(Material mat, bool disableComponet)
        {

        }


        /// <summary>
        /// Return edit folder path Assets/Softeffects/EditFolder (or create if folder not exist)
        /// </summary>
        /// <returns></returns>
        public string GetSEEditFolder()
        {
            string folder = "";
            string seFolder = AssetsFolder + "/" + AssetName;

            // check and create main folder for data : Assets/AssetName
            if (AssetDatabase.IsValidFolder(seFolder))
            {
                if (SoftEffects.debuglog) Debug.Log("Folder exist : " + seFolder);
            }
            else
            {
                AssetDatabase.CreateFolder(AssetsFolder, AssetName);
                if (SoftEffects.debuglog) Debug.Log("Create new folder : ");
            }

            // check and create subfolder 
            if (AssetDatabase.IsValidFolder(seFolder + "/" + EditFolderName))
            {
                if (SoftEffects.debuglog) Debug.Log("Folder exist : " + seFolder + "/" + EditFolderName);

            }
            else
            {
                AssetDatabase.CreateFolder(seFolder, EditFolderName);
                if (SoftEffects.debuglog) Debug.Log("Create new folder : ");
            }

            var guids = AssetDatabase.FindAssets(EditFolderName);
            if (guids != null && guids.Length > 0)
            {
                folder = System.IO.Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            else
            {
                if (SoftEffects.debuglog) Debug.Log("Asset folder with SoftEffect not found");
            }
            /*   */
            return (folder + "/" + EditFolderName);
        }

        internal void ReleaseData()
        {
            closeShadowOptions_1.ReleaseRenderTexture();
            closeShadowOptions.ReleaseRenderTexture();
            shadowOptions.ReleaseRenderTexture();
            outerGlowOptions.ReleaseRenderTexture();
            strokeOptions.ReleaseRenderTexture();
            bevelOptions.ReleaseRenderTexture();
            innerGlowOptions.ReleaseRenderTexture();
            innerShadowOptions.ReleaseRenderTexture();
            faceOptions.ReleaseRenderTexture();
            faceOptions.ReleaseCombinedRenderTexture();
            faceGradientOptions.ReleaseRenderTexture();

            if (cb != null) cb.Release();
        }

        internal void UpdateMaterial()
        {
            if (SoftMaterial)
            {
                faceOptions.SetMaterialPoperty(SoftMaterial);

                shadowOptions.SetMaterialPoperty(SoftMaterial);
            }
        }

        /// <summary>
        /// Find compute shader and set  eShader var
        /// </summary>
        internal void FindComputeShader()
        {
            if (eShader && eShader != null) return;

            var guids = AssetDatabase.FindAssets("SoftEffectsCompute");
            if (guids != null && guids.Length > 0)
            {
                eShader = (ComputeShader)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (eShader)
                {
                    if (SoftEffects.debuglog) Debug.Log("Compute shader found: " + AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                else Debug.LogError("Compute shader not found. Return.....");
            }

        }

        public Texture2D SaveFaceTexture()
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(FolderGUID);
            string fileName = SoftMaterial.name + "_work" + ".png";
            faceOptions.SaveTexture(folderPath + "/" + fileName);
            Texture2D face = (Texture2D)AssetDatabase.LoadAssetAtPath(folderPath + "/" + fileName, typeof(Texture2D));
            if (Facetarget == FaceTarget.Font) ClassExtensions.ReimportTexture(face, false);
            else if (Facetarget == FaceTarget.Sprite)
            {
                ClassExtensions.ReimportTextureAsSprite_1(folderPath + "/" + fileName, faceOptions.pixelsPerUnit, false);
                GetComponent<SpriteRenderer>().sprite = (Sprite)AssetDatabase.LoadAssetAtPath(folderPath + "/" + fileName, typeof(Sprite));
            }
            return face;
        }

        public void SaveFaceTexturePanel()
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(FolderGUID);
            string fileName = SoftMaterial.name + "_work" + ".png";
            faceOptions.SaveTexturePanel(folderPath, fileName);
        }

        public Texture2D SaveShadowTexture()
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(FolderGUID);
            string fileName = SoftMaterial.name + "_shadow" + ".png";
            shadowOptions.SaveTexture(folderPath + "/" + fileName);
            Texture2D face = (Texture2D)AssetDatabase.LoadAssetAtPath(folderPath + "/" + fileName, typeof(Texture2D));
            ClassExtensions.ReimportTexture(face, false);
            return face;
        }

        public void SaveShadowTexturePanel()
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(FolderGUID);
            string fileName = SoftMaterial.name + "_shadow" + ".png";
            shadowOptions.SaveTexturePanel(folderPath, fileName);
        }

        public void PingEditFolder()
        {
            string path = EditObjectFolder;
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (obj == null) return;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        public void CreateWorkMaterial()
        {
            Texture2D face = SaveFaceTexture();
            string folderPath = AssetDatabase.GUIDToAssetPath(FolderGUID);
            Material material;
            if (Facetarget == FaceTarget.Font)
            {
                material = (!shadowOptions.use) ? new Material(Shader.Find("SoftEffects/SoftShaderFont")) : new Material(Shader.Find("SoftEffects/SoftShaderFontWithShadows"));
                AssetDatabase.CreateAsset(material, folderPath + "/" + SoftMaterial.name + "_work" + ".mat");
                material.SetTexture("_MainTex", face);
            }
            else
            {
                material = (!shadowOptions.use) ? new Material(Shader.Find("Sprites/Default")) : new Material(Shader.Find("SoftEffects/SoftShaderSpriteWithShadows"));
                AssetDatabase.CreateAsset(material, folderPath + "/" + SoftMaterial.name + "_work" + ".mat");
            }


            if (shadowOptions.use)
            {
                Texture2D shadow = SaveShadowTexture();
                material.SetFloat("_OffsetX", shadowOptions.OffsetX);
                material.SetFloat("_OffsetY", shadowOptions.OffsetY);
                material.SetColor("_ShadowColor", shadowOptions.color);
                material.SetInt("_UseShadows", shadowOptions.use ? 1 : 0);
                material.SetTexture("_ShadowTex", shadow);
            }

            ApplyWorkMaterial(material, true);
            EditorUtility.SetDirty(material);
        }

        public void CreateSDT()
        {
            cb.Release();
            gpuWorker.GPURenderFEDTSignedBuffer(faceOptions.mainTexture, ref cb.CBEDTSigned);
        }

        public void CreateAASDT()
        {
            cb.Release();
            gpuWorker.GPURenderAAFEDTSignedBuffer(faceOptions.mainTexture, ref cb.CBEDTAASigned);
        }

        public void RenderBevel()
        {
            cb.Release();
            bevelOptions.RenderTexture(gpuWorker, faceOptions.mainTexture, cb, true);
        }

#endif


    }

}