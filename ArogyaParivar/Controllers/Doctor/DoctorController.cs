using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EHM.Hybrid.Framework.DAL;
using EHM.Hybrid.Framework.BLL;
using ArogyaParivar.Models;
using System.Dynamic;
using Newtonsoft.Json;
using System.Data.Objects;


namespace ArogyaParivar.Controllers
{
    [CustomAuthorize("Doctor")]
    public class DoctorController : Controller
    {

        ScreeningBll screeningBll = new ScreeningBll();
        DocotorScreening doctormodel = new DocotorScreening();
        //
        // GET: /Doctor/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DoctorDashBoard()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            BookList = GetDoctorDashbord();
            return View(BookList);
        }


        public ActionResult DoctorConsultation()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            BookList = GetDoctorDashbord();
            return View(BookList);
        }

        public ActionResult DoctorExamination(int TokenNumber, string ArogyaID)
        {

            Session["Token"] = TokenNumber;
            Session["arogyid"] = ArogyaID;
            LoadData(TokenNumber, ArogyaID);

            return View(doctormodel);
        }

        public IList<DashBordModel> GetDoctorDashbord()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            ArogyaParivarEntities context = new ArogyaParivarEntities();


            var query = from token in context.T_PatientInfo

                        from reg in context.T_Screenings
                      .Where(x => x.ArogyaID == token.ArogyaID)

                        from scr in context.T_Examination
                       .Where(x => x.ArogyaID == token.ArogyaID).DefaultIfEmpty()


                        from reg1 in context.M_PComplaints
                        .Where(x => x.ID == reg.ComplaintID)
                        from gender in context.M_Gender
                        .Where(x => x.ID == token.FK_GenderID)

                        where EntityFunctions.TruncateTime(reg.CreateDate).Value == EntityFunctions.TruncateTime(DateTime.Now).Value && reg.ScreenOutcomeID == 1 || reg.ScreenOutcomeID == 3
                        && scr.ArogyaID == null
                        // where patient.FirstName.Contains(searchString) || patient.SurName.Contains(searchString) || patient.ContactNumber.Contains(searchString)
                        select new DashBordModel
                        {
                            Tokem_Date = reg.CreateDate,
                            ArogyaID = token.ArogyaID,
                            PatientName = token.FirstName + token.SurName,
                            TokenNumber = reg.Token_Number,
                            Age = token.Age,
                            GenderName = gender.Gender,
                            ComplaintName = reg1.M_PresentingComplainting,
                            screeningoutcomeID = reg.ScreenOutcomeID.Value

                        };

            BookList = query.Distinct().ToList();
            if (BookList.Count > 0)
            {
                var updatelist = BookList.Where(x => x.screeningoutcomeID == 3).ToList();
                if (updatelist != null && updatelist.Count > 0)
                    updatelist[0].rowcolor = "Follow Up";


            }

            return BookList;


        }

        private DocotorScreening LoadData(int TokenNumber, string ArogyaID)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            var query1 = (from screening in context.T_Screenings

                          from pc in context.M_PComplaints
                          .Where(x => x.ID == screening.ComplaintID).DefaultIfEmpty()
                          //on screening.ComplaintID equals pc.ID

                          from user in context.M_User
                          .Where(x => x.User_ID == screening.UserID).DefaultIfEmpty()
                          //on screening.UserID equals user.User_ID

                          where screening.Token_Number == TokenNumber
                              && EntityFunctions.TruncateTime(screening.CreateDate).Value == DateTime.Today
                              && screening.ArogyaID == ArogyaID
                          select new DocotorScreening
                          {
                              ChiefComplaintName = pc.M_PresentingComplainting,
                              ChiefComplaintID = screening.ComplaintID.Value,
                              HistoryPresentingComplaint = screening.PresentingComplaint,
                              PastMedicalHistory = screening.PastMedicalHistory,
                              PastSurgicalHistory = screening.PastSurgicalHistory,
                              FamilyHistory = screening.FamilyHistory,
                              CurrentMedication = screening.CurrentMedication,
                              ECGOutcomeID = screening.ECGOutcomeID.Value,
                              ScreenOutcomeID = screening.ScreenOutcomeID.Value
                              //CreateDate = screening.CreateDate
                          }).ToList();
            doctormodel = query1[0];

            var query = context.T_Vitals.Where(o => o.ArogyaID == ArogyaID && o.Token_Number == TokenNumber && EntityFunctions.TruncateTime(o.CreateDate) == DateTime.Today).ToList();
            if (query.Count > 0)
            {
                doctormodel.Weight = query[0].Weight;
                doctormodel.Height = query[0].Height;
                doctormodel.BMI = query[0].BMI;
                doctormodel.DiaBP = query[0].DiaBP;
                doctormodel.SysBP = query[0].SysBP;
                doctormodel.Temparature = query[0].Temparature;
                doctormodel.Pulse = query[0].Pulse;
                doctormodel.Respiratory = query[0].Respiratory;

                var query2 = from allergy in context.T_Allergies

                             from at in context.M_AllergyType
                             .Where(x => x.ID == allergy.AllergenID).DefaultIfEmpty()
                             //on allergy.AllergenID equals at.ID

                             from ar in context.M_Reaction
                             .Where(x => x.ID == allergy.ReactionID).DefaultIfEmpty()
                             //on allergy.ReactionID equals ar.ID

                             from asev in context.M_Severity
                             .Where(x => x.ID == allergy.SeverityID).DefaultIfEmpty()
                             //on allergy.SeverityID equals asev.ID

                             from ast in context.M_Status
                             .Where(x => x.ID == allergy.StatusID).DefaultIfEmpty()
                             //on allergy.StatusID equals ast.ID

                             where allergy.Token_Number == TokenNumber
                                && EntityFunctions.TruncateTime(allergy.CreateDate).Value == DateTime.Today
                                && allergy.ArogyaID == ArogyaID
                             select new AllergyModel
                             {
                                 AllergyTypeName = at.AllergyType,
                                 AllergenName = "",
                                 ReactionName = ar.Reaction,
                                 SeverityName = asev.Severity,
                                 StatusName = ast.M_StatusName,
                                 CreateDate = allergy.CreateDate,
                                 //CreateDateString=allergy.CreateDate.Value.ToString("dd/MM/yyyy")
                             };
                doctormodel.Allergies = query2.ToList();

            }
            return doctormodel;
        }

        public JsonResult GetpresentingCompalints()
        {
            var AllergyListt = screeningBll.GetpresentingCompalints();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.pcName,
                Value = m.pcId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDosage()
        {
            var DoageListt = screeningBll.GetDosage();
            var DosageList = DoageListt.Select(m => new SelectListItem()
            {
                Text = m.DosageName,
                Value = m.DosageId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(DosageList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDosageFrequency()
        {
            var DosageFrequencyListt = screeningBll.Getfrequency();
            var DosageFrequencyList = DosageFrequencyListt.Select(m => new SelectListItem()
            {
                Text = m.FrequencyName,
                Value = m.FrequencyId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(DosageFrequencyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRoute()
        {
            var GetRouteListt = screeningBll.GetRoute();
            var GetRouteList = GetRouteListt.Select(m => new SelectListItem()
            {
                Text = m.RoutName,
                Value = m.RouteID.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(GetRouteList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSpecialty()
        {
            var Specialtyistt = screeningBll.Getspecialty();
            var SpecialtyList = Specialtyistt.Select(m => new SelectListItem()
            {
                Text = m.SpecilatyName,
                Value = m.SpaceialtyId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(SpecialtyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveMedicationData(string DrugName, int Doseid, int Frequencyid, string Duration, int Routid, string Instruction)
        {
            try
            {
                int savedata = 0;
                ArogyaParivarEntities context = new ArogyaParivarEntities();

                T_Medication modelScreening = new T_Medication()
                {
                    DrugName = DrugName,
                    DoseID = Doseid,
                    FrequencyID = Frequencyid,
                    Duration = Duration,
                    RouteID = Routid,
                    SpecialInstruction = Instruction,
                    ArogyaID = Session["arogyid"].ToString(),
                    Token_Number = Convert.ToInt32(Session["Token"]),
                    CreateDate = DateTime.Now,
                    UserID = Convert.ToInt32(Session["UserID"])


                };
                context.T_Medication.Add(modelScreening);
                context.SaveChanges();
                dynamic obj = new ExpandoObject();
                obj.Token = modelScreening.Token_Number;
                obj.Arogyaid = modelScreening.ArogyaID;
                obj.date = modelScreening.CreateDate;
                return Medication(obj.Arogyaid, obj.Token, obj.date);
                // Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Medication(string argid, int token, DateTime dt)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();

            var query = context.SP_GetMedicationDetailsByArogyaID(argid, token, dt);

            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveExaminationData(SaveDocotor Doctormodel)
        {
            try
            {

                ArogyaParivarEntities context = new ArogyaParivarEntities();

                T_Examination modelScreenings = new T_Examination()
                {
                    ComplaintID = Doctormodel.ChiefComplaintID,
                    PresentingComplaint = Doctormodel.HistoryPresentingComplaint,
                    PastMedicalHistory = Doctormodel.PastMedicalHistory,
                    PastSurgicalHistory = Doctormodel.PastSurgicalHistory,
                    FamilyHistory = Doctormodel.FamilyHistory,
                    CurrentMedication = Doctormodel.CurrentMedication,
                    Diagnosis = Doctormodel.Diagnosis,
                    Tests = Doctormodel.Tests,
                    TreatmentPlan = Doctormodel.TreatMentPlan,
                    ArogyaID = Session["arogyid"].ToString(),
                    Token_Number = Convert.ToInt32(Session["Token"]),
                    CreateDate = DateTime.Now,
                    Active = true,
                    UserID = Convert.ToInt32(Session["UserID"]),
                    ScreeningID = Doctormodel.ScreeningoutcomeID
                };
                context.T_Examination.Add(modelScreenings);
                context.SaveChanges();
                return Json("Data Saved", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
