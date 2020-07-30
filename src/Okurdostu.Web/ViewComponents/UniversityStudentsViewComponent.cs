using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.ViewComponents
{
    [ViewComponent(Name = "UniversityStudents")]
    public class UniversityStudentsViewComponent : ViewComponent
    {
        private readonly OkurdostuContext Context;
        public UniversityStudentsViewComponent(OkurdostuContext _context) => Context = _context;


        public async Task<IViewComponentResult> InvokeAsync(string friendlyname)
        {
            List<UniversityStudentsModel> StudentList = new List<UniversityStudentsModel>();

            List<UserEducation> EducationList = await Context.UserEducation.Include(x => x.User).Where(x =>
             x.University.FriendlyName == friendlyname &&
             !x.IsRemoved).ToListAsync();

            foreach (var Education in EducationList)
            {

                if (StudentList.Where(x => x.ID == Education.UserId).FirstOrDefault() != null)
                {
                    continue;
                }
                var Student = new UniversityStudentsModel
                {
                    ID = Education.UserId,
                    FullName = Education.User.FullName,
                    Username = Education.User.Username,
                    ProfilePicture = Education.User.PictureUrl
                };

                StudentList.Add(Student);
            }

            return View(StudentList);
        }
    }
}
