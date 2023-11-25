namespace RSCG_InterceptorTemplateCommon;

[AttributeUsage(AttributeTargets.Assembly,AllowMultiple =true)]
public class InterceptClassMethodsAttribute<T>(string templateName, params string[] methodsName ) : Attribute
    where T : class
{
    public readonly string templateName = templateName;

    public string[] MethodsName { get; } = methodsName;
}

