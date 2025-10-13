using BillingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace BillingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly string _filePath;

        public InvoiceController(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Billing", "TaxInvoiceFormat.xlsx");
        }

        // ✅ Create new invoice
        [HttpPost]
        public IActionResult Create([FromBody] InvoiceModel model)
        {
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            int newRow = lastRow + 1;

            MapModelToRow(ws, newRow, model);
            workbook.Save();

            return Ok(new { message = "Invoice created successfully", row = newRow });
        }

        // ✅ Get all invoices
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<InvoiceModel>();
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                list.Add(RowToModel(row));
            }

            return Ok(list);
        }

        // ✅ Get by RefNo
        [HttpGet("ref/{refNo}")]
        public IActionResult GetByRefNo(int refNo)
        {
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                if (row.Cell(1).GetValue<int>() == refNo)
                    return Ok(RowToModel(row));
            }
            return NotFound(new { message = $"RefNo {refNo} not found" });
        }

        // ✅ Get by InvoiceNo
        [HttpGet("invoiceNo/{invoiceNo}")]
        public IActionResult GetByInvoiceNo(string invoiceNo)
        {
            string decodedInvoiceNo = Uri.UnescapeDataString(invoiceNo); //to handle "/" character in invoiceNo

            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                if (row.Cell(2).GetValue<string>().Equals(decodedInvoiceNo, StringComparison.OrdinalIgnoreCase))
                    return Ok(RowToModel(row));
            }
            return NotFound(new { message = $"InvoiceNo {invoiceNo} not found" });
        }

        // ✅ Full Update by RefNo
        [HttpPut("ref/{refNo}")]
        public IActionResult UpdateByRefNo(int refNo, [FromBody] InvoiceModel model)
        {
            return UpdateInvoice("RefNo", refNo.ToString(), model);
        }

        // ✅ Full Update by InvoiceNo
        [HttpPut("invoiceNo/{invoiceNo}")]
        public IActionResult UpdateByInvoiceNo(string invoiceNo, [FromBody] InvoiceModel model)
        {
            string decodedInvoiceNo = Uri.UnescapeDataString(invoiceNo);
            return UpdateInvoice("InvoiceNo", decodedInvoiceNo, model);
        }

        // ✅ Partial update (PATCH) by RefNo
        [HttpPatch("ref/{refNo}")]
        public IActionResult PatchByRefNo(int refNo, [FromBody] Dictionary<string, object> updates)
        {
            return PatchInvoice("RefNo", refNo.ToString(), updates);
        }

        // ✅ Delete by RefNo
        [HttpDelete("ref/{refNo}")]
        public IActionResult DeleteByRefNo(int refNo)
        {
            return DeleteInvoice("RefNo", refNo.ToString());
        }

        // ✅ Delete by InvoiceNo
        [HttpDelete("invoiceNo/{invoiceNo}")]
        public IActionResult DeleteByInvoiceNo(string invoiceNo)
        {
            string decodedInvoiceNo = Uri.UnescapeDataString(invoiceNo);
            return DeleteInvoice("InvoiceNo", decodedInvoiceNo);
        }



        // 🔹 NEW ADDITION: BillUpdate - Insert last submitted invoice into SingleBillSheet for new sheet of billing to update first row in new sheet
        // ✅ BillUpdate endpoint
        [HttpPost("billupdate")]
        public IActionResult BillUpdate([FromBody] InvoiceModel model)
        {
            try
            {
                //string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TaxInvoiceFormat.xlsx");
                //String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Billing", "TaxInvoiceFormat.xlsx");
                
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string filePath = Path.Combine(projectRoot, "Billing", "TaxInvoiceFormat.xlsx");
                Console.WriteLine($"Looking for Excel file at: {filePath}");

                using (var workbook = new XLWorkbook(filePath))
                {
                    var singleBillSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "SingleBillSheet");

                    if (singleBillSheet == null)
                    {
                        // ✅ Create sheet with headers if it doesn't exist
                        singleBillSheet = workbook.AddWorksheet("SingleBillSheet");

                        singleBillSheet.Cell(1, 1).Value = "RefNo";
                        singleBillSheet.Cell(1, 2).Value = "InvoiceNo";
                        singleBillSheet.Cell(1, 3).Value = "InvoiceDate";
                        singleBillSheet.Cell(1, 4).Value = "BillType";
                        singleBillSheet.Cell(1, 5).Value = "OrderNo";
                        singleBillSheet.Cell(1, 6).Value = "OrderDate";
                        singleBillSheet.Cell(1, 7).Value = "TermsPayment";
                        singleBillSheet.Cell(1, 8).Value = "CustomerName";
                        singleBillSheet.Cell(1, 9).Value = "AddressOne";
                        singleBillSheet.Cell(1, 10).Value = "AddressTwo";
                        singleBillSheet.Cell(1, 11).Value = "AddressThree";
                        singleBillSheet.Cell(1, 12).Value = "AddressFour";
                        singleBillSheet.Cell(1, 13).Value = "CustomerPhone";

                        
                        singleBillSheet.Cell(1, 14).Value = "DeliveryName";
                        singleBillSheet.Cell(1, 15).Value = "DelAddressOne";
                        singleBillSheet.Cell(1, 16).Value = "DelAddressTwo";
                        singleBillSheet.Cell(1, 17).Value = "DelAddressThree";
                        singleBillSheet.Cell(1, 18).Value = "DelAddressFour";
                        singleBillSheet.Cell(1, 19).Value = "DeliveryPhone";

                        singleBillSheet.Cell(1, 20).Value = "CustomerGSTNo";
                        singleBillSheet.Cell(1, 21).Value = "GSTState";
                        singleBillSheet.Cell(1, 22).Value = "ItemNo";
                        singleBillSheet.Cell(1, 23).Value = "Description";
                        singleBillSheet.Cell(1, 24).Value = "HSNSAC";
                        singleBillSheet.Cell(1, 25).Value = "Quantity";
                        singleBillSheet.Cell(1, 26).Value = "Rate";
                        singleBillSheet.Cell(1, 27).Value = "PER";
                        singleBillSheet.Cell(1, 28).Value = "GSTPC";
                        singleBillSheet.Cell(1, 29).Value = "RupeesOne";
                        singleBillSheet.Cell(1, 30).Value = "RupeesTwo";
                    }

                    // ✅ Convert invoice model to array
                    var rowData = new object[]
                    {
                model.RefNo,
                model.InvoiceNo,
                model.InvoiceDate,
                model.BillType,
                model.OrderNo,
                model.OrderDate,
                model.TermsPayment,
                model.CustomerName,
                model.AddressOne,
                model.AddressTwo,
                model.AddressThree,
                model.AddressFour,
                model.CustomerPhone,
                model.DeliveryName,
                model.DelAddressOne,
                model.DelAddressTwo,
                model.DelAddressThree,
                model.DelAddressFour,
                model.DeliveryPhone,
                model.CustomerGSTNo,
                model.GSTState,
                model.ItemNo,
                model.Description,
                model.HSNSAC,
                model.Quantity,
                model.Rate,
                model.PER,
                model.GSTPC,
                model.RupeesOne,
                model.RupeesTwo
                    };

                    // ✅ Insert as row 2 (push down older data)
                    singleBillSheet.Row(2).InsertRowsAbove(1);

                    for (int i = 0; i < rowData.Length; i++)
                    {
                        singleBillSheet.Cell(2, i + 1).Value = rowData[i]?.ToString() ?? string.Empty;
                    }

                    workbook.Save();
                }

                return Ok(new { message = "✅ Invoice added to SingleBillSheet (as first row)" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error in BillUpdate: {ex.Message}");
            }
        }

        // 🔹 END OF BillUpdate addition - for new sheet of billing to update first row in new sheet




        private IActionResult UpdateInvoice(string keyType, string keyValue, InvoiceModel model)
        {
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            int targetRow = -1;
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                string value = keyType == "RefNo" ? row.Cell(1).GetValue<int>().ToString() : row.Cell(2).GetValue<string>();
                if (value.Equals(keyValue, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row.RowNumber();
                    break;
                }
            }

            if (targetRow == -1)
                return NotFound(new { message = $"{keyType} {keyValue} not found" });

            MapModelToRow(ws, targetRow, model);
            workbook.Save();

            return Ok(new { message = $"Invoice {keyType} {keyValue} updated successfully" });
        }

        private IActionResult PatchInvoice(string keyType, string keyValue, Dictionary<string, object> updates)
        {
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            int targetRow = -1;
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                string value = keyType == "RefNo" ? row.Cell(1).GetValue<int>().ToString() : row.Cell(2).GetValue<string>();
                if (value.Equals(keyValue, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row.RowNumber();
                    break;
                }
            }

            if (targetRow == -1)
                return NotFound(new { message = $"{keyType} {keyValue} not found" });

            // Apply updates
            var rowToUpdate = ws.Row(targetRow);
            foreach (var kv in updates)
            {
                var cell = kv.Key switch
                {
                    "RefNo" => rowToUpdate.Cell(1),
                    "InvoiceNo" => rowToUpdate.Cell(2),
                    "InvoiceDate" => rowToUpdate.Cell(3),
                    "BillType" => rowToUpdate.Cell(4),
                    "OrderNo" => rowToUpdate.Cell(5),
                    "OrderDate" => rowToUpdate.Cell(6),
                    "TermsPayment" => rowToUpdate.Cell(7),
                    "CustomerName" => rowToUpdate.Cell(8),
                    "AddressOne" => rowToUpdate.Cell(9),
                    "AddressTwo" => rowToUpdate.Cell(10),
                    "AddressThree" => rowToUpdate.Cell(11),
                    "AddressFour" => rowToUpdate.Cell(12),
                    "CustomerPhone" => rowToUpdate.Cell(13),

                    "DeliveryName" => rowToUpdate.Cell(14),
                    "DelAddressOne" => rowToUpdate.Cell(15),
                    "DelAddressTwo" => rowToUpdate.Cell(16),
                    "DelAddressThree" => rowToUpdate.Cell(17),
                    "DelAddressFour" => rowToUpdate.Cell(18),
                    "DeliveryPhone" => rowToUpdate.Cell(19),

                    "CustomerGSTNo" => rowToUpdate.Cell(20),
                    "GSTState" => rowToUpdate.Cell(21),
                    "ItemNo" => rowToUpdate.Cell(22),
                    "Description" => rowToUpdate.Cell(23),
                    "HSNSAC" => rowToUpdate.Cell(24),
                    "Quantity" => rowToUpdate.Cell(25),
                    "Rate" => rowToUpdate.Cell(26),
                    "PER" => rowToUpdate.Cell(27),
                    "GSTPC" => rowToUpdate.Cell(28),
                    "RupeesOne" => rowToUpdate.Cell(29),
                    "RupeesTwo" => rowToUpdate.Cell(30),
                    _ => null
                };

                if (cell != null)
                {
                    object value = kv.Value;

                    if (value == null)
                        cell.Clear();
                    else if (value is string s)
                        cell.Value = s;
                    else if (value is int i)
                        cell.Value = i;
                    else if (value is long l)
                        cell.Value = l;
                    else if (value is double d)
                        cell.Value = d;
                    else if (value is decimal dec)
                        cell.Value = dec;
                    else if (value is DateTime dt)
                        cell.Value = dt;
                    else
                        cell.Value = value.ToString(); // fallback
                }
            }

            workbook.Save();
            return Ok(new { message = $"Invoice {keyType} {keyValue} patched successfully" });
        }


        private IActionResult DeleteInvoice(string keyType, string keyValue)
        {
            using var workbook = new XLWorkbook(_filePath);
            var ws = workbook.Worksheets.Worksheet(1);

            int targetRow = -1;
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                string value = keyType == "RefNo" ? row.Cell(1).GetValue<int>().ToString() : row.Cell(2).GetValue<string>();
                if (value.Equals(keyValue, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row.RowNumber();
                    break;
                }
            }

            if (targetRow == -1)
                return NotFound(new { message = $"{keyType} {keyValue} not found" });

            ws.Row(targetRow).Delete();
            workbook.Save();
            return Ok(new { message = $"Invoice {keyType} {keyValue} deleted successfully" });
        }

        // 🔹 Mapping helpers
        private void MapModelToRow(IXLWorksheet ws, int row, InvoiceModel m)
        {
            ws.Cell(row, 1).Value = m.RefNo;
            ws.Cell(row, 2).Value = m.InvoiceNo;
            ws.Cell(row, 3).Value = m.InvoiceDate;
            ws.Cell(row, 4).Value = m.BillType;
            ws.Cell(row, 5).Value = m.OrderNo;
            ws.Cell(row, 6).Value = m.OrderDate;
            ws.Cell(row, 7).Value = m.TermsPayment;
            ws.Cell(row, 8).Value = m.CustomerName;
            ws.Cell(row, 9).Value = m.AddressOne;
            ws.Cell(row, 10).Value = m.AddressTwo;
            ws.Cell(row, 11).Value = m.AddressThree;
            ws.Cell(row, 12).Value = m.AddressFour;
            ws.Cell(row, 13).Value = m.CustomerPhone;

            ws.Cell(row, 14).Value = m.DeliveryName;
            ws.Cell(row, 15).Value = m.DelAddressOne;
            ws.Cell(row, 16).Value = m.DelAddressTwo;
            ws.Cell(row, 17).Value = m.DelAddressThree;
            ws.Cell(row, 18).Value = m.DelAddressFour;
            ws.Cell(row, 19).Value = m.DeliveryPhone;

            ws.Cell(row, 20).Value = m.CustomerGSTNo;
            ws.Cell(row, 21).Value = m.GSTState;
            ws.Cell(row, 22).Value = m.ItemNo;
            ws.Cell(row, 23).Value = m.Description;
            ws.Cell(row, 24).Value = m.HSNSAC;
            ws.Cell(row, 25).Value = m.Quantity;
            ws.Cell(row, 26).Value = m.Rate;
            ws.Cell(row, 27).Value = m.PER;
            ws.Cell(row, 28).Value = m.GSTPC;
            ws.Cell(row, 29).Value = m.RupeesOne;
            ws.Cell(row, 30).Value = m.RupeesTwo;
        }

        private InvoiceModel RowToModel(IXLRow row)
        {
            return new InvoiceModel
            {
                RefNo = row.Cell(1).GetValue<int?>() ?? 0,
                InvoiceNo = row.Cell(2).GetValue<string>(),
                InvoiceDate = row.Cell(3).TryGetValue<DateTime>(out var invDate) ? invDate : DateTime.MinValue,
                BillType = row.Cell(4).GetValue<string>(),
                OrderNo = row.Cell(5).GetValue<string>(),
                OrderDate = row.Cell(6).TryGetValue<DateTime>(out var ordDate) ? ordDate : DateTime.MinValue,
                TermsPayment = row.Cell(7).GetValue<string>(),
                CustomerName = row.Cell(8).GetValue<string>(),
                AddressOne = row.Cell(9).GetValue<string>(),
                AddressTwo = row.Cell(10).GetValue<string>(),
                AddressThree = row.Cell(11).GetValue<string>(),
                AddressFour = row.Cell(12).GetValue<string>(),
                CustomerPhone = row.Cell(13).GetValue<string>(),
                DeliveryName = row.Cell(14).GetValue<string>(),
                DelAddressOne = row.Cell(15).GetValue<string>(),
                DelAddressTwo = row.Cell(16).GetValue<string>(),
                DelAddressThree = row.Cell(17).GetValue<string>(),
                DelAddressFour = row.Cell(18).GetValue<string>(),
                DeliveryPhone = row.Cell(19).GetValue<string>(),

                CustomerGSTNo = row.Cell(20).GetValue<string>(),
                GSTState = row.Cell(21).GetValue<string>(),
                ItemNo = row.Cell(22).GetValue<string>(),
                Description = row.Cell(23).GetValue<string>(),
                HSNSAC = row.Cell(24).GetValue<string>(),
                Quantity = row.Cell(25).TryGetValue<int>(out var qty) ? qty : 0,
                Rate = row.Cell(26).TryGetValue<decimal>(out var rate) ? rate : 0,
                PER = row.Cell(27).GetValue<string>(),
                GSTPC = row.Cell(28).TryGetValue<decimal>(out var gst) ? gst : 0,
                RupeesOne = row.Cell(29).GetValue<string>(),
                RupeesTwo = row.Cell(30).GetValue<string>()
            };
        }

    }
}
