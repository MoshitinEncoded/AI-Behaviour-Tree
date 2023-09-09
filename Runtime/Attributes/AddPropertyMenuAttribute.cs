using System;

public class AddPropertyMenuAttribute : Attribute
{
    public string MenuPath = string.Empty;
    public AddPropertyMenuAttribute(string menuPath)
    {
        MenuPath = menuPath;
    }
}
