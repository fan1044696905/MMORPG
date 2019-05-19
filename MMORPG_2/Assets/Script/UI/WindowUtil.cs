using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

/// <summary>
/// 窗口UI管理器
/// </summary>
public class WindowUtil : Singleton<WindowUtil> 
{
    private Dictionary<string, UIWindowViewBase> m_DicWindow = new Dictionary<string, UIWindowViewBase>();

    /// <summary>
    /// 已经打开的窗口数量
    /// </summary>
    public int OpenWindowCount
    {
        get
        {
            return m_DicWindow.Count;
        }
    }

    /// <summary> 关闭所有界面 </summary>
    public void CloseAllWinwow()
    {
        if (m_DicWindow != null)
        {
            m_DicWindow.Clear();
        }
    }

    #region OpenWindow 打开窗口
    /// <summary>
    /// 打开窗口
    /// </summary>
    /// <param name="type">窗口类型</param>
    /// <returns></returns>
    public GameObject OpenWindow(string winName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(winName)) return null;

        GameObject obj = null;
        //如果窗口不存在 则
        if (!m_DicWindow.ContainsKey(winName))
        {
            //枚举的名称要和预设的名称对应
            obj = ResourcesManager.Instance.Load(ResourcesManager.ResourceType.UIWindow, string.Format("{0}", winName), cache: true);
            if (obj == null) return null;
            UIWindowViewBase windowBase = obj.GetComponent<UIWindowViewBase>();
            if (windowBase == null) return null;

            m_DicWindow.Add(winName, windowBase);

            windowBase.CurrentUIType = winName;
            Transform transParent = null;

            switch (windowBase.containerType)
            {
                case WindowUIContainerType.Center:
                    transParent = UISceneCtrl.Instance.CurrentUIScene.Container_Center;
                    break;
            }
            
            obj.transform.SetParent(transParent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);
            obj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            obj.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            StartShowWindow(windowBase, true, onComplete);
        }
        else
        {
            obj = m_DicWindow[winName].gameObject;
        }
        //层级管理
        LayerManager.Instance.SetLayer(obj);
        return obj;
    }

    #endregion

    #region CloseWindow 关闭窗口
    /// <summary> 关闭窗口 </summary>
    /// <param name="winName"></param>
    public void CloseWindow(string winName)
    {
        if (m_DicWindow.ContainsKey(winName))
        {
            StartShowWindow(m_DicWindow[winName], false);
        }
    }
    #endregion

    #region StartShowWindow 开始打开窗口
    /// <summary>
    /// 开始打开窗口
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="isOpen">是否打开</param>
    private void StartShowWindow(UIWindowViewBase windowBase, bool isOpen,Action onComplete = null)
    {
        switch (windowBase.showStyle)
        {
            case WindowShowStyle.Normal:
                ShowNormal(windowBase, isOpen);
                break;
            case WindowShowStyle.CenterToBig:
                ShowCenterToBig(windowBase, isOpen,onComplete);
                break;
            case WindowShowStyle.FromTop:
                ShowFromDir(windowBase, 0, isOpen);
                break;
            case WindowShowStyle.FromDown:
                ShowFromDir(windowBase, 1, isOpen);
                break;
            case WindowShowStyle.FromLeft:
                ShowFromDir(windowBase, 2, isOpen);
                break;
            case WindowShowStyle.FromRight:
                ShowFromDir(windowBase, 3, isOpen);
                break;
        }
    }
    #endregion

    #region 各种打开效果

    /// <summary>
    /// 正常打开
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="isOpen"></param>
    private void ShowNormal(UIWindowViewBase windowBase, bool isOpen)
    {
        if (isOpen)
        {
            windowBase.gameObject.SetActive(true);
        }
        else
        {
            DestroyWindow(windowBase);
        }
    }

    /// <summary>
    /// 中间变大
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isOpen"></param>
    private void ShowCenterToBig(UIWindowViewBase windowBase, bool isOpen,System.Action onComplete = null)
    {
        windowBase.gameObject.SetActive(true);
        windowBase.transform.localScale = Vector3.zero;
        if (windowBase.Tweener==null)
        {
            windowBase.Tweener = windowBase.transform.DOScale(Vector3.one, windowBase.duration)
                .SetAutoKill(false).Pause()
                .SetEase(windowBase.TweenEase)
                .OnComplete(()=> 
                {
                    if (onComplete!=null)
                    {
                        Debug.Log(isOpen + "  " + windowBase.name);
                        onComplete();
                    }
                });
        }
        windowBase.Tweener.OnRewind(() =>
        {
            DestroyWindow(windowBase);
        });

        if (isOpen)
            windowBase.transform.DOPlayForward();
        else
            windowBase.transform.DOPlayBackwards(); 

    }

    /// <summary>
    /// 从不同的方向加载
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="dirType">0=从上 1=从下 2=从左 3=从右</param>
    /// <param name="isOpen"></param>
    private void ShowFromDir(UIWindowViewBase windowBase, int dirType, bool isOpen)
    {
        windowBase.gameObject.SetActive(true);

        Vector3 from = Vector3.zero;
        switch (dirType)
        {
            case 0:
                from = new Vector3(0, 1000, 0);
                break;
            case 1:
                from = new Vector3(0, -1000, 0);
                break;
            case 2:
                from = new Vector3(-1400, 0, 0);
                break;
            case 3:
                from = new Vector3(1400, 0, 0);
                break;
        }
        windowBase.transform.localPosition = from;
        if (windowBase.Tweener == null)
        {
            windowBase.Tweener = windowBase.transform.DOLocalMove(Vector3.zero, windowBase.duration).SetAutoKill(false).Pause().SetEase(windowBase.TweenEase);
        }
        windowBase.Tweener.OnRewind(() =>
        {
            DestroyWindow(windowBase);
        });

        if (isOpen)
            windowBase.transform.DOPlayForward();
        else
            windowBase.transform.DOPlayBackwards();
    }

    #endregion

    #region DestroyWindow 销毁窗口
    /// <summary>
    /// 销毁窗口
    /// </summary>
    /// <param name="obj"></param>
    private void DestroyWindow(UIWindowViewBase windowBase)
    {
        m_DicWindow.Remove(windowBase.CurrentUIType);
        UnityEngine.Object.Destroy(windowBase.gameObject);
    }
    #endregion
}