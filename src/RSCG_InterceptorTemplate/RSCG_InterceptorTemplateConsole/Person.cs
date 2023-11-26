namespace RSCG_InterceptorTemplateConsole;
//internal class PersonLoader
//{
//    public Task<Person[]> GetPersonsFromFile()
//}
internal class Person
{
    static int nrPersons= 0;
    public Person()
    {
        nrPersons++;
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName() => $"{FirstName} {LastName}";

    public string FullNameWithSeparator(string separator) => $"{FirstName}{separator}{LastName}";
    public string Test()
    {
        return FullName();
    }

    public static int PersonsLoaded()
    {
        return nrPersons;
    }

}
