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
                    });
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"File {inputFile} does not exist");
                System.Environment.Exit(0);
            }
            if (outputFile.Length == 0)
            {
                string outputDir = $"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}";
                outputFile = $"{outputDir}{Path.GetFileNameWithoutExtension(inputFile)}.JSON";
            }
            System.Console.WriteLine($"Input file: {inputFile} \nMetadata File: {outputFile}");
            System.Console.WriteLine($"Translation started at: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            FLParse flp = new FLParse();
            if (flp.ParseFile(inputFile))
            {
                Console.WriteLine("Parsing went well!"); 
                flp.DumpAst($"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}{Path.GetFileNameWithoutExtension(outputFile)}.JSON");
                WriteFunloadSQL wfs = new WriteFunloadSQL();
                Console.WriteLine("Generating SQL files...");
                wfs.WriteSQL(flp.Ast, $"{Path.GetDirectoryName(Path.GetFullPath(inputFile))}{Path.DirectorySeparatorChar.ToString()}");
                System.Console.WriteLine($"Translation ended at: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            }
            else
            {
                Console.WriteLine("All didn't go well!");
            }
        }
    }
}
