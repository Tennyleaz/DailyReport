using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DailyReport
{
    class WebDBManager : IDisposable
    {
        private MyHttpClient _httpClient;

        public WebDBManager(string serverURL)
        {
            serverURL.TrimEnd('/');
            _httpClient = new MyHttpClient(serverURL);
        }

        #region read single table
        public async Task<List<ProjectReport>> ReadAllProjectsAsync()
        {
            if (_httpClient == null)
                return null;

            return await _httpClient.QueryAllAsync<ProjectReport>("Project");
        }

        public async Task<ProjectReport> ReadProjectAsync(int id)
        {
            if (_httpClient == null)
                return null;

            return await _httpClient.QueryByIdAsync<ProjectReport>("Project", id);
        }

        /*public async Task<List<DailyReportModel>> ReadReportAsync(DateTime startDate, DateTime endDate)
        {
            if (_httpClient == null)
                return null;

            List<DailyReportModel> list = new List<DailyReportModel>();
            if (endDate <= startDate)
                return list;

            string[] timeArgs = { };
            return await _httpClient.QueryByIdAsync<ProjectReport>("PeriodReport", id);
        }*/

        public async Task<List<PeriodReport>> ReadAllPeriodReportAsync()
        {
            if (_httpClient == null)
                return null;

            List<PeriodReport> list = new List<PeriodReport>();
            return await _httpClient.QueryAsync<PeriodReport>("PeriodReport");
        }

        public async Task<List<PeriodReport>> ReadPeriodReportAsync(DateTime startDate, DateTime endDate)
        {
            if (_httpClient == null)
                return null;

            List<PeriodReport> list = new List<PeriodReport>();
            if (endDate <= startDate)
                return list;

            string[] timeArgs = {
                startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            return await _httpClient.QueryAsync<PeriodReport>("PeriodReport", timeArgs);
        }
        #endregion

        #region write single table
        public async Task<T> AddNew<T>(T t, string objectName)
        {
            if (t == null)
                return default(T);

            return await _httpClient.HttpPostAsync<T>(t, objectName);
        }

        public async Task<bool> Update<T>(T t, string objectName, int id)
        {
            if (t == null)
                return false;

            var response = await _httpClient.HttpPutAsync<T>(t, objectName, id);
            return response.Success;
        }

        public async Task<bool> Delete(string objectName, int id)
        {
            var response = await _httpClient.HttpDeleteAsync(objectName, id);
            return response;
        }
        #endregion

        /// <summary>
        /// 尋找相同的 project，找不到的話就加入 DB
        /// </summary>
        /// <param name="report"></param>
        /// <returns>回傳 project id</returns>
        public async Task<int> TryAddProjectAsync(ProjectReport report)
        {
            if (report == null) return -1;

            try
            {
                report = await _httpClient.HttpPostAsync<ProjectReport>(report, "Project/TryAdd");
                return report.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
