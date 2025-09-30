using System.ComponentModel;

namespace AllTheBeans.Domain.Enums;
public enum BeanColour
{
    [Description("green")]
    Green = 1,

    [Description("golden")]
    Golden = 2,

    [Description("light roast")]
    LightRoast = 3,

    [Description("medium roast")]
    MediumRoast = 4,

    [Description("dark roast")]
    DarkRoast = 5
}
