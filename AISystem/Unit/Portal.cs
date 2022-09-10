using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : Unit
{
    //// 0. Structs(Override)
    //new public struct Information
    //{
    //    string scenePathName;
    //}

    //// 1. Fields(Override)
    //new protected Information info;

    //// 2. Properties(Override)
    //new public Information Info
    //{
    //    get { return info; }
    //    set { info = value; }
    //}

    public string scenePathName;
    protected Collider col;

    private void Awake()
    {
        col = gameObject.GetComponent<Collider>();
    }

    public Collider Col
    {
        get { return col; }
    }
}
