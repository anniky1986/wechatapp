using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.Article.Dto;
using Agility.Zoey.Web.Core.Shared.Consts;
using Furion.DynamicApiController;
using Furion.FriendlyException;

namespace Agility.Zoey.Web.Core.Modules.Article.Services;

[ApiDescriptionSettings(Group = "Article", Order = 300)]
public class ArticleService : IDynamicApiController, ITransient
{
    private readonly IRepository<Article> _articleRepo;

    public ArticleService(IRepository<Article> articleRepo)
    {
        _articleRepo = articleRepo;
    }

    [HttpGet("api/article/page")]
    public async Task<PageResult<ArticleOutput>> GetPage(ArticlePageInput input)
    {
        var query = _articleRepo.AsQueryable()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(input.Title))
        {
            query = query.Where(a => a.Title.Contains(input.Title));
        }

        if (!string.IsNullOrEmpty(input.Category))
        {
            query = query.Where(a => a.Category == input.Category);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(a => a.Status == input.Status.Value);
        }

        var page = input.Page < 1 ? 1 : input.Page;
        var pageSize = input.PageSize < 1 ? SystemConsts.DefaultPageSize : input.PageSize;
        if (pageSize > SystemConsts.MaxPageSize) pageSize = SystemConsts.MaxPageSize;

        if (!string.IsNullOrEmpty(input.SortField) && !string.IsNullOrEmpty(input.SortOrder))
        {
            var isAsc = input.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);
            query = query.OrderBy($"{input.SortField} {(isAsc ? "ASC" : "DESC")}");
        }
        else
        {
            query = query.OrderByDescending(a => a.CreateTime);
        }

        var totalCount = await query.CountAsync();
        var articles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PageResult<ArticleOutput>
        {
            Total = totalCount,
            Items = articles.Select(a => new ArticleOutput
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                Category = a.Category,
                Status = a.Status,
                CreateTime = a.CreateTime
            }).ToList()
        };
    }

    [HttpGet("api/article/{id}")]
    public async Task<ArticleOutput> Get(long id)
    {
        var article = await _articleRepo.GetByIdAsync(id);
        if (article == null || article.IsDeleted)
        {
            throw Oops.Oh("文章不存在");
        }

        return new ArticleOutput
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Category = article.Category,
            Status = article.Status,
            CreateTime = article.CreateTime
        };
    }

    [HttpPost("api/article")]
    public async Task<ArticleOutput> Add(AddArticleInput input)
    {
        var article = new Article
        {
            Title = input.Title,
            Content = input.Content,
            Category = input.Category,
            Status = input.Status,
            TenantId = 0,
            CreateTime = DateTime.Now,
            CreateUserId = 0
        };

        article = await _articleRepo.InsertAsync(article);
        return await Get(article.Id);
    }

    [HttpPut("api/article/{id}")]
    public async Task<ArticleOutput> Update(long id, UpdateArticleInput input)
    {
        var article = await _articleRepo.GetByIdAsync(id);
        if (article == null || article.IsDeleted)
        {
            throw Oops.Oh("文章不存在");
        }

        article.Title = input.Title;
        article.Content = input.Content;
        article.Category = input.Category;
        article.Status = input.Status;
        article.UpdateTime = DateTime.Now;
        article.UpdateUserId = 0;

        await _articleRepo.UpdateAsync(article);
        return await Get(id);
    }

    [HttpDelete("api/article/{id}")]
    public async Task Delete(long id)
    {
        var article = await _articleRepo.GetByIdAsync(id);
        if (article == null || article.IsDeleted)
        {
            throw Oops.Oh("文章不存在");
        }

        await _articleRepo.SoftDeleteAsync(id);
    }
}