using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Okurdostu.Web.Base;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Okurdostu.Data;

public class EducationModel
{
    [Required(ErrorMessage = "Üniversitenizi seçmelisiniz")]
    public short UniversityId { get; set; }
    [Required]
    public long EducationId { get; set; } //it's for edit UserEducation[Table] : Id
    [Required(ErrorMessage = "Başlangıç yılı seçmelisiniz")]
    public int Startyear { get; set; }
    [Required(ErrorMessage = "Bitiş yılı seçmelisiniz")]
    public int Finishyear { get; set; }

    [Required(ErrorMessage = "Bölümünüz girmelisiniz")]
    [MaxLength(50, ErrorMessage = "En fazla 50 karakter")]
    [Display(Name = "Bölüm")]
    public string Department { get; set; }

    [MaxLength(200, ErrorMessage = "En fazla 200 karakter")]
    [Display(Name = "Aktiviteler veya topluluklar")]
    public string ActivitiesSocieties { get; set; }

    [Required]
    public List<SelectListItem> Universities { get; set; } //sabit bir veri bloguna bağlanabilir.

    public List<SelectListItem> StartYears { get; set; }

    public List<SelectListItem> FinishYears { get; set; }

    public void ListYears()
    {
        List<SelectListItem> _StartYears = new List<SelectListItem>();
        List<SelectListItem> _FinishYears = new List<SelectListItem>();

        for (int i = DateTime.Now.Year; i >= 1980; i--)
            _StartYears.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        for (int i = DateTime.Now.Year + 7; i >= 1980; i--)
            _FinishYears.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

        this.StartYears = _StartYears;
        this.FinishYears = _FinishYears;
    }

    public async Task ListUniversitiesAsync()
    {
        using (var Context = new OkurdostuContext())
        {
            var _Universities = new List<SelectListItem>();
            foreach (var item in await Context.University.ToListAsync())
                _Universities.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
            this.Universities = _Universities;

        };
    }
}