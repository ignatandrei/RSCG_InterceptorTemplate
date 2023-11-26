

namespace RSCG_InterceptorTemplate;
public partial struct Argument
{
    public Argument(string typeAndName)
    {
        this.TypeAndName = typeAndName;
        this.Type=typeAndName.Split(' ')[0];
        this.Name=typeAndName.Split(' ')[1];
    }
    public string TypeAndName { get; }
    public string Type { get; }
    public string Name { get; }
}

public partial struct TypeAndMethod 
{
    public TypeAndMethod(string typeOfClass, string methodInvocation, string typeReturn,string nameOfVariable)
    {
        InstanceIsNotNull = true;
        TypeOfClass = typeOfClass;
        MethodInvocation = methodInvocation;
        TypeReturn = typeReturn;
        NameOfVariable = nameOfVariable;
        
    }
    public TypeAndMethod(string staticMethod, string typeReturn)
    {
        InstanceIsNotNull = false;
        var methods = staticMethod.Split('.');
        TypeOfClass = methods[0] + ".";
        for (int i = 1; i < methods.Length - 1; i++)
        {
            TypeOfClass += methods[i];
            TypeOfClass += ".";
        }
        TypeOfClass = TypeOfClass.TrimEnd('.');
        MethodInvocation = staticMethod;
        TypeReturn = typeReturn;
        
        NameOfVariable = "";
    }
    public bool InstanceIsNotNull { get; }
    public string TypeOfClass { get; }
    public string MethodInvocation { get; }
    public string TypeReturn { get; }
    public string NameOfVariable { get; set; }

    public bool HasTaskReturnType
    {
        get
        {
            return TypeReturn.Contains("System.Threading.Tasks.Task");
        }
    }
    public bool IsValid()
    {

        return TypeOfClass.Length > 0 && MethodInvocation.Length > 0;

    }
    public string MethodName
    {
        get
        {
            if (this.InstanceIsNotNull)
            {
                return MethodInvocation;
            }
            else
            {
                return MethodInvocation.Split('.').Last().Replace("()", "");
            }
        }
    }
    public string ReturnString
    {
        get
        {
            if(this.TypeReturn=="void")
            {
                return "";
            }
            else
            {
                return $"return";
            }
        }
    }
    public string CallMethod
    {
        get
        {
            var args=string.Join(",", Arguments.Select(a => a.Name));
            if (this.InstanceIsNotNull)
            {
                return $"{NameOfVariable}.{MethodInvocation}({args})";
            }
            else
            {
                return $"{MethodInvocation}({args})";
            }
        }
    }
    public string ArgumentsForCallMethod
    {
        get
        {
            string args=string.Join(",", Arguments.Select(a => a.TypeAndName));

            if (this.InstanceIsNotNull && args.Length>0)
            {
                //first argument is this
                args = "," + args;
            }
            return args;
        }
    }
    public string ThisArgument()
    {
        if (this.InstanceIsNotNull)
        {
            return $"this {TypeOfClass} {NameOfVariable}";
        }
        else
        {
            return "";
        }
    }

    public string MethodSignature
    {
        get
        {
            var nameOfVariable = NameOfVariable.Replace(".","_");
            return $"Intercept_{nameOfVariable}_{MethodName}";
        }
    }

    public Argument[] Arguments { get; internal set; }
}
