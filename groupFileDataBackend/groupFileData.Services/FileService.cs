using groupFileData.Models.Dtos;
using groupFileData.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Collections.Concurrent;
using System.Globalization;


namespace groupFileData.Services
{
    public class FileService:IFileService
    {
        public async Task<List<ExcelData>> ReadExcelFileAsync(IFormFile file)
        {
            var excelDataList = new List<ExcelData>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    var columnCount = worksheet.Dimension.Columns;

                    // headers
                    var columnHeaders = new List<string>();
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var header = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(header))
                        {
                            columnHeaders.Add(header);
                        }
                    }

                    // Read rows
                    for (int row = 2; row < rowCount; row++)
                    {
                        var excelData = new ExcelData();
                        for (int col = 1; col <= columnCount; col++)
                        {
                            var header = columnHeaders[col - 1];
                            var cellValue = worksheet.Cells[row, col].Value;
                            excelData.Data[header] = cellValue;
                        }
                        excelDataList.Add(excelData);
                    }
                }
            }

            return excelDataList;
        }

        public List<Resource> MapToResources(List<ExcelData> excelDataList)
        {
            return excelDataList
                .Select(d => new Resource
                {
                    ServiceDate = d.Data.ContainsKey("Service Date") ? d.Data["Service Date"]?.ToString() : string.Empty,
                    PONumber = d.Data.ContainsKey("PO Number") ? d.Data["PO Number"]?.ToString() : string.Empty,
                    Account = d.Data.ContainsKey("Account") ? d.Data["Account"]?.ToString() : string.Empty,
                    BillingRate = Convert.ToDecimal(d.Data.ContainsKey("Billing Rate Amount") ? d.Data["Billing Rate Amount"] : 0, CultureInfo.InvariantCulture),
                    TotalGross = Convert.ToDecimal(d.Data.ContainsKey("Total Gross") ? d.Data["Total Gross"] : 0, CultureInfo.InvariantCulture),
                    TotalGrossUsd = Convert.ToDecimal(d.Data.ContainsKey("($) Total Gross") ? d.Data["($) Total Gross"] : 0, CultureInfo.InvariantCulture)
                    // CultureInfo.InvariantCulture ==> different formats for numbers 
                })
                .ToList();
        }

        public object GroupData(IEnumerable<Resource> resources)
        {
            // ServiceDate 
            var groupedByServiceDate = resources
                .AsParallel()
                .GroupBy(r => r.ServiceDate)
                .Select(group => new
                {
                    ServiceDate = group.Key,
                    SubtotalBillingRate = group.Sum(r => r.BillingRate),
                    SubtotalGrossSalary = group.Sum(r => r.TotalGross),
                    SubtotalGrossSalaryUsd = group.Sum(r => r.TotalGrossUsd)
                })
                .ToList();

            // PONumber 
            var groupedByPONumber = resources
                .AsParallel()
                .GroupBy(r => r.PONumber)
                .Select(group => new
                {
                    PONumber = group.Key,
                    SubtotalBillingRate = group.Sum(r => r.BillingRate), 
                    SubtotalGrossSalary = group.Sum(r => r.TotalGross),
                    SubtotalGrossSalaryUsd = group.Sum(r => r.TotalGrossUsd)
                })
                .ToList();

            // Account
            var groupedByAccount = resources
                .AsParallel()
                .GroupBy(r => r.Account)
                .Select(group => new
                {
                    Account = group.Key,
                    SubtotalBillingRate = group.Sum(r => r.BillingRate), 
                    SubtotalGrossSalary = group.Sum(r => r.TotalGross),
                    SubtotalGrossSalaryUsd = group.Sum(r => r.TotalGrossUsd)
                })
                .ToList();

            // total for Gross Salaries
            var overallTotalGrossSalary = resources.Sum(r => r.TotalGross);
            var overallTotalGrossSalaryUsd = resources.Sum(r => r.TotalGrossUsd);

            // Return the grouped data
            return new
            {
                GroupedByServiceDate = new
                {
                    TotalBillingRate = groupedByServiceDate.Sum(g => g.SubtotalBillingRate),
                    TotalGrossSalary = groupedByServiceDate.Sum(g => g.SubtotalGrossSalary),
                    TotalGrossSalaryUsd = groupedByServiceDate.Sum(g => g.SubtotalGrossSalaryUsd),
                    Groups = groupedByServiceDate
                },
                GroupedByPONumber = new
                {
                    TotalBillingRate = groupedByPONumber.Sum(g => g.SubtotalBillingRate),
                    TotalGrossSalary = groupedByPONumber.Sum(g => g.SubtotalGrossSalary),
                    TotalGrossSalaryUsd = groupedByServiceDate.Sum(g => g.SubtotalGrossSalaryUsd),
                    Groups = groupedByPONumber
                },
                GroupedByAccount = new
                {
                    TotalBillingRate = groupedByAccount.Sum(g => g.SubtotalBillingRate),
                    TotalGrossSalary = groupedByAccount.Sum(g => g.SubtotalGrossSalary),
                    TotalGrossSalaryUsd = groupedByServiceDate.Sum(g => g.SubtotalGrossSalaryUsd),
                    Groups = groupedByAccount
                },
                OverallTotalGrossSalary = overallTotalGrossSalary,
                OverallTotalGrossSalaryUsd = overallTotalGrossSalaryUsd
            };
        }

    }
}