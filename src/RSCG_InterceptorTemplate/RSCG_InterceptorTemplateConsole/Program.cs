﻿;

Console.WriteLine("Hello, World!");
var p=new Person();
p.FirstName="Andrei";
p.LastName="Ignat";
Console.WriteLine("debug for "+p.FullName());
Console.WriteLine("this is "+p.FullName());
var x = p.Test();
Console.WriteLine(x);
var newPerson = new Person();
newPerson.FirstName = "Andrei";
newPerson.LastName = "Ignat";
var namePerson = newPerson.FullName();
Console.WriteLine(namePerson);

Console.WriteLine("loaded "+Person.PersonsLoaded());
Console.WriteLine("loaded " + RSCG_DemoObjects.Person.PersonsLoaded());
Console.WriteLine("and again  " + RSCG_DemoObjects.Person.PersonsLoaded());

Console.WriteLine("and now with argument " + newPerson.FullNameWithSeparator("!+"));
Console.WriteLine("and a random person " + Person.ShowRandomPersonNumber(1));

Console.ReadLine();