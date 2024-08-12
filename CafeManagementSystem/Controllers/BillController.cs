using CafeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Web;
using System.IO;
using System.Net.Http.Handlers;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/bill")]
    public class BillController : ApiController
    {
        CafeMgmtSystemEntities db = new CafeMgmtSystemEntities();
        Response response = new Response();
        private string pdfPath = @"E:\Web Development\Angular\CafeManagementSystem\CafeManagementSystem\BillPDF\";

        [HttpPost, Route("generateReport")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GenerateReport([FromBody] Bill bill)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                var ticks = DateTime.Now.Ticks;
                var guid = Guid.NewGuid().ToString();
                var uniqueId = ticks.ToString() + '-' + guid;
                bill.createdBy = tokenClaim.Email;
                bill.uuid = uniqueId;

                // Save the bill to the database
                db.Bills.Add(bill);
                db.SaveChanges();

                // Generate the PDF and save it to disk
                string filePath = GeneratePdfAndSave(bill);

                // Return the UUID as the response
                return Request.CreateResponse(HttpStatusCode.OK, new { uuid = uniqueId });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string GeneratePdfAndSave(Bill bill)
        {
            string filePath = Path.Combine(pdfPath, $"{bill.uuid}.pdf");

            try
            {
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf);

                    // Add header
                    Paragraph header = new Paragraph("Cafe Management System")
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(30)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY);
                    document.Add(header);

                    // New line
                    document.Add(new Paragraph("\n"));

                    // Line separator
                    LineSeparator ls = new LineSeparator(new SolidLine());
                    document.Add(ls);

                    // Customer details
                    Paragraph customerDetails = new Paragraph()
                        .Add(new Text("Customer Details:\n").SetBold())
                        .Add(new Text($"Name: {bill.name}\n"))
                        .Add(new Text($"Email: {bill.email}\n"))
                        .Add(new Text($"Contact Number: {bill.contactNo}\n"))
                        .Add(new Text($"Payment Method: {bill.paymentMethod}\n"))
                        .SetMarginBottom(20);
                    document.Add(customerDetails);

                    // Table for product details
                    Table table = new Table(5, false);
                    table.SetWidth(new UnitValue(UnitValue.PERCENT, 100));

                    // Table headers
                    string[] headers = { "Name", "Category", "Quantity", "Price", "Subtotal" };
                    foreach (var headerTitle in headers)
                    {
                        table.AddHeaderCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .SetBold().Add(new Paragraph(headerTitle)));
                    }

                    // Parse product details from JSON
                    dynamic productDetail = JsonConvert.DeserializeObject(bill.productDetails);
                    foreach (JObject product in productDetail)
                    {
                        table.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(product["product"].ToString())));
                        table.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(product["category"].ToString())));
                        table.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(product["quantity"].ToString())));
                        table.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(product["price"].ToString())));
                        table.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(product["total"].ToString())));
                    }

                    // Add table to document
                    document.Add(table);

                    // Add total amount
                    if (decimal.TryParse(bill.totalAmount.ToString(), out decimal totalAmount))
                    {
                        Paragraph totalAmountParagraph = new Paragraph()
                            .Add(new Text("\n\nTotal Amount: ").SetBold().SetFontSize(12))
                            .Add(new Text(totalAmount.ToString("C2")).SetBold().SetFontSize(12)
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.GREEN))
                            .Add(new Text("\n\nThank you for visiting. Please visit again!\n").SetItalic());
                        document.Add(totalAmountParagraph);
                    }
                    else
                    {
                        // Invalid total amount
                        document.Add(new Paragraph("Invalid total amount")
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
                    }

                    // Close the document
                    document.Close();
                }

                return filePath; // Return the path where the PDF is saved
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.ToString());
                throw; // Consider rethrowing the exception if necessary
            }
        }


        [HttpPost, Route("getPdf")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetPdf([FromBody] Bill bill)
        {
            try
            {
                // Validate the UUID and retrieve the file path
                if (!string.IsNullOrEmpty(bill.uuid))
                {
                    string filePath = Path.Combine(pdfPath, $"{bill.uuid}.pdf");

                    // Check if the file exists
                    if (File.Exists(filePath))
                    {
                        byte[] bytes = File.ReadAllBytes(filePath);

                        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(bytes)
                        };

                        response.Content.Headers.ContentLength = bytes.Length;
                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = $"{bill.uuid}.pdf"
                        };
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                        return response;
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "PDF not found");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid UUID");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet,Route("getAllBills")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllBills()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    var userResult = db.Bills.Where(x => x.createdBy == tokenClaim.Email).AsEnumerable().Reverse();
                    return Request.CreateResponse(HttpStatusCode.OK, userResult);
                }
                var adminResult = db.Bills.AsEnumerable().Reverse();
                return Request.CreateResponse(HttpStatusCode.OK, adminResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpDelete,Route("deleteBill/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage DeleteBill(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Bill bill = db.Bills.Find(id);
                if(bill == null)
                {
                    response.Message = "Bill Not Found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                db.Bills.Remove(bill);
                db.SaveChanges();
                response.Message = "Bill Deleted Suceesfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
    }
}
