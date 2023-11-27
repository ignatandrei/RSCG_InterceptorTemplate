using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
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
        //data = ["FullName", "Test", "PersonsLoaded", "FullNameWithSeparator", "ShowRandomPersonNumber", "Connect", "SavePerson", "InsertPerson"];
        data = ["FullName","Test", "PersonsLoaded"];
        //data = ["Connect"];
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

        var x12= ops.Keys.Count;
        x12+= 1;
        var nrFilesPerMethodAndClass = ops.Keys
            .GroupBy(it => it.TypeOfClass + "_" + it.MethodName)
            .Select(a => new { a.Key, Count = a.Count() })
            .ToDictionary(it => it.Key, it =>(number:0, total:it.Count));

        Dictionary<TypeAndMethod , DataForSerializeFile> dataForSerializeFiles = new();
        foreach (var item in ops.Keys)
        {
            var ser=new DataForSerializeFile();
            dataForSerializeFiles.Add(item, ser);
            ser.item = item;

            var methodName = item.MethodName;
            var typeOfClass = item.TypeOfClass;
            string nameFile = typeOfClass + "_" + methodName;
            var nrFiles = nrFilesPerMethodAndClass[nameFile];
            
            var nr = nrFiles.number;
            nr++;
            nrFilesPerMethodAndClass[nameFile]= (nr, nrFiles.total);
            nameFile += $"_nr_{nr}_from_{nrFiles.total}";
            //var typeReturn = item.TypeReturn;
            ser.nameFileToBeWritten = nameFile;
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
        foreach (var item in ops.Keys)
        {
            var ser = dataForSerializeFiles[item];            
            spc.AddSource(ser.nameFileToBeWritten + ".cs", ser.DataToBeWriten);

        }
        //cnt += "\r\n";
        //cnt += "}//namespace RSCG_InterceptorTemplate";
        //spc.AddSource("RSCG_InterceptorTemplate.g.cs", cnt);
        var x = 1;
    }

    private void ExecuteGenOld(SourceProductionContext spc, ((Compilation Left, ImmutableArray<Tuple<AttributeSyntax, SymbolInfo>> Right) Left, ImmutableArray<AdditionalText> Right) data)
    {
        //data.Left.Left.Assembly.Name = "RSCG_InterceptorTemplate";

        var x = 1;
    }
    public static bool TryGetMapMethodName(SyntaxNode node, out string? methodName)
    {
        methodName = default;
        // Given an invocation like app.MapGet, app.Map, app.MapFallback, etc. get
        // the value of the Map method being access on the the WebApplication `app`.
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
        //if (method == "FullName" || method == "Test")
        //    return true;
        //if(method == "PersonsLoaded")
        //    return true;
        //if (method== "FullNameWithSeparator")
        //    return true;
        //if (method == "ShowRandomPersonNumber")
        //    return true;
        //if(method == "Connect")
        //    return true;
        //if(method == "SavePerson")  
        //    return true;
        //if(method == "InsertPerson")
        //    return true;
        return false;

        //var q=Environment.GetEnvironmentVariable("ASdasd");
        //var x1 = s.ToFullString();
        //if(s  is InvocationExpressionSyntax inv)
        //{
        //    if (x1.Contains("FullName"))
        //    {
        //        var p = inv.Parent;
        //        var s1 = p.ToFullString();
        //        s1 += "asda";

        //    }

        //}
        //if (s is not AttributeSyntax cds) return false;
        //if (!cds.Name.ToFullString().Contains("InterceptClassMethods")) return false;
        //if(cds.Parent is not AttributeListSyntax als) return false;
        //var x = als.Target?.Identifier.Text;
        //if(x != "assembly") return false;
        //return true;

    }
    //private Tuple<AttributeSyntax, SymbolInfo>? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
    //{
    //    var cds = ctx.Node as AttributeSyntax;
    //    if (cds == null) return null;


    //    //if (data == null) return null;
    //    return new Tuple<AttributeSyntax, SymbolInfo>(cds, symbolInfo);

    //}

    private Tuple<AttributeSyntax, SymbolInfo>? OldGetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
    {

        var attributeSyntax = ctx.Node as AttributeSyntax;
        if (attributeSyntax == null) return null;

        var symbolInfo = ctx.SemanticModel.GetSymbolInfo(attributeSyntax);
        var q = ctx.SemanticModel.GetDeclaredSymbol(attributeSyntax);
        var q1 = ctx.SemanticModel.GetTypeInfo(attributeSyntax);
        ITypeSymbol? t = q1.Type;
        var attributeSymbol = t as INamedTypeSymbol;

        if (attributeSymbol == null) return null;

        var genericType = attributeSymbol.TypeArguments.FirstOrDefault();

        return new Tuple<AttributeSyntax, SymbolInfo>(attributeSyntax, symbolInfo);
    }

}