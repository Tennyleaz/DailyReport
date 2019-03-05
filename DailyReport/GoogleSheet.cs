using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Globalization;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DailyReport
{
    internal class GoogleSheet : IDisposable
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        private static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private static readonly string ApplicationName = "Tenny's Report";
        private static readonly string spreadsheetId = "1psHUDLpksJoGdsino3fl-dYbV7GM7kksYHZ597etXSY";
        private SheetsService sheetService;
        private int currentRow;

        public bool Init()
        {
            UserCredential credential;
            if (File.Exists("credentials.json"))
            {
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                sheetService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                return true;                
            }
            else
                MessageBox.Show("Missing credentials.json!");
            return false;
        }

        public async Task<SheetPreview> GetSheetPreviewAsync()
        {
            // Define request parameters.            
            SpreadsheetsResource.ValuesResource.BatchGetRequest request =
                    sheetService.Spreadsheets.Values.BatchGet(spreadsheetId);
            request.Ranges = CalculateRange();
            request.ValueRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            BatchGetValuesResponse response = await request.ExecuteAsync();
            if (response.ValueRanges != null && response.ValueRanges.Count > 0)
            {
                ValueRange valueRange = response.ValueRanges[0];  // I only need first value.
                IList<IList<object>> values = valueRange.Values;  // values is a 2x6 array, 3rd row might be empty!
                if (values != null && values.Count >= 2)
                {
                    SheetPreview sheetPreview = new SheetPreview();
                    IList<object> row0 = values[0];
                    IList<object> row1 = values[1];                    
                    if (row0 != null && row0.Count >= 6)
                    {
                        sheetPreview.PreviousWeekDate = row0[0].ToString();
                        sheetPreview.PreviousWeekContent = row0[5].ToString();
                    }
                    if (row1 != null && row1.Count >= 6)
                    {
                        sheetPreview.PreviousWeekPlan = row1[5].ToString();
                    }
                    // 3rd row (this week) might be empty if no one have filled.
                    if (values.Count >= 3)
                    {
                        IList<object> row2 = values[2];
                        if (row2 != null && row2.Count >= 6)
                        {
                            sheetPreview.NextWeekDate = row2[0].ToString();
                            sheetPreview.NextWeekContent = row2[5].ToString();
                        }
                    }
                    return sheetPreview;
                }
            }
            return null;
        }

        private int CalculateRow()
        {
            int offset = 21;
            DateTime date = DateTime.Today;
            // Return the week of our adjusted day
            // 2019-Jan-04 (Fri), 1st week, is the 27th row in google sheet
            // 2019-Feb-27 (Wed), 9th week, is the 41st row in google sheet
            int weekCount = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekCount * 2 + offset;
        }

        private string CalculateRange()
        {
            currentRow = CalculateRow();
            string range = "'數位開發部'!A" + currentRow + ":F" + (currentRow + 2);
            return range;
        }

        public async Task<bool> UpdateSheetAsync(List<RowData> vData)
        {
            int targetRow = currentRow + 1;  // this is 0-based value!
            UpdateCellsRequest cellRequest = new UpdateCellsRequest();
            cellRequest.Fields = "UserEnteredValue";
            cellRequest.Rows = vData;
            cellRequest.Range = new GridRange()
            {
                SheetId = 288934192,  // 數位開發部 sheet id
                StartRowIndex = targetRow,
                EndRowIndex = targetRow + 2,
                StartColumnIndex = 5,
                EndColumnIndex = 6            
            };
            // make a request
            BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request
            {
                UpdateCells = cellRequest
            });
            // execute request
            try
            {
                var batchUpdateRequest = sheetService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId);
                var response = await batchUpdateRequest.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;  
        }

        public void Dispose()
        {
            sheetService?.Dispose();
        }
    }

    public class SheetPreview
    {
        public string PreviousWeekDate;
        public string PreviousWeekContent;
        public string PreviousWeekPlan;
        public string NextWeekDate;        
        public string NextWeekContent;
    }    
}
