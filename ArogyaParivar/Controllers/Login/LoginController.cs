using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModelLogin = ArogyaParivar.Models.Login;
using ArogyaParivar.Entity;
using EHM.Hybrid.Framework.DAL;
using EHM.Hybrid.Framework.BLL;
using ArogyaParivar.Models;
using System.Data.Objects;
namespace ArogyaParivar.Controllers
{
    public class LoginController : Controller
    {
        LoginBll loginBll = new LoginBll();
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            //Session["UserID"]=null;
            //Session.Abandon();
            Session.Clear();
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel loginModel)
        {


            if (loginModel.UserName != null)
            {
                var jsondata = loginBll.VerifyLoginUser(loginModel.UserName, loginModel.Password);
                jsondata.Display = "none";
                loginModel.ErrorMessage = jsondata.ErrorMessage;

                if (jsondata.UserExist == true)
                {
                    Session["LoginUserDetails"] = jsondata;
                    Session["LoginUser"] = jsondata.UserName;
                    Session["Role"] = jsondata.RolePrives[0].RoleName;
                    Session["UserName"] = jsondata.UserDescription;
                    Session["UserID"] = jsondata.UserId;
                    if (Session["Role"].ToString() == "Reg/Nurse")
                    {
                        IList<DashBordModel> BookList = new List<DashBordModel>();
                        BookList = GetNurseDashbord();
                        return View("../HealthEducator/HEdashboard", BookList);

                    }
                    else
                    {
                         IList<DashBordModel> BookList = new List<DashBordModel>();
                         BookList = GetDoctorDashbord();
                         return View("../Doctor/DoctorDashBoard", BookList);
                    }
                }
                else
                {
                    return View(loginModel);
                }
            }

            else
            {
                return View("../Login/Login");
            }
            // return View();
        }



        public IList<DashBordModel> GetNurseDashbord()
        {
            IList<DashBordModel> BookList = new List<DashBordModel>();
            ArogyaParivarEntities context = new ArogyaParivarEntities();

     



            var query = from token in context.T_Token

                        from reg in context.T_PatientInfo
                        .Where(x=>x.ArogyaID == token.ArogyaID)

                       from scr in context.T_Screenings
                        .Where(x=>x.ArogyaID == token.ArogyaID).DefaultIfEmpty()
                        from gender in context.M_Gender
                        .Where(x => x.ID == reg.FK_GenderID)
                        where  EntityFunctions.TruncateTime( token.Tokem_Date).Value == EntityFunctions.TruncateTime(DateTime.Now).Value &&
                          scr.ArogyaID ==null
                        // where patient.FirstName.Contains(searchString) || patient.SurName.Contains(searchString) || patient.ContactNumber.Contains(searchString)
                        select new DashBordModel
                        {
                            Tokem_Date = token.Tokem_Date,
                            ArogyaID = token.ArogyaID,
                            PatientName = reg.FirstName + reg.SurName,
                            TokenNumber = token.Token_Number,
                            GenderName = gender.Gender,
                            Age = reg.Age
                            

                        };

        
           
            return BookList = query.Distinct().ToList();


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

                        where EntityFunctions.TruncateTime(reg.CreateDate).Value == EntityFunctions.TruncateTime(DateTime.Now).Value && reg.ScreenOutcomeID == 1
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
                            ComplaintName = reg1.M_PresentingComplainting

                        };



            return BookList = query.Distinct().ToList();


        }
    }
}
