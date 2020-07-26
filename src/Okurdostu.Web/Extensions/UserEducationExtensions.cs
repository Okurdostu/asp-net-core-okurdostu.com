using Okurdostu.Data;

namespace Okurdostu.Web.Extensions
{
    public static class UserEducationExtensions
    {
        public static bool AreUniversityorDepartmentCanEditable(this UserEducation edu)
        {
            return edu.IsConfirmed == false && edu.IsRemoved == false;
        }
    }
}
