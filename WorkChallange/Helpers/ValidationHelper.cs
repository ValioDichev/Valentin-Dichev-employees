using WorkChallange.Models;

namespace WorkChallange.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsCsvValid(string filePath, char delimiter = ',')
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Check if file is empty
                    string headerLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        return false;
                    }

                    // Check for any headers and confirm there is more data than only headers
                    string[] fields = headerLine.Split(delimiter);
                    if (fields.Length == 0 && CsvRowCounter(filePath) > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an issue while trying to read the CSV file: {ex.Message}");
                return false;
            }
        }

        public static bool IsCsvFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
        }

        public static int CsvRowCounter(string filePath) 
        {
            try
            {
                int rowCount = 0;

                using (var reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                        rowCount++;
                    }
                }

                return rowCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No actual data in CSV file: {ex.Message}");
                return -1;
            }
        }

        public static List<WorkHistoryModel> ReadEmployeeWorkHistoriesFromCSV(string filePath)
        {
            List<WorkHistoryModel> workHistories = new List<WorkHistoryModel>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    int employeeID, projectID;
                    DateTime dateFrom, dateTo;

                    if (int.TryParse(values[0], out employeeID) &&
                        int.TryParse(values[1], out projectID))
                    {
                        var workHistory = new WorkHistoryModel
                        {
                            EmployeeID = employeeID,
                            ProjectID = projectID,
                            DateFrom = DateTime.TryParse(values[2], out  dateFrom) ? dateFrom : DateTime.Now,
                            DateTo = DateTime.TryParse(values[3], out dateTo) ? dateTo : DateTime.Now
                        };

                        workHistories.Add(workHistory);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping invalid data: {line}");
                    }
                }
            }
            return workHistories;
        }

        public static List<WorkerPairModel> FindLongestWorkedPairs(List<WorkHistoryModel> workHistories)
        {
            Dictionary<Tuple<int, int>, int> pairWorkDays = new Dictionary<Tuple<int, int>, int>();

            // Calculate days worked for each pair of employees on each project
            foreach (var workHistory1 in workHistories)
            {
                foreach (var workHistory2 in workHistories)
                {
                    if (workHistory1.EmployeeID != workHistory2.EmployeeID && workHistory1.ProjectID == workHistory2.ProjectID)
                    {
                        var pair = new Tuple<int, int>(Math.Min(workHistory1.EmployeeID, workHistory2.EmployeeID),
                                                       Math.Max(workHistory1.EmployeeID, workHistory2.EmployeeID));
                        var overlapDays = CalculateOverlapDays(workHistory1.DateFrom, workHistory1.DateTo, workHistory2.DateFrom, workHistory2.DateTo);
                        if (overlapDays > 0)
                        {
                            if (pairWorkDays.ContainsKey(pair))
                                pairWorkDays[pair] += overlapDays;
                            else
                                pairWorkDays[pair] = overlapDays;
                        }
                    }
                }
            }

            int maxDaysWorked = pairWorkDays.Max(x => x.Value);

            var longestWorkedPairs = pairWorkDays.Where(x => x.Value == maxDaysWorked)
                                                 .Select(x => new WorkerPairModel
                                                 {
                                                     EmployeeID1 = x.Key.Item1,
                                                     EmployeeID2 = x.Key.Item2,
                                                     DaysWorked = x.Value
                                                 })
                                                 .ToList();

            return longestWorkedPairs;
        }

        public static int CalculateOverlapDays(DateTime start1, DateTime? end1, DateTime start2, DateTime? end2)
        {
            DateTime end1Value = end1 ?? DateTime.MaxValue;
            DateTime end2Value = end2 ?? DateTime.MaxValue;

            DateTime latestStart = start1 > start2 ? start1 : start2;
            DateTime earliestEnd = end1Value < end2Value ? end1Value : end2Value;

            TimeSpan overlap = earliestEnd - latestStart;

            return (int)Math.Max(overlap.TotalDays, 0);
        }

        //Atempt at all date time format support
        //NOT IMPLEMENTED
        public static string FormatDate(string inputDate)
        {
            DateTime date;
            string[] dateFormats = new string[] {"yyyy-MM-dd","MM/dd/yyyy","dd/MM/yyyy","yyyy/MM/dd","yyyy.MM.dd","dd.MM.yyyy","yyyy MM dd",}; //More date variations are possible and may be added.

            if (DateTime.TryParseExact(inputDate, dateFormats, null, System.Globalization.DateTimeStyles.None, out date))
            {
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                return "Invalid Date";
            }
        }
    }
}

