public readonly struct TypeAndMethod(string TypeOfClass, string Method,string typeReturn)
{
    public string TypeOfClass { get; } = TypeOfClass;
    public string Method { get; } = Method;
    public string TypeReturn { get; } = typeReturn;

    public bool IsValid()
    {
        return TypeOfClass.Length > 0 && Method.Length > 0 ;
    }
}
