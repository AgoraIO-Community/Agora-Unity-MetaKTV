using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
[CreateAssetMenu]

public class CGHolder : ScriptableObject
{

    [Space(8, order = 0)]
    [Header("Curves", order = 1)]
	public AnimationCurve[] curves;

	[Space(8, order = 0)]
	[Header("Gradients", order = 1)]
	public Gradient[] gradients;

    public string TOSTRING()
    {
        return name + "; id: " + GetInstanceID();
    }

    public void Save()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

}


