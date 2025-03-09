using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using shared.Model;

namespace kreddit_app.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI = "";

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        this.baseAPI = configuration["base_api"];
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        return await http.GetFromJsonAsync<Post[]>(url);
    }

    public async Task<Post> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        return await http.GetFromJsonAsync<Post>(url);
    }

    public async Task<Comment> CreateComment(string content, int postId, int userId)
    {
        string url = $"{baseAPI}posts/{postId}/comments";
     
        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { content, userId });

        // Get the JSON string from the response
        string json = msg.Content.ReadAsStringAsync().Result;

        // Deserialize the JSON string to a Comment object
        Comment? newComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties 
        });

        // Return the new comment 
        return newComment;
    }

    public async Task<Post> UpvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/upvote/";

        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");

        // Get the JSON string from the response
        string json = msg.Content.ReadAsStringAsync().Result;

        // Deserialize the JSON string to a Post object
        Post? updatedPost = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties
        });

        // Return the updated post (vote increased)
        return updatedPost;
    }

    public async Task<User[]> GetUsers()
    {
        try
        {
            // Try to get posts with users from API
            string url = $"{baseAPI}posts/";
            var posts = await http.GetFromJsonAsync<Post[]>(url);
            
            if (posts != null && posts.Length > 0)
            {
                // Create a dictionary to store unique users by ID
                Dictionary<int, User> uniqueUsers = new Dictionary<int, User>();
                
                // Add post authors
                foreach (var post in posts)
                {
                    // Debug output for each post
                    Console.WriteLine($"Processing post ID: {post.Id}, Title: {post.Title}");
                    
                    if (post.User != null)
                    {
                        Console.WriteLine($"Post author: ID={post.User.Id}, Username={post.User.Username}");
                        
                        if (post.User.Id > 0 && !uniqueUsers.ContainsKey(post.User.Id))
                        {
                            uniqueUsers[post.User.Id] = post.User;
                            Console.WriteLine($"Added user from post: {post.User.Id} - {post.User.Username}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Post {post.Id} has no user!");
                    }
                    
                    // Add comment authors
                    if (post.Comments != null)
                    {
                        Console.WriteLine($"Post {post.Id} has {post.Comments.Count} comments");
                        foreach (var comment in post.Comments)
                        {
                            if (comment.User != null)
                            {
                                Console.WriteLine($"Comment author: ID={comment.User.Id}, Username={comment.User.Username}");
                                
                                if (comment.User.Id > 0 && !uniqueUsers.ContainsKey(comment.User.Id))
                                {
                                    uniqueUsers[comment.User.Id] = comment.User;
                                    Console.WriteLine($"Added user from comment: {comment.User.Id} - {comment.User.Username}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Comment {comment.Id} has no user!");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Post {post.Id} has no comments collection!");
                    }
                }
                
                // Return the users we found
                Console.WriteLine($"Found {uniqueUsers.Count} users from database");
                foreach (var user in uniqueUsers.Values)
                {
                    Console.WriteLine($"Final user list: {user.Id} - {user.Username}");
                }
                return uniqueUsers.Values.ToArray();
            }
            
            // If we couldn't get any users, return an empty array
            Console.WriteLine("No posts found in database");
            return Array.Empty<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUsers: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Return an empty array in case of error
            return Array.Empty<User>();
        }
    }
}
