using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataListContainerSO<T> : ScriptableObject
{
    public Action<T> OnDataAdded;

    [SerializeField]
    protected List<T> datas;

    public List<T> Datas
    {
        get
        {
            return datas;
        }
    }

    public void AddData(T data)
    {
        datas.Add(data);
        OnDataAdded?.Invoke(data);
    }
}
