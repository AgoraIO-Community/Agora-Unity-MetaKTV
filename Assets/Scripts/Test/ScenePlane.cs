using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenePlane : MonoBehaviour
{
    public GameObject mItemPrefab;
    private Transform mContentTransform;
    private Scrollbar mScrollbar;
    // Use this for initialization
    void Start()
    {
        mContentTransform = this.transform.Find("ScrollRect/ScrollView/Content");
        mScrollbar = this.transform.Find("Scrollbar").GetComponent<Scrollbar>();
        ShowItems();
        mScrollbar.value = 1.0f;
    }

    /// <summary>
    /// 显示Item列表
    /// </summary>
    void ShowItems()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject item = Instantiate(mItemPrefab, transform.position, transform.rotation);
            item.transform.parent = mContentTransform;
        }
    }
}
