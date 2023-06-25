using System.Text;
using CliWrap;

var tikzFilePath = args[0];
var texFilePath = $"{tikzFilePath}.tex";
var templatePath = "tex-template.tex";
var placeholder = "%! content goes here !%";

var templateContent = File.ReadAllText(templatePath);
var tikzFileContent = File.ReadAllText(tikzFilePath);
var texContent = templateContent.Replace(placeholder, tikzFileContent);
File.WriteAllText(texFilePath, texContent);

var stdOutBuffer = new StringBuilder();
var stdErrBuffer = new StringBuilder();

var result = await Cli.Wrap("pdflatex")
    .WithArguments(new[] {texFilePath})
    .WithValidation(CommandResultValidation.None)
    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
    .ExecuteAsync();

var stdOut = stdOutBuffer.ToString();
var stdErr = stdErrBuffer.ToString();

// Console.WriteLine(stdOut);
Console.WriteLine(stdErr);

Console.WriteLine("Done.");