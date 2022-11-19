using System;
using System.Collections.Generic;
using  System.Text.RegularExpressions;

public class LispishParser
{
    public  class Parser {
        List<Node> tokens = new List<Node>();
        int curr = 0;
        
        public Parser(Node [] tokens) {
            this.tokens = new List<Node>(tokens);
            this.tokens.Add(new Node(Symbols.INVALID, ""));
        }

        public Node ParseProgram() {
            // Console.WriteLine($"{cur,10}: Program");
            //<program> ::= {<expr>}
            var children = new List<Node>();

            while (tokens[curr].Symbol != Symbols.INVALID){
                children.Add(ParseSExpr());
                // Console.WriteLine("here");
            }
            
            return new Node(Symbols.Program, children.ToArray());
        }

        public Node ParseSExpr() {
            // decide the case 
            if (tokens[curr].Text == "(") {
                return new Node(Symbols.SExpr, ParseList());
            } else {
                return new Node(Symbols.SExpr, ParseAtom());
            }
        }

        public Node ParseList() {
            // first case - the list is just ()
            if (tokens[curr].Text == "(" 
                && (curr + 1 < tokens.Count 
                    && tokens[curr + 1].Text == ")")) 
            {
                var lParen = ParseLiteral("(");
                var rParen = ParseLiteral(")");
                return new Node(Symbols.List, lParen, rParen);
            }
            // second case - the list is (<expr>)
            else { 
                var lParen = ParseLiteral("(");
                var expr = ParseSeq();
                var rParen = ParseLiteral(")");
                return new Node(Symbols.List, lParen, expr, rParen);
            }
        }

        public Node ParseSeq() {
            // prepare
            var children = new List<Node>();

            // parse SExpr first
            children.Add(ParseSExpr());

            // recursively see if there's more Seq
            if (tokens[curr].Text != ")") {
                children.Add(ParseSeq());
            }

            // result
            return new Node(Symbols.Seq, children.ToArray());
        }

        public Node ParseAtom() {
            // if it's id
            if (tokens[curr].Symbol == Symbols.ID) {
                return new Node(Symbols.Atom, tokens[curr++]);
            } else if (tokens[curr].Symbol == Symbols.INT) {
                return new Node(Symbols.Atom, tokens[curr++]);
            } else if (tokens[curr].Symbol == Symbols.REAL) {
                return new Node(Symbols.Atom, tokens[curr++]);
            } else if (tokens[curr].Symbol == Symbols.STRING) {
                return new Node(Symbols.Atom, tokens[curr++]);
            } else {
                throw new Exception("Syntax error");
            }
        }
    
        public Node ParseLiteral(string lit) {
            if (tokens[curr].Text == lit){
                return tokens[curr++];
            } else {
                throw new Exception("Syntax error");
            }
        }
    }

    public enum Symbols{
        INVALID,
        ID, 
        REAL,
        INT, 
        STRING,
        LITERAL,

        Program, 
        SExpr, 
        List,
        Seq, 
        Atom 
    }

    public class Node
    {
        public Symbols Symbol;
        public string Text = "";
        List<Node> children = new List<Node>();

        public Node(Symbols symbol, string text){
            this.Symbol = symbol;
            this.Text = text;
        }

        public Node(Symbols symbol, params Node[] children){
            this.Symbol = symbol;
            this.Text = "";
            this.children = new List<Node>(children);
        }

        public void Print(string prefix, int padding)
        {
            Console.WriteLine($"{prefix}{Symbol.ToString().PadRight(padding - prefix.Length)} {Text}");
            foreach (var child in children){
                child.Print(prefix + "  ", padding);
            }
        }
    }

    static public List<Node> Tokenize(String src)
    {
        // prepare
        var result = new List<Node>();
        Match m;

        // regex parser
        var WS = new Regex(@"\G\s");
        var LIT = new Regex(@"\G[\\(\\)]");
        var REAL = new Regex(@"\G(?>\+|-)?[0-9]*\.[0-9]+");
        var INT = new Regex(@"\G(?>\+|-)?[0-9]+");
        var STRING = new Regex(@"\G""(?>\\.|[^""])*""");
        var ID = new Regex(@"\G[^\s""\(\)]+");

        // tokenize
        int pos = 0;
        while (pos < src.Length) {
            // Console.WriteLine($"pos={pos} src[pos]={src[pos]}");
            if ((m = WS.Match(src, pos)).Success) {
                pos += m.Length;
            } else if ((m = LIT.Match(src, pos)).Success){
                result.Add(new Node(Symbols.LITERAL, m.Value));
                pos += m.Length;
            } else if ((m = REAL.Match(src, pos)).Success){
                result.Add(new Node(Symbols.REAL, m.Value));
                pos += m.Length;
            } else if ((m = INT.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.INT, m.Value));
                pos += m.Length;
            } else if ((m = STRING.Match(src, pos)).Success){
                result.Add(new Node(Symbols.STRING, m.Value));
                pos += m.Length;
            } else if ((m = ID.Match(src, pos)).Success){
                result.Add(new Node(Symbols.ID, m.Value));
                pos += m.Length;
            } else {
                throw new Exception("Lexer error");
            }
        }

        // return result
        return result;
    }

    static public Node Parse(Node[] tokens)
    {
        var parse = new Parser(tokens);
        var program = parse.ParseProgram();
        return program;
    }

    static private void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.Symbol,-21}\t: {node.Text}");
            }
            Console.WriteLine(new String('-', 50));

            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print("", 40);
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
        // CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\\"" "")");
        // CheckString(@"(define foo ""Say \\""Chease!\\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
        // CheckString(Console.ReadLine());
    }
}

