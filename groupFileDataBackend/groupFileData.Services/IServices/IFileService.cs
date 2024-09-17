using groupFileData.Models.Dtos;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groupFileData.Services.IServices
{
    public interface IFileService
    {
        Task<List<ExcelData>> ReadExcelFileAsync(IFormFile file);

        List<Resource> MapToResources(List<ExcelData> excelDataList);

        object GroupData(IEnumerable<Resource> resources);

    }
}
