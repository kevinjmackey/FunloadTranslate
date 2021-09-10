using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using uast;

namespace FunloadTranslate
{
    public class FLVisitor : FunloadParserBaseVisitor<bool>
    {
        private const bool TRUE = true;
        private string _flFile;
        private uast.UastNode _uastTree;
        private uast.UastNode _currentParent;
        private Stack _parentStack;

        public string FLFile { set => _flFile = value; }
        public UastNode UastTree { get => _uastTree; }

        public FLVisitor() => _parentStack = new Stack();
        public FLVisitor(string flFile)
        {
            _flFile = flFile;
            _parentStack = new Stack();
        }
        public override bool VisitFunload_file([NotNull] FunloadParser.Funload_fileContext context)
        {
            _uastTree = new uast.UastNode();
            _uastTree.InternalType = "fl:File";
            _uastTree.Token = _flFile;
            _uastTree.AddRole(uast.Role.FILE);
            _currentParent = _uastTree;
            bool tf = VisitChildren(context);

            return TRUE;
        }
        public override bool VisitJcl_statement([NotNull] FunloadParser.Jcl_statementContext context)
        {
            Console.WriteLine(context.GetText());
            bool tf = VisitChildren(context);

            return TRUE;
        }
        public override bool VisitDsn_expression([NotNull] FunloadParser.Dsn_expressionContext context) 
        {
            _currentParent.AddProperty("outputFile", context.output_file.Text);

            return TRUE; 
        }
    }
}