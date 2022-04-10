namespace SpotiDown_MusiDown.Models;

public class SearchResponse
{
    public SearchResponse(List<Song> Songs, bool HasMore, string NextToken)
    {
        this.Songs = Songs;
        this.HasMore = HasMore;
        this.NextToken = NextToken;
    }

    public List<Song> Songs { get; set; }
    public bool HasMore { get; set; }
    public string NextToken { get; set; }
}