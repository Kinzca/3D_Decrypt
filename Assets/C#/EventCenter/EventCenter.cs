using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;//添加命名空间

public class EventCenter : MonoBehaviour
{
    #region 事件广播主体
    //此处的代码可以不做修改直接使用
    //设置字典，建立新的委托列表
    private static Dictionary<EventType, Delegate> EventTable = new Dictionary<EventType, Delegate>();

    ///<summary>
    ///添加监听
    ///可以理解为添加号码
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="callBack"></param>

    private static void OnListenerAdding(EventType eventType, Delegate callBack)
    {
        if (!EventTable.ContainsKey(eventType))//判断是否有事件编码
        {
            //如果没有则添加
            EventTable.Add(eventType, null);
        }

        Delegate d = EventTable[eventType];

        if (d != null && d.GetType() != callBack.GetType())//d不为空值，且d的类型不为获取到的类型
        {
            //报错
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件的委托是{1},要添加都得事件委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
            //请检查号码是否输入正确，并重新输入
        }
    }

    ///<summary>
    ///移除监听
    ///可以理解为删除号码
    /// </summary>
    ///<param name="eventType"></param>
    ///<param name="callBack"></param>

    private static void OnListenerRemoving(EventType eventType, Delegate callBack)
    {
        if (EventTable.ContainsKey(eventType))
        {
            Delegate d = EventTable[eventType];
            if (d == null)
            {
                //没有对应信号，或者号码被移除
                throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventType));
            }
            else if (d.GetType() != callBack.GetType())
            {
                //请检查号码是否输入正确，请重新输入
                throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同类型的委托，当前的委托类型为{1},要移除的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
            }
        }
        else
        {
            //没有事件码
            throw new Exception(string.Format("移除监听错误：没有事件码{0}", eventType));
        }
    }

    private static void OnListenerRemoved(EventType eventType)
    {
        if (EventTable[eventType] == null)//如果号码被删除 
        {
            EventTable.Remove(eventType);
        }
    }
    #endregion

    #region 0号广播类型（可复制修改）
    public static void AddListener(EventType eventType, CallBack callBack)
    {
        OnListenerAdding(eventType, callBack);//在基站里添加
        EventTable[eventType] = (CallBack)EventTable[eventType] + callBack;//在手机里添加号码，+xxx,xxx
    }

    public static void RemoveListener(EventType eventType, CallBack callBack)
    {
        OnListenerRemoving(eventType, callBack);//在基站中删除
        EventTable[eventType] = (CallBack)EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);//终止通讯
    }

    public static void Broadcast(EventType eventType)
    {
        Delegate d;
        if (EventTable.TryGetValue(eventType, out d))
        {
            CallBack callBack = d as CallBack;
            if (callBack != null)
            {
                callBack();//拨号
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同地类型", eventType));
            }
        }
    }
    #endregion

    #region 1号广播类型（可复制修改）
    public static void AddListener<T>(EventType eventType, CallBack<T> callBack)
    {
        OnListenerAdding(eventType, callBack);//在基站里添加
        EventTable[eventType] = (CallBack<T>)EventTable[eventType] + callBack;//在手机里添加号码，+xxx,xxx
    }

    public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack)
    {
        OnListenerRemoving(eventType, callBack);//在基站中删除
        EventTable[eventType] = (CallBack<T>)EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);//终止通讯
    }

    public static void Broadcast<T>(EventType eventType, T arg)
    {
        Delegate d;
        if (EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T> callBack = d as CallBack<T>;
            if (callBack != null)
            {
                callBack(arg);//拨号
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同地类型", eventType));
            }
        }
    }
    #endregion

    #region 2号广播类型（可复制修改）
    public static void AddListener<T, X>(EventType eventType, CallBack<T, X> callBack)
    {
        OnListenerAdding(eventType, callBack);//在基站里添加
        EventTable[eventType] = (CallBack<T, X>)EventTable[eventType] + callBack;//在手机里添加号码，+xxx,xxx
    }

    public static void RemoveListener<T, X>(EventType eventType, CallBack<T, X> callBack)
    {
        OnListenerRemoving(eventType, callBack);//在基站中删除
        EventTable[eventType] = (CallBack<T, X>)EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);//终止通讯
    }

    public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2)
    {
        Delegate d;
        if (EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T, X> callBack = d as CallBack<T, X>;
            if (callBack != null)
            {
                callBack(arg1, arg2);//拨号
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同地类型", eventType));
            }
        }
    }
    #endregion

    #region 3号广播类型（可复制修改）
    public static void AddListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack)
    {
        OnListenerAdding(eventType, callBack);//在基站里添加
        EventTable[eventType] = (CallBack<T, X, Y>)EventTable[eventType] + callBack;//在手机里添加号码，+xxx,xxx
    }

    public static void RemoveListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack)
    {
        OnListenerRemoving(eventType, callBack);//在基站中删除
        EventTable[eventType] = (CallBack<T, X, Y>)EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);//终止通讯
    }

    public static void Broadcast<T, X, Y>(EventType eventType, T arg1, X arg2, Y arg3)
    {
        Delegate d;
        if (EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T, X, Y> callBack = d as CallBack<T, X, Y>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3);//拨号
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同地类型", eventType));
            }
        }
    }
    #endregion

    #region 3号广播类型（可复制修改）
    public static void AddListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack)
    {
        OnListenerAdding(eventType, callBack);//在基站里添加
        EventTable[eventType] = (CallBack<T, X, Y, Z>)EventTable[eventType] + callBack;//在手机里添加号码，+xxx,xxx
    }

    public static void RemoveListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack)
    {
        OnListenerRemoving(eventType, callBack);//在基站中删除
        EventTable[eventType] = (CallBack<T, X, Y, Z>)EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);//终止通讯
    }

    public static void Broadcast<T, X, Y, Z>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4)
    {
        Delegate d;
        if (EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T, X, Y, Z> callBack = d as CallBack<T, X, Y, Z>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3, arg4);//拨号
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同地类型", eventType));
            }
        }
    }
    #endregion
}