using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using Data;
using Model;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;

namespace KredditAPI.Service;

public class DataService
{
    private EntryContext db { get; }

    public DataService(EntryContext db)
    {
        this.db = db;
    }
    /// <summary>
    /// Seeder noget nyt data i databasen hvis det er nødvendigt.
    /// </summary>
    public void SeedData()
    {

        Entry entry = db.Entries.FirstOrDefault()!;
        if (entry == null)
        {
            //entry = new Entry { Titel = "Lønseddel", Author = "Karl24", Date = DateTime.Now, Votes = 29, Text = "Her min lønseddel fra Januar 2025" };

            entry = Entry("Lønseddel", "Her min lønseddel fra Januar 2025", DateTime.Now, "Karl24", 29);
            db.Entries.Add(entry);
            //db.Entries.Add(new Entry { Titel = "" });
            //db.Entries.Add(new Entry { Fullname = "Mette" });
        }

        Book book = db.Books.FirstOrDefault()!;
        if (book == null)
        {
            db.Books.Add(new Book { Title = "Harry Potter", Author = entry });
            db.Books.Add(new Book { Title = "Ringenes Herre", Author = entry });
            db.Books.Add(new Book { Title = "Entity Framework for Dummies", Author = entry });
        }

        db.SaveChanges();
    }

    public List<Book> GetBooks()
    {
        return db.Books.Include(b => b.Author).ToList();
    }

    public Book GetBook(int id)
    {
        return db.Books.Include(b => b.Author).FirstOrDefault(b => b.BookId == id);
    }

    public List<Author> GetAuthors()
    {
        return db.Authors.ToList();
    }

    public Author GetAuthor(int id)
    {
        return db.Authors.Include(a => a.Books).FirstOrDefault(a => a.AuthorId == id);
    }

    public string CreateBook(string title, int authorId)
    {
        Author author = db.Authors.FirstOrDefault(a => a.AuthorId == authorId);
        db.Books.Add(new Book { Title = title, Author = author });
        db.SaveChanges();
        return "Book created";
    }

}