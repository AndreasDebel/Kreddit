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
        // Get all posts with their comments and users
        string url = $"{baseAPI}posts/";
        var posts = await http.GetFromJsonAsync<Post[]>(url);
        
        if (posts == null)
            return Array.Empty<User>();
            
        // Create a dictionary to store unique users by ID
        Dictionary<int, User> uniqueUsers = new Dictionary<int, User>();
        
        // Add post authors
        foreach (var post in posts)
        {
            if (post.User != null && post.User.Id > 0 && !uniqueUsers.ContainsKey(post.User.Id))
            {
                uniqueUsers[post.User.Id] = post.User;
            }
            
            // Add comment authors
            if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
                    if (comment.User != null && comment.User.Id > 0 && !uniqueUsers.ContainsKey(comment.User.Id))
                    {
                        uniqueUsers[comment.User.Id] = comment.User;
                    }
                }
            }
        }
        
        // If no users found, add a default user for testing
        if (uniqueUsers.Count == 0)
        {
            uniqueUsers[1] = new User { Id = 1, Username = "DefaultUser" };
        }
        
        // Return as array
        return uniqueUsers.Values.ToArray();
    }
}
