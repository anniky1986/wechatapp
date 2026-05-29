# Agility.Zoey 新增模块开发规范文档

> 版本：v1.0 | 适用项目：Agility.Zoey 权限管理系统 | 前后端分离架构

---

## 目录

- [一、概述](#一概述)
- [二、后端模块开发规范](#二后端模块开发规范)
  - [2.1 项目分层结构](#21-项目分层结构)
  - [2.2 实体创建规范](#22-实体创建规范)
  - [2.3 接口与仓储规范](#23-接口与仓储规范)
  - [2.4 DTO 规范](#24-dto-规范)
  - [2.5 动态 API 服务规范](#25-动态-api-服务规范)
  - [2.6 权限验证与集成](#26-权限验证与集成)
  - [2.7 缓存规范](#27-缓存规范)
  - [2.8 日志规范](#28-日志规范)
  - [2.9 配置规范](#29-配置规范)
  - [2.10 校验器规范](#210-校验器规范)
  - [2.11 事务规范](#211-事务规范)
  - [2.12 数据库唯一性约束](#212-数据库唯一性约束)
  - [2.13 完整新增模块示例（后端）](#213-完整新增模块示例后端)
- [三、前端模块开发规范](#三前端模块开发规范)
  - [3.1 项目目录结构](#31-项目目录结构)
  - [3.2 类型定义规范](#32-类型定义规范)
  - [3.3 API 层规范](#33-api-层规范)
  - [3.4 路由规范](#34-路由规范)
  - [3.5 Store 状态管理规范](#35-store-状态管理规范)
  - [3.6 权限验证与集成](#36-权限验证与集成-1)
  - [3.7 页面布局统一性](#37-页面布局统一性)
  - [3.8 表单风格与布局](#38-表单风格与布局)
  - [3.9 弹出页面标准](#39-弹出页面标准)
  - [3.10 提示规范](#310-提示规范)
  - [3.11 完整新增模块示例（前端）](#311-完整新增模块示例前端)

---

## 一、概述

### 1.1 文档目的

本文档定义了 Agility.Zoey 权限管理系统中**新增业务模块**的完整开发规范。涵盖后端（.NET 9 + Furion + SqlSugar）和前端（Vue 3 + TypeScript + Ant Design Vue）的全链路开发标准，确保新增模块在代码风格、权限集成、缓存策略、日志记录、UI 交互等方面保持统一。

### 1.2 适用场景

- 新增独立的业务模块（如：内容管理、订单管理、报表中心）
- 扩展现有模块的子功能（如：用户管理下新增用户组功能）
- 第三方团队基于本项目进行二次开发

### 1.3 核心原则

| 原则 | 说明 |
|------|------|
| **约定优于配置** | 遵循项目既定目录结构、命名规范，Furion 自动扫描 DI |
| **模块文件夹隔离** | 每个业务模块独立目录，包含 Services/Dto/Validators |
| **权限码贯穿** | 每个 API 方法绑定权限码，前后端双重校验 |
| **事务保证完整** | 多步写操作必须包裹在事务中 |
| **缓存降级可用** | 优先 Redis 分布式缓存，自动降级内存缓存 |
| **前端风格统一** | 所有列表页、表单弹窗遵循相同的 UI 模板 |

---

## 二、后端模块开发规范

### 2.1 项目分层结构

```
Agility.Zoey.Web.Core/
└── Modules/
    └── {ModuleName}/           ← 新模块根目录
        ├── Services/            ← 动态 API 服务类
        │   └── XxxService.cs
        ├── Dto/                 ← 数据传输对象
        │   └── XxxDto.cs
        └── Validators/          ← FluentValidation 校验器
            └── XxxValidator.cs
```

```
Agility.Zoey.Core/
└── Entities/
    └── {Entity}.cs             ← 数据库实体
```

```
Agility.Zoey.Data/
└── DbContext/
    └── SqlSugarDbContext.cs    ← 在此注册新实体表
```

### 2.2 实体创建规范

#### 2.2.1 基类继承规则

```csharp
// 标准业务实体：继承 BaseEntity + ITenant（多租户需要） + ISoftDelete（软删除需要）
[SugarTable("Sys_Xxx")]
[SugarIndex("unique_tenant_name", nameof(TenantId), OrderByType.Asc, nameof(Name), OrderByType.Asc, true)]
public class Xxx : BaseEntity, ITenant, ISoftDelete
{
    // 业务字段
}

// 关联表实体：不需要继承 BaseEntity，使用联合主键
[SugarTable("Sys_XxxYyy")]
public class XxxYyy
{
    [SugarColumn(IsPrimaryKey = true)]
    public long XxxId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public long YyyId { get; set; }
}
```

#### 2.2.2 字段规范

| 要求 | 说明 |
|------|------|
| 主键 | 统一使用 `long Id`，`SugarColumn(IsPrimaryKey = true, IsIdentity = true)` |
| 多租户 | 实现 `ITenant` 接口，必须有 `long TenantId { get; set; }` |
| 软删除 | 实现 `ISoftDelete` 接口，必须有 `bool IsDeleted { get; set; }` |
| 字符串 | 使用 `[MaxLength]` 限制长度，避免 `nvarchar(max)` 滥用 |
| 可空引用 | 使用 `string?` 明确可空，非必填字段用 `?` |
| 默认值 | 数值型状态字段默认 `1`（启用/正常） |
| 数据注解 | 必填字段用 `[Required]`，邮箱用 `[EmailAddress]`，手机号用 `[Phone]` |

#### 2.2.3 唯一索引规范

对有唯一性要求的字段组合，使用类级别的 `[SugarIndex]` 属性：

```csharp
// 单字段唯一（全局）
[SugarIndex("unique_code", nameof(Code), OrderByType.Asc, true)]
public class Tenant : BaseEntity

// 组合唯一（租户内唯一）
[SugarIndex("unique_tenant_name", nameof(TenantId), OrderByType.Asc, nameof(Name), OrderByType.Asc, true)]
[SugarIndex("unique_tenant_email", nameof(TenantId), OrderByType.Asc, nameof(Email), OrderByType.Asc, true)]
public class User : BaseEntity, ITenant, ISoftDelete
```

> `SugarIndex` 的最后一个参数 `true` 表示 `IsUnique = true`。

#### 2.2.4 完整实体示例

```csharp
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using Agility.Zoey.Core.Interfaces;

namespace Agility.Zoey.Core.Entities;

[SugarTable("Sys_Notice")]
[SugarIndex("unique_tenant_title", nameof(TenantId), OrderByType.Asc, nameof(Title), OrderByType.Asc, true)]
public class Notice : BaseEntity, ITenant, ISoftDelete
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Type { get; set; }

    public int Status { get; set; } = 1;

    public DateTime? PublishTime { get; set; }

    public long TenantId { get; set; }

    public bool IsDeleted { get; set; }
}
```

#### 2.2.5 注册到 SqlSugarDbContext

在 [SqlSugarDbContext.cs](file:///workspace/agilityback/src/Agility.Zoey.Data/DbContext/SqlSugarDbContext.cs) 的 `EntityTypes` 数组中添加新实体：

```csharp
private static readonly Type[] EntityTypes =
{
    typeof(User),
    typeof(Role),
    // ... 已有实体 ...
    typeof(Notice),     // ← 新增
};
```

### 2.3 接口与仓储规范

#### 2.3.1 仓储注入

所有服务通过构造函数注入 `IRepository<T>`，**不需要手动在 ServiceCollectionExtensions 中注册**：

```csharp
public class NoticeService : IDynamicApiController, ITransient
{
    private readonly IRepository<Notice> _noticeRepo;

    public NoticeService(IRepository<Notice> noticeRepo)
    {
        _noticeRepo = noticeRepo;
    }
}
```

> Furion 通过 `ITransient` 接口自动注册服务，`IRepository<T>` 通过 `IScoped` 自动注册。

#### 2.3.2 常用仓储方法

| 方法 | 说明 |
|------|------|
| `_repo.GetByIdAsync(id)` | 按主键查询单条 |
| `_repo.AsQueryable()` | 获取可查询对象（支持链式 Where/OrderBy/Skip/Take） |
| `_repo.InsertAsync(entity)` | 插入单条，返回带自增ID的实体 |
| `_repo.InsertRangeAsync(list)` | 批量插入 |
| `_repo.UpdateAsync(entity)` | 更新实体（按主键匹配） |
| `_repo.DeleteAsync(id)` | 物理删除 |
| `_repo.SoftDeleteAsync(id)` | 软删除（设置 IsDeleted=true） |
| `_repo.CountAsync(expr)` | 计数 |
| `_repo.BeginTranAsync()` | 开启事务 |
| `_repo.CommitTranAsync()` | 提交事务 |
| `_repo.RollbackTranAsync()` | 回滚事务 |

### 2.4 DTO 规范

#### 2.4.1 命名与继承规则

| DTO 类型 | 命名 | 继承 |
|----------|------|------|
| 分页查询入参 | `XxxPageInput` | `: PageInput` |
| 新增入参 | `AddXxxInput` | 无 |
| 更新入参 | `UpdateXxxInput` | 无（需包含 `Id` 字段） |
| 返回结果 | `XxxOutput` | 无 |

#### 2.4.2 完整 DTO 示例

```csharp
namespace Agility.Zoey.Web.Core.Modules.Notice.Dto;

public record NoticePageInput : PageInput
{
    public string? Title { get; set; }
    public string? Type { get; set; }
    public int? Status { get; set; }
}

public record AddNoticeInput
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Status { get; set; } = 1;
    public DateTime? PublishTime { get; set; }
}

public record UpdateNoticeInput
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Status { get; set; } = 1;
    public DateTime? PublishTime { get; set; }
}

public record NoticeOutput
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Status { get; set; }
    public DateTime? PublishTime { get; set; }
    public DateTime CreateTime { get; set; }
}
```

> **强制使用 `record` 类型**，不可使用 `class`。`PageInput` 和 `PageResult<T>` 已在 [CommonDto.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Modules/System/Dto/CommonDto.cs) 中定义。

### 2.5 动态 API 服务规范

#### 2.5.1 基本模板

```csharp
[ApiDescriptionSettings(Group = "Notice", Order = 500)]
public class NoticeService : IDynamicApiController, ITransient
{
    // 实现必须同时实现 IDynamicApiController 和 ITransient
}
```

| 部分 | 说明 |
|------|------|
| `Group` | API 文档分组名，对应前端模块名 |
| `Order` | 分组排序，建议按业务域分段（System=100, FileManager=200, Article=300, 自定义=500+） |
| `IDynamicApiController` | Furion 自动将 public 方法转为 RESTful API |
| `ITransient` | Furion 自动以 Transient 生命周期注册 |

#### 2.5.2 API 路由规范

| 方法 | 路由格式 | HTTP 方法 | 示例 |
|------|----------|-----------|------|
| 分页查询 | `api/{module}/page` | GET | `api/notice/page` |
| 单条查询 | `api/{module}/{id}` | GET | `api/notice/5` |
| 新增 | `api/{module}` | POST | `api/notice` |
| 更新 | `api/{module}/{id}` | PUT | `api/notice/5` |
| 删除 | `api/{module}/{id}` | DELETE | `api/notice/5` |
| 批量操作 | `api/{module}/batch` | POST/DELETE | `api/notice/batch` |

#### 2.5.3 分页查询完整代码

```csharp
[HttpGet("api/notice/page")]
[Permission("notice:view")]
public async Task<PageResult<NoticeOutput>> GetPage(NoticePageInput input)
{
    var query = _noticeRepo.AsQueryable()
        .Where(n => !n.IsDeleted);

    if (!string.IsNullOrEmpty(input.Title))
        query = query.Where(n => n.Title.Contains(input.Title));
    if (!string.IsNullOrEmpty(input.Type))
        query = query.Where(n => n.Type == input.Type);
    if (input.Status.HasValue)
        query = query.Where(n => n.Status == input.Status.Value);

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
        query = query.OrderByDescending(n => n.CreateTime);
    }

    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    return new PageResult<NoticeOutput>
    {
        Total = total,
        Items = items.Select(n => new NoticeOutput
        {
            Id = n.Id, Title = n.Title, Content = n.Content,
            Type = n.Type, Status = n.Status,
            PublishTime = n.PublishTime, CreateTime = n.CreateTime
        }).ToList()
    };
}
```

#### 2.5.4 新增方法（含唯一性检查）

```csharp
[HttpPost("api/notice")]
[Permission("notice:add")]
public async Task<NoticeOutput> Add(AddNoticeInput input)
{
    // 唯一性检查（组合唯一）
    var exists = await _noticeRepo.CountAsync(n =>
        n.Title == input.Title && !n.IsDeleted);
    if (exists > 0)
        throw Oops.Oh("公告标题已存在");

    var entity = new Notice
    {
        Title = input.Title,
        Content = input.Content,
        Type = input.Type,
        Status = input.Status,
        PublishTime = input.PublishTime,
        TenantId = _currentUser?.TenantId ?? 0,
        CreateTime = DateTime.Now,
        CreateUserId = _currentUser?.UserId ?? 0
    };

    entity = await _noticeRepo.InsertAsync(entity);
    return await Get(entity.Id);
}
```

#### 2.5.5 更新方法（含事务）

```csharp
[HttpPut("api/notice/{id}")]
[Permission("notice:edit")]
public async Task<NoticeOutput> Update(long id, UpdateNoticeInput input)
{
    var entity = await _noticeRepo.GetByIdAsync(id);
    if (entity == null || entity.IsDeleted)
        throw Oops.Oh("公告不存在");

    // 唯一性检查：排除自身
    var exists = await _noticeRepo.CountAsync(n =>
        n.Title == input.Title && n.Id != id && !n.IsDeleted);
    if (exists > 0)
        throw Oops.Oh("公告标题已存在");

    entity.Title = input.Title;
    entity.Content = input.Content;
    entity.Type = input.Type;
    entity.Status = input.Status;
    entity.PublishTime = input.PublishTime;
    entity.UpdateTime = DateTime.Now;
    entity.UpdateUserId = _currentUser?.UserId ?? 0;

    await _noticeRepo.UpdateAsync(entity);
    return await Get(id);
}
```

#### 2.5.6 删除方法

```csharp
[HttpDelete("api/notice/{id}")]
[Permission("notice:delete")]
public async Task Delete(long id)
{
    var entity = await _noticeRepo.GetByIdAsync(id);
    if (entity == null || entity.IsDeleted)
        throw Oops.Oh("公告不存在");

    await _noticeRepo.SoftDeleteAsync(id);
}
```

#### 2.5.7 错误处理规范

- 业务异常：使用 `throw Oops.Oh("中文错误信息")` （Furion 友好异常）
- 所有异常由 [GlobalExceptionFilter.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Filters/GlobalExceptionFilter.cs) 统一捕获返回
- 不要 `try-catch` 后吞掉异常，除非是需要回滚事务的场景

### 2.6 权限验证与集成

#### 2.6.1 权限码命名规范

```
{模块}:{动作}
```

| 模块 | 示例 |
|------|------|
| 查看 | `notice:view` |
| 新增 | `notice:add` |
| 编辑 | `notice:edit` |
| 删除 | `notice:delete` |
| 导出 | `notice:export` |
| 审核 | `notice:approve` |

#### 2.6.2 定义权限常量

在 [PermissionConsts.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Shared/Consts/PermissionConsts.cs) 中追加：

```csharp
public static class PermissionConsts
{
    // ... 已有权限 ...

    // Notice 模块
    public const string NoticeView = "notice:view";
    public const string NoticeAdd = "notice:add";
    public const string NoticeEdit = "notice:edit";
    public const string NoticeDelete = "notice:delete";
    public const string NoticeApprove = "notice:approve";
}
```

#### 2.6.3 在 API 方法上添加权限

```csharp
[HttpGet("api/notice/page")]
[Permission("notice:view")]
public async Task<PageResult<NoticeOutput>> GetPage(NoticePageInput input)

[HttpPost("api/notice")]
[Permission("notice:add")]
public async Task<NoticeOutput> Add(AddNoticeInput input)
```

> 使用 `[Permission("code")]` 而非 `[Permission(PermissionConsts.NoticeView)]`。运行时由 [PermissionFilter.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Filters/PermissionFilter.cs) 拦截校验。

#### 2.6.4 权限验证流程（自动）

```
请求到达 → PermissionFilter.OnActionExecutionAsync()
    → 读取 Action 的 [Permission] 特性
    → 从 JWT Claims 获取当前用户权限列表
    → 匹配通过 → 继续执行
    → 匹配失败 → 返回 403
```

#### 2.6.5 种子数据中添加菜单按钮

在 [SeedDataInitializer.cs](file:///workspace/agilityback/src/Agility.Zoey.Data/SeedData/SeedDataInitializer.cs) 的 `SeedMenusAsync` 方法中追加：

```csharp
new Menu { Name = "公告管理", Path = "/notice", Component = "notice/NoticeList",
    MenuType = 2, Icon = "NotificationOutlined", OrderNo = 600 },
new Menu { ParentId = noticeMenuId, Name = "新增公告", Permission = "notice:add",
    MenuType = 3, OrderNo = 1 },
new Menu { ParentId = noticeMenuId, Name = "编辑公告", Permission = "notice:edit",
    MenuType = 3, OrderNo = 2 },
new Menu { ParentId = noticeMenuId, Name = "删除公告", Permission = "notice:delete",
    MenuType = 3, OrderNo = 3 },
```

### 2.7 缓存规范

#### 2.7.1 缓存服务注入

```csharp
private readonly CacheService _cache;
private readonly IRepository<Notice> _noticeRepo;

public NoticeService(IRepository<Notice> noticeRepo, CacheService cache)
{
    _noticeRepo = noticeRepo;
    _cache = cache;
}
```

#### 2.7.2 缓存读取模式

```csharp
// 先查缓存，未命中则查数据库并回填缓存
var topNotices = _cache.GetOrCreate<List<NoticeOutput>>(
    CacheKeys.NoticeTop, entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        return _noticeRepo.AsQueryable()
            .Where(n => !n.IsDeleted && n.Status == 1)
            .OrderByDescending(n => n.CreateTime)
            .Take(5)
            .Select(n => new NoticeOutput { Id = n.Id, Title = n.Title, ... })
            .ToList();
    });
```

#### 2.7.3 缓存失效

```csharp
// 新增/更新/删除后清除相关缓存
await _noticeRepo.InsertAsync(entity);
_cache.Remove(CacheKeys.NoticeTop);    // 清除最新公告缓存
_cache.Remove(CacheKeys.NoticeList);   // 清除列表缓存
```

#### 2.7.4 缓存 Key 命名规范

在 [CacheKeys.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Shared/Consts/CacheKeys.cs) 中追加：

```csharp
// 格式：{模块}:{用途}:{动态参数占位符}
public const string NoticeTop = "notice:top";
public const string NoticeList = "notice:list";
public const string NoticeDetail = "notice:detail:{0}";   // {0} = id
```

#### 2.7.5 缓存策略

| 数据类型 | 过期时间 | 策略 |
|----------|----------|------|
| 热点列表 | 5-30 分钟 | 绝对过期 + 写操作清除 |
| 单条详情 | 30 分钟 | 绝对过期 + 写操作清除 |
| 字典数据 | 60 分钟 | 绝对过期 + 字典变更清除 |
| 系统配置 | 30 分钟 | 滑动过期 + 配置更新清除 |
| 用户权限 | Token 有效期 | 与 JWT 绑定 |

### 2.8 日志规范

#### 2.8.1 请求日志（自动）

[RequestLogMiddleware.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Middleware/RequestLogMiddleware.cs) 已自动记录所有请求：
- HTTP 方法、路径、状态码、耗时
- POST/PUT 请求体（截断至 1000 字符）
- 状态码 >= 400 自动以 Warning 级别记录

> **新增模块无需额外编码**，中间件自动覆盖。

#### 2.8.2 操作日志（需手动记录）

对于核心业务操作，注入 `ILogger<T>` 记录关键行为：

```csharp
private readonly ILogger<NoticeService> _logger;

public NoticeService(IRepository<Notice> noticeRepo, ILogger<NoticeService> logger)
{
    _logger = logger;
}

public async Task<NoticeOutput> Add(AddNoticeInput input)
{
    var entity = await _noticeRepo.InsertAsync(notice);
    _logger.LogInformation("公告创建成功: Id={Id}, Title={Title}", entity.Id, entity.Title);
    return await Get(entity.Id);
}
```

#### 2.8.3 日志级别使用指南

| 级别 | 使用场景 |
|------|----------|
| `LogInformation` | 正常的业务操作完成（创建、更新、删除成功） |
| `LogWarning` | 业务校验失败（记录不存在、状态不允许操作） |
| `LogError` | 不可恢复的异常（需要运维关注） |

### 2.9 配置规范

#### 2.9.1 配置原则

- **基础设施配置**（数据库、JWT、Redis）→ `appsettings.json`
- **业务配置**（文件存储策略、安全策略、功能开关）→ `Sys_SystemSetting` 数据库表，由前端配置页面管理

#### 2.9.2 读取运行时配置

```csharp
// 通过 CacheService 读取数据库中的系统配置
if (_cache.TryGetValue<Dictionary<string, object>>(CacheKeys.SystemSettings, out var settings))
{
    var maxUploadSize = settings?.GetValueOrDefault("FileStorage.MaxUploadSize", 10);
}
```

### 2.10 校验器规范

#### 2.10.1 创建校验器

```csharp
using FluentValidation;
using Agility.Zoey.Web.Core.Modules.Notice.Dto;

namespace Agility.Zoey.Web.Core.Modules.Notice.Validators;

public class AddNoticeInputValidator : AbstractValidator<AddNoticeInput>
{
    public AddNoticeInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .MaximumLength(200).WithMessage("标题最多200个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("内容不能为空");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("类型最多50个字符");
    }
}

public class UpdateNoticeInputValidator : AbstractValidator<UpdateNoticeInput>
{
    public UpdateNoticeInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID不能为空");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .MaximumLength(200).WithMessage("标题最多200个字符");
    }
}
```

#### 2.10.2 注册校验器

在 [ServiceCollectionExtensions.cs](file:///workspace/agilityback/src/Agility.Zoey.Web.Core/Extensions/ServiceCollectionExtensions.cs) 中无需手动注册——`AddValidatorsFromAssemblyContaining` 已经通过反射扫描了整个程序集。

### 2.11 事务规范

#### 2.11.1 何时使用事务

**必须使用事务**的场景：
- 插入主表 + 插入关联表（如：创建用户 + 分配角色）
- 删除主表记录 + 清理关联表
- 先删后插的替换操作（如：更新角色的菜单列表）

**不需要事务**的场景：
- 单表单条插入/更新
- 纯查询操作

#### 2.11.2 事务模板

```csharp
[HttpPut("api/notice/{id}/tags")]
[Permission("notice:edit")]
public async Task SetTags(long id, List<string> tags)
{
    var entity = await _noticeRepo.GetByIdAsync(id);
    if (entity == null || entity.IsDeleted)
        throw Oops.Oh("公告不存在");

    try
    {
        await _noticeRepo.BeginTranAsync();

        // 多步写操作
        entity.ExtendData = JsonSerializer.Serialize(tags);
        await _noticeRepo.UpdateAsync(entity);

        // 这里可以操作其他 Repository，共享同一个事务
        // await _otherRepo.InsertAsync(...);

        await _noticeRepo.CommitTranAsync();
    }
    catch
    {
        await _noticeRepo.RollbackTranAsync();
        throw;
    }
}
```

> 所有 `IRepository<T>` 实例共享同一个 `SqlSugarScope`，在任一 Repository 上调用事务方法即可覆盖整个 Scope。

### 2.12 数据库唯一性约束

| 约束层级 | 实现方式 |
|----------|----------|
| 数据库层 | `[SugarIndex]` 类级别属性创建唯一索引 |
| 应用层 | Service 的 Add/Update 方法中先 `CountAsync` 检查再插入 |
| 异常兜底 | SqlSugar 唯一约束冲突时抛出异常，由全局过滤器捕获返回友好信息 |

### 2.13 完整新增模块示例（后端）

以"公告管理（Notice）"模块为例，完整文件清单：

```
Agility.Zoey.Core/
└── Entities/
    └── Notice.cs                        ← 实体定义

Agility.Zoey.Web.Core/
└── Modules/Notice/
    ├── Services/
    │   └── NoticeService.cs             ← 动态 API 服务
    ├── Dto/
    │   └── NoticeDto.cs                 ← DTO
    └── Validators/
        └── NoticeValidator.cs           ← 校验器

Agility.Zoey.Data/
└── DbContext/
    └── SqlSugarDbContext.cs             ← 注册 Notice 到 EntityTypes

Agility.Zoey.Web.Core/Shared/Consts/
├── PermissionConsts.cs                  ← 追加权限常量
└── CacheKeys.cs                         ← 追加缓存 Key
```

---

## 三、前端模块开发规范

### 3.1 项目目录结构

```
src/
├── api/{module}/         ← API 接口层
│   └── notice.ts
├── views/{module}/       ← 页面组件
│   └── NoticeList.vue
├── types/
│   └── index.ts          ← 追加类型定义
├── router/
│   └── generator.ts      ← 追加视图映射
└── store/modules/
    └── user.ts           ← 已有权限 Store，无需新增
```

### 3.2 类型定义规范

在 [types/index.ts](file:///workspace/agilityfrontend/src/types/index.ts) 中追加模块类型：

```typescript
// ============ Notice 模块类型 ============

export interface NoticeRecord {
  id: number
  title: string
  content: string
  type: string
  status: number
  publishTime: string
  createTime: string
}

export interface NoticeQuery extends PageParams {
  title?: string
  type?: string
  status?: number
}
```

> 类型定义位置：**每个模块的类型定义必须放在 `types/index.ts` 中**，不单独创建模块类型文件。

### 3.3 API 层规范

#### 3.3.1 API 文件模板

```typescript
// src/api/notice/notice.ts
import request from '../../utils/request'
import type { PageResult, NoticeRecord, NoticeQuery } from '../../types'

export function getNoticePage(params: NoticeQuery): Promise<PageResult<NoticeRecord>> {
  return request.get('/notice/page', { params })
}

export function getNotice(id: number): Promise<NoticeRecord> {
  return request.get(`/notice/${id}`)
}

export function addNotice(data: Partial<NoticeRecord>): Promise<void> {
  return request.post('/notice', data)
}

export function updateNotice(id: number, data: Partial<NoticeRecord>): Promise<void> {
  return request.put(`/notice/${id}`, data)
}

export function deleteNotice(id: number): Promise<void> {
  return request.delete(`/notice/${id}`)
}
```

#### 3.3.2 API 命名规范

| 操作 | 函数命名 | HTTP 方法 |
|------|----------|-----------|
| 分页查询 | `getXxxPage` | GET |
| 获取详情 | `getXxx` | GET |
| 新增 | `addXxx` | POST |
| 更新 | `updateXxx` | PUT |
| 删除 | `deleteXxx` | DELETE |
| 批量操作 | `batchXxx` | POST |

#### 3.3.3 请求封装说明

所有 API 调用通过 `request.ts` 的统一 Axios 实例，自动：
- 注入 Bearer Token（从 `localStorage` 读取）
- 处理 401 响应（清除 Token + 跳转登录）
- 处理超时（清除 Token + 跳转登录）
- 统一错误提示

### 3.4 路由规范

#### 3.4.1 注册视图组件

在 [generator.ts](file:///workspace/agilityfrontend/src/router/generator.ts) 的 `viewModules` 中追加：

```typescript
const viewModules: Record<string, () => Promise<unknown>> = {
  // ... 已有视图 ...
  'notice/NoticeList': () => import(/* webpackChunkName: "notice" */ '../views/notice/NoticeList.vue'),
}
```

| 规范 | 说明 |
|------|------|
| 路由 Key | 与后端菜单表的 `Component` 字段一致（如 `notice/NoticeList`） |
| Chunk Name | `webpackChunkName` 注释用于代码分包，模块名即分包名 |
| 懒加载 | 必须使用动态 `import()`，禁止顶层静态 `import` |

#### 3.4.2 路由生成流程

```
后端 /api/auth/init 返回 MenuItem[] 
    → 前端 store 存储 menus
    → router/generator.ts: buildRoutes() 递归构建 RouteRecordRaw[]
    → router.addRoute() 动态注入
    → 仅 menuType=1(目录) 和 menuType=2(菜单) 参与路由生成
    → menuType=3(按钮) 不生成路由，仅用于权限码匹配
    → isHide=true 的菜单不生成路由
```

#### 3.4.3 菜单 Meta 字段

```typescript
meta: {
  title: '公告管理',
  icon: 'NotificationOutlined',
  keepAlive: true,       // 是否缓存页面组件
  permission: 'notice:view',
  isFrame: false,
  frameSrc: '',
}
```

### 3.5 Store 状态管理规范

#### 3.5.1 权限判断

所有页面统一使用 `useUserStore().hasPermission()` 判断按钮显示：

```typescript
const userStore = useUserStore()

// 在模板中使用
const canAdd = computed(() => userStore.hasPermission('notice:add'))
const canEdit = computed(() => userStore.hasPermission('notice:edit'))
const canDelete = computed(() => userStore.hasPermission('notice:delete'))
```

#### 3.5.2 字典数据获取

通过 `userStore.dictData` 获取已缓存的字典：

```typescript
const noticeTypeOptions = computed(() => {
  return userStore.dictData?.notice_type ?? []
})
```

### 3.6 权限验证与集成（前端）

#### 3.6.1 按钮级权限控制

```html
<a-button v-if="canAdd" type="primary" @click="handleAdd">
  <PlusOutlined /> 新增公告
</a-button>

<a-button v-if="canEdit" size="small" @click="handleEdit(record)">编辑</a-button>

<a-popconfirm
  v-if="canDelete"
  title="确定要删除该公告吗？"
  @confirm="handleDelete(record.id)"
>
  <a-button size="small" danger>删除</a-button>
</a-popconfirm>
```

#### 3.6.2 路由级权限控制

路由的 `meta.permission` 字段可供路由守卫使用。当前项目主要通过后端返回的菜单树控制可见路由，前端不再额外做路由拦截（因为用户看不到没有权限的菜单入口）。

### 3.7 页面布局统一性

#### 3.7.1 标准列表页布局

所有管理列表页采用统一的**三区域布局**：

```
┌──────────────────────────────────────────────────┐
│  搜索区域（a-card.search-card）                     │
│  [输入框] [输入框] [下拉选择] [查询] [重置]          │
├──────────────────────────────────────────────────┤
│  工具栏（toolbar）                                 │
│  [+ 新增] [导出]                                   │
├──────────────────────────────────────────────────┤
│  表格区域（a-card.table-card）                      │
│  ┌──────────────────────────────────────────────┐│
│  │ 列1   │ 列2   │ 列3   │ ...  │ 操作          ││
│  │ 数据  │ 数据  │ 数据  │ ...  │ [编辑][删除]  ││
│  └──────────────────────────────────────────────┘│
│  分页器                                           │
└──────────────────────────────────────────────────┘
```

#### 3.7.2 布局模板代码

```html
<template>
  <div class="notice-list">
    <!-- 搜索区域 -->
    <a-card class="search-card">
      <a-form :model="searchForm" layout="inline">
        <a-form-item label="标题">
          <a-input v-model:value="searchForm.title" placeholder="请输入标题" allow-clear />
        </a-form-item>
        <a-form-item label="类型">
          <a-select v-model:value="searchForm.type" placeholder="请选择类型" allow-clear style="width: 150px">
            <a-select-option value="general">通用</a-select-option>
            <a-select-option value="important">重要</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="状态">
          <a-select v-model:value="searchForm.status" placeholder="请选择状态" allow-clear style="width: 120px">
            <a-select-option :value="1">启用</a-select-option>
            <a-select-option :value="2">禁用</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" @click="handleSearch">查询</a-button>
            <a-button @click="handleReset">重置</a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <!-- 工具栏 + 表格 -->
    <a-card class="table-card">
      <div class="toolbar">
        <a-space>
          <a-button v-if="canAdd" type="primary" @click="handleAdd">
            <PlusOutlined /> 新增
          </a-button>
          <a-button v-if="canExport" @click="handleExport">
            <ExportOutlined /> 导出
          </a-button>
        </a-space>
      </div>

      <a-table
        :columns="columns"
        :data-source="tableData"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'status'">
            <a-switch
              :checked="record.status === 1"
              :loading="statusLoading[record.id]"
              @change="(checked: boolean) => handleStatusChange(record, checked)"
            />
          </template>
          <template v-else-if="column.dataIndex === 'action'">
            <a-space>
              <a-button v-if="canEdit" size="small" @click="handleEdit(record)">
                编辑
              </a-button>
              <a-popconfirm
                v-if="canDelete"
                title="确定删除？"
                @confirm="handleDelete(record.id)"
              >
                <a-button size="small" danger>删除</a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>
  </div>
</template>
```

#### 3.7.3 样式规范

```less
<style lang="less" scoped>
.notice-list {
  padding: 16px;

  .search-card {
    margin-bottom: 16px;

    :deep(.ant-form-item) {
      margin-bottom: 12px;
    }
  }

  .table-card {
    .toolbar {
      margin-bottom: 16px;
    }
  }
}
</style>
```

> **所有页面均应使用 `lang="less" scoped`**，外层容器类名以模块名命名（如 `.notice-list`）。

### 3.8 表单风格与布局

#### 3.8.1 弹窗表单标准模板

```html
<a-modal
  v-model:open="modalVisible"
  :title="isEdit ? '编辑公告' : '新增公告'"
  :confirm-loading="submitLoading"
  width="640px"
  :destroy-on-close="true"
  @ok="handleSubmit"
  @cancel="handleCancel"
>
  <a-form
    ref="formRef"
    :model="formData"
    :rules="formRules"
    :label-col="{ span: 4 }"
    :wrapper-col="{ span: 18 }"
  >
    <a-form-item label="标题" name="title">
      <a-input v-model:value="formData.title" placeholder="请输入公告标题" :maxlength="200" />
    </a-form-item>

    <a-form-item label="类型" name="type">
      <a-select v-model:value="formData.type" placeholder="请选择类型">
        <a-select-option value="general">通用</a-select-option>
        <a-select-option value="important">重要</a-select-option>
      </a-select>
    </a-form-item>

    <a-form-item label="内容" name="content">
      <a-textarea
        v-model:value="formData.content"
        placeholder="请输入公告内容"
        :rows="6"
        :maxlength="5000"
        show-count
      />
    </a-form-item>

    <a-form-item label="发布时间" name="publishTime">
      <a-date-picker
        v-model:value="formData.publishTime"
        show-time
        format="YYYY-MM-DD HH:mm:ss"
        style="width: 100%"
      />
    </a-form-item>

    <a-form-item label="状态" name="status">
      <a-switch
        :checked="formData.status === 1"
        checked-children="启用"
        un-checked-children="禁用"
        @change="(checked: boolean) => formData.status = checked ? 1 : 2"
      />
    </a-form-item>
  </a-form>
</a-modal>
```

#### 3.8.2 表单规范清单

| 规范项 | 要求 |
|--------|------|
| Modal 宽度 | 简单表单 `460px`，复杂表单 `640px`，含表格 `800px` |
| label-col | `{ span: 4 }` |
| wrapper-col | `{ span: 18 }` |
| destroy-on-close | `true`（关闭时销毁表单状态，防止残留数据） |
| confirm-loading | 绑定提交 loading 状态 |
| 表单校验 | `:rules` 定义在 script 中，使用 `reactive` |
| 输入框 placeholder | 格式：`请输入/请选择 + 字段名称` |
| maxlength | 与后端实体 `[MaxLength]` 保持一致 |

#### 3.8.3 表单校验规则

```typescript
const formRules: Record<string, Rule[]> = {
  title: [
    { required: true, message: '请输入公告标题', trigger: 'blur' },
    { max: 200, message: '标题最多200个字符', trigger: 'blur' },
  ],
  content: [
    { required: true, message: '请输入公告内容', trigger: 'blur' },
  ],
}
```

### 3.9 弹出页面标准

#### 3.9.1 弹窗类型选择

| 场景 | 使用组件 | 说明 |
|------|----------|------|
| 新增/编辑表单 | `a-modal` | 标准表单弹窗 |
| 详情查看 | `a-drawer` | 侧滑抽屉，适合展示长内容 |
| 删除确认 | `a-popconfirm` | 气泡确认框，表格行操作 |
| 复杂多步骤 | `a-modal` + `a-steps` | 分步弹窗 |
| 大表单/全屏编辑 | 独立路由页面 | 不弹窗，跳转新页面 |

#### 3.9.2 Modal 提交流程

```typescript
const handleSubmit = async () => {
  try {
    await formRef.value!.validate()
    submitLoading.value = true

    if (isEdit.value) {
      await updateNotice(editId.value, formData)
      message.success('更新成功')
    } else {
      await addNotice(formData)
      message.success('新增成功')
    }

    modalVisible.value = false
    await loadData()
  } catch (error: any) {
    if (error?.errorFields) return     // 表单校验失败，不提示
    message.error(error?.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

const handleCancel = () => {
  modalVisible.value = false
  formRef.value?.resetFields()
}
```

#### 3.9.3 Drawer 使用示例

```html
<a-drawer
  :open="drawerVisible"
  title="公告详情"
  width="520px"
  @close="drawerVisible = false"
>
  <a-descriptions :column="1" bordered>
    <a-descriptions-item label="标题">{{ detail.title }}</a-descriptions-item>
    <a-descriptions-item label="内容">{{ detail.content }}</a-descriptions-item>
    <a-descriptions-item label="发布时间">{{ detail.publishTime }}</a-descriptions-item>
    <a-descriptions-item label="创建时间">{{ detail.createTime }}</a-descriptions-item>
  </a-descriptions>
</a-drawer>
```

### 3.10 提示规范

#### 3.10.1 消息提示

| 场景 | 方法 | 示例 |
|------|------|------|
| 操作成功 | `message.success()` | `message.success('新增成功')` |
| 操作失败 | `message.error()` | `message.error('操作失败，请重试')` |
| 警告信息 | `message.warning()` | `message.warning('请先选择一条记录')` |
| 删除确认 | `a-popconfirm` | `<a-popconfirm title="确定删除？" @confirm="...">` |

#### 3.10.2 提示文案规范

| 操作 | 成功文案 | 失败文案 |
|------|----------|----------|
| 新增 | `新增成功` | `新增失败` |
| 更新 | `更新成功` | `更新失败` |
| 删除 | `删除成功` | `删除失败` |
| 导出 | `导出成功` | `导出失败` |
| 状态变更 | `状态变更成功` | `状态变更失败` |
| 上传 | `${filename} 上传成功` | `${filename} 上传失败` |

#### 3.10.3 Loading 状态

每个异步操作必须有 loading 状态：

```typescript
const loading = ref(false)            // 表格/页面级别
const submitLoading = ref(false)      // 弹窗提交按钮
const statusLoading = reactive<Record<number, boolean>>({})  // 行级操作
```

### 3.11 完整新增模块示例（前端）

以"公告管理（Notice）"模块为例：

```
src/
├── api/notice/
│   └── notice.ts                      ← API 接口
├── views/notice/
│   └── NoticeList.vue                 ← 列表页面
├── types/
│   └── index.ts                       ← 追加 NoticeRecord、NoticeQuery
└── router/
    └── generator.ts                   ← 追加 viewModules
```

---

## 附录 A：新增模块检查清单

### 后端

- [ ] 确认模块是否需要多租户（`ITenant`）和软删除（`ISoftDelete`）
- [ ] Entity 添加到 `SqlSugarDbContext.EntityTypes` 数组
- [ ] Entity 添加必要的 `[SugarIndex]` 唯一约束
- [ ] 创建 `Dto/` 目录，DTO 使用 `record` 类型
- [ ] 创建 `Services/XxxService.cs`，实现 `IDynamicApiController, ITransient`
- [ ] 每个 API 方法添加 `[Permission("xxx:action")]` 特性
- [ ] 在 `PermissionConsts.cs` 添加权限常量
- [ ] 在 `CacheKeys.cs` 添加缓存 Key（如有缓存需求）
- [ ] 创建 `Validators/XxxValidator.cs`
- [ ] 多步写操作包裹 `BeginTranAsync` / `CommitTranAsync`
- [ ] 新增/更新方法进行唯一性检查
- [ ] 在 `SeedDataInitializer.cs` 添加默认菜单

### 前端

- [ ] 在 `types/index.ts` 追加类型定义
- [ ] 创建 `api/xxx/xxx.ts`
- [ ] 在 `generator.ts` 的 `viewModules` 添加路由映射
- [ ] 在 `views/xxx/` 下创建页面组件
- [ ] 页面使用标准三区域布局（搜索 + 工具栏 + 表格）
- [ ] 按钮使用 `useUserStore().hasPermission()` 控制显示
- [ ] 弹窗表单遵循标准模板（`destroy-on-close`, `confirm-loading`）
- [ ] 异步操作有 loading 状态
- [ ] 操作成功/失败使用 `message.success()` / `message.error()` 提示
- [ ] 样式使用 `lang="less" scoped`

---

## 附录 B：技术栈版本

| 技术 | 版本 |
|------|------|
| .NET | 9.0 |
| Furion | 4.9.7 |
| SqlSugar | 5.1.4.176 |
| Vue | 3.x |
| Vite | 5.x |
| TypeScript | 5.x |
| Ant Design Vue | 4.x |
| Pinia | 2.x |
| Vue Router | 4.x |