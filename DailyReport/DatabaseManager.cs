using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DailyReport
{
    class DatabaseManager : IDisposable
    {
        private const string _dbPath = @".\Test.db";
        private SQLiteAsyncConnection _connection;

        //public async Task<List<DailyReportModel>> ReadAllReportsAsync()
        //{
        //    if (_connection == null)
        //        return null;
        //    var query = _connection.Table<DailyReportModel>();
        //    return await query.ToListAsync();
        //}

        //public async Task<DailyReportModel> ReadReportAsync(int reportID)
        //{
        //    if (_connection == null)
        //        return null;
        //    var query = _connection.Table<DailyReportModel>().Where(r => r.Id == reportID);
        //    return await query.FirstAsync();
        //}

        public async Task<List<ProjectReport>> ReadAllProjectsAsync()
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<ProjectReport>();
            return await query.ToListAsync();
        }

        public async Task<ProjectReport> ReadProjectAsync(int id)
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<ProjectReport>().Where(r => r.Id == id);
            return await query.FirstAsync();
        }

        public async Task<List<DailyReportModel>> ReadAllReportsAsync()
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<DailyReportModel>();
            return await query.ToListAsync();
        }

        public async Task<List<Commit>> ReadAllCommitsAsync()
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<Commit>();
            return await query.ToListAsync();
        }

        public async Task<List<Mantis>> ReadAllMantisAsync()
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<Mantis>();
            return await query.ToListAsync();
        }

        public async Task<Commit> ReadCommitAsync(int id)
        {
            if (_connection == null)
                return null;

            var query = _connection.Table<Commit>().Where(r => r.Id == id);
            return await query.FirstAsync();
        }

        public async Task<List<DailyReportModel>> ReadReportAsync(DateTime startDate, DateTime endDate)
        {
            List<DailyReportModel> list = new List<DailyReportModel>();
            if (endDate <= startDate)
                return list;

            var query = _connection.Table<DailyReportModel>().Where(x => x.Date >= startDate && x.Date < endDate);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<PeriodReport>> ReadPeriodReportAsync(DateTime startDate, DateTime endDate)
        {
            string query = "select * from DailyReportModel "
                + "inner join ProjectReport on DailyReportModel.ProjectId = ProjectReport.Id "
                + "where DailyReportModel.Date >= ? "
                + "order by ProjectReport.ProjectName, ProjectReport.Version, DailyReportModel.Date";

            return await _connection.DeferredQueryAsync<PeriodReport>(query, startDate);
        }

        public async Task<IEnumerable<PeriodReport>> ReadAllPeriodReportAsync()
        {
            string query = "select * from DailyReportModel "
                + "inner join ProjectReport on DailyReportModel.ProjectId = ProjectReport.Id "
                //+ "where DailyReportModel.Date >= ? "
                + "order by ProjectReport.ProjectName, ProjectReport.Version, DailyReportModel.Date";

            return await _connection.DeferredQueryAsync<PeriodReport>(query);
        }

        public async Task<bool> WriteAsync<T>(T report) where T : BaseModel
        {
            if (report == null)
                return false;
            if (_connection == null)
                return false;

            int rtn = await _connection.InsertAsync(report);
            return true;
        }

        public async Task<bool> UpdateAsync<T>(T report) where T : BaseModel
        {
            if (report == null)
                return false;
            if (_connection == null)
                return false;

            int rtn = await _connection.UpdateAsync(report);
            return true;
        }

        /// <summary>
        /// 尋找相同的 project，找不到的話就加入 DB
        /// </summary>
        /// <param name="report"></param>
        /// <returns>回傳 project id</returns>
        public async Task<int> TryAddProject(ProjectReport report)
        {
            if (report == null) return -1;

            try
            {
                var query = _connection.Table<ProjectReport>().Where(r => r.ProjectName == report.ProjectName && r.Version == report.Version);
                int count = await query.CountAsync();
                if (count == 0)
                    await WriteAsync(report);
                ProjectReport existingProject = await query.FirstAsync();
                if (existingProject != null)
                    return existingProject.Id;
                else
                    return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        public async Task<List<DailyReportView>> GetViewAsync(DateTime startDate, DateTime endDate)
        {
            List <DailyReportView> reportViewList = new List<DailyReportView>();
            if (endDate < startDate)
                return reportViewList;
            if (startDate <= DateTime.MinValue)
                return reportViewList;

            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1);

            long startUnixTime = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            long endUnixTime = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

            var queryC = _connection.Table<Commit>().Where(c => c.UnixTimestamp >= startUnixTime && c.UnixTimestamp <= endUnixTime);
            List<Commit> commitList = await queryC.ToListAsync();
            commitList.Sort();

            Action splitAction = () =>
            {
                for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
                {
                    long startTS = ((DateTimeOffset)date).ToUnixTimeSeconds();
                    long endTS = ((DateTimeOffset)date.AddDays(1)).ToUnixTimeSeconds();
                    List<Commit> daliyList = commitList.Where(c => c.UnixTimestamp >= startTS && c.UnixTimestamp < endTS)?.ToList();
                    if (daliyList != null && daliyList.Count > 0)
                    {
                        DailyReportView drv = new DailyReportView();
                        drv.Date = date;
                        drv.CommitList = daliyList;
                        reportViewList.Add(drv);
                    }
                }
            };
            await Task.Run(splitAction);

            return reportViewList;
        }

        public async Task WriteReportAsync(DailyReportView drv)
        {
            if (drv == null) return;
            if (drv.CommitList == null) return;

            foreach (Commit c in drv.CommitList)
            {
                await WriteAsync(c);
            }
        }

        public bool Open()
        {
            _connection = OpenConnection(_dbPath);
            if (_connection == null)
                return false;
            else
                return true;
        }

        public async Task<bool> Init()
        {
            //if (File.Exists(_dbPath))
            //{                
            //    return true;
            //}
            //else
            {
                try
                {
                    await CreateTable(OpenConnection(_dbPath));
                    if (File.Exists(_dbPath))
                        return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return false;
        }

        private SQLiteAsyncConnection OpenConnection(string dbPath)
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(dbPath);
            return connection;
        }

        private async Task CreateTable(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync(typeof(Commit), CreateFlags.None);
            await connection.CreateTableAsync(typeof(ProjectReport), CreateFlags.None);
            await connection.CreateTableAsync(typeof(DailyReportModel), CreateFlags.None);
            await connection.CreateTableAsync(typeof(Mantis), CreateFlags.None);            
            await connection.CloseAsync();
        }

        public void Dispose()
        {
            _connection?.CloseAsync();
            _connection = null;
        }
    }
}
