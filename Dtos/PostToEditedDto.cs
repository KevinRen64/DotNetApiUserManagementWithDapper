namespace UserManagement.Dtos
{
  public class PostToEditedDto
  { 
    public int PostId { get; set; }
    public string PostTitle { get; set; } = "";
    public string PostContent { get; set; } = "";
  }
}