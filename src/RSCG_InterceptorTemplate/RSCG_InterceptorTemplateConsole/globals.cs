global using RSCG_InterceptorTemplateCommon;
global using RSCG_InterceptorTemplateConsole;

[assembly: InterceptClassMethodsAttribute<Person>("personTemplate",nameof(Person.FullName))]