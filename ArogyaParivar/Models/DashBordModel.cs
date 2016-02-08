using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArogyaParivar.Models
{
    public class DashBordModel
    {
        public Nullable<System.DateTime> Tokem_Date { get; set; }
        public int ?TokenNumber { get; set; }
        public string PatientName { get; set; }
        public string ArogyaID { get; set; }
        public int? Age { get; set; }
        public string UserName { get; set; }
        public string GenderName { get; set; }
        public string ArogaId { get; set; }
        public string ComplaintName { get; set; }
        public int screeningoutcomeID { get; set; }
        public string rowcolor { get; set; }
    }

}