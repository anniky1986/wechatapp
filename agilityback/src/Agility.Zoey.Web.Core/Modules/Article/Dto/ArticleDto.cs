namespace Agility.Zoey.Web.Core.Modules.Article.Dto;

public record ArticlePageInput : PageInput
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public int? Status { get; set; }
}

public record AddArticleInput
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Status { get; set; } = 1;
}

public record UpdateArticleInput
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Status { get; set; } = 1;
}

public record ArticleOutput
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Status { get; set; }
    public DateTime CreateTime { get; set; }
}