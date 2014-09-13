using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace CSCodeSyntaxHighlighter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 2)
            {
                Console.WriteLine("Usage: cs2html [.cs file path] [Save path]");
                return;
            }

            var code = "";
            Console.WriteLine("Input: {0}", args[0]);
            Console.WriteLine("SavePath: {0}", args[1]);

            using (var reader = new StreamReader(args[0]))
            {
                code = reader.ReadToEnd();
            }

            var builder = new HtmlBuilder(args[0]);
            var walker = new CSSyntaxWalker();
            walker.Analyze(code, builder);

            using (var writer = new StreamWriter(args[1]))
            {
                writer.WriteLine(builder.BuildHtml());
            }

            Console.WriteLine("Finished!");
        }
    }
}
