namespace RSCG_InterceptorTemplateConsole;
internal class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName() => $"{FirstName} {LastName}";

    public string Test()
    {
        return FullName();
    }
}
