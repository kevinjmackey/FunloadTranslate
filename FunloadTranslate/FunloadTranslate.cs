using System;
using System.IO;
using CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FunloadTranslate
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "The Funload file to translate")]
        public string InputFile { get; set; }
        [Option('o', "output", Required = false, HelpText = "The translated SQL output file")]
        public string OutputFile { get; set; }
    }
    class FunloadTranslate
    {
        static void Main(string[] args)
        {
            string inputFile = "";
            string outputFile = "";
            Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        inputFile = o.InputFile;
                        outputFile = (o.OutputFile != null) ? o.OutputFile : "";
                    });
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"File {inputFile} does not exist");
                System.Environment.Exit(0);
            }
            if (outputFile.Length == 0)
            {
                string outputDir = $"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}";
                outputFile = $"{outputDir}{Path.GetFileNameWithoutExtension(inputFile)}.sql";
            }
            System.Console.WriteLine($"Input file: {inputFile} \nOutput File: {outputFile}");
            FLParse flp = new FLParse();
            if (flp.ParseFile(inputFile))
            {
                Console.WriteLine("Parsing went well!");
                flp.DumpAst($"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}{Path.GetFileNameWithoutExtension(outputFile)}.JSON");
                WriteFunloadSQL wfs = new WriteFunloadSQL();
                Console.WriteLine("Generating SQL files...");
                wfs.WriteSQL(flp.Ast, $"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}");
            }
            else
            {
                Console.WriteLine("All didn't go well!");
            }
        }
    }
}
