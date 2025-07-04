using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using UserManagement.Data;
using UserManagement.Dtos;
using UserManagement.Models;

namespace UserManagement.Controllers
{
  // Controller for managing post-related API actions
  [Authorize]   
  [ApiController]  
  [Route("[controller]")]    // Base route: /Post 

  public class PostController : ControllerBase
  {
    private readonly DataContextDapper _dapper;

    // Constructor: initializes Dapper data context with app configuration
    public PostController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
    }


    // ========================================
    // GET /Post/Posts
    // Retrieve all posts from the database
    // ========================================
    [HttpGet("Posts")]
    public IEnumerable<Post> GetPosts()
    {
      string sql = @"SELECT [PostId],
              [UserId],
              [PostTitle],
              [PostContent],
              [PostCreated],
              [PostUpdated] 
          FROM TutorialAppSchema.Posts";

      return _dapper.LoadData<Post>(sql);
    }


    // =========================================
    // GET /Post/PostSingle/{postId}
    // Retrieve a single post by its ID
    // =========================================
    [HttpGet("PostSingle/{postId}")]
    public Post GetPostSingle(int postId)
    {
      string sql = @"SELECT [PostId],
              [UserId],
              [PostTitle],
              [PostContent],
              [PostCreated],
              [PostUpdated] 
          FROM TutorialAppSchema.Posts
          WHERE PostId = @PostId";
      var postIdParam = new { PostId = postId };
      return _dapper.LoadDataSingleWithParameters<Post>(sql, postIdParam);
    }


    // ==========================================
    // GET /Post/PostByUser/{userId}
    // Retrieve all posts made by a specific user
    // ==========================================
    [HttpGet("PostByUser/{userId}")]
    public IEnumerable<Post> GetPostByUser(int userId)
    {
      string sql = @"SELECT [PostId],
              [UserId],
              [PostTitle],
              [PostContent],
              [PostCreated],
              [PostUpdated] 
          FROM TutorialAppSchema.Posts
          WHERE UserId = @UserId";
      var userIdParam = new { UserId = userId };
      return _dapper.LoadDataWithParameters<Post>(sql, userIdParam);
    }


    // ======================================
    // GET /Post/MyPosts
    // Retrieve posts by the current user
    // ======================================
    [HttpGet("MyPosts")]
    public IEnumerable<Post> MyPosts()
    {
      string sql = @"SELECT [PostId],
              [UserId],
              [PostTitle],
              [PostContent],
              [PostCreated],
              [PostUpdated] 
          FROM TutorialAppSchema.Posts
          WHERE UserId = @UserId";
      var userIdParam = new { UserId = this.User.FindFirst("userId")?.Value };
      return _dapper.LoadDataWithParameters<Post>(sql, userIdParam);
    }


    // =====================================
    // POST /Post/Post
    // Create a new post
    // =====================================
    [HttpPost("Post")]
    public IActionResult AddPost(PostToAddDto postToAdd)
    {
      string sql = @"
      INSERT INTO TutorialAppSchema.Posts (
          [UserId],
          [PostTitle],
          [PostContent],
          [PostCreated],
          [PostUpdated] 
          ) VALUES (
          @UserId,
          @PostTitle,
          @PostContent,
          GETDATE(),
          GETDATE()
          )";

      var postParameter = new List<SqlParameter>
      {
        new SqlParameter("@UserId", SqlDbType.Int) { Value = this.User.FindFirst("userId")?.Value},
        new SqlParameter("@PostTitle", SqlDbType.NVarChar) { Value = postToAdd.PostTitle },
        new SqlParameter("@PostContent", SqlDbType.NVarChar) { Value = postToAdd.PostContent },
      };

      if (_dapper.ExecuteSqlWithParameters(sql, postParameter))
      {
        return Ok();
      }
      throw new Exception("Failed to crate new post!");
    }


    // =====================================
    // PUT /Post/Post
    // Update an existing post (must be user's own)
    // =====================================
    [HttpPut("Post")]
    public IActionResult EditPost(PostToEditedDto postToEdit)
    {

      string sql = @"
        UPDATE TutorialAppSchema.Posts 
        SET [PostTitle] = @PostTitle,
            [PostContent] = PostContent,
            [PostUpdated] = GETDATE()
        WHERE PostId = @PostId AND UserId = @UserId";   // Ensure user can only edit their own post

      var postParameter = new List<SqlParameter>
      {
        new SqlParameter("@PostTitle", SqlDbType.NVarChar) { Value = postToEdit.PostTitle},
        new SqlParameter("@PostContent", SqlDbType.NVarChar) { Value = postToEdit.PostContent },
        new SqlParameter("@PostId", SqlDbType.Int) { Value = postToEdit.PostId },
        new SqlParameter("@UserId", SqlDbType.Int) { Value = this.User.FindFirst("userId")?.Value},
      };

      if (_dapper.ExecuteSqlWithParameters(sql, postParameter))
      {
        return Ok();
      }
      throw new Exception("Failed to edit Post");
    }


    // ======================================
    // DELETE /Post/Delete/{postId}
    // Delete a post (must be user's own)
    // ======================================
    [HttpDelete("Delete/{postId}")]
    public IActionResult RemoveUser(int postId)
    {
      string sql = @"
      DELETE FROM TutorialAppSchema.Posts
      WHERE PostId = @PostId AND UserId = @UserId";  // Prevent users from deleting others' posts

      var postParameter = new List<SqlParameter>
      {
        new SqlParameter("@PostId", SqlDbType.Int) { Value = postId},
        new SqlParameter("@UserId", SqlDbType.Int) { Value = this.User.FindFirst("userId")?.Value},
      };

      if (_dapper.ExecuteSqlWithParameters(sql, postParameter))
      {
        return Ok();
      }
      throw new Exception("Failed to delete Post");
    }
  

    // =================================================
    // GET /Post/PostsBySearch/{searchParam}
    // Search for posts by title or content
    // =================================================
    [HttpGet("PostsBySearch/{searchParam}")]
    public IEnumerable<Post> PostsBySearch(string searchParam)
    {
      string sql = @"SELECT [PostId],
              [UserId],
              [PostTitle],
              [PostContent],
              [PostCreated],
              [PostUpdated] 
          FROM TutorialAppSchema.Posts
              WHERE PostTitle LIKE @Search
              OR PostContent LIKE @Search";

      var param = new { Search = "%" + searchParam + "%"};   // Add wildcard for partial match
      return _dapper.LoadDataWithParameters<Post>(sql, param);
    }
  }
}