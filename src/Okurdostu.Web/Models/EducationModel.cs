using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

public class EducationModel
{
    [Required]
    public int UniversityId { get; set; }
    [Required]
    public long EducationId { get; set; } //it's for edit UserEducation[Table] : Id
    [Required]
    public int Startyear { get; set; }
    [Required]
    public int Finishyear { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "En fazla 50 karakter")]
    [Display(Name = "Bölüm")]
    public string Department { get; set; }

    [MaxLength(200, ErrorMessage = "En fazla 200 karakter")]
    [Display(Name = "Aktiviteler veya topluluklar")]
    public string ActivitiesSocieties { get; set; }

    [Required]
    [Display(Name = "Okul")]
    public List<SelectListItem> Universities { get; set; }

    public List<SelectListItem> StartYears { get; set; }

    public List<SelectListItem> FinishYears { get; set; }

    public void ListYears()
    {
        List<SelectListItem> _StartYears = new List<SelectListItem>();
        List<SelectListItem> _FinishYears = new List<SelectListItem>();

        for (int i = DateTime.Now.Year;  i >= 1980; i--)
            _StartYears.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        for (int i = DateTime.Now.Year + 7; i >= 1980; i--)
            _FinishYears.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

        this.StartYears = _StartYears;
        this.FinishYears = _FinishYears;

    }
}