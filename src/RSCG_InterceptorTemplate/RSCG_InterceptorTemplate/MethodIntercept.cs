using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Immutable;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
//https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/gen/StaticRouteHandlerModel/InvocationOperationExtensions.cs
//https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/gen/RequestDelegateGenerator.cs
//https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/gen/StaticRouteHandlerModel/InvocationOperationExtensions.cs
namespace RSCG_InterceptorTemplate;

[Generator]
public class MethodIntercept : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        var methods = Environment.GetEnvironmentVariable("InterceptMethods");
        var data = methods?.Split(';');
        data ??= [];
        //data = ["FullName", "Test", "PersonsLoaded", "TestFullNameWithArguments", "ShowRandomPersonNumber", "Connect", "SavePerson", "InsertPerson"];
        //data = ["FullName","Test", "PersonsLoaded"];
        //data = ["Connect"];
        //data = ["FullName"];
        
        //data = ["TestFullNameWithArguments"];
        var classesToIntercept = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => IsSyntaxTargetForGeneration(s,data),
                transform: static (context, token) =>
                {
                    var operation = context.SemanticModel.GetOperation(context.Node, token);
                    return operation;
                })
            .Where(static m => m is not null)!
            ;

        var compilationAndData
                 = context.CompilationProvider.Combine(classesToIntercept.Collect()).Combine(context.AdditionalTextsProvider.Collect());
        context.RegisterSourceOutput(compilationAndData,
           (spc, data) =>
           ExecuteGen(spc, data!));
    }
    private void ExecuteGen(SourceProductionContext spc, ((Compilation Left, ImmutableArray<IOperation> Right) Left, ImmutableArray<AdditionalText> Right) value)
    {
        
        var textes = value
            .Right.ToArray()
            .Select(it=>new { it.Path, text = it.GetText()?.ToString() })
            .ToArray();
            ;

        var compilation = value.Left.Left;
        
        

        var ops = value
            .Left.Right
            .Select(op =>
            {
                TryGetMapMethodName(op.Syntax, out var methodName);  
                var typeReturn = op.Type;
                var invocation = op  as IInvocationOperation;
                Argument[] arguments = [];
                if (invocation != null && invocation.Arguments.Length > 0)
                {
                    arguments = invocation
                    .Arguments
                    .Where(it=>it?.Parameter != null)
                    .Select(it => new Argument(it!.Parameter!.ToDisplayString()))
                    .ToArray();
                }
                var instance = invocation?.Instance as ILocalReferenceOperation;
                TypeAndMethod typeAndMethod;
                if(instance == null)
                {
                    var staticMember = invocation?.TargetMethod?.ToDisplayString();
                    var justMethod = staticMember?.IndexOf("(");
                    if(justMethod != null && justMethod > 0)
                    {
                        staticMember = staticMember?.Substring(0,justMethod.Value);
                    }
                    //if(staticMember != null)
                    //{
                    //    var typeOfClass = compilation.GetTypeByMetadataName(staticMember);
                    //}
                    typeAndMethod = new TypeAndMethod(staticMember?? "", typeReturn?.ToString() ?? "");
                    var nameMethod = typeAndMethod.MethodName;
                    string fullCall = op.Syntax.ToFullString();
                    justMethod = fullCall.IndexOf("(");
                    if (justMethod != null && justMethod > 0)
                    {
                        fullCall = fullCall.Substring(0, justMethod.Value);                        
                    }
                    var nameVar = fullCall.Substring(0,fullCall.Length-nameMethod.Length -".".Length );
                    typeAndMethod.NameOfVariable=nameVar;

                }
                else
                {
                    var typeOfClass = instance.Type;
                    var nameVar= instance.Local.Name;
                    typeAndMethod = new TypeAndMethod(typeOfClass?.ToString() ?? "", methodName ?? "", typeReturn?.ToString() ?? "",nameVar);

                }
                typeAndMethod.Arguments = arguments;
                return new { typeAndMethod,op};

            })
            .Where(it=>it.typeAndMethod.IsValid())
            .GroupBy(x => x.typeAndMethod)
            .ToDictionary(x => x.Key, x => x.ToArray())
            ;

        if (ops.Keys.Count == 0)
            return;
        var nrFilesPerMethodAndClass = ops.Keys
            .GroupBy(it => it.TypeOfClass + "_" + it.MethodName)
            .Select(a => new { a.Key, Count = a.Count() })
            .ToDictionary(it => it.Key, it =>(number:0, total:it.Count));

        Dictionary<TypeAndMethod , DataForSerializeFile> dataForSerializeFiles = new();
        foreach (var item in ops.Keys)
        {
            var ser=new DataForSerializeFile(item);
            dataForSerializeFiles.Add(item, ser);
            ser.item = item;

            var methodName = item.MethodName;
            var typeOfClass = item.TypeOfClass;
            //string nameFile = typeOfClass + "_" + methodName;
            //var nrFiles = nrFilesPerMethodAndClass[nameFile];
            
            //var nr = nrFiles.number;
            //nr++;
            //nrFilesPerMethodAndClass[nameFile]= (nr, nrFiles.total);
            //nameFile += $"_nr_{nr}_from_{nrFiles.total}";
            //var typeReturn = item.TypeReturn;
            //ser.nameFileToBeWritten = nameFile;
            var nameOfVariable = item.NameOfVariable;
            int extraLength = ser.extraLength;

            foreach (var itemData in ops[item])
            {
                
                DataForEachIntercept dataForEachIntercept = new();
                ser.dataForEachIntercepts.Add(dataForEachIntercept);
                var op= itemData.op;
                var tree = op.Syntax.SyntaxTree;
                var filePath = compilation.Options.SourceReferenceResolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
                var location = tree.GetLocation(op.Syntax.Span);             
                var lineSpan = location.GetLineSpan();
                var startLinePosition = lineSpan.StartLinePosition;
                SourceText sourceText = location.SourceTree!.GetText();
                var line = sourceText.Lines[startLinePosition.Line];
                string code = line.ToString();
                dataForEachIntercept.code = code;
                dataForEachIntercept.Path = lineSpan.Path;
                dataForEachIntercept.Line = startLinePosition.Line + 1;
                dataForEachIntercept.StartMethod = startLinePosition.Character + 1 + extraLength;
            }
            
        }

        foreach (var ser in dataForSerializeFiles)
        {
            var name = ser.Key.MethodName;
            var fileText=textes.FirstOrDefault(it=>it.Path.EndsWith(name+".txt"))?.text;
            if(fileText == null)
            {
                fileText = textes.FirstOrDefault(it => it.Path.EndsWith("GenericInterceptorForAllMethods.txt"))?.text;
                
            }
            if(fileText != null)
            {
                var template = Template.Parse(fileText);
                string fileContent = template.Render(new { ser= ser.Value, Version= "8.2023.2811.524" }, m => m.Name);
                spc.AddSource(ser.Value.nameFileToBeWritten + ".cs", fileContent);
                
                continue;
            }
            Diagnostic d = Diagnostic.Create(
                new DiagnosticDescriptor("RSCG_InterceptorTemplate", 
                "RSCG_InterceptorTemplate_001", 
                $"no template for method {name}", "RSCG_InterceptorTemplate", 
                DiagnosticSeverity.Warning, 
                true), 
                Location.None);

            spc.ReportDiagnostic(d);
            //write default?            
            spc.AddSource(ser.Value.nameFileToBeWritten + ".cs", ser.Value.DataToBeWriten);
            continue;


        }
    }

   public static bool TryGetMapMethodName(SyntaxNode node, out string? methodName)
    {
        methodName = default;
       if (node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: { Identifier: { ValueText: var method } } } })
        {
            methodName = method;
            return true;
        }
        return false;
    }
    private bool IsSyntaxTargetForGeneration(SyntaxNode s, string[] data)
    {
        if(data.Length == 0)return false;
        if (!TryGetMapMethodName(s, out var method))
            return false;
        if(data.Contains(method))
            return true;
        return false;

    }
    

}