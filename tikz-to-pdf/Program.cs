using System.Runtime.Versioning;
using System.Text;
using CliWrap;

namespace tikz_to_pdf;

/// Main Program class
[SupportedOSPlatform("Windows")]
[SupportedOSPlatform("Linux")]
[SupportedOSPlatform("macOS")]
[SupportedOSPlatform("Android31.0")]
public static class Program
{
    /// <summary>
    /// Renders a tikz picture into a pdf, using a LaTeX template and pdflatex.
    /// </summary>
    /// <param name="argument">
    /// The .tikz file path.
    /// </param>
    /// <param name="png">
    /// Renders also into a png image.
    /// </param>
    /// <param name="keepTempFiles">
    /// If true, temporary files are left in the same directory as the output file.
    /// Otherwise, they are left in a temporary directory.
    /// </param>
    /// <param name="outputNearSource">
    /// If true, the output file is left in the same directory as the input tikz file.
    /// Otherwise, it is instead left in the current directory.
    /// </param>
    /// <param name="fontsize">
    /// Font size used in the LaTeX template.
    /// </param>
    /// <param name="confirm">
    /// If true, prints "Done." to confirm success.
    /// </param>
    /// <param name="verbose">
    /// Prints debug info.
    /// </param>
    public static async Task Main(
        string argument,
        bool png = false,
        bool keepTempFiles = false,
        bool outputNearSource = true,
        string fontsize = "11pt",
        bool confirm = false,
        bool verbose = false
    )
    {
        var tikzFilePath = argument;
        var tikzFileName = Path.GetFileName(argument);

        var sourceDir = Directory.GetParent(tikzFilePath)!.FullName;
        var outDir = outputNearSource ?
            sourceDir :
            Directory.GetCurrentDirectory();
        var workDir = keepTempFiles ?
            outDir :
            Directory.CreateTempSubdirectory().FullName;

        if (verbose) {
            Console.WriteLine($"sourceDir: {sourceDir}");
            Console.WriteLine($"outDir: {outDir}");
            Console.WriteLine($"workDir: {workDir}");
        }

        var texFileName = $"{tikzFileName}.tex";
        var texFilePath = Path.Join(workDir, texFileName);
        if (verbose) {
            Console.WriteLine($"texFilePath: {texFilePath}");
        }

        var templatePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "tex-template.tex");
        var contentMarker = "%! content goes here !%";
        var fontsizeMarker = "%! fontsize !%";

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var tikzFileContent = await File.ReadAllTextAsync(tikzFilePath);
        var texContent = templateContent
            .Replace(contentMarker, tikzFileContent)
            .Replace(fontsizeMarker, fontsize);
        await File.WriteAllTextAsync(texFilePath, texContent);

        var baseName = Path.GetFileNameWithoutExtension(texFilePath);
        var pdfFileName = baseName + ".pdf";
        var outPdfFilePath = Path.Join(outDir, pdfFileName);
        if (verbose) {
            Console.WriteLine($"outPdfFilePath: {outPdfFilePath}");
        }

        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        if (workDir == outDir)
        {
            if( File.Exists(outPdfFilePath) ) {
                File.Delete(outPdfFilePath);
            }
        }

        var result = await Cli.Wrap("pdflatex")
            .WithArguments(new[] {texFilePath})
            .WithWorkingDirectory(workDir)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdOut = stdOutBuffer.ToString();
        var stdErr = stdErrBuffer.ToString();

        // Console.WriteLine(stdOut);
        if (stdErr.Length > 0) {
            Console.WriteLine(stdErr);
        }

        if (workDir != outDir)
        {
            var tmpPdfFilePath = Path.Join(workDir, pdfFileName);
            if (verbose) {
                Console.WriteLine($"tmpPdfFilePath: {tmpPdfFilePath}");
            }
        }

        if (workDir != outDir)
        {
            var tmpPdfFilePath = Path.Join(workDir, pdfFileName);
            if (!File.Exists(tmpPdfFilePath))
            {
                Console.WriteLine("No pdf produced!");
                Environment.Exit(1);
            }
            File.Copy(tmpPdfFilePath, outPdfFilePath, true);
        }

        if (!File.Exists(outPdfFilePath))
        {
            Console.WriteLine("No pdf produced!");
            Environment.Exit(1);
        }

        if (png)
        {
            var pngFileName = baseName + ".png";
            var outPngFilePath = Path.Join(outDir, pngFileName);

            var pdfContent = await File.ReadAllBytesAsync(outPdfFilePath);

            File.Delete(outPngFilePath);
            PDFtoImage.Conversion.SavePng(outPngFilePath, pdfContent);
        }

        if (confirm) {
            Console.WriteLine("Done.");
        }
    }
}