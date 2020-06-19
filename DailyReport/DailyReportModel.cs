using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DailyReport
{
    public class PeriodReport : IComparable<PeriodReport>, INotifyPropertyChanged
    {
        private string message;
        public event PropertyChangedEventHandler PropertyChanged;
        public DateTime Date { get; set; }
        public int ReoprtID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }

        /// <summary>
        /// Only this item could fire property changed event. Because other items can't change after saved to DB.
        /// </summary>
        public string Message
        {
            get => message;
            set
            {
                if (value != message)
                {
                    message = value;
                    FirePropertyChanged();
                }
            }
        }

        public string Version { get; set; }
        public string FullDisplayProject
        {
            get
            {
                return ProjectName + " " + Version;
            }
        }
        public string DisplayDate
        {
            get
            {
                return Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public override string ToString()
        {
            return DisplayDate + "\t" + FullDisplayProject + "\t" + Message;
        }

        int IComparable<PeriodReport>.CompareTo(PeriodReport other)
        {
            if (other == null)
                return -1;
            else
            {
                if (ProjectID == other.ProjectID)
                {
                    if (Date == other.Date)
                        return Message.CompareTo(other.Message);
                    else
                        return Date.CompareTo(other.Date);
                }
                else if (ProjectName == other.ProjectName)
                {
                    if (Version == null && other.Version == null)
                        return 0;
                    else if (Version == null)
                        return 1;
                    else if (other.Version == null)
                        return -1;
                    else if (Version == other.Version)
                    {
                        Console.WriteLine("shouldn't go here...");
                        return 0;
                    }
                    else
                        return Version.CompareTo(other.Version);
                }
                else
                {
                    return ProjectName.CompareTo(other.ProjectName);
                }
            }
        }

        private void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
                {
                    if (string.IsNullOrEmpty(ProjectVersion) && string.IsNullOrEmpty(other.ProjectVersion))
                        return 0;
                    if (string.IsNullOrEmpty(ProjectVersion))
                        return 1;
                    return ProjectVersion.CompareTo(other.ProjectVersion);
                }
                else
                    return rtn;
            }                
        }
    }

    public class Mantis : BaseModel
    {
        public DateTime Date { get; set; }
        public string MantisNumber { get; set; }
        public int ProjectId { get; set; }
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
