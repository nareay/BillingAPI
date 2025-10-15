using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BillingAPI.Controllers
{
    [ApiController]
    [Route("api/download")]
    public class DownloadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DownloadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("excel")]
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "TaxInvoiceFormat.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found on server" });

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        "TaxInvoiceFormat.xlsx");
        }
    }
}
