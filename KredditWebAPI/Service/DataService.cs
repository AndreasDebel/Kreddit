using Microsoft.EntityFrameworkCore;
using Data;
using shared.Model;

namespace KredditAPI.Service;

public class DataService
{
    private EntryContext db { get; }

    public DataService(EntryContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Seeds initial data in the database if necessary.
    /// </summary>
    public void SeedData()
    {
        Post post = db.Posts.FirstOrDefault()!;
        if (post == null)
        {
            var karl = new User("Karl24");
            post = new Post(karl, "Lønseddel", "Her min lønseddel fra Januar 2025") { Upvotes = 29 };
            post.Comments.Add(new Comment("Hvor meget fik du udbetalt?", 5, 0, new User("PengeMand")));
            post.Comments.Add(new Comment("Fedt med åbenhed om løn!", 12, 0, new User("LønDebat")));

            var traveler = new User("TravelLover");
            var post2 = new Post(traveler, "Ferierejse", "Se billeder fra min tur til Spanien") { Upvotes = 45 };
            post2.Comments.Add(new Comment("Hvilket hotel boede I på?", 3, 0, new User("Rejsefan")));
            post2.Comments.Add(new Comment("Flotte billeder! Især stranden", 8, 0, new User("PhotoLover")));

            var chef = new User("ChefMaster");
            var post3 = new Post(chef, "Madopskrift", "Den bedste lasagne opskrift nogensinde") { Upvotes = 122 };
            post3.Comments.Add(new Comment("Hvilken type ost bruger du?", 15, 0, new User("OstElsker")));
            post3.Comments.Add(new Comment("Tak for opskriften, den prøver jeg i weekenden!", 7, 0, new User("Madglad")));
            
            db.Posts.Add(post);
            db.Posts.Add(post2);
            db.Posts.Add(post3);
            
            db.SaveChanges();
        }
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
