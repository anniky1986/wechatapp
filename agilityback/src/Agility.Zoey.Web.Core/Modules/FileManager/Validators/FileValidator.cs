using Agility.Zoey.Web.Core.Modules.FileManager.Dto;
using FluentValidation;

namespace Agility.Zoey.Web.Core.Modules.FileManager.Validators;

public class AddFileFolderInputValidator : AbstractValidator<AddFileFolderInput>
{
    public AddFileFolderInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("文件夹名称不能为空")
            .MaximumLength(128).WithMessage("文件夹名称最多128个字符");

        RuleFor(x => x.Color)
            .MaximumLength(32).WithMessage("颜色标识最多32个字符");

        RuleFor(x => x.Icon)
            .MaximumLength(64).WithMessage("图标最多64个字符");

        RuleFor(x => x.Sort)
            .GreaterThanOrEqualTo(0).WithMessage("排序值不能为负数");
    }
}

public class UpdateFileFolderInputValidator : AbstractValidator<UpdateFileFolderInput>
{
    public UpdateFileFolderInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("文件夹ID不能为空");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("文件夹名称不能为空")
            .MaximumLength(128).WithMessage("文件夹名称最多128个字符");

        RuleFor(x => x.Color)
            .MaximumLength(32).WithMessage("颜色标识最多32个字符");

        RuleFor(x => x.Icon)
            .MaximumLength(64).WithMessage("图标最多64个字符");

        RuleFor(x => x.Sort)
            .GreaterThanOrEqualTo(0).WithMessage("排序值不能为负数");
    }
}