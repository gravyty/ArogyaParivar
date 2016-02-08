using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using EHM.Hybrid.Framework.DAL;
namespace ArogyaParivar.Models
{
    public class Patient
    {



        public Patient()
        {

        }
       [Required(ErrorMessage = "PatientName is required")]
        public string PatientName { get; set; }
       public string ArogyaID { get; set; }
       [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }
        public int Age { get; set; }
        public int RefBy { get; set; }
        public string DOB { get; set; }
        public string RefName { get; set; }
        public int Gender { get; set; }
        [Required(ErrorMessage = "AadharID is required")]
        public string AadharID { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        public decimal Village { get; set; }
        public string Town { get; set; }
        public string TownName { get; set; }
        public string VillageName { get; set; }
        public int District { get; set; }
        public string DistrictName { get; set; }
        public string StateName { get; set; }
        public bool? Consent { get; set; }
        public int Sate { get; set; }
        public string ContactNo { get; set; }
        public int Cal { get; set; }
        public string UserName { get; set; }
        public string GenderName { get; set; }
      
      
        
    }

    public class Uploads
    {
        [Key]
        public virtual int Upload_id { get; set; }
        [Required]
        public virtual string Title { get; set; }

    }

    public class Screening
    {
        public int ID { get; set; }
        public string AllergyType { get; set; }
        public string ReactionName { get; set; }
        public string ArogyaID { get; set; }
        public int RefBy { get; set; }
        public string RefName { get; set; }
        public string Surname { get; set; }
        public int? Age { get; set; }
        public int? Gender { get; set; }
        public string AadharID { get; set; }
        public string Address { get; set; }
        public string PatientName { get; set; }
        //public int ReactionId { get; set; }
        //public string SeverityName { get; set; }
        //public int SeverityId { get; set; }
        //public string StatusName { get; set; }
        //public int StatusId { get; set; }
        //public string SpecilatyName { get; set; }
        //public int SpaceialtyId { get; set; }
        //public string ScreeningOutcome { get; set; }
        //public int ScreeningID { get; set; }
        //public string RoutName { get; set; }
        //public int RouteID { get; set; }
        //public string RefByName { get; set; }
        //public int RefByID { get; set; }
        //public string FrequencyName { get; set; }
        //public int FrequencyId { get; set; }
        //public string EcgOutcome { get; set; }
        //public int EcgOutcomeId { get; set; }
        //public string DosageName { get; set; }
        //public int DosageId { get; set; }
        //public string pcName { get; set; }
        //public int pcId { get; set; }
        public List<Allergy> Allergys { get; set; }

        public string Height { get; set; }
        public string Weight { get; set; }
        public string BMI { get; set; }
        public string SysBP { get; set; }
        public string DiaBP { get; set; }
        public string Temparature { get; set; }
        public string Pulse { get; set; }
        public string Respiratory { get; set; }

        public int ComplaintID { get; set; }
        public int CheckListID { get; set; }
        public string PresentingComplaint { get; set; }
        public string PastMedicalHistory { get; set; }
        public string PastSurgicalHistory { get; set; }
        public string FamilyHistory { get; set; }
        public string CurrentMedication { get; set; }
        public int ECGOutcomeID { get; set; }
        public int ScreenOutcomeID { get; set; }
        public int SpecialtyID { get; set; }
        public DateTime AppointDate { get; set; }
        public DateTime CreateDate { get; set; }

        public int chkId { get; set; }
        public List<int> Anemiaradio { get; set; }
        public List<int> AnemiaWeightage { get; set; }

        public List<int> Cadmiaradio { get; set; }
        public List<int> CadmiaWeightage { get; set; }

        public List<int> Repmiaradio { get; set; }
        public List<int> RepemiaWeightage { get; set; }
        

        
    }

    public class Student
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Age { get; set; }
    }
    public class StudentCollection : List<Student>
    {
        public void Add(Student st)
        {
            base.Add(st);
        }
    }

    public class Allergy
    {
        public string AllergyType { get; set; }
        public string ReactionName { get; set; }
        public string SeverityrName { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }

    }

    public class DocotorScreening
    {
        public int ID { get; set; }
        public string AllergyType { get; set; }
        public string ReactionName { get; set; }
        public string ArogyaID { get; set; }
        public int ReactionId { get; set; }
        public string SeverityName { get; set; }
        public int SeverityId { get; set; }
        public string StatusName { get; set; }
        public int StatusId { get; set; }
        public string SpecilatyName { get; set; }
        public int SpaceialtyId { get; set; }
        public string ScreeningOutcome { get; set; }
        public int ScreeningID { get; set; }
        public string RoutName { get; set; }
        public int RouteID { get; set; }
        public string RefByName { get; set; }
        public int RefByID { get; set; }
        public string FrequencyName { get; set; }
        public int FrequencyId { get; set; }
        public string EcgOutcome { get; set; }
       // public int EcgOutcomeId { get; set; }
        public string DosageName { get; set; }
        public int DosageId { get; set; }
        public string pcName { get; set; }
        public int pcId { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string BMI { get; set; }
        public string SysBP { get; set; }
        public string DiaBP { get; set; }
        public string Temparature { get; set; }
        public string Pulse { get; set; }
        public string Respiratory { get; set; }

        public int ChiefComplaintID { get; set; }
        public int CheckListID { get; set; }
        public string ChiefComplaintName { get; set; }
        public string HistoryPresentingComplaint { get; set; }
        public string PastMedicalHistory { get; set; }
        public string PastSurgicalHistory { get; set; }
        public string FamilyHistory { get; set; }
        public string CurrentMedication { get; set; }
        public string Diagnosis { get; set; }
        public string Tests { get; set; }
        public string TreatMentPlan { get; set; }
        public int ECGOutcomeID { get; set; }
        public int ScreenOutcomeID { get; set; }
        public int SpecialtyID { get; set; }
        public DateTime AppointDate { get; set; }
        public DateTime CreateDate { get; set; }
        public List<AllergyModel> Allergies { get; set; }
    }

    public class SaveDocotor
    {
        public int ChiefComplaintID { get; set; }
        public string HistoryPresentingComplaint { get; set; }
        public string PastMedicalHistory { get; set; }
        public string PastSurgicalHistory { get; set; }
        public string FamilyHistory { get; set; }
        public string CurrentMedication { get; set; }
        public string Diagnosis { get; set; }
        public string Tests { get; set; }
        public string TreatMentPlan { get; set; }
        public int ScreeningoutcomeID { get; set; }
    }
}