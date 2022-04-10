namespace SpotiDown_MusiDown.Models;

public class Song
{
    public Song(string Id, string Name, string Author, int Year, string ThumbUrl, string CoverUrl, long Views, bool Gold, bool Downloadable, bool HasPreview)
    {
        this.Id = Id;
        this.Name = Name;
        this.Author = Author;
        this.Year = Year;
        this.ThumbUrl = ThumbUrl;
        this.CoverUrl = CoverUrl;
        this.Views = Views;
        this.Gold = Gold;
        this.Downloadable = Downloadable;
        this.HasPreview = HasPreview;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
    public string ThumbUrl { get; set; }
    public string CoverUrl { get; set; }
    public long Views { get; set; }
    public bool Gold { get; set; }
    public bool Downloadable { get; set; }
    public bool HasPreview { get; set; }
}