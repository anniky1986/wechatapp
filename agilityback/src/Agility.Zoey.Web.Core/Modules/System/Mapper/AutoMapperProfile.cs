using Agility.Zoey.Core.Entities;
using Agility.Zoey.Web.Core.Modules.FileManager.Dto;
using Agility.Zoey.Web.Core.Modules.System.Dto;
using Mapster;

namespace Agility.Zoey.Web.Core.Modules.System.Mapper;

public class AutoMapperProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserOutput>();
        config.NewConfig<Role, RoleOutput>();
        config.NewConfig<Menu, MenuOutput>();
        config.NewConfig<Menu, MenuTreeOutput>();
        config.NewConfig<Dept, DeptOutput>();
        config.NewConfig<Dept, DeptTreeOutput>();
        config.NewConfig<Tenant, TenantOutput>();
        config.NewConfig<Dict, DictOutput>();
        config.NewConfig<DictItem, DictItemOutput>();
        config.NewConfig<SystemSetting, SettingOutput>();
        config.NewConfig<SysFile, FileOutput>();
        config.NewConfig<SysFileFolder, FileFolderOutput>();
        config.NewConfig<SysFileFolder, FileFolderTreeOutput>();
        config.NewConfig<OperationLog, OperationLogOutput>();
        config.NewConfig<LoginLog, LoginLogOutput>();
    }
}