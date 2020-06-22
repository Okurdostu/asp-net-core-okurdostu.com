using Microsoft.EntityFrameworkCore;
using Okurdostu.Data;
using Okurdostu.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Models
{
    public class UniversityStudentsModel
    {
        private long ID;
        public string FullName;
        public string Username;
        public string ProfilePicture;

        public async Task<List<UniversityStudentsModel>> Students(string schoolfriendlyname)
        {
            var Context = new OkurdostuContext();
            List<UniversityStudentsModel> List = new List<UniversityStudentsModel>();

            List<UserEducation> EducationList = await Context.UserEducation.Include(x => x.User).Where(x =>
             x.University.FriendlyName == schoolfriendlyname &&
             !x.IsRemoved).ToListAsync();
            foreach (var Education in EducationList)
            {

                if (List.Where(x => x.ID == Education.UserId).FirstOrDefault() != null)
                    continue;
                /*
                    Bir öğrencinin aynı üniversite'de birden fazla eğitimi olabilir (Kısaca ikinci eğitimden aynı user yaklanrsa ekleme)
                    örnek:
                    a öğrencisinin b üniversitesinde iki adet eğitimi varsa
                    ve
                    a öğrencisi UniversityStudentModel('List' Yukarıda tanımlanan) Listesine UserID'si ile kaydedildiyse
                    ve
                    tekrar: a öğrencisi Foreach: Education'dan yakalanırsa continue ile eş geç.
                */
                var Student = new UniversityStudentsModel();
                Student.ID = Education.UserId;
                Student.FullName = Education.User.FullName;
                Student.Username = Education.User.Username;
                Student.ProfilePicture = Education.User.PictureUrl;

                List.Add(Student);
            }
            return List;
        }
    }
}