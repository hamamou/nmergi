using PdfSharp.Pdf.IO;

namespace nmergi;

public class PdfMerger
{
    public string OutputFileName { get; }
    private IPdfDocumentWrapper OutputDocumentWrapper { get; }
    private IFileUtilities FileUtilities { get; }
    private IPdfReader PdfReader { get; }

    public PdfMerger(
        IPdfDocumentWrapper outputDocumentWrapper,
        IPdfReader pdfReader,
        IFileUtilities fileUtilities,
        string? outputFileName
    )
    {
        OutputDocumentWrapper =
            outputDocumentWrapper ?? throw new ArgumentNullException(nameof(outputDocumentWrapper));
        PdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        FileUtilities = fileUtilities ?? throw new ArgumentNullException(nameof(fileUtilities));
        OutputFileName = outputFileName ?? FileUtilities.GetTempPdfFullFileName("merged");
    }

    public void MergePdfs(string[] pdfPaths)
    {
        if (pdfPaths == null || pdfPaths.Length == 0)
            throw new ArgumentException("PDF paths cannot be null or empty.", nameof(pdfPaths));

        foreach (var path in pdfPaths)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "A provided PDF path is null or empty.",
                    nameof(pdfPaths)
                );

            var filePaths = FileUtilities.GetPdfFilePaths(path);
            AddFileContentToPdf(OutputDocumentWrapper, filePaths);
        }

        OutputDocumentWrapper.Save(OutputFileName);
        FileUtilities.ShowDocument(OutputFileName);
    }

    private void AddFileContentToPdf(IPdfDocumentWrapper outputDocument, IList<string> pdfFilePaths)
    {
        if (outputDocument == null)
            throw new ArgumentNullException(nameof(outputDocument));
        if (pdfFilePaths == null || pdfFilePaths.Count == 0)
            throw new ArgumentException("No PDF file paths were provided.", nameof(pdfFilePaths));

        foreach (var filePath in pdfFilePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A file path is null or empty.", nameof(pdfFilePaths));

            var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            if (inputDocument == null || inputDocument.PageCount == 0)
                throw new InvalidOperationException(
                    $"Input PDF document at '{filePath}' is null or has no pages."
                );

            for (var i = 0; i < inputDocument.PageCount; i++)
            {
                var page = inputDocument.Pages?[i];
                if (page == null)
                    throw new InvalidOperationException(
                        $"Page {i} in document '{filePath}' is null."
                    );

                outputDocument.AddPage(page);
            }
        }
    }
}
