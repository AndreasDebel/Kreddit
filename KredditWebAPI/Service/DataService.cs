using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using Data;
using shared.Model;
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

        Post entry = db.Posts.FirstOrDefault()!;
        if (entry == null)
        {
            entry = new Entry("Lønseddel", "Her min lønseddel fra Januar 2025", DateTime.Now, "Karl24", 29);
            entry.CommentList.Add(new Comment("Hvor meget fik du udbetalt?", DateTime.Now.AddHours(-2), "PengeMand", 5));
            entry.CommentList.Add(new Comment("Fedt med åbenhed om løn!", DateTime.Now.AddHours(-1), "LønDebat", 12));

            var entry2 = new Entry("Ferierejse", "Se billeder fra min tur til Spanien", DateTime.Now.AddDays(-2), "TravelLover", 45);
            entry2.CommentList.Add(new Comment("Hvilket hotel boede I på?", DateTime.Now.AddDays(-1), "Rejsefan", 3));
            entry2.CommentList.Add(new Comment("Flotte billeder! Især stranden", DateTime.Now.AddDays(-1).AddHours(-6), "PhotoLover", 8));

            var entry3 = new Entry("Madopskrift", "Den bedste lasagne opskrift nogensinde", DateTime.Now.AddDays(-5), "ChefMaster", 122);
            entry3.CommentList.Add(new Comment("Hvilken type ost bruger du?", DateTime.Now.AddDays(-4), "OstElsker", 15));
            entry3.CommentList.Add(new Comment("Tak for opskriften, den prøver jeg i weekenden!", DateTime.Now.AddDays(-3), "Madglad", 7));
            
            db.Entries.Add(entry);
            db.Entries.Add(entry2);
            db.Entries.Add(entry3);
        }


        db.SaveChanges();
    }

    //public List<Book> GetBooks()
    //{
    //    return db.Books.Include(b => b.Author).ToList();
    //}

    //public Book GetBook(int id)
    //{
    //    return db.Books.Include(b => b.Author).FirstOrDefault(b => b.BookId == id);
    //}

    //public List<Author> GetAuthors()
    //{
    //    return db.Authors.ToList();
    //}

    //public Author GetAuthor(int id)
    //{
    //    return db.Authors.Include(a => a.Books).FirstOrDefault(a => a.AuthorId == id);
    //}

    //public string CreateBook(string title, int authorId)
    //{
    //    Author author = db.Authors.FirstOrDefault(a => a.AuthorId == authorId);
    //    db.Books.Add(new Book { Title = title, Author = author });
    //    db.SaveChanges();
    //    return "Book created";
    //}

}
