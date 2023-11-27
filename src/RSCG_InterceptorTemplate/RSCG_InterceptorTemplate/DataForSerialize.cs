using System.Runtime.ConstrainedExecution;

namespace RSCG_InterceptorTemplate;
class DataForSerializeFile
{
    public TypeAndMethod item;

    public int extraLength
    {
        get
        {
            var extra = item.NameOfVariable.Length;
            if (extra > 0)
            {
                //acknowledge the dot
                extra += 1;
            }
            return extra;
        }
    }

    public string Declaration { get {
            return $$"""

    //[System.Diagnostics.DebuggerStepThrough()]
    public static {{(item.HasTaskReturnType ? "async" : "")}} {{item.TypeReturn}} {{item.MethodSignature}}({{item.ThisArgument}} {{item.ArgumentsForCallMethod}} )  {
         //return "Andrei";
         Console.WriteLine("beginX-->{{item.CallMethod}}");
        {{item.ReturnString}} {{(item.HasTaskReturnType ? "await" : "")}} {{item.CallMethod}};
         Console.WriteLine("endY-->{{item.MethodSignature}}");

    }
}                
""";
        } }

    public string nameFileToBeWritten { get; internal set; }

    internal string startContent = $$"""
static partial class SimpleIntercept
{
            
""";
    public List<DataForEachIntercept> dataForEachIntercepts=new();

    internal static string cntPrefix = $$""""
#pragma warning disable CS1591 
#pragma warning disable CS9113
namespace System.Runtime.CompilerServices{
[AttributeUsage(AttributeTargets.Method,AllowMultiple =true)]
file class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
{
}
}//end namespace

namespace RSCG_InterceptorTemplate{

"""";

    internal static string cntSuffix = $$""""

}//namespace RSCG_InterceptorTemplate

"""";

    public string DataToBeWriten 
    {
        get
        {
            var cnt = cntPrefix;
            cnt += startContent;
            dataForEachIntercepts.ForEach(it => cnt += it.DataToBeWriten);
            cnt += Declaration;
            cnt += cntSuffix;
            return cnt;
        }
    }
}
