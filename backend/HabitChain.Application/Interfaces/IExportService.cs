using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

/// <summary>
/// Service interface for data export operations
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Generates CSV data for user export
    /// </summary>
    /// <param name="userId">The ID of the user requesting the export</param>
    /// <param name="options">Export configuration options</param>
    /// <returns>CSV content as string</returns>
    Task<string> GenerateCSVAsync(string userId, ExportOptionsDto options);

    /// <summary>
    /// Generates PDF data for user export
    /// </summary>
    /// <param name="userId">The ID of the user requesting the export</param>
    /// <param name="options">Export configuration options</param>
    /// <returns>PDF content as byte array</returns>
    Task<byte[]> GeneratePDFAsync(string userId, ExportOptionsDto options);

    /// <summary>
    /// Gets a preview of what would be included in an export
    /// </summary>
    /// <param name="userId">The ID of the user requesting the preview</param>
    /// <param name="options">Export configuration options</param>
    /// <returns>Export preview information</returns>
    Task<ExportPreviewDto> GetExportPreviewAsync(string userId, ExportOptionsDto options);

    /// <summary>
    /// Fetches and aggregates user data for export
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="options">Export configuration options</param>
    /// <returns>Aggregated export data</returns>
    Task<ExportDataDto> GetUserExportDataAsync(string userId, ExportOptionsDto options);
} 