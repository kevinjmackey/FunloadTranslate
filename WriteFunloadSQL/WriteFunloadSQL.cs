using System;
using System.Text;
using System.Collections.Generic;
using Antlr4.StringTemplate;
using uast;
using FunloadMetadata.Models;
using System.Linq;

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
        public static string GetRepeatedCharacter(string _character, int _repeatCount)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _repeatCount; i++)
                sb.Append(_character);
            return sb.ToString();
        }
    }
    public class VariableType
    {
        public string name { get; set; }
        public string datatype { get; set; }
        public string initialValue { get; set; }
        public bool isInitialValueFunction { get; set; }
    }
    public class AssignmentStatement
    {
        public string lhs { get; set; }
        public string rhs { get; set; }
        public bool NotNeeded { get; set; }
    }
    public class OutputValue
    {
        public string outputType { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string dataType { get; set; }
        public int position { get; set; }
        public int length { get; set; }
        public string rectype { get; set; }
        public string funout { get; set; }
        public string missingValue { get; set; }
        public string formatString { get; set; }
        public string alignment { get; set; }
        public bool occurs { get; set; }
        public string occno { get; set; }
        public string outputString { get; set; }
    }
    public class SortColumn
    {
        public string Column { get; set; }
        public string order { get; set; }
    }
    public class SortValue
    {
        public int start { get; set; }
        public int length { get; set; }
        public string order { get; set; }
    }
    public class WriteFunloadSQL
    {
        private Antlr4.StringTemplate.TemplateGroup stg;
        private Dictionary<string, string> functions = new Dictionary<string, string>();
        private Dictionary<string, string> m204FilesDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> columnDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> conditionalOperators = new Dictionary<string, string>();
        private Dictionary<string, MFDTableDTO> mfdFromClauses = new Dictionary<string, MFDTableDTO>();
        private Dictionary<string, List<string>> whenConditions = new Dictionary<string, List<string>>();
        private Dictionary<string, string> rectypeValues = new Dictionary<string, string>();
        private string selectColumn = "";
        private List<string> fileGroups = new List<string>();
        private List<string> outputFileList = new List<string>();
        private List<UastNode> putStatementList = new List<UastNode>();
        private List<UastNode> ifStatementList = new List<UastNode>();
        private List<UastNode> conditionList = new List<UastNode>();
        private List<UastNode> assignmentStatementList = new List<UastNode>();
        private List<UastNode> sortStatementList = new List<UastNode>();
        private List<M204FileDTO> m204FileList = new List<M204FileDTO>();
        private List<MFDTableDTO> mfdTables = new List<MFDTableDTO>();
        private List<MFDTableColumnDTO> mfdColumnList = new List<MFDTableColumnDTO>();
        private List<OutputValue> outputValues = new List<OutputValue>();
        private List<VariableType> variables = new List<VariableType>();
        private List<OutputValue> outputList = new List<OutputValue>();
        private List<SortValue> sortValuesList = new List<SortValue>();
        private List<SortColumn> sortColumnList = new List<SortColumn>();
        private bool sortBySubstring = false;
        private bool sortByColumn = false;
        private string previousConjuction = "";

        public WriteFunloadSQL()
        {
            LoadFunctionDictionary();
            LoadM205FileDictionary();
            LoadConditionalOperatorsDictionary();
            LoadColumnDictionary();
        }
        private void LoadM205FileDictionary()
        {
            m204FilesDictionary["ASHB"] = "ASH";
            m204FilesDictionary["BUS"] = "BUS";
            m204FilesDictionary["BAC"] = "BAC";
            m204FilesDictionary["BSP"] = "BSP";
            m204FilesDictionary["BSRA"] = "BSR";
            m204FilesDictionary["BSRB"] = "BSR";
            m204FilesDictionary["BSRC"] = "BSR";
            m204FilesDictionary["BSRD"] = "BSR";
            m204FilesDictionary["BSRE"] = "BSR";
            m204FilesDictionary["BSRF"] = "BSR";
            m204FilesDictionary["BSRG"] = "BSR";
            m204FilesDictionary["BSRH"] = "BSR";
            m204FilesDictionary["BSRI"] = "BSR";
            m204FilesDictionary["BSRJ"] = "BSR";
            m204FilesDictionary["BSRK"] = "BSR";
            m204FilesDictionary["BSRL"] = "BSR";
            m204FilesDictionary["BSRM"] = "BSR";
            m204FilesDictionary["BSRN"] = "BSR";
            m204FilesDictionary["BSRO"] = "BSR";
            m204FilesDictionary["BSRP"] = "BSR";
            m204FilesDictionary["BUSB"] = "BUS";
            m204FilesDictionary["BUSAUDIT"] = "BUSAUDIT";
            m204FilesDictionary["BSRHS01"] = "BSR_HIST";
            m204FilesDictionary["BSRHS02"] = "BSR_HIST";
            m204FilesDictionary["BSRHS03"] = "BSR_HIST";
            m204FilesDictionary["BSRHS05"] = "BSR_HIST";
            m204FilesDictionary["BSRHS06"] = "BSR_HIST";
            m204FilesDictionary["BSRHS07"] = "BSR_HIST";
            m204FilesDictionary["BSRHS08"] = "BSR_HIST";
            m204FilesDictionary["BSRHS09"] = "BSR_HIST";
            m204FilesDictionary["BSRHS10"] = "BSR_HIST";
            m204FilesDictionary["BSRHS11"] = "BSR_HIST";
            m204FilesDictionary["BSRHS12"] = "BSR_HIST";
            m204FilesDictionary["BSRHS13"] = "BSR_HIST";
            m204FilesDictionary["BSRHS14"] = "BSR_HIST";
            m204FilesDictionary["BSRHS15"] = "BSR_HIST";
            m204FilesDictionary["BSRHS16"] = "BSR_HIST";
            m204FilesDictionary["BSRHS17"] = "BSR_HIST";
            m204FilesDictionary["BSRHS18"] = "BSR_HIST";
            m204FilesDictionary["BSRHS19"] = "BSR_HIST";
            m204FilesDictionary["BSRHS20"] = "BSR_HIST";
            m204FilesDictionary["BSRHS21"] = "BSR_HIST";
            m204FilesDictionary["BSRHS22"] = "BSR_HIST";
            m204FilesDictionary["BSRHS23"] = "BSR_HIST";
            m204FilesDictionary["BSRHS24"] = "BSR_HIST";
            m204FilesDictionary["BSRHS25"] = "BSR_HIST";
            m204FilesDictionary["BSRHS26"] = "BSR_HIST";
            m204FilesDictionary["BSRHS27"] = "BSR_HIST";
            m204FilesDictionary["BSRHS28"] = "BSR_HIST";
            m204FilesDictionary["BSRHS29"] = "BSR_HIST";
            m204FilesDictionary["BSRHS30"] = "BSR_HIST";
            m204FilesDictionary["BSRHS31"] = "BSR_HIST";
            m204FilesDictionary["BSRHS32"] = "BSR_HIST";
            m204FilesDictionary["BSRHS33"] = "BSR_HIST";
            m204FilesDictionary["BSRHS34"] = "BSR_HIST";
            m204FilesDictionary["BSRHS35"] = "BSR_HIST";
            m204FilesDictionary["BSRHS36"] = "BSR_HIST";
            m204FilesDictionary["CARE"] = "CARE";
            m204FilesDictionary["CAR"] = "CAR";
            m204FilesDictionary["CARS"] = "CARS";
            m204FilesDictionary["CCD"] = "CCD";
            m204FilesDictionary["CCS"] = "CCS";
            m204FilesDictionary["CIF"] = "CIF";
            m204FilesDictionary["CLT"] = "CLT";
            m204FilesDictionary["CINV"] = "CINV";
            m204FilesDictionary["COLA"] = "COLA";
            m204FilesDictionary["COLC"] = "COLC";
            m204FilesDictionary["COLE"] = "COLE";
            m204FilesDictionary["COLM"] = "COLM";
            m204FilesDictionary["COLP"] = "COLP";
            m204FilesDictionary["CRN"] = "CRN";
            m204FilesDictionary["CRNA"] = "CRN";
            m204FilesDictionary["CRNB"] = "CRN";
            m204FilesDictionary["CRNC"] = "CRN";
            m204FilesDictionary["CRND"] = "CRN";
            m204FilesDictionary["CRNE"] = "CRN";
            m204FilesDictionary["CRNF"] = "CRN";
            m204FilesDictionary["CRNG"] = "CRN";
            m204FilesDictionary["CRNH"] = "CRN";
            m204FilesDictionary["CRNI"] = "CRN";
            m204FilesDictionary["CRNJ"] = "CRN";
            m204FilesDictionary["CRNK"] = "CRN";
            m204FilesDictionary["CRNL"] = "CRN";
            m204FilesDictionary["CRNM"] = "CRN";
            m204FilesDictionary["CRNN"] = "CRN";
            m204FilesDictionary["CRNO"] = "CRN";
            m204FilesDictionary["CRNP"] = "CRN";
            m204FilesDictionary["CSR"] = "CSR";
            m204FilesDictionary["CSVC"] = "CSVC";
            m204FilesDictionary["CUR"] = "CUR";
            m204FilesDictionary["DXL"] = "DXL";
            m204FilesDictionary["ECP"] = "ECP";
            m204FilesDictionary["ECM"] = "ECM";
            m204FilesDictionary["EQP"] = "EQP";
            m204FilesDictionary["EQPSTAT"] = "EQPSTAT";
            m204FilesDictionary["FAC"] = "FAC";
            m204FilesDictionary["GBL"] = "GBL";
            m204FilesDictionary["GIF"] = "GIF";
            m204FilesDictionary["IMGXX"] = "IMGX";
            m204FilesDictionary["INV"] = "INV";
            m204FilesDictionary["INVB"] = "INV";
            m204FilesDictionary["INVC"] = "INV";
            m204FilesDictionary["INVCAFA"] = "INVCAF";
            m204FilesDictionary["INVCAFB"] = "INVCAF";
            m204FilesDictionary["MNFST"] = "MNFST";
            m204FilesDictionary["NAD"] = "NAD";
            m204FilesDictionary["OSDA"] = "OSDA";
            m204FilesDictionary["OSDD"] = "OSDD";
            m204FilesDictionary["OSDE"] = "OSDE";
            m204FilesDictionary["OSDN"] = "OSDN";
            m204FilesDictionary["PCF"] = "PCF";
            m204FilesDictionary["PCM"] = "PCM";
            m204FilesDictionary["PMA"] = "PMA";
            m204FilesDictionary["RIP"] = "RIP";
            m204FilesDictionary["RDWAPSY"] = "RDWAPSY";
            m204FilesDictionary["RCS"] = "RCS";
            m204FilesDictionary["RGN"] = "RGN";
            m204FilesDictionary["RMT"] = "RMT";
            m204FilesDictionary["RMTB"] = "RMT";
            m204FilesDictionary["RMTC"] = "RMT";
            m204FilesDictionary["RIP"] = "RIP";
            m204FilesDictionary["SCHM"] = "SCHM";
            m204FilesDictionary["SHP"] = "SHP";
            m204FilesDictionary["SHPA"] = "SHP";
            m204FilesDictionary["SHPB"] = "SHP";
            m204FilesDictionary["SHPC"] = "SHP";
            m204FilesDictionary["SHPD"] = "SHP";
            m204FilesDictionary["SHPE"] = "SHP";
            m204FilesDictionary["SHPF"] = "SHP";
            m204FilesDictionary["SHPG"] = "SHP";
            m204FilesDictionary["SHPH"] = "SHP";
            m204FilesDictionary["SHPI"] = "SHP";
            m204FilesDictionary["SHPJ"] = "SHP";
            m204FilesDictionary["SHPK"] = "SHP";
            m204FilesDictionary["SHPL"] = "SHP";
            m204FilesDictionary["SHPM"] = "SHP";
            m204FilesDictionary["SHPN"] = "SHP";
            m204FilesDictionary["SHPO"] = "SHP";
            m204FilesDictionary["SHPP"] = "SHP";
            m204FilesDictionary["SHPS"] = "SHPS";
            m204FilesDictionary["SHPXA"] = "SHPX";
            m204FilesDictionary["SLIA"] = "SLI";
            m204FilesDictionary["SLIB"] = "SLI";
            m204FilesDictionary["SLIC"] = "SLI";
            m204FilesDictionary["SLID"] = "SLI";
            m204FilesDictionary["SLIE"] = "SLI";
            m204FilesDictionary["SLIF"] = "SLI";
            m204FilesDictionary["SLIG"] = "SLI";
            m204FilesDictionary["SLIH"] = "SLI";
            m204FilesDictionary["SLII"] = "SLI";
            m204FilesDictionary["SLIJ"] = "SLI";
            m204FilesDictionary["SLIK"] = "SLI";
            m204FilesDictionary["SLIL"] = "SLI";
            m204FilesDictionary["SLIM"] = "SLI";
            m204FilesDictionary["SLIN"] = "SLI";
            m204FilesDictionary["SLIP"] = "SLI";
            m204FilesDictionary["SLIO"] = "SLI";
            m204FilesDictionary["SVC"] = "SVC";
            m204FilesDictionary["TBL"] = "TBL";
            m204FilesDictionary["TFA"] = "TFA";
            m204FilesDictionary["TFI"] = "TFI";
            m204FilesDictionary["TFIB"] = "TFI";
            m204FilesDictionary["TFQ"] = "TFQ";
            m204FilesDictionary["TFQB"] = "TFQ";
            m204FilesDictionary["TWGA"] = "TWG";
            m204FilesDictionary["TWGB"] = "TWG";
            m204FilesDictionary["TWGC"] = "TWG";
            m204FilesDictionary["TWGD"] = "TWG";
            m204FilesDictionary["TWGG"] = "TWG";
            m204FilesDictionary["TWGH"] = "TWG";
            m204FilesDictionary["VTINFOA"] = "VTINFO";
            m204FilesDictionary["VTINFOB"] = "VTINFO";
            m204FilesDictionary["WGP"] = "WGP";
            m204FilesDictionary["WGPA"] = "WGP";
            m204FilesDictionary["WGPB"] = "WGP";
            m204FilesDictionary["WGPC"] = "WGP";
            m204FilesDictionary["WGPD"] = "WGP";
            m204FilesDictionary["WGPE"] = "WGP";
            m204FilesDictionary["WGPF"] = "WGP";
            m204FilesDictionary["WGPG"] = "WGP";
            m204FilesDictionary["WGPH"] = "WGP";
            m204FilesDictionary["WGPI"] = "WGP";
            m204FilesDictionary["WGPJ"] = "WGP";
            m204FilesDictionary["WGPK"] = "WGP";
            m204FilesDictionary["WGPL"] = "WGP";
            m204FilesDictionary["WGPM"] = "WGP";
            m204FilesDictionary["WGPN"] = "WGP";
            m204FilesDictionary["WGPO"] = "WGP";
            m204FilesDictionary["WGPP"] = "WGP";
            m204FilesDictionary["WGPAPT"] = "WGPAPT";
            m204FilesDictionary["WGPAUDI"] = "WGPAUD";
            m204FilesDictionary["WGPAUDD"] = "WGPAUD";
            m204FilesDictionary["WGPAUDC"] = "WGPAUD";
            m204FilesDictionary["WGPAUDB"] = "WGPAUD";
            m204FilesDictionary["WGPAUDA"] = "WGPAUD";
            m204FilesDictionary["WGPAUDH"] = "WGPAUD";
            m204FilesDictionary["WGPAUDG"] = "WGPAUD";
            m204FilesDictionary["WGPAUDF"] = "WGPAUD";
            m204FilesDictionary["WGPAUDE"] = "WGPAUD";
            m204FilesDictionary["WGPAUDP"] = "WGPAUD";
            m204FilesDictionary["WGPAUDO"] = "WGPAUD";
            m204FilesDictionary["WGPAUDN"] = "WGPAUD";
            m204FilesDictionary["WGPAUDM"] = "WGPAUD";
            m204FilesDictionary["WGPAUDL"] = "WGPAUD";
            m204FilesDictionary["WGPAUDK"] = "WGPAUD";
            m204FilesDictionary["XORZ"] = "XORZ";
            m204FilesDictionary["XORZLINK"] = "XORZLINK";
        }
        private void LoadConditionalOperatorsDictionary()
        {
            conditionalOperators["NE"] = "<>";
            conditionalOperators["EQ"] = "=";
            conditionalOperators["LE"] = "<=";
            conditionalOperators["LT"] = "<";
            conditionalOperators["GE"] = ">=";
            conditionalOperators["GT"] = ">";
            conditionalOperators["^="] = "<>";
            conditionalOperators["MISSING"] = "IS NULL";
            conditionalOperators["EXISTS"] = "IS NOT NULL";
        }
        private void LoadColumnDictionary()
        {
            columnDictionary["DATE"] = "DATE_R";
            columnDictionary["TIME"] = "TIME_R";
            columnDictionary["FUNCTION"] = "FUNCTION_R";
            columnDictionary["PROCEDURE"] = "PROCEDURE_R";
            columnDictionary["USER"] = "USER_R";
        }
        private void LoadFunctionDictionary()
        {
            functions["#DATE"] = "GETDATE";
            functions["#DATECNV"] = "CONVERT";
            functions["#DATECHK"] = "CONVERT";
            functions["#DATECHG"] = "DATEADD";
            functions["#TRANSLATE"] = "REPLACE";
            functions["#RECIN"] = "";
            functions["#LEN"] = "LEN";
            functions["#SUBSTR"] = "SUBSTRING";
            functions["#CONCAT"] = "CONCAT";
            functions["#CONCAT_TRUNC"] = "CONCAT"; //with truncate to 255
            functions["#INDEX"] = "CHARINDEX";
            functions["FILENAME"] = "M204 File";
            functions["#DATEDIF"] = "DATEDIFF";
            functions["#ONEOF"] = "still to be worked out"; //Test for existence among a set of possibilities (like EXISTS or IN...likely in a WHERE context)
            functions["#NUM2STR"] = "CAST";
        }
        private List<MFDTableDTO> GetMFDTables(FunloadMetadata _metadata, string _m204File, Dictionary<string, string> _rectypes, bool _rectypesInFile)
        {
            List<MFDTableDTO> tables = _metadata.GetAzureMFDTables(_metadata.MaxMetadataVersion, _m204File);
            List<MFDTableDTO> tablesToRemove = new List<MFDTableDTO>();
            Dictionary<string, string> rectypes = new Dictionary<string, string>();
            foreach (string key in _rectypes.Keys)
                rectypes[StringExtensions.ConvertPeriods(key)] = _rectypes[key];
            bool comparisonEquals = false;
            foreach (string comparison in rectypes.Values)
                if (comparison == "=")
                    comparisonEquals = true;
            if(rectypes.Count > 0)
            {
                foreach(MFDTableDTO table in tables)
                {
                    if (!rectypes.ContainsKey(table.Rectype) && comparisonEquals == true && table.Rectype != "No Rectype")
                        tablesToRemove.Add(table);
                }
                foreach (MFDTableDTO table in tables)
                {
                    if (rectypes.ContainsKey(table.Rectype) && comparisonEquals == false && table.Rectype != "No Rectype")
                        tablesToRemove.Add(table);
                }
                foreach (MFDTableDTO table in tables)
                {
                    if (_rectypesInFile == true && table.Rectype == "No Rectype")
                        tablesToRemove.Add(table);
                }
            }
            if (tablesToRemove.Count > 0)
            {
                foreach (MFDTableDTO table in tablesToRemove)
                    tables.Remove(table);
            }
            return tables;
        }
        private List<MFDTableColumnDTO> GetMFDColumnMetadata(List<MFDTableDTO> _mfdTables, Dictionary<string,string> _rectypes, FunloadMetadata _metadata)
        {
            List<MFDTableColumnDTO> mfdTableColumns = new List<MFDTableColumnDTO>();
            foreach(MFDTableDTO table in _mfdTables)
            {
                Console.WriteLine($"Getting the column list for {table.TableName}...");
                if (_rectypes.Count > 0)
                    foreach (string rectype in _rectypes.Keys)
                    {
                        mfdTableColumns.AddRange(_metadata.GetAzureMFDColumns(_metadata.MaxMetadataVersion, table.M204File,
                            (table.Rectype == "No Rectype" ? table.Rectype : rectype.Replace(".","_"))));
                    }
                else
                    mfdTableColumns.AddRange(_metadata.GetAzureMFDColumns(_metadata.MaxMetadataVersion, table.M204File, "No Rectype"));
            }
            return mfdTableColumns;
        }
        private VariableType GetExistingVariable(string _variableName)
        {
            VariableType variable = new VariableType();

            foreach (VariableType existingVariable in variables)
            {
                if (existingVariable.name == _variableName)
                {
                    variable = existingVariable;
                    break;
                }
            }
            return variable;
        }
        private uast.UastNode GetVariablesCollection(UastNode _jobNode)
        {
            return _jobNode.GetNodeByType("fl:VariableCollection");
        }
        private Dictionary<string,string> GetRectypes(List<UastNode> _putStatementList, List<UastNode> _conditionList)
        {
            Dictionary<string, string> rectypes = new Dictionary<string, string>();
            foreach (UastNode node in _putStatementList)
            {
                if (node.HasProperty("rectype"))
                {
                    string[] rtypes = node.GetProperty("rectype").Split(",");
                    foreach (string rtype in rtypes)
                        if (!rectypes.ContainsKey(rtype))
                            rectypes[rtype] = (conditionalOperators.ContainsKey(node.GetProperty("selectValueComparison")) ? conditionalOperators[node.GetProperty("selectValueComparison")] : node.GetProperty("selectValueComparison"));
                }
            }
            foreach (UastNode node in _conditionList)
            {
                if (node.HasProperty("rectype"))
                {
                    string[] rtypes = node.GetProperty("rectype").Split(",");
                    foreach (string rtype in rtypes)
                    {
                        if (!rectypes.ContainsKey(rtype))
                            rectypes[rtype] = (conditionalOperators.ContainsKey(node.GetProperty("operator")) ? conditionalOperators[node.GetProperty("operator")] : node.GetProperty("operator"));
                    }
                }
            }
            foreach (UastNode node in _putStatementList)
            {
                node.AddProperty("rectype", string.Join(",", rectypes.Keys));
            }
            return rectypes;
        }
        private List<M204FileDTO> GetM204Files(List<string> _fileGroups, FunloadMetadata _metadata)
        {
            List<M204FileDTO> m204Sources = new List<M204FileDTO>();
            List<string> files = new List<string>();
            foreach (string file in _fileGroups)
            {
                if(!files.Contains(m204FilesDictionary[file]))
                {
                    files.Add(m204FilesDictionary[file]);
                }
            }
            foreach(string file in files)
            {
                m204Sources.Add(_metadata.GetAzureFileData(_metadata.MaxMetadataVersion, file));
            }
            return m204Sources;
        }
        private List<VariableType> GetVariables(UastNode _jobNode)
        {
            List<VariableType> variables = new List<VariableType>();
            UastNode variableCollection = _jobNode.GetNodeByType("fl:VariableCollection");
            if (variableCollection != null)
            {
                foreach (var variable in variableCollection.Children)
                {
                    variables.Add(new VariableType()
                    {
                        name = variable.RawToken.Replace("%", ""),
                        datatype = variable.GetProperty("datatype")
                    });
                }
            }
            return variables;
        }
        private List<string> GetTablesList(UastNode _jobNode)
        {
            List<string> tables = new List<string>();
            UastNode fields = _jobNode.GetNodeByType("fl:FieldsCollection");
            foreach(UastNode field in fields.Children)
            {
                if(field.HasProperty("mfdTable") && field.GetProperty("mfdTable") != "Not in Table")
                {
                    if (!tables.Contains(field.GetProperty("mfdTable")))
                        tables.Add(field.GetProperty("mfdTable"));
                }
            }
            tables.Sort();
            return tables;
        }
        private string DeclareVariables(List<VariableType> _variables, TemplateGroup _stg)
        {
            StringBuilder sb = new StringBuilder();
            if(_variables.Count > 0)
            {
                Template variableTemplate = _stg.GetInstanceOf("variable_def");
                variableTemplate.Add("variables", _variables);
                sb.Append(variableTemplate.Render());
            }
            return sb.ToString();
        }
        private string RhsFunction(UastNode _function, string _rhsDataType)
        {
            string rhs = "";
            VariableType variable;
            string sqlFunction = functions[_function.RawToken];
            string[] args = _function.GetProperty("arguments").Replace("(","").Replace(")","").Split(",");
            switch (_function.RawToken)
            {
                case "#DATE":
                    rhs = $"CAST({sqlFunction}() AS DATE)"; //GETDATE needs no arguments
                    break;
                case "#DATECNV":
                    rhs = $"CAST({args[2].Replace("%","@")} AS DATE)"; //#DATECNV is just a date formatting function (I believe)
                    break;
                case "#DATECHG":
                    rhs = $"{sqlFunction}(DD, {args[2].Replace("%", "@")}, {args[1].Replace("%", "@")})"; //Translate to DATEDIFF
                    break;
                case "#SUBSTR":
                    variable = GetExistingVariable(args[0].Replace("%", ""));
                    if (variable.datatype == "DATE")
                        args[0] = $"CONVERT(VARCHAR(8), {args[0].Replace("%", "@")}, 112)";
                    rhs = $"{sqlFunction}({args[0].Replace("%", "@")}, {args[1].Replace("%", "@")}, {args[2].Replace("%", "@")})";
                    break;
                case "#CONCAT":
                    variable = GetExistingVariable(args[0].Replace("%", ""));
                    if (variable.datatype == "DATE")
                        args[0] = $"CONVERT(VARCHAR(8), {args[0].Replace("%", "@")}, 112)";
                    rhs = $"{sqlFunction}({args[0].Replace("%", "@")}, {args[1].Replace("%", "@")})";
                    break;
                case "#TRANSLATE":
                    StringBuilder sbT = new StringBuilder();
                    sbT.Append($"{sqlFunction}(");
                    sbT.Append($"{_function.Children[0].RawToken.Replace("%", "@")},");
                    sbT.Append($"{_function.Children[1].RawToken},");
                    sbT.Append($"{_function.Children[2].RawToken}");
                    sbT.Append(")");
                    rhs = sbT.ToString(); //Translate to REPLACE
                    break;
                case "#NUM2STR":
                    StringBuilder sbN = new StringBuilder();
                    sbN.Append($"{sqlFunction}(");
                    sbN.Append($"{_function.Children[0].RawToken.Replace("%", "@").Replace(".", "_")}");
                    sbN.Append(" AS VARCHAR(");
                    sbN.Append($"{_function.Children[1].RawToken}");
                    sbN.Append("))");
                    rhs = sbN.ToString(); //Translate to CAST(<value> as VARCHAR(<len>)
                    break;
            }
            return (_rhsDataType == "DATE" ? $"CONVERT(VARCHAR(8), {rhs}, 112)" : rhs);
        }
        private List<AssignmentStatement> GetAssignments(List<UastNode> _assignmentStatements)
        {
            List<AssignmentStatement> statements = new List<AssignmentStatement>();
            foreach(UastNode assignmentlStatement in _assignmentStatements)
            {
                AssignmentStatement statement = new AssignmentStatement();
                VariableType variable = null;
                foreach (UastNode child in assignmentlStatement.Children)
                {
                    switch (child.RawInternalType)
                    {
                        case "fl:Variable":
                            statement.lhs = child.RawToken.Replace("%", "");
                            if (child.HasProperty("value"))
                                statement.rhs = child.GetProperty("value");
                            variable = GetExistingVariable(child.RawToken.Replace("%", ""));
                            break;
                        case "fl:Function":
                            statement.rhs = RhsFunction(child, (variable == null ? "" : variable.datatype));
                            break;
                        case "fl:Expression":
                            statement.rhs = child.RawToken.Replace("%", "@");
                            break;
                    }
                }
                statements.Add(statement);
            }
            return statements;
        }
        private string AddDateFormat(string _dateValue, TemplateGroup _stg)
        {
            Template dateFormatTemplate = _stg.GetInstanceOf("date_format");
            dateFormatTemplate.Add("date", _dateValue);
            return dateFormatTemplate.Render();
        }
        private List<OutputValue> GetOutputValuesForSelect(List<OutputValue> _outputValues, MFDTableDTO _table, TemplateGroup _stg, bool _m204FileContainsRectypes, string _mainConditions)
        {
            List<OutputValue> result;
            if (_m204FileContainsRectypes == true && _table.Rectype != "No Rectype")
            {
                result = new List<OutputValue>();
                foreach(var o in _outputValues.Where(o => o.rectype.Contains(_table.Rectype)))
                {
                    if (o.outputType == "field")
                        if (_m204FileContainsRectypes == true && !o.name.EndsWith("RECTYPE"))
                            result.Add(o);
                        else 
                        {
                            result.Add(new OutputValue()
                            {
                                position = o.position,
                                value = $"'{_table.Rectype}'",
                                length = o.length,
                                outputType = "constant",
                                outputString = ""
                            });
                        }
                    else
                        result.Add(o);
                }
                foreach(var o in _outputValues)
                {
                    if (o.rectype == "" && _table.Rectype != "No Rectype")
                        result.Add(o);
                }
            }
            else
            {
                result = _outputValues;
            }
            foreach (OutputValue output in result)
            {
                switch (output.outputType)
                {
                    case "field":
                        output.value = GetColumn((output.name.Contains("(") == true ? StringExtensions.Left(output.name, output.name.IndexOf("(")) : output.name), _table);
                        if (output.formatString != "")
                        {
                            Template formatTemplate = _stg.GetInstanceOf("format_function");
                            formatTemplate.Add("column", output.value);
                            formatTemplate.Add("format_string", output.formatString);
                            output.value = formatTemplate.Render();
                        }
                        output.dataType = GetColumnDataType((output.name.Contains("(") == true ? StringExtensions.Left(output.name, output.name.IndexOf("(")) : output.name), _table);
                        output.occurs = (output.value.StartsWith("[reoccur]") ? true : false);
                        output.occno = (output.occno != "No occno" ? output.occno : null);
                        if (output.occurs == false)
                        {
                            Template outputFieldTemplate = _stg.GetInstanceOf("output_value");
                            outputFieldTemplate.Add("position", output.position);
                            outputFieldTemplate.Add("value", (output.dataType.StartsWith("DATE") ? AddDateFormat(output.value, _stg) : output.value));
                            outputFieldTemplate.Add("length", output.length);
                            if (output.missingValue.Length > 0)
                                outputFieldTemplate.Add("missing_value", output.missingValue);
                            output.outputString = outputFieldTemplate.Render();
                        }
                        else
                        {
                            Template occursTemplate = _stg.GetInstanceOf("occurs_clause");
                            occursTemplate.Add("from", _table.FromCaluse.Replace("[base]","[ibase]"));
                            occursTemplate.Add("join", _table.JoinCaluse.Replace("[reoccur]", "[ireoccur]"));
                            occursTemplate.Add("where", "WHERE");
                            //occursTemplate.Add("where", (_mainConditions.Length > 0 ? $"WHERE {_mainConditions.Replace("[base]", "[ibase]").Replace("[reoccur]", "[ireoccur]")} AND" : "WHERE"));
                            if (output.occno != null)
                                occursTemplate.Add("occno", output.occno);
                            else
                                occursTemplate.Add("value", output.value.Replace("[reoccur]", "[ireoccur]"));

                            Template outputReoccurTemplate = _stg.GetInstanceOf("output_value_occurs");
                            outputReoccurTemplate.Add("position", output.position);
                            outputReoccurTemplate.Add("value", (output.dataType.StartsWith("DATE") ? AddDateFormat(output.value, _stg) : output.value).Replace("[reoccur]", "[ireoccur]"));
                            outputReoccurTemplate.Add("length", output.length);
                            if (output.occno == null)
                                outputReoccurTemplate.Add("top", "1");
                            if (output.missingValue.Length > 0)
                                outputReoccurTemplate.Add("missing_value", output.missingValue);
                            outputReoccurTemplate.Add("occurs_clause", occursTemplate.Render());
                            output.outputString = outputReoccurTemplate.Render();
                        }
                        break;
                    case "variable":
                        Template outputVariableTemplate = _stg.GetInstanceOf("output_value");
                        outputVariableTemplate.Add("position", output.position);
                        outputVariableTemplate.Add("value", output.value.Replace("%","@"));
                        outputVariableTemplate.Add("length", output.length);
                        output.outputString = outputVariableTemplate.Render();
                        break;
                    case "constant":
                        Template outputConstantTemplate = _stg.GetInstanceOf("output_value");
                        outputConstantTemplate.Add("position", output.position);
                        outputConstantTemplate.Add("value", output.value.Replace("%", ""));
                        outputConstantTemplate.Add("length", output.length);
                        output.outputString = outputConstantTemplate.Render();
                        break;
                    case "recin":
                        Template outputRecinTemplate = _stg.GetInstanceOf("recin_function");
                        outputRecinTemplate.Add("position", output.position);
                        outputRecinTemplate.Add("value", output.value);
                        outputRecinTemplate.Add("length", output.length);
                        output.outputString = outputRecinTemplate.Render();
                        break;
                    case "filename":
                        Template outputFilenameTemplate = _stg.GetInstanceOf("filename_function");
                        outputFilenameTemplate.Add("position", output.position);
                        outputFilenameTemplate.Add("value", _table.TableName);
                        outputFilenameTemplate.Add("length", output.length);
                        output.outputString = outputFilenameTemplate.Render();
                        break;
                }
            }
            List<OutputValue> sortedResult = result.OrderBy(o => o.position).ToList();
            if(sortedResult.Count > 0)
            {
                List<OutputValue> gapFillers = new List<OutputValue>();
                int previousPosition = 1;
                int previousLength = 0;
                foreach(OutputValue output in sortedResult)
                {
                    if(output.position > previousPosition + previousLength)
                    {
                        gapFillers.Add(new OutputValue()
                        {
                            position = previousPosition + previousLength,
                            value = $"SPACE({output.position - (previousPosition + previousLength)})",
                            length = output.position - (previousPosition + previousLength),
                            outputType = "constant",
                            outputString = $"[P{previousPosition + previousLength}] = SPACE({output.position - (previousPosition + previousLength)})"
                        });
                    }
                    previousPosition = output.position;
                    previousLength = output.length;
                }
                if (gapFillers.Count > 0)
                {
                    sortedResult.AddRange(gapFillers);
                }
            }
            return sortedResult.OrderBy(o => o.position).ToList();
        }
        private List<OutputValue> GetOutputValues(List<UastNode> _putStatements)
        {
            int i = 1;
            List<OutputValue> outputValues = new List<OutputValue>();
            foreach(UastNode putStatement in _putStatements)
            {
                OutputValue outputValue = new OutputValue();
                if(putStatement.HasProperty("location"))
                {
                    outputValue.position = int.Parse(putStatement.GetProperty("location"));
                }
                else
                {
                    outputValue.position = i;
                    i++;
                }
                string[] typeArgs = { "" };
                outputValue.dataType = putStatement.GetProperty("type");
                outputValue.funout = putStatement.GetProperty("funout");
                outputValue.rectype = (putStatement.HasProperty("rectype")? StringExtensions.ConvertPeriods(putStatement.GetProperty("rectype")) : "");
                if (putStatement.HasProperty("typeArgs"))
                {
                    typeArgs = putStatement.GetProperty("typeArgs").Replace("(", "").Replace(")", "").Split(",");
                }
                else
                {
                    Array.Resize<string>(ref typeArgs, 3);
                    int j = 0;
                    foreach (var child in putStatement.Children)
                    {
                        if (child.RawInternalType == "fl:Argument" && j < 3)
                        {
                            typeArgs.SetValue(child.RawToken, j);
                            j++;
                        }
                    }
                }
                outputValue.length = (typeArgs[0] == "" ? outputValue.length = -1 : int.Parse(typeArgs[0].Replace("'", "")));
                if(typeArgs.Length > 1)
                {
                    outputValue.alignment = (typeArgs[1] == null ? "" : typeArgs[1].Replace("'", ""));
                    if (typeArgs.Length > 2)
                        outputValue.formatString = (typeArgs[2] == null ? "" :StringExtensions.GetRepeatedCharacter(typeArgs[2].Replace("'", ""), outputValue.length));
                    else
                        outputValue.formatString = "";
                }
                else
                {
                    outputValue.alignment = "";
                    outputValue.formatString = "";
                }
                if (putStatement.HasProperty("missingValue"))
                {
                    outputValue.missingValue = putStatement.GetProperty("missingValue");
                }
                foreach(var child in putStatement.Children)
                {
                    switch(child.RawInternalType)
                    {
                        case "fl:Constant":
                            outputValue.outputType = "constant";
                            outputValue.value = child.RawToken;
                            outputValue.length = (outputValue.length == -1 ? child.RawToken.Replace("'", "").Length : outputValue.length);
                            outputValue.occurs = false;
                            outputValue.occno = "No occno";
                            break;
                        case "fl:Field":
                            outputValue.outputType = "field";
                            outputValue.value = child.RawToken;
                            outputValue.name = child.RawToken;
                            outputValue.missingValue = (putStatement.HasProperty("missingValue") ? putStatement.GetProperty("missingValue") : "");
                            if(child.HasProperty("occurs"))
                            {
                                outputValue.occurs = true;
                                outputValue.occno = child.GetProperty("occurs");
                            }
                            else
                            {
                                outputValue.occurs = false;
                                outputValue.occno = "No occno";
                            }
                            break;
                        case "fl:Variable":
                            outputValue.outputType = "variable";
                            outputValue.value = child.RawToken;
                            outputValue.name = child.RawToken;
                            outputValue.missingValue = child.RawToken;
                            outputValue.occurs = false;
                            outputValue.occno = "No occno";
                            break;
                        case "fl:Recin":
                            outputValue.outputType = "recin";
                            outputValue.value = child.RawToken;
                            outputValue.occurs = false;
                            outputValue.occno = "No occno";
                            break;
                        case "fl:Filename":
                            outputValue.outputType = "filename";
                            outputValue.value = child.RawToken;
                            outputValue.occurs = false;
                            outputValue.occno = "No occno";
                            break;
                    }
                }
                outputValues.Add(outputValue);
            }
            return outputValues;
        }
        private Dictionary<string, MFDTableDTO> GetMfdFromClauses(List<MFDTableDTO> _mfdTables, TemplateGroup _stg)
        {
            Dictionary<string, MFDTableDTO> fromClauses = new Dictionary<string, MFDTableDTO>();
            foreach(MFDTableDTO table in _mfdTables)
            {
                Template fromClauseTemplate = _stg.GetInstanceOf("from_main_clause");
                fromClauseTemplate.Add("baseTable", table.TableName);
                table.FromCaluse = fromClauseTemplate.Render();
                if (table.ReoccurTableName != "")
                {
                    Template joinTemplate = _stg.GetInstanceOf("from_reoccur_clause");
                    joinTemplate.Add("reoccurTable", table.ReoccurTableName);
                    table.JoinCaluse = joinTemplate.Render();
                }
                fromClauses[table.TableName] = table;
            }
            return fromClauses;
        }
        private string GetColumnDataType(string _column, MFDTableDTO _table)
        {
            string result = "";
            string columnName = (columnDictionary.ContainsKey(_column) ? columnDictionary[_column] : _column);
            var columns = mfdColumnList.Where(c => c.ColumnName == columnName && c.TableName == _table.TableName);
            foreach (var column in columns)
            {
                if (result == "")
                {
                    result = column.SqlType;
                }
            }
            if (result == "")
            {
                columns = mfdColumnList.Where(c => c.ColumnName == _column && c.TableName == _table.ReoccurTableName);
                foreach (var column in columns)
                {
                    if (result == "")
                    {
                        result = column.SqlType;
                    }
                }
            }
            return result;
        }
        private string GetColumn(string _column, MFDTableDTO _table)
        {
            string result = "";
            string columnName = (columnDictionary.ContainsKey(_column) ? columnDictionary[_column] : _column);
            var columns = mfdColumnList.Where(c => c.ColumnName == columnName && c.TableName == _table.TableName);
            foreach (var column in columns)
            {
                if(result == "")
                {
                    result = $"[{column.Alias}].[{column.ColumnName}]";
                }
            }
            if (result == "")
            {
                columns = mfdColumnList.Where(c => c.ColumnName == _column && c.TableName == _table.ReoccurTableName);
                foreach (var column in columns)
                {
                    if (result == "")
                    {
                        result = $"[{column.Alias}].[{column.ColumnName}]";
                    }
                }
            }
            if (result == "")
                result = _column;
            return result;
        }
        private string GetOccursCondition(string _leftOperand, MFDTableDTO _table, TemplateGroup _stg)
        {
            Template occursConditionTemplate = _stg.GetInstanceOf("occurs_condition");
            string column = GetColumn((_leftOperand.Contains("(") == true ? StringExtensions.Left(_leftOperand, _leftOperand.IndexOf("(")) : _leftOperand), _table);
            string occno = _leftOperand.Substring(_leftOperand.IndexOf("(") + 1, (_leftOperand.Length - (_leftOperand.IndexOf("(") + 2)));

            occursConditionTemplate.Add("reoccurTable", _table.ReoccurTableName);
            occursConditionTemplate.Add("column", column);
            occursConditionTemplate.Add("occno", occno);
            return occursConditionTemplate.Render();
        }
        private void GetConditionText(UastNode _condition, MFDTableDTO _table, TemplateGroup _stg, bool _m204FileContainsRectypes, LinkedList<string> _whereClauseElements)
        {
            if(!(_m204FileContainsRectypes == true && _condition.GetProperty("left_operand").Contains("RECTYPE")))
            {
                Template conditionTemplate = _stg.GetInstanceOf("condition");
                if (_condition.HasProperty("lparen"))
                    _whereClauseElements.AddLast("(");

                string leftOperand = _condition.GetProperty("left_operand").Replace(".", "_").Replace("%", "");
                if(leftOperand.Contains("(") == true)
                {
                    conditionTemplate.Add("left_operand", GetOccursCondition(leftOperand, _table, _stg));
                }
                else
                {
                    string column = GetColumn(leftOperand, _table);
                    conditionTemplate.Add("left_operand", column);
                }

                string op = (conditionalOperators.ContainsKey(_condition.GetProperty("operator")) ? conditionalOperators[_condition.GetProperty("operator")] : _condition.GetProperty("operator"));
                conditionTemplate.Add("operator", op);

                if (_condition.HasProperty("right_operand"))
                    conditionTemplate.Add("right_operand", _condition.GetProperty("right_operand").Replace("%", "@"));

                _whereClauseElements.AddLast(conditionTemplate.Render());

                if (_condition.HasProperty("rparen"))
                    _whereClauseElements.AddLast(")");
            }
        }
        private bool ConditionContainsRectype(UastNode _ifStatement, bool _m204FileContainsRectypes)
        {
            UastNode condition = new UastNode();
            foreach(var child in _ifStatement.Children)
            {
                if (child.RawInternalType == "fl:Condition")
                    condition = child;
            }
            return _m204FileContainsRectypes == true && condition.GetProperty("left_operand").Contains("RECTYPE") ? true : false;
        }
        private void GetWHEREConditions(UastNode _ifStatement, MFDTableDTO _table, TemplateGroup _stg, bool _m204FileContainsRectypes, LinkedList<string> _whereClauseElements)
        {
            foreach(var child in _ifStatement.Children)
            {
                switch (child.RawInternalType)
                {
                    case "fl:Conjunction":
                        if (_whereClauseElements.Count > 0)
                            _whereClauseElements.AddLast($" {child.RawToken} ");
                        break;
                    case "fl:If":
                        if(!ConditionContainsRectype(_ifStatement, _m204FileContainsRectypes))
                            _whereClauseElements.AddLast($" AND ");
                        GetWHEREConditions(child, _table, _stg, _m204FileContainsRectypes, _whereClauseElements);
                        break;
                    case "fl:Condition":
                        GetConditionText(child, _table, _stg, _m204FileContainsRectypes, _whereClauseElements);
                        break;
                }
            }
        }
        private void DumpConditionList(LinkedList<string> _whereClauseElements)
        {
            LinkedListNode<string> nextNode = _whereClauseElements.First;
            while(nextNode != null)
            {
                System.Console.WriteLine(nextNode.Value);
                nextNode = nextNode.Next;
            }
        }
        private string GetMainWhereClause(UastNode _ifStatement, MFDTableDTO _table, TemplateGroup _stg, bool _m204FileContainsRectypes)
        {
            StringBuilder sb = new StringBuilder();
            LinkedList<string> whereClauseElements = new LinkedList<string>();
            GetWHEREConditions(_ifStatement, _table, stg, _m204FileContainsRectypes, whereClauseElements);
            LinkedListNode<string> openingAND = whereClauseElements.Find(" AND ");
            //DumpConditionList(whereClauseElements);
            int parentheses = 0;
            LinkedListNode<string> nextNode = (openingAND == null ? null : openingAND.Next);
            while (nextNode != null)
            {
                switch (nextNode.Value)
                {
                    case "(":
                        parentheses++;
                        break;
                    case ")":
                        parentheses--;
                        break;
                    case " AND ":
                        if (parentheses == 0)
                            openingAND = nextNode;
                        else if(parentheses == 1)
                        {
                            whereClauseElements.AddBefore(nextNode, new LinkedListNode<string>(")"));
                            parentheses--;
                            openingAND = nextNode;
                        }
                        break;
                    case " OR ":
                        if (parentheses == 0)
                        {
                            whereClauseElements.AddAfter(openingAND, new LinkedListNode<string>("("));
                            parentheses++;
                        }
                        break;
                }
                nextNode = nextNode.Next;
            }
            if (parentheses == 1)
                whereClauseElements.AddLast(")");

            foreach (string element in whereClauseElements)
                sb.Append(element);
            return sb.ToString();
        }
        private List<SortValue> GetSortValues(List<UastNode> _sortStatementList)
        {
            List<SortValue> result = new List<SortValue>();
            foreach(UastNode sortStatement in _sortStatementList)
            {
                if(sortStatement.RawToken == "FIELDS" && int.TryParse(sortStatement.FirstChild().RawToken, out _) == true)
                {
                    sortBySubstring = true;
                    for(int i = 0; i < sortStatement.ChildCount; i = i + 4)
                    {
                        result.Add(new SortValue()
                        {
                            start = int.Parse(sortStatement.Children[i].RawToken),
                            length = int.Parse(sortStatement.Children[i + 1].RawToken),
                            order = (sortStatement.Children[i + 3].RawToken == "A" ? "ASC" : "DESC")
                        });
                    }
                }
            }
            return result;
        }
        private List<SortColumn> GetSortColumns(List<UastNode> _sortStatementList)
        {
            List<SortColumn> result = new List<SortColumn>();
            foreach (UastNode sortStatement in _sortStatementList)
            {
                if (sortStatement.RawToken == "FIELDS" && int.TryParse(sortStatement.FirstChild().RawToken, out _) == false)
                {
                    sortByColumn = true;
                    for (int i = 0; i < sortStatement.ChildCount; i = i + 2)
                    {
                        result.Add(new SortColumn()
                        {
                            Column = sortStatement.Children[i].RawToken.Replace(".","_"),
                            order = (sortStatement.Children[i + 1].RawToken == "A" ? "ASC" : "DESC")
                        });
                    }
                }
            }
            return result;
        }
        private void WalkTheTree(UastNode _node)
        {
            switch (_node.RawInternalType)
            {
                case "fl:Assignment":
                    assignmentStatementList.Add(_node);
                    break;
                case "fl:Case":
                    whenConditions[selectColumn].Add(_node.GetProperty("value"));
                    break;
                case "fl:Condition":
                    conditionList.Add(_node);
                    break;
                case "fl:If":
                    ifStatementList.Add(_node);
                    break;
                case "fl:Job":
                    if(_node.HasProperty("m204File"))
                        fileGroups.AddRange(_node.GetProperty("m204File").Split(","));
                    break;
                case "fl:Put":
                    putStatementList.Add(_node);
                    break;
                case "fl:Output":
                    if (!outputFileList.Contains(_node.GetProperty("funout")))
                        outputFileList.Add(_node.GetProperty("funout"));
                    break;
                case "fl:Select":
                    selectColumn = _node.GetProperty("selectIdentifier").Replace(".", "_").Replace("%", "");
                    whenConditions[selectColumn] = new List<string>();
                    break;
                case "fl:Sort":
                    sortStatementList.Add(_node);
                    break;
            }
            foreach (var child in _node.Children)
                WalkTheTree(child);

        }
        private void PrepareJobStructure(UastNode _jobNode)
        {
            fileGroups.Clear();
            m204FileList.Clear();
            rectypeValues.Clear();
            mfdFromClauses.Clear();
            mfdColumnList.Clear();
            putStatementList.Clear();
            ifStatementList.Clear();
            assignmentStatementList.Clear();
            conditionList.Clear();
            outputFileList.Clear();
            outputValues.Clear();
            variables.Clear();
            mfdTables.Clear();
            selectColumn = "";
            whenConditions.Clear();
            sortStatementList.Clear();
            sortValuesList.Clear();
            sortColumnList.Clear();
            sortBySubstring = false;
            sortByColumn = false;

            WalkTheTree(_jobNode);

            if(fileGroups.Count > 0 && putStatementList.Count > 0)
            {
                bool rectypesInFile = false;
                FunloadMetadata metadata = new FunloadMetadata();
                m204FileList = GetM204Files(fileGroups, metadata);
                foreach (M204FileDTO file in m204FileList)
                    if (file.RecTypeColumn != "No Rectype")
                        rectypesInFile = true;
                if(rectypesInFile == true)
                    rectypeValues = GetRectypes(putStatementList, conditionList);
                foreach (var m204File in m204FileList)
                {
                    mfdTables.AddRange(GetMFDTables(metadata, m204File.M204File, rectypeValues, rectypesInFile));
                }
                if(rectypeValues.Count == 0 && rectypesInFile == true)
                {
                    foreach(MFDTableDTO table in mfdTables)
                    {
                        if (!rectypeValues.ContainsKey(table.Rectype) && table.Rectype != "No Retype")
                            rectypeValues[table.Rectype] = "=";
                    }
                }
                mfdColumnList = GetMFDColumnMetadata(mfdTables, rectypeValues, metadata);
                outputValues = GetOutputValues(putStatementList);
                variables = GetVariables(_jobNode);
                sortValuesList = GetSortValues(sortStatementList);
                sortColumnList = GetSortColumns(sortStatementList);
            }
        }
        private void ProcessJob(string _headerText, UastNode _jobNode, string _outputFolder, TemplateGroup _stg)
        {
            List<string> tablenames = new List<string>();
            StringBuilder sb = new StringBuilder();
            sb.Append(_headerText);

            int tables = 0;
            string jobName = _jobNode.RawToken;
            string step = _jobNode.GetProperty("step");
            string outputFilePath = $"{_outputFolder}mcc{jobName}job_{step}.SQL";
            string mainConditions = "";
            bool m204FileContainsRectypes = false;

            foreach (MFDTableDTO table in mfdTables)
            {
                if (table.Rectype != "No Rectype")
                    m204FileContainsRectypes = true;
            }

            Template jobHeaderTemplate = _stg.GetInstanceOf("job_header");
            jobHeaderTemplate.Add("jobname", jobName);
            jobHeaderTemplate.Add("step", step);
            foreach (var mfdTable in mfdTables)
                tablenames.Add(mfdTable.TableName);
            jobHeaderTemplate.Add("mfd_tables", tablenames);
            sb.Append(jobHeaderTemplate.Render());

            Template unionTemplate = _stg.GetInstanceOf("union_clause");
            string unionText = unionTemplate.Render();

            if(putStatementList.Count > 0)
            {
                sb.Append(DeclareVariables(variables, _stg));
                mfdFromClauses = GetMfdFromClauses(mfdTables, _stg);
                int jobChildNumber = 0;
                List<UastNode> globalAssignmentStatements = new List<UastNode>();
                foreach(var assignment in assignmentStatementList)
                {
                    if (assignment.HasProperty("withinForEach") && assignment.GetProperty("withinForEach") == "False")
                        globalAssignmentStatements.Add(assignment);
                }
                if(globalAssignmentStatements.Count > 0)
                {
                    List<AssignmentStatement> gblAssignments = GetAssignments(globalAssignmentStatements);
                    Template assignmentTemplate = _stg.GetInstanceOf("assignment_statement");
                    assignmentTemplate.Add("assignment_statements", gblAssignments);
                    sb.Append(assignmentTemplate.Render());
                }
                if (sortBySubstring == true)
                {
                    Template sortedOpenTemplate = _stg.GetInstanceOf("select_statement_sorted_open");
                    sb.Append(sortedOpenTemplate.Render());
                }
                foreach (MFDTableDTO table in mfdTables)
                {
                    bool reoccurInMainWhere = false;
                    Template whereTemplate = _stg.GetInstanceOf("where_clause");
                    Template conditionTemplate = _stg.GetInstanceOf("condition");
                    if (whenConditions.Count > 0)
                    {
                        if (m204FileContainsRectypes == false)
                        {
                            string column = GetColumn(selectColumn, table);
                            conditionTemplate.Add("left_operand", column);
                            conditionTemplate.Add("operator", "=");
                            conditionTemplate.Add("right_operand", whenConditions[selectColumn][0]);
                            whereTemplate.Add("select_case", conditionTemplate.Render());
                        }
                        else if (!selectColumn.EndsWith("RECTYPE"))
                        {
                            string column = GetColumn(selectColumn, table);
                            conditionTemplate.Add("left_operand", column);
                            conditionTemplate.Add("operator", "IN");
                            conditionTemplate.Add("right_operand", $"({whenConditions[selectColumn][0]})");
                            whereTemplate.Add("select_case", conditionTemplate.Render());
                        }
                    }
                    if (ifStatementList.Count > 0 && ifStatementList[0].GetProperty("withinPUT") == "false")
                    {
                        if(mainConditions == "")
                        {
                            previousConjuction = "";
                            mainConditions = GetMainWhereClause(ifStatementList[0], table, _stg, m204FileContainsRectypes);
                            reoccurInMainWhere = (mainConditions.Contains("reoccur") ? true : false);
                        }
                    }
                    outputList = GetOutputValuesForSelect(outputValues, table, _stg, m204FileContainsRectypes, mainConditions);

                    Template selectTemplate = _stg.GetInstanceOf("select_statement_outer");
                    selectTemplate.Add("output_list", outputList);
                    sb.Append(selectTemplate.Render());

                    selectTemplate = _stg.GetInstanceOf("select_statement");
                    selectTemplate.Add("output_list", outputList);
                    sb.Append(selectTemplate.Render());

                    Template fromTemplate = _stg.GetInstanceOf("from");
                    fromTemplate.Add("from", table.FromCaluse);
                    if (reoccurInMainWhere == true)
                        fromTemplate.Add("join", table.JoinCaluse);
                    sb.Append(fromTemplate.Render());

                    if (mainConditions.Length > 0)
                    {
                        whereTemplate.Add("where", mainConditions);
                        sb.Append(whereTemplate.Render());
                    }
                    if (sortByColumn == true)
                    {
                        foreach (SortColumn sortColumn in sortColumnList)
                            sortColumn.Column = GetColumn(sortColumn.Column, table);
                        Template sortedByTemplate = _stg.GetInstanceOf("sort_by_column");
                        sortedByTemplate.Add("order_by", sortColumnList);
                        sb.Append(sortedByTemplate.Render());
                    }
                    selectTemplate = _stg.GetInstanceOf("select_statement_outer_close");
                    selectTemplate.Add("table_number", tables);
                    sb.Append(selectTemplate.Render());
                    tables++;
                    if (tables < mfdTables.Count)
                        sb.Append(unionText);
                }
                if (sortBySubstring == true)
                {
                    Template sortedCloseTemplate = _stg.GetInstanceOf("select_statement_sorted_close");
                    sortedCloseTemplate.Add("order_by", sortValuesList);
                    sb.Append(sortedCloseTemplate.Render());
                }

                foreach (var child in _jobNode.Children)
                {
                    
                    jobChildNumber++;
                }
            }
            else
            {
                foreach(var child in _jobNode.Children)
                {
                    if(child.RawInternalType == "fl:SpecialFunloadStatements")
                    {
                        Template sfiTemplate = _stg.GetInstanceOf("sfs");
                        sfiTemplate.Add("text", child.RawToken);
                        sb.Append(sfiTemplate.Render());
                    }
                    if (child.RawInternalType == "fl:UAI")
                    {
                        Template sfiTemplate = _stg.GetInstanceOf("uai");
                        sfiTemplate.Add("m204File", _jobNode.GetProperty("m204File"));
                        sb.Append(sfiTemplate.Render());
                    }
                }
            }
            
            System.IO.File.WriteAllText(outputFilePath, sb.ToString());
        }
        public bool WriteSQL(UastNode _root, string _outputFolder)
        {
            bool result = true;
            string templateText = System.IO.File.ReadAllText("FunloadSQLTemplate.stg");
            stg = new TemplateGroupString(templateText);
            string funloadFile = _root.RawToken;
            foreach (UastNode job in _root.Children)
            {
                 PrepareJobStructure(job);
                 StringBuilder sb = new StringBuilder();
                 Template headerTemplate = stg.GetInstanceOf("fl_header");
                 headerTemplate.Add("fl_file", funloadFile);
                 headerTemplate.Add("generation_date", DateTime.Now.ToShortDateString());
                 headerTemplate.Add("generation_time", DateTime.Now.ToShortTimeString());

                 ProcessJob(headerTemplate.Render(), job, _outputFolder, stg);
            }
            return result;
        }
    }
}
