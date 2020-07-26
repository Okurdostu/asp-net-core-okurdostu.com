using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Web.Base;
using Okurdostu.Web.Extensions;
using Okurdostu.Web.Filters;
using Okurdostu.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api
{
    //api/education/{actionroutename}
    public class EducationController : ApiController
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IHostingEnvironment Environment;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        public EducationController(IHostingEnvironment env) => Environment = env;
#pragma warning restore CS0618 // Type or member is obsolete

        [NonAction]
        public bool DeleteFileFromServer(string filePathAfterRootPath)
        {
            if (System.IO.File.Exists(Environment.WebRootPath + filePathAfterRootPath))
            {
                System.IO.File.Delete(Environment.WebRootPath + filePathAfterRootPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        [Authorize, HttpGet("get/{EducationId}")] //api/education/list/EducationId
        public async Task<IActionResult> Get(long EducationId) //get education informations
        {
            JsonReturnModel jsonReturnModel = new JsonReturnModel();
            var AuthenticatedUserId = User.Identity.GetUserId();
            var Education = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == EducationId && !x.IsRemoved && x.UserId == long.Parse(AuthenticatedUserId));

            if (Education != null)
            {
                var educationModel = new EducationModel
                {
                    UniversityId = Education.UniversityId,
                    Department = Education.Department,
                    EducationId = EducationId,
                    ActivitiesSocieties = Education.ActivitiesSocieties,
                    Startyear = int.Parse(Education.StartYear),
                    Finishyear = int.Parse(Education.EndYear),

                };
                educationModel.AreUniversityorDepartmentCanEditable = Education.AreUniversityorDepartmentCanEditable();
                if (educationModel.ActivitiesSocieties == null || educationModel.ActivitiesSocieties == "undefined")
                {
                    educationModel.ActivitiesSocieties = "";
                }

                jsonReturnModel.Data = educationModel;
            }
            else
            {
                jsonReturnModel.Code = 404;
                return Error(jsonReturnModel);
            }

            return Succes(jsonReturnModel);
        }

        [ServiceFilter(typeof(ConfirmedEmailFilter))] //api/education/post?EducationId=....&Somevalue=...
        [HttpPost("post"), Authorize, ValidateAntiForgeryToken] //deleting, adding, editing a education information
        public async Task<IActionResult> Post(EducationModel Model, long educationIdForRemove)
        {
            var AuthenticatedUserId = User.Identity.GetUserId();
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (educationIdForRemove > 0) //deleting a education information
            {
                var deletedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == educationIdForRemove && !x.IsRemoved && long.Parse(AuthenticatedUserId) == x.UserId);

                if (deletedEducation != null)
                {
                    var AuthUserActiveNeedCount = Context.Need.Where(x => !x.IsRemoved && x.UserId == long.Parse(AuthenticatedUserId)).Count();
                    if (deletedEducation.IsActiveEducation && AuthUserActiveNeedCount > 0)
                    {
                        jsonReturnModel.Code = 200;
                        jsonReturnModel.Message = "Bu eğitimi silemezsiniz";

                        TempData["ProfileMessage"] = "İhtiyaç kampanyanız olduğu için" +
                            "<br />" +
                            "Aktif olan eğitim bilginizi silemezsiniz." +
                            "<br />" +
                            "Aktif olan eğitim bilgisi, belge yollayarak hala burada okuduğunuzu iddia ettiğiniz bir eğitim bilgisidir." +
                            "<br/>" +
                            "Daha fazla ayrıntı ve işlem için: info@okurdostu.com";

                        return Error(jsonReturnModel);
                    }
                    else
                    {
                        deletedEducation.IsRemoved = true;
                        var result = await Context.SaveChangesAsync();
                        if (result > 0)
                        {
                            jsonReturnModel.Message = "Eğitim bilgisi kaldırıldı";
                            try
                            {
                                if (deletedEducation.IsSentToConfirmation)
                                {
                                    var educationDocuments = await Context.UserEducationDoc.Where(x => x.UserEducationId == deletedEducation.Id).ToListAsync();
                                    foreach (var item in educationDocuments)
                                    {
                                        if (DeleteFileFromServer(item.PathAfterRoot))
                                        {
                                            Context.Remove(item);
                                        }
                                    }
                                    await Context.SaveChangesAsync();
                                }
                            }
                            catch (Exception)
                            {

                            }

                            return Succes(jsonReturnModel);
                        }
                    }
                }
                else
                {
                    jsonReturnModel.Code = 404;
                    return Error(jsonReturnModel);
                }
            }

            if (Model.Startyear > Model.Finishyear)
            {
                jsonReturnModel.Message = "Başlangıç yılı, bitiş yılından büyük olamaz";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
            }
            else if (Model.Startyear < 1980 || Model.Startyear > DateTime.Now.Year || Model.Finishyear < 1980 || Model.Startyear > DateTime.Now.Year + 7)
            {
                jsonReturnModel.Message = "Başlangıç yılı, bitiş yılı ile alakalı bilgileri kontrol edip, tekrar deneyin";
                jsonReturnModel.Code = 200;
                return Error(jsonReturnModel);
            }

            if (Model.EducationId > 0) //editing a education information
            {
                var editedEducation = await Context.UserEducation.FirstOrDefaultAsync(x => x.Id == Model.EducationId && !x.IsRemoved && long.Parse(AuthenticatedUserId) == x.UserId);

                if (editedEducation != null)
                {
                    editedEducation.StartYear = Model.Startyear.ToString();
                    editedEducation.EndYear = Model.Finishyear.ToString();
                    editedEducation.ActivitiesSocieties = Model.ActivitiesSocieties;

                    if (editedEducation.AreUniversityorDepartmentCanEditable())
                    {
                        editedEducation.UniversityId = Model.UniversityId;
                        editedEducation.Department = Model.Department;
                    }
                }
                else
                {
                    jsonReturnModel.Code = 404;
                    return Error(jsonReturnModel);
                }
            }
            else //adding a new education information
            {
                var NewEducation = new UserEducation
                {
                    UserId = long.Parse(AuthenticatedUserId),
                    UniversityId = Model.UniversityId,
                    Department = Model.Department,
                    ActivitiesSocieties = Model.ActivitiesSocieties,
                    StartYear = Model.Startyear.ToString(),
                    EndYear = Model.Finishyear.ToString(),
                };
                await Context.AddAsync(NewEducation);
            }

            try //db savechanges, editing and adding
            {
                var result = await Context.SaveChangesAsync();
                if (result > 0)
                {
                    jsonReturnModel.Message = "Eğitim bilgisi kaydedildi";
                    return Succes(jsonReturnModel);
                }
                else
                {
                    jsonReturnModel.Message = "Bir işlem yapılmadı";
                    jsonReturnModel.Code = 1001;
                    return Error(jsonReturnModel);
                }
            }
            catch (Exception e)
            {
                jsonReturnModel.Code = 200;

                string innerMessage = (e.InnerException != null) ? e.InnerException.Message.ToLower() : "";

                if (innerMessage.Contains("department"))
                {
                    jsonReturnModel.Message = "Bölüm bilgilerine ulaşamadık veya eksik";
                }
                else if (innerMessage.Contains("university"))
                {
                    jsonReturnModel.Message = "Üniversite bilgilerine ulaşamadık veya eksik";
                }
                else if (innerMessage.Contains("startyear") || innerMessage.Contains("endyear"))
                {
                    jsonReturnModel.Message = "Başlangıç veya bitiş yılını kontrol edin";
                }
                else
                {
                    jsonReturnModel.Message = "Başaramadık ve ne olduğunu bilmiyoruz, tekrar deneyin";
                }

                return Error(jsonReturnModel);
            }
        }

        [HttpGet("list/{username}")] //api/education/list/username
        public async Task<IActionResult> List(string username)
        {
            var AppUser = await Context.User.FirstOrDefaultAsync(x => x.Username == username);
            JsonReturnModel jsonReturnModel = new JsonReturnModel();

            if (AppUser != null)
            {
                var Educations = await Context.UserEducation.Where(x => x.UserId == AppUser.Id && !x.IsRemoved).Select(x => new
                {
                    x.Id,
                    x.EndYear,
                    x.StartYear,
                    //x.IsRemoved,
                    x.IsConfirmed,
                    x.IsActiveEducation,
                    x.IsSentToConfirmation,

                    universityPageUrl = "/universite/" + x.University.FriendlyName,
                    universityName = x.University.Name,
                    x.University.LogoUrl,
                    x.UniversityId,
                }).ToListAsync();

                if (Educations != null)
                {
                    jsonReturnModel.Data = Educations;
                    return Succes(jsonReturnModel);
                }
            }

            jsonReturnModel.Code = 404;
            return Error(jsonReturnModel);
        }

    }
}
