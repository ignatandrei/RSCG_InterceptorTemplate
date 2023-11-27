namespace RSCG_InterceptorTemplate;
public class DataForSerializeFile
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
    public static {{(item.HasTaskReturnType ? "async" : "")}} {{item.TypeReturn}} {{item.MethodSignature}}({{item.ThisArgument()}} {{item.ArgumentsForCallMethod}} )  {
         //return "Andrei";
         Console.WriteLine("beginX-->{{item.CallMethod}}");
        {{item.ReturnString}} {{(item.HasTaskReturnType ? "await" : "")}} {{item.CallMethod}};
         Console.WriteLine("endY-->{{item.MethodSignature}}");

    }
}                
""";
        } }

    internal string startContent = $$"""
static partial class SimpleIntercept
{
            
""";
    public List<DataForEachIntercept> dataForEachIntercepts=new();
}

public struct DataForEachIntercept
{
    public string CodeNumbered
    {
        get
        {
            int numberCode = 0;
            string codeNumbered = "";
            while (numberCode < code.Length)
            {
                numberCode++;
                var nr1 = numberCode % 10;
                if (nr1 == 0)
                {
                    codeNumbered += "!";
                }
                else
                {
                    codeNumbered += (nr1).ToString();
                }

            }
            return codeNumbered;
        }
    }
    public string code { get; set; }

    public string Path { get; set; }
    public int Line { get; internal set; }
    public int StartMethod { get; internal set; }
    public string DataToBeWriten
    {
        get
        {            
            var   content = "\r\n";
            content += $@"//replace code: {code}";
            content += "\r\n";
            content += $@"//replace code: {CodeNumbered}";
            content += "\r\n";
            content+=$$""" 
[System.Runtime.CompilerServices.InterceptsLocation(@"{{Path}}", {{Line}}, {{StartMethod}})]                
""";
            return content;
        }
    } 
}
