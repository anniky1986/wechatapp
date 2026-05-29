using Agility.Zoey.Web.Core.Modules.System.Dto;
using FluentValidation;

namespace Agility.Zoey.Web.Core.Modules.System.Validators;

public class AddUserInputValidator : AbstractValidator<AddUserInput>
{
    public AddUserInputValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名最少3个字符")
            .MaximumLength(64).WithMessage("用户名最多64个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码最少6个字符")
            .MaximumLength(256).WithMessage("密码最多256个字符");

        RuleFor(x => x.NickName)
            .MaximumLength(64).WithMessage("昵称最多64个字符");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class UpdateUserInputValidator : AbstractValidator<UpdateUserInput>
{
    public UpdateUserInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户ID不能为空");

        RuleFor(x => x.NickName)
            .MaximumLength(64).WithMessage("昵称最多64个字符");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}