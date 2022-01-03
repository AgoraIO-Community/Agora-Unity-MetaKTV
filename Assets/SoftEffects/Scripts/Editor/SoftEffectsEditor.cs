using UnityEngine;
using UnityEditor;

namespace Mkey
{
    [CustomEditor(typeof(SoftEffects))]
    public class SoftEffectsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }

        internal bool OnDrawSupportCompute()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Warning.Your GPU not support Compute Shaders. Effect not work.");
                EditorGUILayout.EndHorizontal();
                return false;
            }
            return true;
        }

        internal bool OndrawTargetMissed(SoftEffects myScript)
        {
            string missedError = "";
            if (myScript.IsTargetObjectMissed(ref missedError))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(missedError);
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        internal bool OndrawEdiObjectMissed(SoftEffects myScript)
        {
            string missedError = "";
            if (myScript.IsSoftObjectMissed(ref missedError))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(missedError);
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        internal bool OndrawEditMaterialMissed(SoftEffects myScript)
        {
            string missedError = "";
            if (myScript.IsEditMaterialMissed(ref missedError))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(missedError);
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        internal bool OndrawEditTextureMissed(SoftEffects myScript)
        {
            string missedError = "";
            if (myScript.IsEditTextureMissed(ref missedError))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(missedError);
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return false;
        }

        internal void OnDrawTitle()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(GetType().ToString(), EditorUIUtil.guiTitleStyle);

            EditorGUILayout.Space();
        }

        internal void OnDrawCreate(SoftEffects myScript)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("For using effects create please edit object.");
            if (GUILayout.Button("Create"))
            {
                myScript.ApplySoftEffect(true);
            }
            EditorGUILayout.EndHorizontal();
        }

        internal void OnDrawSave(SoftEffects myScript)
        {    /*
        EditorGUILayout.BeginHorizontal();
    
        if (GUILayout.Button("Save Face Dialog"))
        {
            myScript.SaveFaceTexturePanel();
        }
        if (GUILayout.Button("Save Face to edit folder"))
        {
            myScript.SaveFaceTexture();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Shadow Dialog"))
        {
            myScript.SaveShadowTexturePanel();
        }

        if (GUILayout.Button("Save Shadow to edit folder"))
        {
            myScript.SaveShadowTexture();
        }

        EditorGUILayout.EndHorizontal();
        */
            if (GUILayout.Button("Save working material to edit folder"))
            {
                myScript.CreateWorkMaterial();
            }
        }

        internal void OnDrawSoftFolder(SoftEffects myScript)
        {
            if (myScript.HaveEditFolder)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                string[] folder = myScript.EditObjectFolder.Split('/');
                EditorGUILayout.LabelField("Working Folder :  " + folder[folder.Length - 1]);
                if (GUILayout.Button("Ping"))
                {
                    myScript.PingEditFolder();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (myScript.HaveSourceObject)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Force Rebuild : " + myScript.SourceObjectName);
                    if (GUILayout.Button("Rebuild"))
                    {
                        myScript.ApplySoftEffect(false);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

            }
        }

        internal void OnDrawRebuild(SoftEffects myScript)
        {

        }

        internal void OnDrawSpacing(SoftEffects myScript)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            myScript.faceOptions.extPixels = (int)EditorGUILayout.Slider("Spacing, px", myScript.faceOptions.extPixels, 0, 100);
            myScript.faceOptions.extPixels = Mathf.Clamp(myScript.faceOptions.extPixels, 0, 100);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        internal void OnDrawTargetProp(SoftEffects myScript)
        {
            if (myScript.Facetarget != FaceTarget.Sprite)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                if (myScript.Facetarget == FaceTarget.Font)
                {
                    myScript.SoftFontTextureCase = (FontTextureCase)EditorGUILayout.EnumPopup("FontTextureCase", myScript.SoftFontTextureCase);
                    if (myScript.SoftFontTextureCase == FontTextureCase.CustomSet)
                    {
                        myScript.customCharacters = EditorGUILayout.TextField("Custom characters", myScript.customCharacters);
                    }
                    else if (myScript.SoftFontTextureCase == FontTextureCase.Dynamic)
                    {
                        EditorGUILayout.LabelField("Can't use Dynamic. Default set to ASCII.");
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        internal void OnDrawOptions(SoftEffects myScript)
        {
            // DrawDefaultInspector();
            #region bevel
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            bevelOptionsVisible = EditorGUILayout.Foldout(bevelOptionsVisible, "Bevel");
            myScript.bevelOptions.use = EditorGUILayout.Toggle("Use Bevel: ", myScript.bevelOptions.use);
            EditorGUILayout.EndHorizontal();
            if (bevelOptionsVisible) DrawBevelOptions(myScript);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            #endregion bevel

            #region stroke
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            strokeOptionsVisible = EditorGUILayout.Foldout(strokeOptionsVisible, "Stroke");
            myScript.strokeOptions.use = EditorGUILayout.Toggle("Use Stroke: ", myScript.strokeOptions.use);
            EditorGUILayout.EndHorizontal();
            if (strokeOptionsVisible) DrawStrokeOptions(myScript);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            #endregion stroke

            #region face color
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            faceColorOptionsVisible = EditorGUILayout.Foldout(faceColorOptionsVisible, "Color Overlay");
            myScript.faceOptions.useColor = EditorGUILayout.Toggle("Use Color Overlay: ", myScript.faceOptions.useColor);
            EditorGUILayout.EndHorizontal();
            if (faceColorOptionsVisible) DrawFaceColorOptions(myScript);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            #endregion face color

            #region close shadow
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            closeShadowOptionsVisible = EditorGUILayout.Foldout(closeShadowOptionsVisible, "Drop CloseShadow");
            myScript.closeShadowOptions.use = EditorGUILayout.Toggle("Use Drop CloseShadow: ", myScript.closeShadowOptions.use);
            EditorGUILayout.EndHorizontal();
            if (closeShadowOptionsVisible) DrawCloseShadowOptions(myScript);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            #endregion close shadow
        }

        private bool strokeOptionsVisible;
        private void DrawStrokeOptions(SoftEffects script)
        {
            SerializedProperty strokeSP = serializedObject.FindProperty("strokeOptions");

            script.strokeOptions.size = (int)EditorGUILayout.Slider("Size, px", script.strokeOptions.size, 0, 250);

            script.strokeOptions.pos = (StrokeOptions.Position)EditorGUILayout.EnumPopup("Position", script.strokeOptions.pos);
            script.strokeOptions.fillType = (StrokeOptions.FillType)EditorGUILayout.EnumPopup("FillType", script.strokeOptions.fillType);

            EditorGUI.indentLevel++;
            switch (script.strokeOptions.fillType)
            {
                case StrokeOptions.FillType.Color:
                    SerializedProperty coll = strokeSP.FindPropertyRelative("color");
                    EditorGUILayout.PropertyField(coll, new GUIContent("Color"));
                    break;
                case StrokeOptions.FillType.Gradient:
                    SerializedProperty gradient = strokeSP.FindPropertyRelative("gradient");
                    EditorGUILayout.PropertyField(gradient, new GUIContent("Gradient"));
                    script.strokeOptions.gradType = (StrokeOptions.GradientType)EditorGUILayout.EnumPopup("Gradient Type", script.strokeOptions.gradType);
                    script.strokeOptions.angle = (int)EditorGUILayout.Slider("Angle", script.strokeOptions.angle, 0, 360);
                    break;

                    // case StrokeOptions.FillType.Pattern:
                    //     SerializedProperty texture = strokeSP.FindPropertyRelative("pattern");
                    //     EditorGUILayout.PropertyField(texture, new GUIContent("Pattern"));
                    //    break;
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private bool outerGlowOptionsVisible;

        private bool closeShadowOptionsVisible;
        private void DrawCloseShadowOptions(SoftEffects script)
        {
            SerializedProperty optionSP = serializedObject.FindProperty("closeShadowOptions");
            CloseShadowOptions opt = script.closeShadowOptions;
            EditorGUI.indentLevel++;
            EditorGUILayout.Space();
            opt.blur = (int)EditorGUILayout.Slider("Size, px", opt.blur, 0, 250);
            opt.spread = (int)EditorGUILayout.Slider("Spread", opt.spread, 0, 100);
            opt.angle = (int)EditorGUILayout.Slider("Light Angle", opt.angle, 0, 360);
            opt.offset = (int)EditorGUILayout.Slider("Offset, px", opt.offset, 0, 100);
            EditorGUILayout.PropertyField(optionSP.FindPropertyRelative("color"), new GUIContent("Color"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(optionSP.FindPropertyRelative("contour"), new GUIContent("Contour"));
            // opt.noise = (int)EditorGUILayout.Slider("Noise", opt.noise, 0, 100);
        }

        private bool closeShadowOptionsVisible_1;

        private bool shadowOptionsVisible;

        private bool bevelOptionsVisible;
        private void DrawBevelOptions(SoftEffects script)
        {
            SerializedProperty bevelSP = serializedObject.FindProperty("bevelOptions");

            EditorGUI.indentLevel++;
            script.bevelOptions.bStyle = (BevelOptions.Style)EditorGUILayout.EnumPopup("Bevel Style", script.bevelOptions.bStyle);
            EditorGUILayout.Space();
            script.bevelOptions.bTechnique = (BevelOptions.BevelTechnique)EditorGUILayout.EnumPopup("Bevel Type", script.bevelOptions.bTechnique);

            script.bevelOptions.size = (int)EditorGUILayout.Slider("Size, px", script.bevelOptions.size, 0, 250);
            script.bevelOptions.depth = (int)EditorGUILayout.Slider("Depth", script.bevelOptions.depth, 1, 1000);
            script.bevelOptions.smoothing = (int)EditorGUILayout.Slider("Smoothing", script.bevelOptions.smoothing, 0, 16);

            EditorGUILayout.LabelField("Shading");

            script.bevelOptions.angle = (int)EditorGUILayout.Slider("Light Angle ", script.bevelOptions.angle, -180, 180);
            script.bevelOptions.lightAltitude = (int)EditorGUILayout.Slider("Light Altitude ", script.bevelOptions.lightAltitude, 0, 90);
            script.bevelOptions.lightBlendMode = (BevelOptions.BLightMode)EditorGUILayout.EnumPopup("Light blend mode", script.bevelOptions.lightBlendMode);
            script.bevelOptions.shadowBlendMode = (BevelOptions.BShadowMode)EditorGUILayout.EnumPopup("Shadow blend mode", script.bevelOptions.shadowBlendMode);
            SerializedProperty coll = bevelSP.FindPropertyRelative("lightColor");
            EditorGUILayout.PropertyField(coll, new GUIContent("Light Color"));

            SerializedProperty cols = bevelSP.FindPropertyRelative("shadowColor");
            EditorGUILayout.PropertyField(cols, new GUIContent("Shadow Color"));

            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            SerializedProperty contour = bevelSP.FindPropertyRelative("contour");
            EditorGUILayout.PropertyField(contour, new GUIContent("Contour"));
        }

        private bool innerGlowOptionsVisible;

        private bool innerShadowOptionsVisible;

        private bool faceColorOptionsVisible;
        private void DrawFaceColorOptions(SoftEffects script)
        {
            SerializedProperty faceSP = serializedObject.FindProperty("faceOptions");
            EditorGUI.indentLevel++;
            script.faceOptions.cBlendMode = (FaceOptions.CBlendMode)EditorGUILayout.EnumPopup("BlendMode", script.faceOptions.cBlendMode);
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            SerializedProperty coll = faceSP.FindPropertyRelative("fColor");
            EditorGUILayout.PropertyField(coll, new GUIContent("Face Color"));
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private bool faceGradientOptionsVisible;

        private bool facePatternOptionsVisible;
    }

    public class EditorUIUtil
    {

        public static GUIStyle guiTitleStyle
        {
            get
            {
                var guiTitleStyle = new GUIStyle(GUI.skin.label);
                guiTitleStyle.normal.textColor = Color.black;
                guiTitleStyle.fontSize = 16;
                guiTitleStyle.fixedHeight = 30;

                return guiTitleStyle;
            }
        }

        public static GUIStyle guiMessageStyle
        {
            get
            {
                var messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.wordWrap = true;

                return messageStyle;
            }
        }

    }
}
