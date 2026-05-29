namespace Agility.Zoey.Web.Core.Modules.FileManager.Dto;

public record FilePageInput : PageInput
{
    public string? OriginalName { get; set; }
    public string? Category { get; set; }
    public long? FolderId { get; set; }
}

public record FileOutput
{
    public long Id { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public string? FileHash { get; set; }
    public string? Category { get; set; }
    public long? FolderId { get; set; }
    public string? FolderName { get; set; }
    public int StorageProvider { get; set; }
    public long UploadUserId { get; set; }
    public string? UploadUserName { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

public record AddFileFolderInput
{
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
}

public record UpdateFileFolderInput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
}

public record FileFolderOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public DateTime CreateTime { get; set; }
}

public record FileFolderTreeOutput
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public List<FileFolderTreeOutput> Children { get; set; } = new();
}