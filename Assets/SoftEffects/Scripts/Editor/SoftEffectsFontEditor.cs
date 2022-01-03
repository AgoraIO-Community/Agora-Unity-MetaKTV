using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Mkey
{
    [CustomEditor(typeof(SoftEffectsFont))]
    public class SoftEffectsFontEditor : SoftEffectsEditor
    {
        private bool drawDefault = false;

        public override void OnInspectorGUI()
        {
            if (drawDefault) DrawDefaultInspector();
            SoftEffects myScript = (SoftEffects)target;
            if (!myScript.gameObject.activeSelf || !myScript.enabled) return;
            if (!OnDrawSupportCompute()) return;

            OnDrawTitle();
            if (OndrawTargetMissed(myScript)) return;

            OnDrawCreate(myScript);
            OnDrawSpacing(myScript);
            OnDrawTargetProp(myScript);

            if (OndrawEdiObjectMissed(myScript)) return;

            OnDrawSoftFolder(myScript);

            if (OndrawEditMaterialMissed(myScript)) return;

            if (OndrawEditTextureMissed(myScript)) return;

            OnDrawSave(myScript);

            EditorGUI.BeginChangeCheck();

            OnDrawOptions(myScript);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                myScript.AdjustTextureOnLine();
                if (!SceneManager.GetActiveScene().isDirty) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            /*
            if (UnityEngine.GUILayout.Button("Rebuild SDT"))
            {
                Mkey.Utils.Measure("SDT create: ",()=> {  myScript.CreateSDT();});

            }

            if (UnityEngine.GUILayout.Button("Rebuild AASDT"))
            {
                Mkey.Utils.Measure("AASDT create: ", () => { myScript.CreateAASDT(); });

            }

            if (UnityEngine.GUILayout.Button("Render Bevel"))
            {
                Mkey.Utils.Measure("Render Bevel: ", () => { myScript.RenderBevel(); });

            }
            */
        }
    }
}