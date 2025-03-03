// Hej Sigurd
using Model;

using (var db = new EntryContext())
{
    Console.WriteLine($"Database path: {db.DbPath}.");

    Console.WriteLine("Indsæt opslag");
    db.Add(new Entry("")
}