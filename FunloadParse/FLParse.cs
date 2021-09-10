using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using uast;

namespace FunloadTranslate
{
    public class FLParse
    {
        private uast.UastNode _ast;
        public uast.UastNode Ast { get => _ast; set => _ast = value; }

        private string ReadFile(string _filepath)
        {
            StringBuilder sb = new StringBuilder();
            if (File.Exists(_filepath))
            {
                IEnumerable<String> textLines = File.ReadLines(_filepath);
                foreach (string line in textLines)
                {
                    sb.Append(line);
                }
            }
            else
            {
                System.Console.WriteLine($"File: {_filepath} not found!");
            }
            return sb.ToString();
        }
        public bool ParseFile(string _filePath)
        {
            bool result = false;
            try
            {
                ICharStream chars = CharStreams.fromPath(_filePath);
                FunloadLexer lexer = new FunloadLexer(chars);
                CommonTokenStream stream = new CommonTokenStream(lexer);
                FunloadParser parser = new FunloadParser(stream);

                var tree = parser.funload_file();

                ParseTreeWalker walker = new ParseTreeWalker();

                FLListener listener = new FLListener();

                listener.FLFile = Path.GetFileName(_filePath);
                walker.Walk(listener, tree);

                _ast = listener.UastTree;

                result = true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
            }

            return result;
        }
        public void DumpAst(string _filePath)
        {
            using (FileStream fs = new FileStream(_filePath, FileMode.Create))
            {
                UastNode.Dumps(_ast).CopyTo(fs);
                fs.Flush();
            }
        }
    }
}
