using EHM.Hybrid.Framework.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArogyaParivar.Models
{
    public class Episode
    {
        public Episode()
        {
            encounterSummaryList = new List<EncounterSummaryModel>();
        }

        public int ComplaintID { get; set; }
        public string ComplaintName { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public List<EncounterSummaryModel> encounterSummaryList { get; set; }
    }
}