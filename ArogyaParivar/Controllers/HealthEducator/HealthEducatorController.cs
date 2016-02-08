using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EHM.Hybrid.Framework.BLL;
using EHM.Hybrid.Framework;
using ArogyaParivar.Models;
using EHM.Hybrid.Framework.DAL;
using System.Web.Services;
using ArogyaParivar.Entity;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.Objects;
namespace ArogyaParivar.Controllers.HealthEducatorController
{
    [CustomAuthorize("Reg/Nurse")]
    public class HealthEducatorController : Controller
    {
        //
        // GET: /HealthEducator/
        PatientBll patientBll = new PatientBll();
        ScreeningBll screeningBll = new ScreeningBll();
        public string strArogyaID;
        public int intTokenNumber;

       

        public ActionResult Index()
        {
            return View();
        }

        
        public ActionResult PatientHealthRecord(string arogyaID, int tokenNumber)
        {

            ArogyaParivarEntities context = new ArogyaParivarEntities();
            Screening model = new Screening();


            string searchString = "";

            if (arogyaID != "")
            {
                searchString = arogyaID;
            }
            Session["strArogyaID"] = arogyaID;
            Session["intTokenNumber"] = tokenNumber;

            strArogyaID = Session["strArogyaID"].ToString();
            intTokenNumber = Convert.ToInt32(Session["intTokenNumber"]);
            string sortOrder = "";
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IList<PatientModel> BookList = new List<PatientModel>();
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (!String.IsNullOrEmpty(searchString))
            {

                BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            }

            else
            {
                searchString = "";
                BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            }

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

                         where allergy.Token_Number == tokenNumber
                            && EntityFunctions.TruncateTime(allergy.CreateDate).Value == EntityFunctions.TruncateTime(DateTime.Now).Value
                            && allergy.ArogyaID == arogyaID
                         select new Allergy
                         {
                             AllergyType = at.AllergyType,

                             ReactionName = ar.Reaction,
                             SeverityrName = asev.Severity,
                             Status = ast.M_StatusName,
                             // CreateDate =  EntityFunctions.TruncateTime(allergy.CreateDate).Value.ToString()
                             //CreateDateString=allergy.CreateDate.Value.ToString("dd/MM/yyyy")
                         };
            model.Allergys = query2.ToList();
            return View(model);
        }

        public ActionResult VideoPatientHealthRecord()
        {

            //ArogyaParivarEntities context = new ArogyaParivarEntities();
            //Screening model = new Screening();


            //string searchString = "";

            //if (arogyaID != "")
            //{
            //    searchString = arogyaID;
            //}
            //Session["strArogyaID"] = arogyaID;
            //Session["intTokenNumber"] = tokenNumber;

            //strArogyaID = Session["strArogyaID"].ToString();
            //intTokenNumber = Convert.ToInt32(Session["intTokenNumber"]);
            //string sortOrder = "";
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            //IList<PatientModel> BookList = new List<PatientModel>();
            //ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            //if (!String.IsNullOrEmpty(searchString))
            //{

            //    BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            //}

            //else
            //{
            //    searchString = "";
            //    BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            //}

            //var query2 = from allergy in context.T_Allergies

            //             from at in context.M_AllergyType
            //             .Where(x => x.ID == allergy.AllergenID).DefaultIfEmpty()
            //             //on allergy.AllergenID equals at.ID

            //             from ar in context.M_Reaction
            //             .Where(x => x.ID == allergy.ReactionID).DefaultIfEmpty()
            //             //on allergy.ReactionID equals ar.ID

            //             from asev in context.M_Severity
            //             .Where(x => x.ID == allergy.SeverityID).DefaultIfEmpty()
            //             //on allergy.SeverityID equals asev.ID

            //             from ast in context.M_Status
            //             .Where(x => x.ID == allergy.StatusID).DefaultIfEmpty()
            //             //on allergy.StatusID equals ast.ID

            //             where allergy.Token_Number == tokenNumber
            //                && EntityFunctions.TruncateTime(allergy.CreateDate).Value == EntityFunctions.TruncateTime(DateTime.Now).Value
            //                && allergy.ArogyaID == arogyaID
            //             select new Allergy
            //             {
            //                 AllergyType = at.AllergyType,

            //                 ReactionName = ar.Reaction,
            //                 SeverityrName = asev.Severity,
            //                 Status = ast.M_StatusName,
            //                 // CreateDate =  EntityFunctions.TruncateTime(allergy.CreateDate).Value.ToString()
            //                 //CreateDateString=allergy.CreateDate.Value.ToString("dd/MM/yyyy")
            //             };
            //model.Allergys = query2.ToList();
            return View();
        }

        // By Abhijit


        public ActionResult EncounterSummary(string arogyaID)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            List<Episode> episodes = new List<Episode>();
            List<Episode> finalEpisodes = new List<Episode>();

            EncounterSummaryBll encounterSummaryBll=new EncounterSummaryBll();

            List<EncounterSummaryModel> encounters = encounterSummaryBll.GetEncounterList(arogyaID).OrderBy(x => x.CreateDate.Value).ToList();
            List<T_Appointments> appointments = encounterSummaryBll.GetAppointmentList(arogyaID);

            foreach (EncounterSummaryModel encounter in encounters)
            {
                Episode episode;
                var eps = episodes.FindAll(x => x.ComplaintID == encounter.ComplaintID.Value);
                if (eps.Count == 0)
                {
                    episode = new Episode { ComplaintID = encounter.ComplaintID.Value, ComplaintName = encounter.ComplaintName, LastModifiedDate = encounter.CreateDate.Value };
                    episode.encounterSummaryList.Add(encounter);
                    episodes.Add(episode);
                }
                else
                {
                    if (encounter.CreateDate.Value.Date.Subtract(eps[eps.Count - 1].LastModifiedDate.Date).Days < 30
                        || appointments.Where(x => encounter.CreateDate.Value.Date.Subtract(x.ApptDate.Value.Date).Days < 30
                                                    && x.ApptDate.Value.Date.Subtract(eps[eps.Count - 1].LastModifiedDate.Date).Days > 0
                                                    && x.ComplaintID == encounter.ComplaintID
                                             ).ToList().Count > 0)
                    {
                        eps[eps.Count - 1].LastModifiedDate = encounter.CreateDate.Value;
                        eps[eps.Count - 1].encounterSummaryList.Add(encounter);
                    }
                    else
                    {
                        episode = new Episode { ComplaintID = encounter.ComplaintID.Value, ComplaintName = encounter.ComplaintName, LastModifiedDate = encounter.CreateDate.Value };
                        episode.encounterSummaryList.Add(encounter);
                        episodes.Add(episode);
                    }
                }
            }
            episodes = episodes.OrderByDescending(x => x.LastModifiedDate).ToList();
            ViewBag.Episodes = JsonConvert.SerializeObject(episodes);
            return View();
        }

        public ActionResult VideoCasesheet(string arogyaID)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            List<Episode> episodes = new List<Episode>();
            List<Episode> finalEpisodes = new List<Episode>();

            EncounterSummaryBll encounterSummaryBll = new EncounterSummaryBll();

            List<EncounterSummaryModel> encounters = encounterSummaryBll.GetEncounterList(arogyaID).OrderBy(x => x.CreateDate.Value).ToList();
            List<T_Appointments> appointments = encounterSummaryBll.GetAppointmentList(arogyaID);

            foreach (EncounterSummaryModel encounter in encounters)
            {
                Episode episode;
                var eps = episodes.FindAll(x => x.ComplaintID == encounter.ComplaintID.Value);
                if (eps.Count == 0)
                {
                    episode = new Episode { ComplaintID = encounter.ComplaintID.Value, ComplaintName = encounter.ComplaintName, LastModifiedDate = encounter.CreateDate.Value };
                    episode.encounterSummaryList.Add(encounter);
                    episodes.Add(episode);
                }
                else
                {
                    if (encounter.CreateDate.Value.Date.Subtract(eps[eps.Count - 1].LastModifiedDate.Date).Days < 30
                        || appointments.Where(x => encounter.CreateDate.Value.Date.Subtract(x.ApptDate.Value.Date).Days < 30
                                                    && x.ApptDate.Value.Date.Subtract(eps[eps.Count - 1].LastModifiedDate.Date).Days > 0
                                                    && x.ComplaintID == encounter.ComplaintID
                                             ).ToList().Count > 0)
                    {
                        eps[eps.Count - 1].LastModifiedDate = encounter.CreateDate.Value;
                        eps[eps.Count - 1].encounterSummaryList.Add(encounter);
                    }
                    else
                    {
                        episode = new Episode { ComplaintID = encounter.ComplaintID.Value, ComplaintName = encounter.ComplaintName, LastModifiedDate = encounter.CreateDate.Value };
                        episode.encounterSummaryList.Add(encounter);
                        episodes.Add(episode);
                    }
                }
            }
            episodes = episodes.OrderByDescending(x => x.LastModifiedDate).ToList();
            ViewBag.Episodes = JsonConvert.SerializeObject(episodes);
            return View();
        }


        public ActionResult PreviousHistory()
        {
           
            return View();
        }

        public JsonResult GetEncounterSummary(int tokenNumber, DateTime createdDate, string arogyaID)
        {
            EncounterSummaryModel encounter = new EncounterSummaryBll().GetEncounterSummary(tokenNumber, createdDate, arogyaID);
            return Json(JsonConvert.SerializeObject(encounter), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintPage()
        {
            return View();
        }

        public JsonResult GetUploadDocuments(string arogyaID)
        {
            List<UploadModel> uploads = new UploadDocumentsBll().GetUploadList(arogyaID);
            return Json(uploads, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadDocuments(int tokenNumber, string arogyaID, string complaintID, int userID)
        {
            ViewBag.ArogyaID = arogyaID;
            List<UploadModel> uploads = new UploadDocumentsBll().GetUploadList(arogyaID);
            ViewBag.UploadData = JsonConvert.SerializeObject(uploads);
            ViewBag.tokenNumber = tokenNumber;
            ViewBag.arogyaID = arogyaID;
            ViewBag.complaintID = complaintID;
            ViewBag.userID = userID;
            ViewBag.uploadPath = ConfigurationSettings.AppSettings["DocumentUploadPath"];
            return View();
        }

        public ActionResult UploadControl()
        {
            //ViewBag.ReloadTableScript = "";
            return View();
        }

        [HttpPost]
        public ActionResult UploadDoc(string txtComment, string txtTokenNumber, string txtArogyaID, string txtComplaintID, int txtUserID)
        {
            try
            {
                var fileName = "";
                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath(ConfigurationSettings.AppSettings["DocumentUploadPath"]), fileName);
                        file.SaveAs(path);
                    }

                    UploadModel upload = new UploadModel
                    {
                        Active = true,
                        ArogyaID = txtArogyaID,
                        Comments = txtComment,
                        ComplaintID = txtComplaintID == "" ? (int?)null : Convert.ToInt32(txtComplaintID),
                        CreateDate = DateTime.Now,
                        FileName = fileName,
                        Token_Number = txtTokenNumber == "" ? (int?)null : Convert.ToInt32(txtTokenNumber),
                        UserID = txtUserID
                    };

                    int count = new UploadDocumentsBll().SaveUpload(upload);

                    if (count > 0)
                        TempData["Success"] = "Upload Successful";
                    else
                        TempData["Success"] = "Upload failed";
                }
                else
                {
                    TempData["Success"] = "Please select a valid file to upload";
                }
            }
            catch (Exception ex)
            {
                TempData["Success"] = "Upload failed";
            }
            return RedirectToAction("UploadDocuments", new { tokenNumber = txtTokenNumber, arogyaID = txtArogyaID, complaintID = txtComplaintID, userID = txtUserID }); //View();
        }


        //BY Veeranki

        public JsonResult GetAllergytypes()
        {
            var AllergyListt = screeningBll.GetAllergyType();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.AllergyName,
                Value = m.Id.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReations(int id)
        {
            var ReactionListt = screeningBll.GetReations(id);
            var ReactionyList = ReactionListt.Select(m => new SelectListItem()
            {
                Text = m.ReactionName,
                Value = m.ReactionId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(ReactionyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getseverity()
        {
            var AllergyListt = screeningBll.Getseverityy();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.SeverityName,
                Value = m.SeverityId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Getstatus()
        {
            var AllergyListt = screeningBll.Getstatus();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.StatusName,
                Value = m.StatusId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Getspecialty()
        {
            var AllergyListt = screeningBll.Getspecialty();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.SpecilatyName,
                Value = m.SpaceialtyId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Getscreeningoutcome()
        {
            var AllergyListt = screeningBll.Getscreeningoutcome();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.ScreeningOutcome,
                Value = m.ScreeningID.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRoute()
        {
            var AllergyListt = screeningBll.GetRoute();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.RoutName,
                Value = m.RouteID.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRefBy()
        {
            var AllergyListt = screeningBll.GetRefBy();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.RefByName,
                Value = m.RefByID.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Getfrequency()
        {
            var AllergyListt = screeningBll.Getfrequency();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.FrequencyName,
                Value = m.FrequencyId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetECGoutcome()
        {
            var AllergyListt = screeningBll.GetECGoutcome();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.EcgOutcome,
                Value = m.EcgOutcomeId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDosage()
        {
            var AllergyListt = screeningBll.GetDosage();
            var AllergyList = AllergyListt.Select(m => new SelectListItem()
            {
                Text = m.DosageName,
                Value = m.DosageId.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(AllergyList, JsonRequestBehavior.AllowGet);
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


        public ActionResult PrintCard(string ArogyaID)
        {

            string searchString = "";

            if (ArogyaID != "")
            {
                searchString = ArogyaID;
            }
            string sortOrder = "";
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IList<PatientModel> BookList = new List<PatientModel>();
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (!String.IsNullOrEmpty(searchString))
            {

                BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            }

            else
            {
                searchString = "";
                BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            }


            return View(BookList);


        }

        public ActionResult UpdateRegistration(string ArogyaID)
        {


            string searchString = "";

            if (ArogyaID != "")
            {
                searchString = ArogyaID;
            }
            string sortOrder = "";
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IList<PatientModel> BookList = new List<PatientModel>();
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (!String.IsNullOrEmpty(searchString))
            {

                BookList = patientBll.Get_PrintCard(sortOrder, searchString);
            }

            else
            {
                searchString = "";
                BookList = patientBll.Get_PrintCard(sortOrder, searchString);

            }
            GetLoadGender(BookList[0].Gender.Value);
            GetLoadState(BookList[0].Sate.Value);
            GetLoadCity(BookList[0].Sate.Value, BookList[0].District.Value);
            GetLoadMandal(BookList[0].Sate.Value, BookList[0].District.Value, BookList[0].Town.ToString());
            GetLoadVillage(BookList[0].Sate.Value, BookList[0].District.Value, BookList[0].Town, Convert.ToInt64(BookList[0].Village));
            GetLoadAgeType();
            GetLoadRefBy();
            Patient UpdatePatient = new Patient()
             {
                 PatientName = BookList[0].PatientName,
                 Surname = BookList[0].Surname,
                 Age = BookList[0].Age.Value,
                 Cal = BookList[0].AgeType.Value,
                 Gender = BookList[0].Gender.Value,
                 AadharID = BookList[0].AadharID,
                 Address = BookList[0].Address,
                 ContactNo = BookList[0].ContactNo,
                 Village = BookList[0].Village,
                 Town = BookList[0].Town.ToString(),
                 RefBy = BookList[0].RefBy,
                 RefName = BookList[0].RefName,
                 Consent = true,
                 District = BookList[0].District.Value,
                 Sate = BookList[0].Sate.Value,
             };

            ViewBag.Arogyaid = ArogyaID;
            return View(UpdatePatient);

        }
        public ActionResult PatientRegistration()
        {

            return View();
        }
        public ActionResult Sample()
        {
            return View();
        }
        public void GetLoadState(int State)
        {
            var stateListt = patientBll.GetState();
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.StateName,
                Value = m.Sate.ToString()
            });

            ViewBag.State = new SelectList(statesList, "Value", "Text", State);

        }


        public void GetLoadGender(int gender)
        {
            var stateListt = GetListGender();
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.GenderName,
                Value = m.Gender.ToString(),
            });

            ViewBag.Gender = new SelectList(statesList, "Value", "Text", gender);

        }


        public void GetLoadAgeType()
        {
            List<SelectListItem> statesList = new List<SelectListItem>();
            statesList.Add(new SelectListItem { Text = "Years", Value = "0" });
            statesList.Add(new SelectListItem { Text = "Months", Value = "1" });
            statesList.Add(new SelectListItem { Text = "Days", Value = "2" });

            ViewBag.AgeType = new SelectList(statesList, "Value", "Text", 0);

        }

        public void GetLoadRefBy()
        {
            var stateListt = patientBll.RefBy();
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.RefName,
                Value = m.RefBy.ToString()
            });

            ViewBag.RefBy = new SelectList(statesList, "Value", "Text", 1);

        }
        public void GetLoadCity(int id, int did)
        {
            var stateListt = patientBll.GetDistrict(Convert.ToInt32(id));
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.DistrictName,
                Value = m.District.ToString()
            });


            ViewBag.District = new SelectList(statesList, "Value", "Text", did);
        }

        public void GetLoadMandal(int sId, int dId, string tid)
        {
            var stateListt = patientBll.GetMandalData(sId, dId);
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.TownName,
                Value = m.Town.ToString()
            });

            ViewBag.Town = new SelectList(statesList, "Value", "Text", tid);
        }


        public void GetLoadVillage(int sId, int dId, string id, Int64 vid)
        {

            var stateListt = patientBll.GetVillageData(sId, dId, id);
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.VillageName,
                Value = m.Village.ToString()
            });

            ViewBag.Village = new SelectList(statesList, "Value", "Text", vid);
        }
        public JsonResult GetState()
        {
            var stateListt = patientBll.GetState();
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.StateName,
                Value = m.Sate.ToString()
            });

            //ViewBag.SubjectName = new SelectList(statesList, "StateName", "StateName", "Uttar Pradesh");
            return Json(statesList, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult FileUpload(HttpPostedFileBase Files)
        //{
        //    //create object of LINQ to SQL class

        //    //loop through request file collection 
        //    foreach (string upload in Request.Files)
        //    {
        //        //create byte array of size equal to file input stream
        //        byte[] fileData = new byte[Request.Files[upload].InputStream.Length];
        //        //add file input stream into byte array
        //        Request.Files[upload].InputStream.Read(fileData, 0, Convert.ToInt32(Request.Files[upload].InputStream.Length));
        //        //create system.data.linq object using byte array
        //        System.Data.Linq.Binary binaryFile = new System.Data.Linq.Binary(fileData);
        //        //initialise object of FileDump LINQ to sql class passing values to be inserted
        //        FileDump record = new FileDump { FileData = binaryFile, FileName = System.IO.Path.GetFileName(Request.Files[upload].FileName) };
        //        //call InsertOnsubmit method to pass new object to entity
        //        dataContext.FileDumps.InsertOnSubmit(record);
        //        //call submitChanges method to execute implement changes into database
        //        dataContext.SubmitChanges();
        //    }
        //    var returnData = dataContext.FileDumps;
        //    ViewData.Model = returnData.ToList();
        //    return View();
        //}


        public IList<Patient> GetListGender()
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();

            var query = (from m in context.M_Gender

                         select new Patient
                         {
                            Gender=m.ID,
                            GenderName=m.Gender

                         }).Distinct();


            query = query.OrderBy(m => m.Gender);

            return query.ToList();


        }

        public JsonResult GetGender()
        {

            var stateListt = GetListGender();
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.GenderName,
                Value = m.Gender.ToString(),
            });

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCal()
        {
            List<SelectListItem> statesList = new List<SelectListItem>();
            statesList.Add(new SelectListItem { Text = "Years", Value = "0" });
            statesList.Add(new SelectListItem { Text = "Months", Value = "1" });
            statesList.Add(new SelectListItem { Text = "Days", Value = "2" });
            return Json(statesList, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult GetRefBy()
        //{
        //    var stateListt = patientBll.RefBy();
        //    var statesList = stateListt.Select(m => new SelectListItem()
        //    {
        //        Text = m.RefName,
        //        Value = m.RefBy.ToString()
        //    });
        //    return Json(statesList, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetCity(int id)
        {
            var stateListt = patientBll.GetDistrict(Convert.ToInt32(id));
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.DistrictName,
                Value = m.District.ToString()
            });


            return Json(statesList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMandal(int sId, int dId)
        {
            var stateListt = patientBll.GetMandalData(sId, dId);
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.TownName,
                Value = m.Town.ToString()
            });

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVillage(int sId, int dId, string id)
        {

            var stateListt = patientBll.GetVillageData(sId, dId, id);
            var statesList = stateListt.Select(m => new SelectListItem()
            {
                Text = m.VillageName,
                Value = m.Village.ToString()
            });

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }

        public ViewResult HEdashboard()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            BookList = GetNurseDashbord();
            return View(BookList);
        }


        public IList<DashBordModel> GetNurseDashbord()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            var query = from token in context.T_Token

                        from reg in context.T_PatientInfo
                        .Where(x => x.ArogyaID == token.ArogyaID)

                        from scr in context.T_Screenings
                         .Where(x => x.ArogyaID == token.ArogyaID).DefaultIfEmpty()
                        from gender in context.M_Gender
                        .Where(x => x.ID == reg.FK_GenderID)
                        where EntityFunctions.TruncateTime(token.Tokem_Date).Value == EntityFunctions.TruncateTime(DateTime.Now).Value &&
                          scr.ArogyaID == null
                        // where patient.FirstName.Contains(searchString) || patient.SurName.Contains(searchString) || patient.ContactNumber.Contains(searchString)
                        select new DashBordModel
                        {
                            Tokem_Date = token.Tokem_Date,
                            ArogyaID = token.ArogyaID,
                            PatientName = reg.FirstName + reg.SurName,
                            TokenNumber = token.Token_Number,
                            GenderName=gender.Gender,
                            Age = reg.Age

                        };



            return BookList = query.Distinct().ToList();


        }

        [HttpGet]
        public ActionResult Changephoto()
        {
            if (Convert.ToString(Session["val"]) != string.Empty)
            {
                ViewBag.pic = "http://localhost:55694/WebImages/" + Session["val"].ToString();
            }
            else
            {
                ViewBag.pic = "../../WebImages/person.jpg";
            }
            return View();
        }

        public JsonResult Dateofbirth(string Age)
        {
            int age = Convert.ToInt32(Age);
            DateTime CurrentDate = DateTime.Now;
            DateTime YearOfBirth = new DateTime(CurrentDate.AddMonths(-age).Year, 1, 1);
            return Json(YearOfBirth.ToString("dd/MM/yyyy"), JsonRequestBehavior.AllowGet);

        }
        public void uplaod()
        {

            int Id = patientBll.GetID();
            if (Id > 0)
            {
                if (Request.Files["file"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["exampleInputFile"].FileName);
                    string path1 = string.Format("{0}/{1}{2}", Server.MapPath("~/documents/Files"), Id, extension);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);

                    Request.Files["exampleInputFile"].SaveAs(path1);

                }
                ViewData["Success"] = "Success";
            }
            else
            {
                ViewData["Success"] = "Upload Failed";
            }




        }

        public ViewResult PatientSearch(string sortOrder, string name, string name1, string mobileNo, string ArogyaID)
        {
            IList<PatientModel> BookList = new List<PatientModel>();
            string searchString = "";
            //if (command.Equals("Reset"))
            //{
            //    ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
             
            //    ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            //    if (!String.IsNullOrEmpty(ArogyaID))
            //    {

            //        BookList = patientBll.Get_PateintDetails(sortOrder, ArogyaID);
            //    }

            //    else
            //    {
            //        searchString = "";
            //        BookList = patientBll.Get_PateintDetails(sortOrder, ArogyaID);

            //    }
            //}
            //else
            //{
                if (name != "")
                {
                    searchString = name;
                }
                else if (mobileNo != "")
                {
                    searchString = mobileNo;
                }
                else if (ArogyaID != "")
                {
                    searchString = ArogyaID;
                }
                else
                {
                    searchString = name1;
                }
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
         
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                if (!String.IsNullOrEmpty(searchString))
                {

                    BookList = patientBll.Get_PateintDetails(sortOrder, searchString);
                }

                else
                {
                    searchString = "";
                    BookList = patientBll.Get_PateintDetails(sortOrder, searchString);

                }
            //}
            return View(BookList);

        }

        public ViewResult PatientDetails(string sortOrder, string searchString)
        {

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IList<PatientModel> BookList = new List<PatientModel>();
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (!String.IsNullOrEmpty(searchString))
            {

                BookList = patientBll.Get_PateintDetails(sortOrder, searchString);
            }

            else
            {
                searchString = "";
                BookList = patientBll.Get_PateintDetails(sortOrder, searchString);

            }
            return View(BookList);

        }


        [HttpPost]
        public ActionResult PatientRegistration(Patient model, string command, HttpPostedFileBase Files)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            try
            {
                try
                {



                    ObjectParameter outParm = new ObjectParameter("callid", typeof(string));

                    context.SP_TokenGenration(outParm);

                    int tokennumber = Convert.ToInt32(outParm.Value);

                    ObjectParameter outParm1 = new ObjectParameter("callid", typeof(string));

                    context.SP_GenarateArogyaID(outParm1);

                    int intarogyaID = Convert.ToInt32(outParm1.Value);
                    string arogyaID = "A" + intarogyaID;
                    var webCamePath = ConfigurationSettings.AppSettings["WebCampath"];
                
                    if (command.Equals("Reset"))
                    {
                        return RedirectToAction("PatientRegistration");
                    }
                    else
                    {
                        var fileName = "";
                        if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                        {
                            var file = Request.Files[0];

                            if (file != null && file.ContentLength > 0)
                            {
                                fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath(ConfigurationSettings.AppSettings["DocumentUploadPath"]), fileName);
                                file.SaveAs(path);
                            }

                            T_RegUpload upload = new T_RegUpload()
                            {
                                Active = true,
                                ArogyaID = arogyaID,
                                CreateDate = DateTime.Now,
                                FileName = fileName,
                                UserID = Convert.ToInt32(Session["UserID"])
                            };
                            context.T_RegUpload.Add(upload);
                            context.SaveChanges();
                        }
                      

                        PatientModel book = new PatientModel()
                        {

                            PatientName = model.PatientName,
                            ArogyaID = arogyaID,
                            Surname = model.Surname,
                            Age = model.Age,
                            AgeType = model.Cal,
                            Gender = model.Gender,
                            AadharID = model.AadharID,
                            Address = model.Address,
                            ContactNo = model.ContactNo,
                            Village = Convert.ToInt64(Request.Form["Village"]),
                            Town = Request.Form["Mandal"].ToString(),
                            RefBy = Convert.ToInt32(Request.Form["RefBy"]),
                            RefName = model.RefName,
                            Consent = true,
                            District = Convert.ToInt32(Request.Form["city"].ToString()),
                            Sate = model.Sate,

                        };

                        T_Token tokn = new T_Token()
                        {
                            ArogyaID = arogyaID,
                            Token_Number = tokennumber,
                            Tokem_Date = DateTime.Now
                        };

                        context.T_Token.Add(tokn);
                        context.SaveChanges();
                        patientBll.Save(book);
                        return RedirectToAction("PrintCard", new { ArogyaID = arogyaID });
                 
                    }
                }
                catch
                {
                    return View(model);
                }


            }
            catch
            {
                return View(model);
            }
        }



        [HttpPost]
        public ActionResult UpdateRegistration(Patient model, string command, HttpPostedFileBase Files)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();

            try
            {
                try
                {

                    ObjectParameter outParm = new ObjectParameter("callid", typeof(string));

                    context.SP_TokenGenration(outParm);

                    int tokennumber = Convert.ToInt32(outParm.Value);
                    if (command.Equals("Reset"))
                    {
                        return RedirectToAction("PatientRegistration");
                    }
                    else
                    {
                    
                        
                        T_Token tokn = new T_Token()
                        {
                            ArogyaID = model.ArogyaID,
                            Token_Number = tokennumber,
                            Tokem_Date = DateTime.Now
                        };
                        
                      context.T_Token.Add(tokn);
                       var value= context.T_PatientInfo.Where(x => x.ArogyaID == model.ArogyaID).FirstOrDefault();
                        

                            value.FirstName = model.PatientName;
                            value.ArogyaID = model.ArogyaID;
                            value.SurName = model.Surname;
                            value.Age = model.Age;
                            value.AgeType = Convert.ToInt32(Request.Form["AgeType"]);
                            value.FK_GenderID = model.Gender;
                            value.ID_Number = model.AadharID;
                            value.Address = model.Address;
                            value.ContactNumber = model.ContactNo;
                            value.VillageId = Convert.ToInt64(Request.Form["Village"]);
                            value.TownId = Request.Form["Town"];
                            value.RefBy = Convert.ToInt32(Request.Form["RefBy"]);
                            value.RefName = model.RefName;
                            value.Consen = true;
                            value.DistrictId = Convert.ToInt32(Request.Form["District"].ToString());
                            value.SateId = model.Sate;

                        context.SaveChanges();
                        return RedirectToAction("PatientSearch");
                    }
                }
                catch
                {
                    return View(model);
                }


            }
            catch
            {
                return View(model);
            }
        }


        public JsonResult saveVitaltData(Screening model)
        {
            try
            {

                ArogyaParivarEntities context = new ArogyaParivarEntities();
                T_Vitals modelVitals = new T_Vitals()
                {
                    Height = model.Height,
                    Weight = model.Weight,
                    BMI = model.BMI,
                    SysBP = model.SysBP,
                    Pulse = model.Pulse,
                    DiaBP = model.DiaBP,
                    Temparature = model.Temparature,
                    Respiratory = model.Respiratory,
                    ArogyaID = Session["strArogyaID"].ToString(),
                    UserID = Convert.ToInt32(Session["UserID"]),
                    Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                    CreateDate = DateTime.Now,
                };
                context.T_Vitals.Add(modelVitals);
                context.SaveChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult AneSaveData(Screening model)
        {
            //string str = "Anemia";

            ArogyaParivarEntities context = new ArogyaParivarEntities();
            double k = 0.0;
            try
            {
                for (int i = 1; i <= 4; i++)
                {
                    k += model.Anemiaradio[i - 1] * model.AnemiaWeightage[i - 1] / 100.0;

                    T_ChkAnemia chkanemia = new T_ChkAnemia()
                    {

                        ChkID = i,
                        AnsID = model.Anemiaradio[i - 1],
                        ArogyaID = Session["strArogyaID"].ToString(),
                        Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                        CreateDate = DateTime.Now,
                        UserID = Convert.ToInt32(Session["UserID"])


                    };

                    context.T_ChkAnemia.Add(chkanemia);
                    context.SaveChanges();
                }

                return Json(new { score = k }, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult RepSaveData(Screening model)
        {
            //string str = "Anemia";

            ArogyaParivarEntities context = new ArogyaParivarEntities();
            double k = 0.0;
            try
            {
                for (int i = 1; i <= 9; i++)
                {
                    k += model.Repmiaradio[i - 1] * model.RepemiaWeightage[i - 1] / 100.0;

                    T_ChkAnemia chkanemia = new T_ChkAnemia()
                    {

                        ChkID = i,
                        AnsID = model.Repmiaradio[i - 1],
                        ArogyaID = Session["strArogyaID"].ToString(),
                        Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                        CreateDate = DateTime.Now,
                        UserID = Convert.ToInt32(Session["UserID"])


                    };

                    context.T_ChkAnemia.Add(chkanemia);
                    context.SaveChanges();
                }

                return Json(new { score = k }, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CadSaveData(Screening model)
        {
            //string str = "Anemia";

            ArogyaParivarEntities context = new ArogyaParivarEntities();
            double k = 0.0;
            try
            {
                for (int i = 1; i <= 7; i++)
                {
                    k += model.Cadmiaradio[i - 1] * model.CadmiaWeightage[i - 1] / 100.0;

                    T_ChkAnemia chkanemia = new T_ChkAnemia()
                    {

                        ChkID = i,
                        AnsID = model.Cadmiaradio[i - 1],
                        ArogyaID = Session["strArogyaID"].ToString(),
                        Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                        CreateDate = DateTime.Now,
                        UserID = Convert.ToInt32(Session["UserID"])


                    };

                    context.T_ChkAnemia.Add(chkanemia);
                    context.SaveChanges();
                }

                return Json(new { score = k }, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult saveScreenData(Screening model)
        {



            try
            {

                ArogyaParivarEntities context = new ArogyaParivarEntities();




                T_Screenings modelScreenings = new T_Screenings()
                {
                    PresentingComplaint = model.PresentingComplaint,
                    PastMedicalHistory = model.PastMedicalHistory,
                    PastSurgicalHistory = model.PastSurgicalHistory,
                    FamilyHistory = model.FamilyHistory,
                    CurrentMedication = model.CurrentMedication,
                    ECGOutcomeID = model.ECGOutcomeID,
                    ScreenOutcomeID = model.ScreenOutcomeID,
                    ArogyaID = Session["strArogyaID"].ToString(),
                    Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                    ComplaintID = model.ComplaintID,
                    CheckListID = model.chkId,
                    CreateDate = DateTime.Now,
                    UserID = Convert.ToInt32(Session["UserID"]),
                };

                context.T_Screenings.Add(modelScreenings);
                context.SaveChanges();


                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult saveData(int AllrgyTypid, int Reactionid, int Severityid, int Statusid)
        {
            try
            {

                ArogyaParivarEntities context = new ArogyaParivarEntities();

                T_Allergies modelScreening = new T_Allergies()
                  {
                      AllergenID = AllrgyTypid,
                      ReactionID = Reactionid,
                      SeverityID = Severityid,
                      StatusID = Statusid,
                      ArogyaID = Session["strArogyaID"].ToString(),
                      Token_Number = Convert.ToInt32(Session["intTokenNumber"].ToString()),
                      CreateDate = DateTime.Now,
                      UserID = Convert.ToInt32(Session["UserID"])
                  };
                context.T_Allergies.Add(modelScreening);
                context.SaveChanges();
                return Allrgy(Convert.ToInt32(Session["intTokenNumber"]), Session["strArogyaID"].ToString());
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }




        public JsonResult Allrgy(int tokenNumber, string arogyaID)
        {

            ArogyaParivarEntities context = new ArogyaParivarEntities();
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

                         where allergy.Token_Number == tokenNumber
                            && EntityFunctions.TruncateTime(allergy.CreateDate).Value == EntityFunctions.TruncateTime(DateTime.Now).Value
                            && allergy.ArogyaID == arogyaID
                         select new Allergy
                         {
                             AllergyType = at.AllergyType,

                             ReactionName = ar.Reaction,
                             SeverityrName = asev.Severity,
                             Status = ast.M_StatusName,
                             CreateDate = allergy.CreateDate.Value
                             //CreateDateString=allergy.CreateDate.Value.ToString("dd/MM/yyyy")
                         };

            return Json(query2.ToList(), JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult PatientHealthRecord(Screening model, string command)
        {
            ArogyaParivarEntities context = new ArogyaParivarEntities();
            try
            {
                try
                {



                    T_Vitals Vitals = new T_Vitals()
                    {
                        Height = model.Height,
                        Weight = model.Weight,
                        BMI = model.BMI,
                        SysBP = model.SysBP,
                        DiaBP = model.DiaBP,
                        Temparature = model.Temparature,
                        Pulse = model.Pulse,
                        Respiratory = model.Respiratory,
                        Token_Number = 1

                    };

                    T_Screenings screenings = new T_Screenings()
                    {
                        ComplaintID = model.ComplaintID,
                        CheckListID = model.CheckListID,
                        PresentingComplaint = model.PresentingComplaint,
                        PastMedicalHistory = model.PastMedicalHistory,
                        PastSurgicalHistory = model.PastSurgicalHistory,
                        FamilyHistory = model.FamilyHistory,
                        CurrentMedication = model.CurrentMedication,
                        ECGOutcomeID = model.ECGOutcomeID,
                        ScreenOutcomeID = model.ScreenOutcomeID,
                        SpecialtyID = model.SpecialtyID,
                        AppointDate = model.AppointDate,
                        CreateDate = model.CreateDate,
                        Token_Number = 1
                    };
                    context.T_Screenings.Add(screenings);
                    context.SaveChanges();
                    return RedirectToAction("PatientSearch");

                }
                catch
                {
                    return View(model);
                }


            }
            catch
            {
                return View(model);
            }
        }

        //public JsonResult GetPreviousHistory()
        //{

        //}

    }
}
