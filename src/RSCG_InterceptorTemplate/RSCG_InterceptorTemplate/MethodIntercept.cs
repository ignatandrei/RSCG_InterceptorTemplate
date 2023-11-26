using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Net;
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

        var x = Environment.GetEnvironmentVariable("ASdasd");
        var classesToIntercept = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => IsSyntaxTargetForGeneration(s),
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
        var compilation = value.Left.Left;
        var cnt = "";
        cnt += $$""""
#pragma warning disable CS1591 
#pragma warning disable CS9113
namespace System.Runtime.CompilerServices{
[AttributeUsage(AttributeTargets.Method,AllowMultiple =true)]
sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
{
}
}
namespace RSCG_InterceptorTemplate{

"""";
        var ops = value
            .Left.Right
            .Select(op =>
            {
                TryGetMapMethodName(op.Syntax, out var methodName);  
                var typeReturn = op.Type;
                var invocation = op  as IInvocationOperation;
                var instance = invocation?.Instance as ILocalReferenceOperation;
                TypeAndMethod typeAndMethod;
                if(instance == null)
                {
                    var staticMember = invocation?.TargetMethod?.ToString();
                    //if(staticMember != null)
                    //{
                    //    var typeOfClass = compilation.GetTypeByMetadataName(staticMember);
                    //}
                    typeAndMethod = new TypeAndMethod(staticMember?? "", typeReturn?.ToString() ?? "");
                    var nameMethod = typeAndMethod.MethodName;
                    string fullCall = op.Syntax.ToFullString();
                    var nameVar = fullCall.Substring(0,fullCall.Length-nameMethod.Length -"()".Length -".".Length );
                    typeAndMethod.NameOfVariable=nameVar;

                }
                else
                {
                    var typeOfClass = instance.Type;
                    var nameVar= instance.Local.Name;
                    typeAndMethod = new TypeAndMethod(typeOfClass?.ToString() ?? "", methodName ?? "", typeReturn?.ToString() ?? "",nameVar);

                }
                return new { typeAndMethod,op};

            })
            .Where(it=>it.typeAndMethod.IsValid())
            .GroupBy(x => x.typeAndMethod)
            .ToDictionary(x => x.Key, x => x.ToArray())
            ;

        var x12= ops.Keys.Count;
        x12+= 1;
        foreach (var item in ops.Keys)
        {
            var methodName = item.MethodName;
            var typeOfClass = item.TypeOfClass; 
            var typeReturn = item.TypeReturn;
            var nameOfVariable = item.NameOfVariable;
            int extraLength = nameOfVariable.Length;
            if(extraLength > 0)
            {
                //acknowledge the dot
                extraLength += 1;
            }
            var content = $$"""

static partial class SimpleIntercept
{
            
"""
            ;

            foreach (var itemData in ops[item])
            {
                var op= itemData.op;
                var tree = op.Syntax.SyntaxTree;

                var filePath = compilation.Options.SourceReferenceResolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
                var location = tree.GetLocation(op.Syntax.Span);
                
                var lineSpan = location.GetLineSpan();
                var startLinePosition = lineSpan.StartLinePosition;

                SourceText sourceText = location.SourceTree!.GetText();

                
                var line = sourceText.Lines[startLinePosition.Line];

                // Now 'line' contains the line of code from the location
                string code = line.ToString();
                int nr = 0;
                string codeNumbered = "";
                while (nr < code.Length)
                {
                    nr++;
                    var nr1 = nr % 10;
                    if (nr1 == 0)
                    {
                        codeNumbered += "!";
                    }
                    else
                    {
                        codeNumbered += (nr1).ToString();
                    }
                    
                }
                content += "\r\n";
                content += $@"//replace code: {codeNumbered}";
                content += "\r\n";
                content += $@"//replace code: {code}";
                content += "\r\n";
                content += $"//variable : {item.NameOfVariable}";
                content += "\r\n";
                content += $$""" 
[System.Runtime.CompilerServices.InterceptsLocation(@"{{lineSpan.Path}}", {{startLinePosition.Line + 1}}, {{startLinePosition.Character + 1+ extraLength }})]
                
""";
            }

            content += $$"""

    //[System.Diagnostics.DebuggerStepThrough()]
    public static {{typeReturn}} {{item.MethodSignature}}({{item.ThisArgument()}})  {
         //return "A12";
        return {{item.CallMethod}};
    
    }
}                
""";

            cnt += content;
        }
        cnt += "\r\n";
        cnt += "}//namespace RSCG_InterceptorTemplate";
        spc.AddSource("RSCG_InterceptorTemplate.g.cs", cnt);
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
    private bool IsSyntaxTargetForGeneration(SyntaxNode s)
    {
        if (!TryGetMapMethodName(s, out var method))
            return false;
        if (method == "FullName" || method == "Test")
            return true;
        if(method == "PersonsLoaded")
            return true;
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