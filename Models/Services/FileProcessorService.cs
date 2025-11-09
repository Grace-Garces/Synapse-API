using System.Globalization;
using System.Text;
using CsvHelper;
using OfficeOpenXml;
using UglyToad.PdfPig;

namespace BrainAPI.Services;

/// <summary>
/// Serviço para extrair texto puro de diferentes tipos de arquivos (PDF, CSV, Excel).
/// </summary>
public class FileProcessorService
{
    private readonly ILogger<FileProcessorService> _logger;

    // ← ADICIONE O CONSTRUTOR COM LOGGER
    public FileProcessorService(ILogger<FileProcessorService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        _logger.LogInformation($"Detectado tipo de arquivo: {fileExtension}");

        using var stream = file.OpenReadStream();

        try
        {
            switch (fileExtension)
            {
                case ".pdf":
                    _logger.LogInformation("Processando arquivo PDF...");
                    return ExtractTextFromPdf(stream);
                    
                case ".csv":
                    _logger.LogInformation("Processando arquivo CSV...");
                    return await ExtractTextFromCsvAsync(stream);
                    
                case ".xlsx":
                    _logger.LogInformation("Processando arquivo XLSX...");
                    return ExtractTextFromExcelAsync(stream);
                    
                case ".txt":
                    _logger.LogInformation("Processando arquivo TXT...");
                    return await new StreamReader(stream).ReadToEndAsync();
                    
                default:
                    throw new NotSupportedException($"Formato de arquivo '{fileExtension}' não é suportado. Use: PDF, CSV, XLSX ou TXT.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao extrair texto do arquivo: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Extrai texto de um arquivo PDF.
    /// </summary>
    private string ExtractTextFromPdf(Stream stream)
    {
        try
        {
            var content = new StringBuilder();
            
            // Certifique-se de que o stream está no início
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using (var document = PdfDocument.Open(stream))
            {
                int pageCount = document.NumberOfPages;
                _logger.LogInformation($"PDF tem {pageCount} páginas");

                foreach (var page in document.GetPages())
                {
                    content.AppendLine(page.Text);
                }
            }
            return content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao extrair PDF: {ex.Message}");
            throw new InvalidOperationException("Não foi possível processar o arquivo PDF. Verifique se o arquivo não está corrompido.", ex);
        }
    }

    /// <summary>
    /// Extrai texto de um arquivo CSV, formatando-o como texto legível.
    /// </summary>
    private async Task<string> ExtractTextFromCsvAsync(Stream stream)
    {
        try
        {
            var content = new StringBuilder();
            
            // Certifique-se de que o stream está no início
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Lê o cabeçalho
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                _logger.LogInformation($"CSV tem {headers?.Length ?? 0} colunas");
                
                if (headers == null || headers.Length == 0)
                {
                    throw new InvalidOperationException("O arquivo CSV não contém cabeçalhos válidos.");
                }

                content.AppendLine("Cabeçalhos: " + string.Join(", ", headers ?? Array.Empty<string>()));
                content.AppendLine("---");

                // Lê as linhas
                int lineCount = 0;
                while (await csv.ReadAsync())
                {
                    var recordLine = new List<string>();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var value = csv.GetField(headers[i]);
                        recordLine.Add($"{headers[i]}: {value}");
                    }
                    content.AppendLine(string.Join(" | ", recordLine));
                    lineCount++;
                }

                _logger.LogInformation($"CSV processado: {lineCount} linhas");
            }
            return content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao extrair CSV: {ex.Message}");
            throw new InvalidOperationException("Não foi possível processar o arquivo CSV. Verifique o formato.", ex);
        }
    }

    /// <summary>
    /// Extrai texto de um arquivo Excel (.xlsx), lendo todas as planilhas.
    /// </summary>
    private string ExtractTextFromExcelAsync(Stream stream)
    {
        try
        {
            var content = new StringBuilder();
            
            // Certifique-se de que o stream está no início
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            // ← CORREÇÃO 1: Defina a licença CORRETAMENTE para EPPlus 8+
            // Se usar EPPlus < 8, descomente a linha com LicenseContext
            // Se usar EPPlus >= 8, use ExcelPackage.License
            
            try
            {
                // Tente a forma nova (EPPlus 8+)
                // ExcelPackage.License = "seu-license-aqui"; // Se tiver license
                
                // Para desenvolvimento, use:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            }
            catch
            {
                // Se der erro, tente a forma antiga
                _logger.LogWarning("Versão do EPPlus pode ser incompatível");
            }

            using (var package = new ExcelPackage(stream))
            {
                int sheetCount = package.Workbook.Worksheets.Count;
                _logger.LogInformation($"Excel tem {sheetCount} planilha(s)");

                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    content.AppendLine($"=== Planilha: {worksheet.Name} ===");
                    
                    // Validação simples para não ler planilhas vazias
                    if (worksheet.Dimension == null)
                    {
                        _logger.LogWarning($"Planilha '{worksheet.Name}' está vazia");
                        continue;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    
                    _logger.LogInformation($"  Planilha '{worksheet.Name}': {rowCount} linhas x {colCount} colunas");

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var rowContent = new List<string>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            // Obtém o valor da célula
                            var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                rowContent.Add(cellValue.Trim());
                            }
                        }
                        
                        if (rowContent.Any())
                        {
                            content.AppendLine(string.Join(" | ", rowContent));
                        }
                    }
                    content.AppendLine("---"); // Separa as planilhas
                }
            }
            
            return content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao extrair Excel: {ex.GetType().Name} - {ex.Message}");
            throw new InvalidOperationException("Não foi possível processar o arquivo Excel. Verifique se o arquivo é válido e se a licença do EPPlus está configurada.", ex);
        }
    }
}