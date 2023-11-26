public struct TypeAndMethod(string TypeOfClass, string Method)
{
    public string TypeOfClass { get; } = TypeOfClass;
    public string Method { get; } = Method;

    public bool IsValid()
    {
        return TypeOfClass.Length > 0 && Method.Length > 0;
    }
}
