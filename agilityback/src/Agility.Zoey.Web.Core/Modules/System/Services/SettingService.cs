using Agility.Zoey.Core.Entities;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using Microsoft.Extensions.Caching.Memory;

namespace Agility.Zoey.Web.Core.Modules.System.Services;

[ApiDescriptionSettings(Group = "System", Order = 160)]
public class SettingService : IDynamicApiController, ITransient
{
    private readonly IRepository<SystemSetting> _settingRepo;
    private readonly IMemoryCache _cache;

    public SettingService(IRepository<SystemSetting> settingRepo, IMemoryCache cache)
    {
        _settingRepo = settingRepo;
        _cache = cache;
    }

    [HttpGet("api/setting/group/{group}")]
    public async Task<List<SettingOutput>> GetByGroup(string group)
    {
        var settings = await _settingRepo.AsQueryable()
            .Where(s => s.Group == group && s.Status == 1)
            .OrderBy(s => s.Sort)
            .ToListAsync();

        return settings.Select(s => new SettingOutput
        {
            Id = s.Id,
            ConfigKey = s.ConfigKey,
            ConfigValue = s.ConfigValue,
            ValueType = s.ValueType,
            Group = s.Group,
            Description = s.Description,
            Sort = s.Sort,
            Status = s.Status
        }).ToList();
    }

    [HttpGet("api/setting/all")]
    public async Task<List<SettingOutput>> GetAll()
    {
        var settings = await _settingRepo.AsQueryable()
            .Where(s => s.Status == 1)
            .OrderBy(s => s.Sort)
            .ToListAsync();

        return settings.Select(s => new SettingOutput
        {
            Id = s.Id,
            ConfigKey = s.ConfigKey,
            ConfigValue = s.ConfigValue,
            ValueType = s.ValueType,
            Group = s.Group,
            Description = s.Description,
            Sort = s.Sort,
            Status = s.Status
        }).ToList();
    }

    [HttpPut("api/setting")]
    public async Task Update(List<UpdateSettingInput> inputs)
    {
        foreach (var input in inputs)
        {
            var setting = await _settingRepo.AsQueryable()
                .Where(s => s.ConfigKey == input.ConfigKey)
                .FirstAsync();

            if (setting != null)
            {
                setting.ConfigValue = input.ConfigValue;
                setting.UpdateTime = DateTime.Now;
                setting.UpdateUserId = 0;
                await _settingRepo.UpdateAsync(setting);
            }
        }

        _cache.Remove("system:settings");
    }

    [HttpGet("api/setting/cache/reload")]
    public async Task<string> ReloadCache()
    {
        _cache.Remove("system:settings");
        return "缓存已刷新";
    }
}