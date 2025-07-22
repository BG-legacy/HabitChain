using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Export Management Controller
/// 
/// This controller handles all data export operations including:
/// - CSV export generation and download
/// - PDF export generation and download
/// - Data aggregation for export purposes
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - Users can only export their own data
/// - User ID is extracted from JWT token claims for security
/// - Prevents unauthorized access to other users' data
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - Export data ownership verification ensures data privacy
/// - File generation is done server-side for security
/// - Rate limiting should be considered for export operations
/// - Large exports should be paginated or limited in scope
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Export User Data as CSV
    /// 
    /// Generates and returns a CSV file containing the user's habit tracking data.
    /// The user ID is extracted from the JWT token claims, ensuring users can only export their own data.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from request parameters
    /// - Data filtering is applied based on user ownership
    /// 
    /// Use case: Downloading user data for analysis in spreadsheet applications
    /// </summary>
    [HttpPost("csv")]
    public async Task<IActionResult> ExportCSV([FromBody] ExportOptionsDto options)
    {
        try
        {
            // Extract authenticated user ID from JWT token
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Validate export options
            if (options == null)
            {
                return BadRequest(new { message = "Export options are required." });
            }

            // Generate CSV data
            var csvData = await _exportService.GenerateCSVAsync(authenticatedUserId, options);
            
            if (string.IsNullOrEmpty(csvData))
            {
                return BadRequest(new { message = "No data available for export." });
            }

            // Prepare file download
            var fileName = $"habitchain-export-{DateTime.UtcNow:yyyy-MM-dd}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csvData);
            
            return File(fileBytes, "text/csv", fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the error (implement logging service)
            Console.WriteLine($"Error exporting CSV: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while generating the export." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Export User Data as PDF
    /// 
    /// Generates and returns a PDF file containing the user's habit tracking data with formatting.
    /// The user ID is extracted from the JWT token claims, ensuring users can only export their own data.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from request parameters
    /// - Data filtering is applied based on user ownership
    /// 
    /// Use case: Downloading formatted reports for sharing or printing
    /// </summary>
    [HttpPost("pdf")]
    public async Task<IActionResult> ExportPDF([FromBody] ExportOptionsDto options)
    {
        try
        {
            // Extract authenticated user ID from JWT token
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Validate export options
            if (options == null)
            {
                return BadRequest(new { message = "Export options are required." });
            }

            // Generate PDF data
            var pdfData = await _exportService.GeneratePDFAsync(authenticatedUserId, options);
            
            if (pdfData == null || pdfData.Length == 0)
            {
                return BadRequest(new { message = "No data available for export." });
            }

            // Prepare file download
            var fileName = $"habitchain-export-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
            
            return File(pdfData, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the error (implement logging service)
            Console.WriteLine($"Error exporting PDF: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while generating the export." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Export Preview Data
    /// 
    /// Returns a summary of the data that would be included in an export based on the given options.
    /// This allows users to preview what will be exported before generating the actual file.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token
    /// - Data filtering is applied based on user ownership
    /// 
    /// Use case: Frontend preview of export data before actual export
    /// </summary>
    [HttpPost("preview")]
    public async Task<ActionResult<ExportPreviewDto>> GetExportPreview([FromBody] ExportOptionsDto options)
    {
        try
        {
            // Extract authenticated user ID from JWT token
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Validate export options
            if (options == null)
            {
                return BadRequest(new { message = "Export options are required." });
            }

            // Get preview data
            var preview = await _exportService.GetExportPreviewAsync(authenticatedUserId, options);
            
            return Ok(preview);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the error (implement logging service)
            Console.WriteLine($"Error getting export preview: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while generating the preview." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Available Export Formats
    /// 
    /// Returns information about the available export formats and their capabilities.
    /// This is useful for frontend applications to display format options to users.
    /// 
    /// Use case: Displaying export format options in the frontend
    /// </summary>
    [HttpGet("formats")]
    public ActionResult<ExportFormatsDto> GetExportFormats()
    {
        var formats = new ExportFormatsDto
        {
            Formats = new List<ExportFormatInfo>
            {
                new ExportFormatInfo
                {
                    Format = "CSV",
                    Description = "Comma-separated values format for spreadsheet applications",
                    MimeType = "text/csv",
                    Extension = ".csv",
                    Features = new List<string>
                    {
                        "Raw data export",
                        "Compatible with Excel, Google Sheets",
                        "Easy data manipulation",
                        "Lightweight file size"
                    }
                },
                new ExportFormatInfo
                {
                    Format = "PDF",
                    Description = "Formatted document for sharing and printing",
                    MimeType = "application/pdf",
                    Extension = ".pdf",
                    Features = new List<string>
                    {
                        "Professional formatting",
                        "Charts and visualizations",
                        "Print-ready layout",
                        "Universal compatibility"
                    }
                }
            }
        };

        return Ok(formats);
    }

    /// <summary>
    /// TEST ENDPOINT - Generate a test PDF
    /// 
    /// This endpoint generates a simple test PDF to verify the PDF generation is working.
    /// </summary>
    [HttpGet("test-pdf")]
    public IActionResult TestPDF()
    {
        try
        {
            // Create a simple test PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Text("HABITCHAIN TEST PDF").FontSize(20).Bold();
                            row.ConstantItem(50).Height(50).Background(Colors.Blue.Lighten1).AlignCenter().AlignMiddle().Text("📊").FontSize(24);
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content.Column(column =>
                        {
                            column.Item().Text("This is a test PDF generated by HabitChain").FontSize(16).Bold();
                            column.Item().Height(20);
                            column.Item().Text($"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}").FontSize(10);
                            column.Item().Height(20);
                            column.Item().Text("If you can see this PDF, the PDF generation is working correctly!").FontSize(12);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "habitchain-test.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating test PDF: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while generating the test PDF." });
        }
    }
} 