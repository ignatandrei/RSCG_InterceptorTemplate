namespace RSCG_DemoObjects;

public class Person
{
    static int nrPersons = 0;
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
    public static int ShowRandomPersonNumber(int min)
    {
        return Random.Shared.Next(min, nrPersons);
    }
}
