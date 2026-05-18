using System.ComponentModel.DataAnnotations;

namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NotDefaultAttribute : ValidationAttribute
{
    public const string DefaultErrorMessage = "The {0} field must not have its default value.";

    public NotDefaultAttribute() : base(DefaultErrorMessage) { }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;

        var type = value.GetType();

        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return !value.Equals(defaultValue);
        }

        return true;
    }
}
