using Microsoft.EntityFrameworkCore;
using Data;
using shared.Model;
using Microsoft.Extensions.Hosting;
namespace Service;

public class DataService
{
    private PostContext db { get; }

    public DataService(PostContext db)
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

    public List<Post> GetPosts()
    {
        return db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .ToList();
    }

    public Post GetPost(int id)
    {
        var post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User) // include user for each comment
            .AsNoTracking() // Use AsNoTracking for better performance in read-only scenarios
            .FirstOrDefault(p => p.Id == id);

        return post == null ? throw new KeyNotFoundException($"Post with ID {id} not found") : post;
    }

    public Comment CreateComment(string? content, int postId, int userId)
    {
        // Find the post
        Post? post = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {postId} not found");
        }
        
        // Find the existing user
        // First try locating user in Posts
        User? user = db.Posts.Where(p => p.User.Id == userId)
                             .Select(p => p.User)
                             .FirstOrDefault();

        // If not found in posts, try to find in comments
        if (user == null)
        {
            user = db.Posts.SelectMany(p => p.Comments)
                           .Where(c => c.User.Id == userId)
                           .Select(c => c.User)
                           .FirstOrDefault();
        }

        // Else throw exception
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Create new comment
        Comment newComment = new Comment(content ?? "", 0, 0, user);

        // Add comment to post
        post.Comments.Add(newComment);

        db.SaveChanges();

        return newComment;


    }

}
