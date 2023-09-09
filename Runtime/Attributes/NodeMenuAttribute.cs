using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMenuAttribute : System.Attribute
{
    public string Path;
    public NodeMenuAttribute(string path)
    {
        this.Path = path;
    }
}
