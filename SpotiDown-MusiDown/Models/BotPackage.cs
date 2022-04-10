namespace SpotiDown_MusiDown.Models;

public class BotPackage
{
    public BotPackage(string Identifier, string Name, string Version, string Description, string EndPoint, string SearchMethod, string DownloadMethod, string PreviewMethod, string SearchOptionalParams, string DownloadOptionalParams, string PreviewOptionalParams)
    {
        this.Identifier = Identifier;
        this.Name = Name;
        this.Version = Version;
        this.Description = Description;

        this.EndPoint = EndPoint;

        this.SearchMethod = SearchMethod;
        this.DownloadMethod = DownloadMethod;
        this.PreviewMethod = PreviewMethod;

        this.SearchOptionalParams = SearchOptionalParams;
        this.DownloadOptionalParams = DownloadOptionalParams;
        this.PreviewOptionalParams = PreviewOptionalParams;
    }

    public string Identifier { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    
    public string EndPoint { get; set; }
    
    public string SearchMethod { get; set; }
    public string DownloadMethod { get; set; }
    public string PreviewMethod { get; set; }
    
    public string SearchOptionalParams { get; set; }
    public string DownloadOptionalParams { get; set; } 
    public string PreviewOptionalParams { get; set; }
}