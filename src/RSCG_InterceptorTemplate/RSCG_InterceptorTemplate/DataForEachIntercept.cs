namespace RSCG_InterceptorTemplate;

class DataForEachIntercept
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
