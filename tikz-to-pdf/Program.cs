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
    /// <param name="verbose">
    /// Prints debug info.
    /// </param>
    public static async Task Main(
        string argument,
        bool png = false,
        bool keepTempFiles = false, 
        bool outputNearSource = true, 
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
        var placeholder = "%! content goes here !%";

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var tikzFileContent = await File.ReadAllTextAsync(tikzFilePath);
        var texContent = templateContent.Replace(placeholder, tikzFileContent);
        await File.WriteAllTextAsync(texFilePath, texContent);

        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

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
        Console.WriteLine(stdErr);

        if (workDir != outDir)
        {
            var baseName = Path.GetFileNameWithoutExtension(texFilePath);
            var pdfFileName = baseName + ".pdf";
            var tmpPdfFilePath = Path.Join(workDir, pdfFileName);
            var outPdfFilePath = Path.Join(outDir, pdfFileName);
            File.Copy(tmpPdfFilePath, outPdfFilePath, true);            
        }

        if (png)
        {
            var baseName = Path.GetFileNameWithoutExtension(texFilePath);
            var pdfFileName = baseName + ".pdf";
            var outPdfFilePath = Path.Join(outDir, pdfFileName);
            var pngFileName = baseName + ".png";
            var outPngFilePath = Path.Join(outDir, pngFileName);

            var pdfContent = File.ReadAllBytes(outPdfFilePath);
            
            PDFtoImage.Conversion.SavePng(outPngFilePath, pdfContent);
        }

        Console.WriteLine("Done.");
    }
}