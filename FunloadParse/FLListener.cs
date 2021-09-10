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
    public static class StringExtensions
    {
        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            return str.Substring(str.Length - Math.Min(length, str.Length));
        }
        public static string ConvertPeriods(this string str)
        {
            return str.Replace(".", "_");
        }
    }
    class PrimaryConditions
    {
        public string CurrentSelectIdentifier = "";
        public string CurrentSelectValue = "";
        public string CurrentRectype = "";
        public string ComparisonOperator = "=";
    }
    class FLListener : FunloadParserBaseListener
    {
        private const bool TRUE = true;
        private string _flFile;
        private uast.UastNode _uastTree;
        private uast.UastNode _currentParent;
        private uast.UastNode _currentJob;
        private PrimaryConditions _currentPrimaryConditions;
        private Stack _parentStack;
        private bool continuation = false;
        private bool withinPutStatements = false;
        private bool withinForEachStatement = false;

        public string FLFile { set => _flFile = value; }
        public UastNode UastTree { get => _uastTree; }

        public FLListener() => _parentStack = new Stack();
        public FLListener(string flFile)
        {
            _flFile = flFile;
            _parentStack = new Stack();
        }
        private void AddFieldsCollection()
        {
            uast.UastNode fields = new UastNode();
            fields.InternalType = "fl:FieldsCollection";
            _currentJob.AddChild(fields);
            fields.Parent = _currentJob;
            fields.AddRole(Role.COLLECTION);
        }
        private void AddVariablesCollection()
        {
            uast.UastNode variables = new UastNode();
            variables.InternalType = "fl:VariableCollection";
            _currentJob.AddChild(variables);
            variables.Parent = _currentJob;
            variables.AddRole(Role.COLLECTION);
        }
        private uast.UastNode GetFieldsCollection()
        {
            return _currentJob.GetNodeByType("fl:FieldsCollection");
        }
        private uast.UastNode GetVariablesCollection()
        {
            return _currentJob.GetNodeByType("fl:VariableCollection");
        }
        private uast.UastNode GetExistingField(string _fieldToken)
        {
            uast.UastNode returnNode = new UastNode();

            uast.UastNode fields = GetFieldsCollection();

            foreach (uast.UastNode field in fields.Children)
            {
                if (field.RawToken == _fieldToken)
                {
                    returnNode = field;
                    break;
                }
            }
            return returnNode;
        }
        private uast.UastNode GetExistingVariable(string _variableToken)
        {
            uast.UastNode returnNode = new UastNode();

            uast.UastNode variables = GetVariablesCollection();

            foreach (uast.UastNode variable in variables.Children)
            {
                if (variable.RawToken == _variableToken)
                {
                    returnNode = variable;
                    break;
                }
            }
            return returnNode;
        }
        private uast.UastNode ParseOutOccurs(uast.UastNode _field, FunloadParser.Column_nameContext _columnName)
        {
            if(_columnName.ChildCount > 1)
            {
                if(!_field.HasProperty("occurs"))
                {
                    _field.AddProperty("occurs", _columnName.GetChild(1).GetText().Replace("(", "").Replace(")",""));
                    _field.AddProperty("alias", "reoccur");
                }
            }

            return _field;
        }
        public override void EnterFunload_file([NotNull] FunloadParser.Funload_fileContext context)
        {
            _uastTree = new uast.UastNode();
            _uastTree.InternalType = "fl:File";
            _uastTree.Token = _flFile;
            _uastTree.AddRole(uast.Role.FILE);
            _currentParent = _uastTree;
        }
        public override void EnterJob_block([NotNull] FunloadParser.Job_blockContext context) 
        {
            uast.UastNode node = new uast.UastNode();
            node.InternalType = "fl:Job";
            node.Token = "";
            node.AddRole(uast.Role.BLOCK);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            _parentStack.Push(_currentParent);
            _currentParent = node;
            _currentJob = node;

            AddFieldsCollection();
            AddVariablesCollection();
        }
        public override void ExitJob_block([NotNull] FunloadParser.Job_blockContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
            _currentJob = null;
        }
        public override void EnterJob_statement([NotNull] FunloadParser.Job_statementContext context) 
        {
            _currentParent.Token = context.jobname.GetText();
            _currentParent.AddProperty("step", context.stepname.GetText());
            _currentPrimaryConditions = new PrimaryConditions();
        }
        public override void EnterAssignment_statement([NotNull] FunloadParser.Assignment_statementContext context)
        {
            uast.UastNode node = new uast.UastNode();
            node.InternalType = "fl:Assignment";
            node.Token = "";
            node.AddRole(uast.Role.ASSIGNMENT);
            node.AddProperty("withinForEach", withinForEachStatement.ToString());
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            node.AddProperty("withinPUT", withinPutStatements.ToString());
            _parentStack.Push(_currentParent);
            _currentParent = node;

            uast.UastNode variable = new uast.UastNode();
            foreach (var child in context.children)
            {
                if(child.GetType() == typeof(FunloadParser.VariableContext))
                {
                    variable = GetExistingVariable(child.GetText());
                    if(variable.RawInternalType == "Unknown")
                    {
                        variable.Token = child.GetText();
                        variable.InternalType = "fl:Variable";
                        variable.AddRole(Role.VARIABLE);
                        uast.UastNode variables = GetVariablesCollection();
                        variable.Parent = variables;
                        variables.AddChild(variable);
                    }
                    _currentParent.AddChild(variable);
                }
                if (child.GetType() == typeof(FunloadParser.IdentifierContext))
                {
                    variable.AddProperty("value", child.GetText());
                }
                if (child.GetType() == typeof(FunloadParser.Integer_valueContext))
                {
                    variable.AddProperty("value", child.GetText());
                }
                if (child.GetType() == typeof(FunloadParser.FunctionContext))
                {
                    string possibleDataType = "";
                    if(child.GetText().StartsWith("#DATE"))
                    {
                        possibleDataType = "DATE";
                    }
                    if (child.GetText().StartsWith("#SUBSTR"))
                    {
                        possibleDataType = "VARCHAR(255)";
                    }
                    if (child.GetText().StartsWith("#CONCAT"))
                    {
                        possibleDataType = "VARCHAR(255)";
                    }
                    if(!variable.HasProperty("datatype"))
                    {
                        variable.AddProperty("datatype", possibleDataType);
                    }
                }
                if (child.GetType() == typeof(FunloadParser.ExpressionContext))
                {
                    variable.AddProperty("datatype", "int");
                }
            }
            node.AddProperty("operator", context.GetChild(1).GetText());
        }
        public override void ExitAssignment_statement([NotNull] FunloadParser.Assignment_statementContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterPut_statement([NotNull] FunloadParser.Put_statementContext context)
        {
            uast.UastNode node = new uast.UastNode();
            node.InternalType = "fl:Put";
            node.Token = "PUT";
            node.AddRole(uast.Role.WRITE);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            if(_currentPrimaryConditions.CurrentSelectValue != "")
            {
                node.AddProperty("selectValue", _currentPrimaryConditions.CurrentSelectValue);
                node.AddProperty("selectValueComparison", _currentPrimaryConditions.ComparisonOperator);
            }
            if (_currentPrimaryConditions.CurrentRectype != "")
            {
                node.AddProperty("rectype", _currentPrimaryConditions.CurrentRectype);
                node.AddProperty("selectValueComparison", _currentPrimaryConditions.ComparisonOperator);
            }

            _parentStack.Push(_currentParent);
            _currentParent = node;

            withinPutStatements = true;

            foreach (var child in context.children)
            {
                if (child.GetType() == typeof(FunloadParser.Column_nameContext))
                {
                    FunloadParser.Column_nameContext columnCtx = (FunloadParser.Column_nameContext)child;
                    uast.UastNode field = GetExistingField(columnCtx.GetText().Replace(".","_"));
                    if (field.RawInternalType == "Unknown")
                    {
                        uast.UastNode fields = GetFieldsCollection();
                        fields.AddChild(field);

                        field.Token = columnCtx.GetText().Replace(".", "_");
                        field.InternalType = "fl:Field";
                        field.AddRole(Role.IDENTIFIER);
                    }
                    if(!field.HasProperty("output"))
                    {
                        field.AddProperty("output", "true");
                    }
                    field = ParseOutOccurs(field, columnCtx);
                    field.Parent = _currentParent;
                    _currentParent.AddChild(field);
                }
                if (child.GetType() == typeof(FunloadParser.VariableContext))
                {
                    uast.UastNode variable = new UastNode();
                    FunloadParser.VariableContext variableCtx = (FunloadParser.VariableContext)child;
                    variable = GetExistingVariable(variableCtx.GetText());
                    if (variable.RawInternalType == "Unknown")
                    {
                        uast.UastNode variables = GetVariablesCollection();
                        variable.Parent = variables;
                        variables.AddChild(variable);

                        variable.Token = variableCtx.GetText();
                        variable.InternalType = "fl:Variable";
                        variable.AddRole(Role.IDENTIFIER);
                    }
                    if (!variable.HasProperty("output"))
                    {
                        variable.AddProperty("output", "true");
                    }
                    variable.Parent = _currentParent;
                    _currentParent.AddChild(variable);
                }
                if (child.GetType() == typeof(FunloadParser.ConstantContext))
                {
                    uast.UastNode constant = new UastNode();
                    FunloadParser.ConstantContext constantCtx = (FunloadParser.ConstantContext)child;

                    constant.Token = constantCtx.GetText();
                    constant.InternalType = "fl:Constant";
                    constant.AddRole(Role.PRIMITIVE);
                    constant.AddProperty("output", "true");
                    constant.Parent = _currentParent;
                    _currentParent.AddChild(constant);
                }
                if (child.GetType() == typeof(FunloadParser.IdentifierContext))
                {
                    uast.UastNode ident = new UastNode();
                    FunloadParser.IdentifierContext identifierCtx = (FunloadParser.IdentifierContext)child;

                    ident.Token = identifierCtx.GetText();
                    ident.InternalType = "fl:Constant";
                    ident.AddRole(Role.IDENTIFIER);
                    ident.AddProperty("output", "true");
                    ident.Parent = _currentParent;
                    _currentParent.AddChild(ident);
                }
                if (child.GetType() == typeof(FunloadParser.To_output_clauseContext))
                {
                    node.AddProperty("funout", child.GetChild(1).GetText());
                }
            }
            if (!node.HasProperty("funout"))
                node.AddProperty("funout", "DEFAULT");
        }
        public override void ExitPut_statement([NotNull] FunloadParser.Put_statementContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterPosition_clause([NotNull] FunloadParser.Position_clauseContext context) 
        {
            _currentParent.AddProperty("location", context.location.GetText());
        }
        public override void EnterFormat_spec([NotNull] FunloadParser.Format_specContext context) 
        {
            _currentParent.AddProperty("type", context.datatype.Text);
            if (context.ChildCount == 3)
            {
                _currentParent.AddProperty("typeArgs", context.GetChild(2).GetText());
            }
        }
        public override void EnterMissing_default_clause([NotNull] FunloadParser.Missing_default_clauseContext context) 
        {
            _currentParent.AddProperty("missingValue", context.missing_value.GetText());
        }
        public override void EnterSelect_clause([NotNull] FunloadParser.Select_clauseContext context) 
        {
            UastNode field = GetExistingField(context.GetChild(0).GetText().Replace(".", "_"));
            if (field.RawInternalType == "Unknown")
            {
                field.Token = context.GetChild(0).GetText().Replace(".", "_");
                uast.UastNode fields = GetFieldsCollection();
                fields.AddChild(field);

                field.InternalType = "fl:Field";
                field.AddRole(Role.IDENTIFIER);
                field.AddProperty("predicate", "true");
            }
            UastNode node = new UastNode();
            node.InternalType = "fl:Select";
            node.AddRole(Role.SELECT_KEYWORD);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);
            node.AddProperty("selectIdentifier", context.GetChild(1).GetText());
            _currentPrimaryConditions.CurrentSelectIdentifier = context.GetChild(1).GetText();

            _parentStack.Push(_currentParent);
            _currentParent = node;
        }
        public override void ExitEnd_select_clause([NotNull] FunloadParser.End_select_clauseContext context)
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterWhen_clause([NotNull] FunloadParser.When_clauseContext context) 
        {
            string whenValue = "";
            UastNode node = new UastNode();
            node.InternalType = "fl:Case";
            node.AddRole(Role.CASE);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            foreach(var child in context.children)
            {
                if (child.GetType() == typeof(FunloadParser.ConstantContext))
                    whenValue = (whenValue.Length == 0 ? child.GetText() : $"{whenValue},{child.GetText()}");
            }
            if(_currentParent.HasProperty("selectIdentifier") && _currentParent.GetProperty("selectIdentifier").Contains("RECTYPE"))
            {
                _currentPrimaryConditions.CurrentRectype = whenValue.Replace("'","");
            }
            _currentPrimaryConditions.CurrentSelectValue = whenValue.Replace("'", "");
            node.AddProperty("value", _currentPrimaryConditions.CurrentSelectValue);

            _parentStack.Push(_currentParent);
            _currentParent = node;
        }
        public override void ExitWhen_clause([NotNull] FunloadParser.When_clauseContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
            _currentPrimaryConditions.CurrentSelectValue = "";
        }
        public override void EnterOtherwise_clause([NotNull] FunloadParser.Otherwise_clauseContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:Otherwise";
            node.AddRole(Role.DEFAULT);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            _parentStack.Push(_currentParent);
            _currentParent = node;
        }
        public override void ExitOtherwise_clause([NotNull] FunloadParser.Otherwise_clauseContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterFor_statement([NotNull] FunloadParser.For_statementContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:For";
            node.AddRole(Role.FOR);
            node.AddRole(Role.ITERATOR);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            foreach (var child in context.children)
            {
                uast.UastNode variable = GetExistingVariable(child.GetText());
                if (child.GetType() == typeof(FunloadParser.IdentifierContext))
                {
                    if (variable.RawInternalType == "Unknown")
                    {
                        variable.Token = child.GetText();
                        variable.InternalType = "fl:Variable";
                        variable.AddRole(Role.VARIABLE);
                        variable.AddProperty("datatype", "INT");
                        uast.UastNode variables = GetVariablesCollection();
                        variable.Parent = variables;
                        variables.AddChild(variable);
                    }
                    node.AddChild(variable);
                }
                if (child.GetType() == typeof(FunloadParser.Integer_valueContext))
                {
                    variable.AddProperty((variable.HasProperty("initialValue") ? "finalValue" : "initialValue"), child.GetText());
                    node.AddProperty((node.HasProperty("initialValue") ? "finalValue" : "initialValue"), child.GetText());
                }
                if (child.GetType() == typeof(FunloadParser.Column_nameContext))
                {
                    uast.UastNode field = GetExistingField(child.GetText().Replace(".", "_"));
                    if (field.RawInternalType == "Unknown")
                    {
                        uast.UastNode fields = GetFieldsCollection();
                        fields.AddChild(field);

                        field.Token = child.GetText().Replace(".", "_");
                        field.InternalType = "fl:Field";
                        field.AddRole(Role.IDENTIFIER);
                    }
                    field = ParseOutOccurs(field, (FunloadParser.Column_nameContext)child);
                    node.AddChild(field);
                }
            }
        }
        public override void EnterSkip_statement([NotNull] FunloadParser.Skip_statementContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:Skip";
            node.AddRole(Role.NOOP);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);
        }
        public override void EnterIf_statement([NotNull] FunloadParser.If_statementContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:If";
            node.AddRole(Role.IF);
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            node.AddProperty("withinPUT", (withinPutStatements == true ? "true" : "false"));
            _parentStack.Push(_currentParent);
            _currentParent = node;
        }
        public override void EnterEndif_clause([NotNull] FunloadParser.Endif_clauseContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterComplex_conditional_expression([NotNull] FunloadParser.Complex_conditional_expressionContext context) 
        { 
            if(continuation == true)
            {
                continuation = false;
            }
            foreach (var child in context.children)
            {
                if(child.GetType() == typeof(FunloadParser.Conditional_expressionContext))
                {
                    uast.UastNode node = new UastNode();
                    node.InternalType = "fl:Condition";
                    node.AddRole(Role.CONDITION);
                    node.Parent = _currentParent;
                    _currentParent.AddChild(node);

                    FunloadParser.Conditional_expressionContext condition = (FunloadParser.Conditional_expressionContext)child;
                    foreach(var grandChild in condition.children)
                    {
                        if(grandChild.GetType() == typeof(FunloadParser.Column_nameContext))
                        {
                            UastNode field = GetExistingField(grandChild.GetText().Replace(".", "_"));
                            if (field.RawInternalType == "Unknown")
                            {
                                uast.UastNode fields = GetFieldsCollection();

                                field.Token = grandChild.GetText().Replace(".", "_");
                                field.InternalType = "fl:Field";
                                field.AddRole(Role.IDENTIFIER);
                                field.Parent = fields;
                                fields.AddChild(field);
                            }
                            if(!field.HasProperty("predicate"))
                            {
                                field.AddProperty("predicate", "true");
                            }
                            field = ParseOutOccurs(field, (FunloadParser.Column_nameContext)grandChild);
                            node.AddProperty("left_operand", grandChild.GetText());
                        }
                        if (grandChild.GetType() == typeof(FunloadParser.Conditional_operatorContext))
                        {
                            node.AddProperty("operator", grandChild.GetText());
                        }
                        if (grandChild.GetType() == typeof(FunloadParser.ConstantContext))
                        {
                            node.AddProperty("right_operand", grandChild.GetText());
                            if(node.GetProperty("left_operand").EndsWith("RECTYPE"))
                            {
                                _currentPrimaryConditions.CurrentRectype = grandChild.GetText().Replace("'", "");
                                _currentPrimaryConditions.ComparisonOperator = node.GetProperty("operator");
                                node.AddProperty("rectype", _currentPrimaryConditions.CurrentRectype);

                            }
                        }
                        if (grandChild.GetType() == typeof(FunloadParser.VariableContext))
                        {
                            node.AddProperty("right_operand", grandChild.GetText());
                        }
                        if(grandChild.GetType() == typeof(Antlr4.Runtime.Tree.TerminalNodeImpl))
                        {
                            node.AddProperty((grandChild.GetText() == "(" ? "lparen" : "rparen"), grandChild.GetText());
                        }
                        if (grandChild.GetType() == typeof(FunloadParser.ContinuationContext))
                        {
                            continuation = true;
                        }
                    }
                }
                if (child.GetType() == typeof(FunloadParser.ConjunctionContext))
                {
                    uast.UastNode op = new UastNode();
                    op.InternalType = "fl:Conjunction";
                    op.Token = child.GetText();
                    op.Parent = _currentParent;
                    _currentParent.AddChild(op);
                }
                if (child.GetType() == typeof(FunloadParser.ContinuationContext))
                {
                    continuation = true;
                }
            }
        }
        public override void EnterExpression([NotNull] FunloadParser.ExpressionContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:Expression";
            node.AddRole(Role.EXPRESSION);
            node.Token = context.GetText();
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            foreach(var child in context.children)
            {
                UastNode childNode = new UastNode();
                childNode.Token = child.GetText();
                childNode.Parent = node;
                if(child.GetType() == typeof(FunloadParser.VariableContext))
                {
                    childNode.InternalType = "fl:Variable";
                    childNode.AddRole(Role.VARIABLE);
                    childNode.AddRole(Role.OPERAND);
                }
                if(child.GetType() == typeof(Antlr4.Runtime.Tree.TerminalNodeImpl))
                {
                    if (childNode.RawToken == "+" || childNode.RawToken == "-")
                    {
                        childNode.InternalType = "fl:Operator";
                        childNode.AddRole(Role.OPERATOR);
                    }
                    else
                    {
                        childNode.InternalType = "fl:Operator";
                        childNode.AddRole(Role.OPERAND);
                    }
                }
            }
        }
        public override void EnterFunction([NotNull] FunloadParser.FunctionContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:Function";
            node.AddRole(Role.FUNCTION);
            node.Token = $"#{context.GetChild(1).GetText()}";
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            _parentStack.Push(_currentParent);
            _currentParent = node;
        }
        public override void ExitFunction([NotNull] FunloadParser.FunctionContext context) 
        {
            _currentParent = (UastNode)_parentStack.Pop();
        }
        public override void EnterArgs([NotNull] FunloadParser.ArgsContext context)
        {
            if (_currentParent.RawInternalType == "fl:Function")
                _currentParent.AddProperty("arguments", context.GetText());
        }
        public override void EnterArgument([NotNull] FunloadParser.ArgumentContext context) 
        {
            if(_currentParent.RawInternalType != "fl:Job")
            {
                uast.UastNode arg = new UastNode();
                arg.InternalType = "fl:Argument";
                arg.Token = context.GetText();
                arg.AddRole(Role.ARGUMENT);
                arg.Parent = _currentParent;
                _currentParent.AddChild(arg);
            }
        }
        public override void EnterOutput_statement([NotNull] FunloadParser.Output_statementContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:Output";
            node.AddRole(Role.WRITE);
            node.Token = "OUTPUT";
            node.Parent = _currentParent;
            _currentParent.AddChild(node);

            node.AddProperty("funout", context.GetChild(0).GetType() == typeof(FunloadParser.To_output_clauseContext) ? context.GetChild(0).GetChild(1).GetText() : "DEFAULT");

            withinPutStatements = false;
        }
        public override void EnterDsn_expression([NotNull] FunloadParser.Dsn_expressionContext context)
        {
            _uastTree.AddProperty("outputFile", context.output_file.Text);
        }
        public override void EnterOpen_statement([NotNull] FunloadParser.Open_statementContext context)
        {
            string m204File = "";
            foreach(var child in context.children)
            {
                if(child.GetType() == typeof(FunloadParser.IdentifierContext))
                {
                    m204File = child.GetText();
                }
                if(child.GetType() == typeof(FunloadParser.Open_file_listContext))
                {
                    m204File = $"{m204File}{child.GetText().Replace("-","")}";
                }
            }
            _currentJob.AddProperty("m204File", m204File);
        }
        public override void EnterSpecial_funload_statements([NotNull] FunloadParser.Special_funload_statementsContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:SpecialFunloadStatements";
            node.AddRole(Role.UNANNOTATED);
            node.Token = context.GetText();
            node.Parent = _currentParent;
            _currentParent.AddChild(node);
        }
        public override void EnterUnload_all_information_statement([NotNull] FunloadParser.Unload_all_information_statementContext context) 
        {
            UastNode node = new UastNode();
            node.InternalType = "fl:UAI";
            node.AddRole(Role.WRITE);
            node.Token = context.GetText();
            node.Parent = _currentParent;
            _currentParent.AddChild(node);
        }
    }
}
