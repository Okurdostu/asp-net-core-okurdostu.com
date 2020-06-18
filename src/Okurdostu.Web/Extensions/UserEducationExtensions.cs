using Okurdostu.Data.Model;

namespace Okurdostu.Web.Extensions
{
    public static class UserEducationExtensions
    {
        public static bool IsUniversityInformationsCanEditable(this UserEducation edu)
        {
            return !edu.IsConfirmed && !edu.IsRemoved;
        }
    }
}
