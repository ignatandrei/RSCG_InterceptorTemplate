https://github.com/ignatandrei/RSCG_InterceptorTemplate




Add to your project (>= .NET 8 ) the nuget package RSCG_InterceptorTemplate

```xml
<ItemGroup>
 <PackageReference Include="RSCG_InterceptorTemplate" Version="8.2023.2811.524" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GX</CompilerGeneratedFilesOutputPath>
    <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);RSCG_InterceptorTemplate</InterceptorsPreviewNamespaces>
</PropertyGroup>
```

Make a folder Interceptors in the project and add also at least the generic interceptor

```xml
  <ItemGroup>
    <!-- <AdditionalFiles Include="Interceptors\TestFullNameWithArguments.txt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles> -->
    <AdditionalFiles Include="Interceptors\GenericInterceptorForAllMethods.txt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>
```

The interceptor will not run template at build time in Visual Studiom, but it will run at build time in dotnet build.

For this, you need to have something like that ( powershell file) -I named mine compile.ps1

```powershell
cls
#not necessary for CI builds, but only for debugging purposes
Write-Host "delete obj and bin"
gci obj -recurse | foreach{ri $_.FullName -recurse -force }
gci bin -recurse | foreach{ri $_.FullName -recurse -force }
#for windows batch file 
#setx InterceptMethods "FullName"
#echo Environment variable InterceptMethods has been set to %InterceptMethods%
#put here the names of the methods you want to intercept , separated by ;
$env:InterceptMethods = "FullName;Test;PersonsLoaded;TestFullNameWithArguments;ShowRandomPersonNumber;Connect;SavePerson;InsertPerson"
Write-Host "Environment variable  $env:InterceptMethods  has been set to " $env:InterceptMethods
dotnet clean
dotnet restore
dotnet build /p:EmitCompilerGeneratedFiles=true --disable-build-servers --force
#debug only
# dotnet run --project RSCG_InterceptorTemplateConsole/RSCG_InterceptorTemplateConsole.csproj

```

Enjoy!
