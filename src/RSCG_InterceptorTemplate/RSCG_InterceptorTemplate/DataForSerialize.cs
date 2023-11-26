
using System.Reflection;

namespace RSCG_InterceptorTemplate;
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
    public string CallMethod
    {
        get
        {
            if (this.InstanceIsNotNull)
            {
                return $"{NameOfVariable}.{MethodInvocation}()";
            }
            else
            {
                return MethodInvocation;
            }
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

    
}
