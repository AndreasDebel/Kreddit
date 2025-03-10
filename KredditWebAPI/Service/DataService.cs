using Microsoft.EntityFrameworkCore;
using Data;
using shared.Model;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Service;

public class DataService
{
    private PostContext db { get; }

    public DataService(PostContext db)
    {
        this.db = db;
    }

    // Class to deserialize the JSON seed data
    private class SeedDataModel
    {
        public List<PostSeedData> Posts { get; set; } = new List<PostSeedData>();
    }

    private class PostSeedData
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Username { get; set; } = "";
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public int DaysAgo { get; set; }
        public List<CommentSeedData> Comments { get; set; } = new List<CommentSeedData>();
    }

    private class CommentSeedData
    {
        public string Content { get; set; } = "";
        public string Username { get; set; } = "";
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }

    /// <summary>
    /// Seeds initial data in the database if necessary.
    /// </summary>
    public void SeedData()
    {
        // Check if database already has posts
        Post post = db.Posts.FirstOrDefault()!;
        if (post == null)
        {
            try
            {
                // Read the seed data from JSON file
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "seed-data.json");
                string jsonData = File.ReadAllText(jsonFilePath);
                
                // Deserialize the JSON data
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var seedData = JsonSerializer.Deserialize<SeedDataModel>(jsonData, options);
                
                if (seedData != null && seedData.Posts.Count > 0)
                {
                    // Create posts from seed data
                    foreach (var postData in seedData.Posts)
                    {
                        var user = new User(postData.Username);
                        var newPost = new Post(user, postData.Title, postData.Content)
                        {
                            Upvotes = postData.Upvotes,
                            Downvotes = postData.Downvotes,
                            CreatedAt = DateTime.Now.AddDays(-postData.DaysAgo)
                        };
                        
                        // Add comments to the post
                        foreach (var commentData in postData.Comments)
                        {
                            var commentUser = new User(commentData.Username);
                            var comment = new Comment(commentData.Content, commentData.Upvotes, commentData.Downvotes, commentUser)
                            {
                                CreatedAt = DateTime.Now.AddDays(-postData.DaysAgo + new Random().Next(0, postData.DaysAgo))
                            };
                            newPost.Comments.Add(comment);
                        }
                        
                        db.Posts.Add(newPost);
                    }
                    
                    db.SaveChanges();
                    Console.WriteLine($"Successfully seeded {seedData.Posts.Count} posts from JSON file.");
                }
                else
                {
                    Console.WriteLine("No seed data found in JSON file.");
                    SeedFallbackData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data from JSON: {ex.Message}");
                // If there's an error reading or parsing the JSON, use fallback data
                SeedFallbackData();
            }
        }
    }
    
    /// <summary>
    /// Seeds fallback data if JSON loading fails
    /// </summary>
    private void SeedFallbackData()
    {
        Console.WriteLine("Using fallback seed data.");
        var karl = new User("Karl24");
        var post1 = new Post(karl, "Lønseddel", "Her min lønseddel fra Januar 2025") { Upvotes = 29, CreatedAt = DateTime.Now.AddDays(-5) };
        post1.Comments.Add(new Comment("Hvor meget fik du udbetalt?", 5, 0, new User("PengeMand")));
        post1.Comments.Add(new Comment("Fedt med åbenhed om løn!", 12, 0, new User("LønDebat")));

        var traveler = new User("TravelLover");
        var post2 = new Post(traveler, "Ferierejse", "Se billeder fra min tur til Spanien") { Upvotes = 45, CreatedAt = DateTime.Now.AddDays(-3) };
        post2.Comments.Add(new Comment("Hvilket hotel boede I på?", 3, 0, new User("Rejsefan")));
        post2.Comments.Add(new Comment("Flotte billeder! Især stranden", 8, 0, new User("PhotoLover")));

        var chef = new User("ChefMaster");
        var post3 = new Post(chef, "Madopskrift", "Den bedste lasagne opskrift nogensinde") { Upvotes = 122, CreatedAt = DateTime.Now.AddDays(-1) };
        post3.Comments.Add(new Comment("Hvilken type ost bruger du?", 15, 0, new User("OstElsker")));
        post3.Comments.Add(new Comment("Tak for opskriften, den prøver jeg i weekenden!", 7, 0, new User("Madglad")));
        
        db.Posts.Add(post1);
        db.Posts.Add(post2);
        db.Posts.Add(post3);
        
        db.SaveChanges();
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
        Comment newComment = new Comment(content ?? "", 0, 0, user) { CreatedAt = DateTime.Now };

        // Add comment to post
        post.Comments.Add(newComment);

        db.SaveChanges();

        return newComment;
    }

    public Post CreatePost(string title, string content, int userId)
    {
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

        // Create new post
        Post newPost = new Post(user, title, content) { CreatedAt = DateTime.Now };

        // Add post to database
        db.Posts.Add(newPost);
        db.SaveChanges();

        return newPost;
    }

    public Post UpvotePost(int id)
    {
        // Find the post
        Post? post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == id);

        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        // Increment upvotes
        post.Upvotes++;

        // Save changes
        db.SaveChanges();

        return post;
    }

    public Post DownvotePost(int id)
    {
        // Find the post
        Post? post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == id);

        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        // Increment upvotes
        post.Downvotes++;

        // Save changes
        db.SaveChanges();

        return post;
    }

    public Comment UpvoteComment(int id)
    {
        // Find the post
        Comment? comment = db.Posts
            .SelectMany(p => p.Comments)
            .FirstOrDefault(c => c.Id == id);


        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {id} not found");
        }

        // Increment upvotes
        comment.Upvotes++;

        // Save changes
        db.SaveChanges();

        return comment;
    }

    public Comment DownvoteComment(int id)
    {
        // Find the post
        Comment? comment = db.Posts
            .SelectMany(p => p.Comments)
            .FirstOrDefault(c => c.Id == id);


        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {id} not found");
        }

        // Increment upvotes
        comment.Downvotes++;

        // Save changes
        db.SaveChanges();

        return comment;
    }
}
