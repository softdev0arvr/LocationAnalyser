using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
namespace MIS.Domain.Enums
{
    public enum ApiStatus
    {
        Success = 0,
        Error = 1
    }
    public enum ApiErrors
    {
        [Description("Unauthorized access")]
        Unauthorized,
        [Description("Model state is not valid")]
        ModelState,
        [Description("not found")]
        NotFound,
        [Description("Something went wrong")]
        DefaultError,
        [Description("User is either inactive or deleted")]
        InActive,
        [Description("Only sales person are allowed to login")]
        InValidRole,
        [Description("The Provided credentials are not correct")]
        WrongEmailPass,
        [Description("Already exists")]
        AreadyExists,
        [Description("Old Password is incorrect")]
        IncorrectOldPassword
    }
    public static class EnumExtension
    {
        public static string GetDescription(this Enum enumValue)
        {
            //Look for DescriptionAttributes on the enum field
            object[] attr = enumValue.GetType().GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attr.Length > 0) // a DescriptionAttribute exists; use it
                return ((DescriptionAttribute)attr[0]).Description;
            //The above code is all you need if you always use DescriptionAttributes;
            return enumValue.ToString();
        }
    }
}
