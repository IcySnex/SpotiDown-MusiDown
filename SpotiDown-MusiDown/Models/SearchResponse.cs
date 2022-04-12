namespace SpotiDown_MusiDown.Models;

public class SearchResponse
{
    public SearchResponse(IEnumerable<Song> Songs, bool HasMore, string NextToken)
    {
        this.Songs = Songs;
        this.HasMore = HasMore;
        this.NextToken = NextToken;
    }

    public IEnumerable<Song> Songs { get; set; }
    public bool HasMore { get; set; }
    public string NextToken { get; set; }
}