using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DailyReport
{
    public class PeriodReport
    {
        public DateTime Date { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string Message { get; set; }
        public string Version { get; set; }
    }

    public abstract class BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }

    public class DailyReportView
    {
        public DateTime Date;
        public List<Commit> CommitList;
    }

    //public class DailyReportModel : BaseModel
    //{
    //    public DateTime Date { get; set; }
    //    public string Lists { get; set; }
    //    [Ignore]
    //    public int[] CommitList
    //    {
    //        get
    //        {
    //            return Lists.Split(',').Select(x => int.Parse(x)).ToArray();
    //        }
    //        set
    //        {
    //            if (value == null)
    //                Lists = string.Empty;
    //            else
    //                Lists = string.Join(",", value);
    //        }
    //    }
    //}

    public class AccumulatedReport : IComparable<AccumulatedReport>
    {
        public int ProjectId;
        public string ProjectName;
        public string ProjectVersion;
        public List<string> ProjectContent;
        public HashSet<string> MantisList;

        public AccumulatedReport()
        {
            ProjectId = 0;
            ProjectName = string.Empty;
            ProjectVersion = string.Empty;
            ProjectContent = new List<string>();
            MantisList = new HashSet<string>();
        }

        public new string ToString()
        {
            string message = ProjectName + " " + ProjectVersion + ":\n";
            int i = 0;
            for (i=0; i<ProjectContent.Count; i++)
            {
                ProjectContent[i] = ProjectContent[i].TrimStart(' ');
                message += (i + 1).ToString() + ". " + ProjectContent[i] + Environment.NewLine;
            }
            if (MantisList.Count > 0)
                message += (i + 1).ToString() + ". 修正 Mantis 問題：" + string.Join("、", MantisList) + "。" + Environment.NewLine;
            return message;
        }

        int IComparable<AccumulatedReport>.CompareTo(AccumulatedReport other)
        {
            if (other == null)
                return -1;
            else
            {
                int rtn = ProjectName.CompareTo(other.ProjectName);
                if (rtn == 0)
                    return ProjectVersion.CompareTo(other.ProjectVersion);
                else
                    return rtn;
            }                
        }
    }

    public class Mantis : BaseModel
    {
        public DateTime Date { get; set; }
        public string MantisNumber { get; set; }
    }

    public class DailyReportModel : BaseModel
    {
        public DateTime Date { get; set; }
        public int ProjectId { get; set; }
        public string Message { get; set; }
    }

    public class ProjectReport : BaseModel
    {
        public string ProjectName { get; set; }
        public string Version { get; set; }
    }

    public class Commit : BaseModel, IComparable<Commit>
    {
        public int ProjectId { get; set; }
        public long UnixTimestamp { get; set; }        
        public string Message { get; set; }
        public string Author { get; set; }
        public string Hash { get; set; }

        public int CompareTo(Commit obj)
        {
            if (obj == null)
                return -1;
            return UnixTimestamp.CompareTo(obj.UnixTimestamp);
        }
    }
}
