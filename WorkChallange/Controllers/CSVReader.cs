using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WorkChallange.Models;

using WorkChallange.Helpers;

namespace WorkChallange.Controllers
{
    public class CSVReader : Controller
    {
        private readonly ILogger<CSVReader> _logger;

        public CSVReader(ILogger<CSVReader> logger)
        {
            _logger = logger;
        }
        public bool IsUploadSuccessful { get; set; } = false;
        public IActionResult ReadCSV()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReadCSV(IFormFile file) 
        {
            //Create a storage folder and save the file in the root folder of the app
            if (file != null && file.Length > 0)
            {
                string storageFolder = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Storage";

                if (!Directory.Exists(storageFolder))
                {
                    Directory.CreateDirectory(storageFolder);
                }

                var filePath = Path.Combine(storageFolder, file.FileName);

                //Confirm the specific file type before storage
                if (ValidationHelper.IsCsvFile(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
               
                if (ValidationHelper.IsCsvValid(filePath))
                {
                    IsUploadSuccessful = true;
                    ViewBag.UploadStatus = IsUploadSuccessful;
                    ViewBag.ValidationMessage = "The CSV file was successfuly uploaded";

                    List<WorkHistoryModel> workHistories = ValidationHelper.ReadEmployeeWorkHistoriesFromCSV(filePath);
                    List<WorkerPairModel> longestWorkedPairs = ValidationHelper.FindLongestWorkedPairs(workHistories);
                    
                    if (longestWorkedPairs.Any())
                    {
                        WorkerPairModel longestPair = longestWorkedPairs.First();
                        ViewBag.WorkerPair = longestPair;
                    }
                }
                else
                {
                    ViewBag.UploadStatus = IsUploadSuccessful;
                    ViewBag.ValidationMessage = "The file format is wrong or the CSV file is empty";
                }
                
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
