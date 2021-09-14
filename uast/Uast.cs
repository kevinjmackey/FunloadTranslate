using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;

namespace uast
{
    internal static class CorrelationIdGenerator
    {
        private static readonly string _encode32Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

        private static long _lastId = DateTime.UtcNow.Ticks;

        public static string GetNextId() => GenerateId(System.Threading.Interlocked.Increment(ref _lastId));
        private static string GenerateId(long id)
        {
            var buffer = new char[13];

            buffer[0] = _encode32Chars[(int)(id >> 60) & 31];
            buffer[1] = _encode32Chars[(int)(id >> 55) & 31];
            buffer[2] = _encode32Chars[(int)(id >> 50) & 31];
            buffer[3] = _encode32Chars[(int)(id >> 45) & 31];
            buffer[4] = _encode32Chars[(int)(id >> 40) & 31];
            buffer[5] = _encode32Chars[(int)(id >> 35) & 31];
            buffer[6] = _encode32Chars[(int)(id >> 30) & 31];
            buffer[7] = _encode32Chars[(int)(id >> 25) & 31];
            buffer[8] = _encode32Chars[(int)(id >> 20) & 31];
            buffer[9] = _encode32Chars[(int)(id >> 15) & 31];
            buffer[10] = _encode32Chars[(int)(id >> 10) & 31];
            buffer[11] = _encode32Chars[(int)(id >> 5) & 31];
            buffer[12] = _encode32Chars[(int)id & 31];

            return new string(buffer, 0, 13);
        }
    }
    public enum Role
    {
        // Invalid Role is assigned as a zero value since protobuf enum definition must start at 0.
        INVALID = 0,
        // Identifier is any form of identifier, used for variable names, functions, packages, etc.
        IDENTIFIER = 1,
        // Qualified is a kind of property identifiers may have, when it's composed
        // of multiple simple identifiers.
        QUALIFIED = 2,
        // Operator is any form of operator.
        OPERATOR = 3,
        // Binary is any form of binary operator, in contrast with unary operators.
        BINARY = 4,
        // Unary is any form of unary operator, in contrast with binary operators.
        UNARY = 5,
        // Left is a left hand side in a binary expression.
        LEFT = 6,
        // Right is a right hand side if a binary expression.
        RIGHT = 7,
        // Infix should mark the nodes which are parents of expression nodes using infix notation, e.g.: a+b.
        // Nodes without Infix or Postfix mark are considered in prefix order by default.
        INFIX = 8,
        // Postfix should mark the nodes which are parents of nodes using postfix notation, e.g.: ab+.
        // Nodes without Infix or Postfix mark are considered in prefix order by default.
        POSTFIX = 9,
        // Bitwise is any form of bitwise operation.
        BITWISE = 10,
        // Boolean is any form of boolean operation.
        BOOLEAN = 11,
        // Unsigned is an form of unsigned operation.
        UNSIGNED = 12,
        // LeftShift is a left shift operation (i.e. `<<`, `rol`, etc.)
        LEFT_SHIFT = 13,
        // RightShift is a right shift operation (i.e. `>>`, `ror`, etc.)
        RIGHT_SHIFT = 14,
        // Or is an OR operation (i.e. `||`, `or`, `|`, etc.)
        OR = 15,
        // Xor is an exclusive OR operation  (i.e. `~`, `^`, etc.)
        XOR = 16,
        // And is an AND operation (i.e. `&&`, `&`, `and`, etc.)
        AND = 17,
        // Expression is a construct computed to produce some value.
        EXPRESSION = 18,
        // Statement is some action to be carried out.
        STATEMENT = 19,
        // Equal is an eaquality predicate (i.e. `=`, `==`, etc.),
        EQUAL = 20,
        // Not is a negation operation. It may be used to annotate a complement of an operator.
        NOT = 21,
        // LessThan is a comparison predicate that checks if the lhs value is smaller than the rhs value (i. e. `<`.)
        LESS_THAN = 22,
        // LessThanOrEqual is a comparison predicate that checks if the lhs value is smaller or equal to the rhs value (i.e. `<=`.),
        LESS_THAN_OR_EQUAL = 23,
        // GreaterThan is a comparison predicate that checks if the lhs value is greather than the rhs value (i. e. `>`.)
        GREATER_THAN = 24,
        // GreaterThanOrEqual is a comparison predicate that checks if the lhs value is greather than or equal to the rhs value (i.e. 1>=`.),
        GREATER_THAN_OR_EQUAL = 25,
        // Identical is an identity predicate (i. e. `===`, `is`, etc.),
        IDENTICAL = 26,
        // Contains is a membership predicate that checks if the lhs value is a member of the rhs container (i.e. `in` in Python.)
        CONTAINS = 27,
        // Increment is an arithmetic operator that increments a value (i. e. `++i`.)
        INCREMENT = 28,
        // Decrement is an arithmetic operator that decrements a value (i. e. `--i`.)
        DECREMENT = 29,
        // Negative is an arithmetic operator that negates a value (i.e. `-x`.)
        NEGATIVE = 30,
        // Positive is an arithmetic operator that makes a value positive. It's usually redundant (i.e. `+x`.)
        POSITIVE = 31,
        // Dereference is an operation that gets the actual value of a pointer or reference (i.e. `*x`.)
        DEREFERENCE = 32,
        // TakeAddress is an operation that gets the memory address of a value (i. e. `&x`.)
        TAKE_ADDRESS = 33,
        // File is the root node of a single file AST.
        FILE = 34,
        // Add is an arithmetic operator (i.e. `+`.)
        ADD = 35,
        // Substract in an arithmetic operator (i.e. `-`.)
        SUBSTRACT = 36,
        // Multiply is an arithmetic operator (i.e. `*`.)
        MULTIPLY = 37,
        // Divide is an arithmetic operator (i.e. `/`.)
        DIVIDE = 38,
        // Modulo is an arithmetic operator (i.e. `%`, `mod`, etc.)
        MODULO = 39,
        // Package indicates that a package level property.
        PACKAGE = 40,
        // Declaration is a construct to specify properties of an identifier.
        DECLARATION = 41,
        // Import indicates an import level property.
        IMPORT = 42,
        // Pathname is a qualified name of some construct.
        PATHNAME = 43,
        // Alias is an alternative name for some construct.
        ALIAS = 44,
        // Function is a sequence of instructions packaged as a unit.
        FUNCTION = 45,
        // Body is a sequence of instructions in a block.
        BODY = 46,
        // Name is an identifier used to reference a value.
        NAME = 47,
        // Receiver is the target of a construct (message, function, etc.)
        RECEIVER = 48,
        // Argument is variable used as input/output in a function.
        ARGUMENT = 49,
        // Value is an expression that cannot be evaluated any further.
        VALUE = 50,
        // ArgsList is variable number of arguments (i.e. `...`, `Object...`, `*args`, etc.)
        ARGS_LIST = 51,
        // Base is the parent type of which another type inherits.
        BASE = 52,
        // Implements is the type (usually an interface) that another type implements.
        IMPLEMENTS = 53,
        // Instance is a concrete occurrence of an object.
        INSTANCE = 54,
        // Subtype is a type that can be used to substitute another type.
        SUBTYPE = 55,
        // Subpackage is a package that is below another package in the hierarchy.
        SUBPACKAGE = 56,
        // Module is a set of funcitonality grouped.
        MODULE = 57,
        // Friend is an access granter for some private resources.
        FRIEND = 58,
        // World is a set of every component.
        WORLD = 59,
        // If is used for if-then
        // An if-then tree will look like:
        // 
        //     If, Statement {
        //         **
        //             If, Condition {
        //                 
        //                      }
        //         }
        //         **
        //             If, Then {
        //                 
        //             }
        //         }
        //         **
        //             If, Else {
        //                 
        //             }
        //         }
        //     }
        // 
        // The Else node is optional. The order of Condition, Then and
        // Else is not defined.
        IF = 60,
        // Condition is a condition in an IfStatement or IfExpression.
        CONDITION = 61,
        // Then is the clause executed when the Condition is true.
        THEN = 62,
        // Else is the clause executed when the Condition is false.
        ELSE = 63,
        // Switch is used to represent a broad of switch flavors. An expression
        // is evaluated and then compared to the values returned by different
        // case expressions, executing a body associated to the first case that
        // matches. Similar constructions that go beyond expression comparison
        // (such as pattern matching in Scala's match) should not be annotated
        // with Switch.
        SWITCH = 64,
        // Case is a clause whose expression is compared with the condition.
        CASE = 65,
        // Default is a clause that is called when no other clause is matches.
        DEFAULT = 66,
        // For is a loop with an initialization, a condition, an update and a body.
        FOR = 67,
        // Initialization is the assignment of an initial value to a variable
        // (i.e. a for loop variable initialization.)
        INITIALIZATION = 68,
        // Update is the assignment of a new value to a variable or data in a file/database
        // (i.e. a for loop variable update.)
        UPDATE = 69,
        // Iterator is the element that iterates over something.
        ITERATOR = 70,
        // While is a loop construct with a condition and a body.
        WHILE = 71,
        // DoWhile is a loop construct with a body and a condition.
        DO_WHILE = 72,
        // Break is a construct for early exiting a block.
        BREAK = 73,
        // Continue is a construct for continuation with the next iteration of a loop.
        CONTINUE = 74,
        // Goto is an unconditional transfer of control statement.
        GOTO = 75,
        // Block is a group of statements. If the source language has block scope,
        // it should be annotated both with Block and BlockScope.
        BLOCK = 76,
        // Scope is a range in which a variable can be referred.
        SCOPE = 77,
        // Return is a return statement. It might have a child expression or not
        // as with naked returns in Go or return in void methods in Java.
        RETURN = 78,
        // Try is a statement for exception handling.
        TRY = 79,
        // Catch is a clause to capture exceptions.
        CATCH = 80,
        // Finally is a clause for a block executed after a block with exception handling.
        FINALLY = 81,
        // Throw is a statement that creates an exception.
        THROW = 82,
        // Assert checks if an expression is true and if it is not, it signals
        // an error/exception, possibly stopping the execution.
        ASSERT = 83,
        // Call is any call, whether it is a function, procedure, method or macro.
        // In its simplest form, a call will have a single child with a function
        // name (callee). Arguments are marked with Argument and Positional or Name.
        // In OO languages there is usually a Receiver too.
        CALL = 84,
        // Callee is the callable being called. It might be the name of a
        // function or procedure, it might be a method, it might a simple name
        // or qualified with a namespace.
        CALLEE = 85,
        // Positional is an element which position has meaning (i.e. a positional argument in a call).
        POSITIONAL = 86,
        // Noop is a construct that does nothing.
        NOOP = 87,
        // Literal is a literal value.
        LITERAL = 88,
        // Byte is a single-byte element.
        BYTE = 89,
        // ByteString is a raw byte string.
        BYTE_STRING = 90,
        // Character is an encoded character.
        CHARACTER = 91,
        // List is a sequence.
        LIST = 92,
        // Map is a collection of key, value pairs.
        MAP = 93,
        // Null is an empty value.
        NULL = 94,
        // Number is a numeric value. This applies to any numeric value
        // whether it is integer or float, any base, scientific notation or not,
        // etc.
        NUMBER = 95,
        // Regexp is a regular expression.
        REGEXP = 96,
        // Set is a collection of values.
        SET = 97,
        // String is a sequence of characters.
        STRING = 98,
        // Tuple is an finite ordered sequence of elements.
        TUPLE = 99,
        // Type is a classification of data.
        TYPE = 100,
        // Entry is a collection element.
        ENTRY = 101,
        // Key is the index value of a map.
        KEY = 102,
        // Primitive is a language builtin.
        PRIMITIVE = 103,
        // Assignment is an assignment operator.
        ASSIGNMENT = 104,
        // This represents the self-reference of an object instance in
        // one of its methods. This corresponds to the `this` keyword
        // (e.g. Java, C++, PHP), `self` (e.g. Smalltalk, Perl, Swift) and `Me`
        // (e.g. Visual Basic).
        THIS = 105,
        // Comment is a code comment.
        COMMENT = 106,
        // Documentation is a node that represents documentation of another node,
        // such as function or package. Documentation is usually in the form of
        // a string in certain position (e.g. Python docstring) or comment
        // (e.g. Javadoc, godoc).
        DOCUMENTATION = 107,
        // Whitespace.
        WHITESPACE = 108,
        // Incomplete express that the semantic meaning of the node roles doesn't express
        // the full semantic information. Added in BIP-002.
        INCOMPLETE = 109,
        // Unannotated will be automatically added by the SDK for nodes that did not receive
        // any annotations with the current version of the driver's `annotations.go` file.
        // Added in BIP-002.
        UNANNOTATED = 110,
        // Visibility is an access granter role, usually together with an specifier role
        VISIBILITY = 111,
        // Annotation is syntactic metadata
        ANNOTATION = 112,
        // Anonymous is an unbound construct
        ANONYMOUS = 113,
        // Enumeration is a distinct type that represents a set of named constants
        ENUMERATION = 114,
        // Arithmetic is a type of operation
        ARITHMETIC = 115,
        // Relational is a type of operation
        RELATIONAL = 116,
        // Variable is a symbolic name associatend with a value
        VARIABLE = 117,
        // Start a database transaction
        BEGINTRAN = 118,
        // Commit a database transaction
        COMMITTRAN = 119,
        // Rollback a database transaction
        ROLLBACKTRAN = 120,
        // Write/Store/Insert data to file/database
        WRITE = 121,
        // Delete data from a file/database
        DELETE = 123,
        // Read/Select data from a file/database
        READ = 124,
        // Create a file/table/view/procedure/etc.
        CREATE = 125,
        // Alter a file/table/view/procedure/etc.
        ALTER = 126,
        // Delete a file/table/view/procedure/etc.
        DROP = 127,
        // Compiler or other processing directive
        DIRECTIVE = 128,
        // Aggregate funtion (Min/Max/Avg/etc.)
        AGGREGATE = 129,
        // Data Source (File/Database Table/Cube)
        DATASOURCE = 130,
        // Join clause for database tables
        JOINCLAUSE = 131,
        // Table hint for database tables
        TABLEHINT = 132,
        // Left Parenthesis
        LPAREN = 133,
        // Right Parenthesis
        RPAREN = 134,
        // SELECT keywords such as DISTINCT, ALL, INTO, etc.
        SELECT_KEYWORD = 135,
        // Aggregate keywords such as GROUP BY, HAVING, etc.
        AGGREGATE_KEYWORD = 136,
        // SQL WHERE keyword
        WHERE_KEYWORD = 137,
        // SELECT INTO keyword
        INTO_KEYWORD = 138,
        // SQL FROM keyword
        FROM_KEYWORD = 139,
        // End of a statement block/Case block
        END_BLOCK = 140,
        // Class, Database Entity, Graph Node
        ENTITY = 141,
        // Graph Edge, Relationship
        EDGE = 142,
        // Relationship between two entities, an edge between two nodes
        RELATIONSHIP = 143,
        // A generic name for some grouping of elements
        COLLECTION = 144,
        // Constant is any string or numeric constant
        CONSTANT = 145,
        // Operand is any operand forming part of an expression
        OPERAND = 146,
        // Filter is any predicate or condition used to reduce a set of data
        FILTER = 147,
        // SORT is any operation or clause used to order a set of data
        SORT = 148
    }
    public class UastNode
    {
        private string _ID;
        private const int zero = 0;
        private List<UastNode> _children; 
        private string _internalType;
        private Hashtable _properties; 
        private List<uast.Role> _roles;
        private string _token;
        private UastNode _parent;

        public UastNode()
        {
            _ID = CorrelationIdGenerator.GetNextId();
            _children = new List<UastNode>();
            _properties = new Hashtable();
            _roles = new List<uast.Role>();
            _internalType = "Unknown";
        }
        public UastNode Parent
        {
            get => _parent;
            set => _parent = value;
        }
        public string InternalType
        {
            set => _internalType = value;
            get => $"\"InternalType\": \"{_internalType}\"";
        }
        public string RawInternalType
        {
            get => _internalType;
        }
        public string Token
        {
            set => _token = value;
            get => $"\"Token\": \"{_token}\"";
        }

        public string RawToken
        {
            get => _token;
        }
        public List<UastNode> Children
        {
            get => _children;
        }
        public string Properties
        {
            get 
            {
                if (_properties.Count > 0)
                {
                    List<string> p = new List<string>();
                    foreach (DictionaryEntry di in _properties)
                    {
                        string value = di.Value.ToString().Trim().StartsWith("[") ? di.Value.ToString() : $"\"{di.Value}\"";
                        string prop = $"\"{di.Key}\": {value}";
                        p.Add("{" + prop + "}");
                    }
                    return $"\"Properties\": [{String.Join(",", p)}]";
                }
                else
                {
                    return "";
                }
            }
        }
        public string Roles
        {
            get
            {
                if (_roles.Count > zero)
                {
                    List<string> r = new List<string>();
                    foreach (uast.Role role in _roles)
                    {
                        r.Add($"\"{role.ToString()}\"");
                    }
                    return $"\"Roles\": [{String.Join(",", r)}]";
                }
                else
                {
                    return "";
                }
            }
        }
        public void AddRole(uast.Role _role) => _roles.Add(_role);
        public void AddChild(uast.UastNode _child) => _children.Add(_child);
        public void AddProperty(string _name, string _value)
        {
            _properties[_name] = _value.Trim().StartsWith("[") ? _value : _value.Replace("\"", "");
        }

        public override string ToString()
        {
            List<string> v = new List<string>();
            v.Add(InternalType);
            if (_token != null)
            {
                v.Add(Token);
            }
            if (_properties.Count > 0)
            {
                v.Add(Properties);
            }
            if (_roles.Count > 0)
            {
                v.Add(Roles);
            }

            List<string> c = new List<string>();
            if (_children.Count > 0)
            {
                foreach (UastNode n in _children)
                {
                    c.Add($"{n.ToString()}");
                }
                v.Add($"\"Children\": [{String.Join(",", c)}]");
            }
            return "{" + String.Join(",", v) + "}";  
        }
        public UastNode GetNodeByType(string _internalType)
        {
            UastNode node = new UastNode();
            if (this._internalType == _internalType)
            {
                node = this;
            }
            else
            {
                foreach (UastNode child in _children)
                {
                    if (child.RawInternalType == _internalType)
                    {
                        node = child;
                        break;
                    }
                }
                if (node.RawInternalType == "Unknown")
                {
                    foreach (UastNode child in _children)
                    {
                        foreach (UastNode grandChild in child.Children)
                        {
                            node = grandChild.GetNodeByType(_internalType);
                            if (node._internalType == _internalType)
                                break;
                        }
                        if (node._internalType == _internalType)
                            break;
                    }
                }
            }
            return node;
        }
        public bool HasChildren() => _children.Count > 0;
        public int ChildCount => _children.Count;
        public UastNode FirstChild() => (HasChildren() == true) ? _children[0] : null;
        public bool HasRole(uast.Role _role) => _roles.Contains(_role);
        public bool HasProperty(string _property) => _properties.ContainsKey(_property);
        public string GetProperty(string _property) => _properties.ContainsKey(_property) ? _properties[_property].ToString() : null;
        public static string FormatJson(string json) => Newtonsoft.Json.Linq.JToken.Parse(json).ToString(Formatting.Indented);
        public static Newtonsoft.Json.Linq.JObject Loads(string _json) => Newtonsoft.Json.Linq.JObject.Parse(_json);
        public static MemoryStream Dumps(UastNode _node)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.WriteLine(FormatJson(_node.ToString()));
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
        public static UastNode BuildAstTree(Newtonsoft.Json.Linq.JObject _jObject)
        {
            UastNode tree = new UastNode();
            tree.InternalType = (string)_jObject["InternalType"];
            tree.Token = (string)_jObject["Token"];
            Newtonsoft.Json.Linq.JArray properties = (Newtonsoft.Json.Linq.JArray) _jObject["Properties"];
            if (properties != null)
            {
                foreach (Newtonsoft.Json.Linq.JObject property in properties)
                {
                    foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> prop in property)
                    {
                        tree.AddProperty(prop.Key, (string)prop.Value);
                    }
                }
            }
            Newtonsoft.Json.Linq.JArray roles = (Newtonsoft.Json.Linq.JArray)_jObject["Roles"];
            foreach (Newtonsoft.Json.Linq.JValue roleString in roles)
            {
                Enum.TryParse(roleString.ToString(), out uast.Role role);
                tree.AddRole(role);
            }
            Newtonsoft.Json.Linq.JArray children = (Newtonsoft.Json.Linq.JArray)_jObject["Children"];
            foreach(Newtonsoft.Json.Linq.JObject child in children)
            {
                tree.AddChild(ParseNode(child, tree));
            }
            return tree;
        }
        private static UastNode ParseNode(Newtonsoft.Json.Linq.JObject _child, UastNode _parent)
        {
            UastNode node = new UastNode();
            node.InternalType = (string)_child["InternalType"];
            node.Token = (string)_child["Token"];
            node.Parent = _parent;
            Newtonsoft.Json.Linq.JArray properties = (Newtonsoft.Json.Linq.JArray)_child["Properties"];
            if (properties != null)
            {
                foreach (Newtonsoft.Json.Linq.JObject property in properties)
                {
                    foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> prop in property)
                    {
                        node.AddProperty(prop.Key, (string)prop.Value);
                    }
                }
            }
            Newtonsoft.Json.Linq.JArray roles = (Newtonsoft.Json.Linq.JArray)_child["Roles"];
            foreach (Newtonsoft.Json.Linq.JValue roleString in roles)
            {
                Enum.TryParse(roleString.ToString(), out uast.Role role);
                node.AddRole(role);
            }
            Newtonsoft.Json.Linq.JArray children = (Newtonsoft.Json.Linq.JArray)_child["Children"];
            if (children != null)
            {
                foreach (Newtonsoft.Json.Linq.JObject child in children)
                {
                    node.AddChild(ParseNode(child, node));
                }
            }
            return node;
        }
    }
}
