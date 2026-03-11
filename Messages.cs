using System;

[Serializable]
public class InputMsg
{
    public string method;
    public string arg;
}

[Serializable]
public class OutputMsg
{
    public string type;
    public string[] content;
}
