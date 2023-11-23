namespace RSCG_InterceptorTemplateCommon;

[AttributeUsage(AttributeTargets.Assembly,AllowMultiple =true)]
public class InterceptClassMethodsAttribute<T>(string templateName) : Attribute
    where T : class
{
    public readonly string templateName = templateName;
}

