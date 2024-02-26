# tikz-to-pdf

Command line utility to directly compile .tikz files into .pdf and .png.

## Use case

TikZ is a great tool to make high-quality figures in LaTeX documents.
However, it does not allow one to see what a figure looks like until it is compiled within a document, which may slow down the workflow for larger documents and/or collaborative work.
For example, if somewhere else in the document there is a syntax mistake, compilation becomes impossible and one has to fixed that before the figure can be seen.

This tool is meant to remove this dependency, allowing one to iterate on the figures independently of the main document.

Put in `figure.tikz` the code for your figure, then run

```
tikz-to-pdf figure.tikz --png
```

You will then find `figure.tikz.pdf` and `figure.tikz.png` in the same folder.

## Requirements

Building this project requires .Net 8.0 SDK.
The program launches `pdflatex`, which needs to be installed on available from PATH.

The `install.ps1` PowerShell script works only on Windows.
