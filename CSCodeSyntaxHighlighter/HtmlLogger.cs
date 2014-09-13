using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace CSCodeSyntaxHighlighter
{
    class HtmlLogger
    {
        private StringBuilder builder = null;

        private StringBuilder syntaxDiagnosticsBuilder = null;

        private StringBuilder semanticDiagnosticsBuilder = null;

        private string targetFilePath = "";

#region "Html Templates"
        private const string DIAGNOSTIC_ITEM_TEMPLATE = @" <li class='{0}'>#{2} <strong>{0}</strong> {1} @ {3}</li>";

        private const string TEMPLATE = @"
<!DOCTYPE html>
<html lang='ja'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title>{1} - cs2html</title>
    <style>
        {0}
    </style>
</head>
<body>
    <header>
        <h1>CSCodeSyntaxHighlighter</h1>
        <p>Simple C# Syntax Highlighter Powered by <a href='http://roslyn.codeplex.com/' target='_blank'>Roslyn </a></p>
    </header>
    <div id='maincontents'>
        <div id='main'>
            <div id='info'>
                <p><span id='langName'> C#</span></p>
                <p>File Name: <span id='fileName'>{1}</span></p>
            </div>
            <div id='syntax-result'>
                <h3>Syntax Diagnostics</h3>
                <ul>
                    {2}
                </ul>
            </div>
            <div id='semantic-result'>
                <h3>Semantic Diagnostics</h3>
                <ul>
                    {3}
                </ul>
            </div>
            <h2>コード</h2>
            <p id='code'>{4}</p>
        </div>

        <footer>
            <div id='footeritem-container'>
                <p id='copyright' class='wrapper'>&copy; Copyrights 2014 <a href='http://bonprosoft.com/'>Yuki Igarashi</a> All Rights Reserved.</p>
            </div>
        </footer>
    </div>

</body>
</html>
";

        private const string CSS = @" 
        * {
            margin: 0;
            padding: 0;
            outline: 0;
            text-decoration: none;
            -webkit-box-sizing: border-box !important;
            -moz-box-sizing: border-box !important;
            -ms-box-sizing: border-box !important;
            box-sizing: border-box !important;
        }

        html {
            background: #ececec;
        }

        html, body {
            margin: 0px;
            padding: 0;
        }

        body {
            background: #CACACA;
            color: #333;
            font-size: 14px;
            font-family: Meiryo, 'メイリオ', ArialMT, Hiragino Kaku Gothic Pro, 'ヒラギノ角ゴ Pro W3', Osaka, Verdana, 'ＭＳ Ｐゴシック';
            line-height: 160%;
            /*margin-top: 170px;*/
            min-width: 300px;
        }

        h1, h2, h3, h4, h5, h6 {
            font-weight: normal;
        }

        h1 {
            margin-bottom: 2px;
            font-size: 240%;
            line-height: 1.2em;
            font-family: 'Josefin Sans', sans-serif;
        }

        h2 {
           	font-size: 1.5em;
            border-style: none none solid solid;
            border-width: 0 0 0.1em 0.5em;
            border-color: purple;
            padding: 0.2em 0em 0.2em 0.4em;
            margin-bottom: 1em;
        }

        header {
            background-color: purple;
            overflow: hidden;
            background: #2f4f4f;
            border-bottom: 4px solid #727272;
            margin: 0;
            padding: 10px;
        }

            header * {
                color: white;
            }

            header h1 {
                color: white;
                font-weight: bold;
            }

        #maincontents {
            /*background: white;*/
            margin: 1.4em 0 1.4em 0;
            clear: both;
            overflow: hidden;
        }

        #main {
            margin: 10px 10px;
            background: white;
            border-radius: 4px;
            box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.25);
            padding: 1.3em;
            overflow: hidden;
        }

        #info {
            border-bottom: solid #CCC 1px;
            margin-bottom: 10px;
        }

            #info p #langName {
                color: rgb(224,0,0);
                display: block;
                font-size: 1em;
            }

        #syntax-result {
            border: 1px solid red;
            padding: 0;
            margin-bottom: 20px;
        }

        #syntax-result h3 {
            background-color: red;
            padding: 0.5em;
            color: white;
        }

        #syntax-result ul {
            padding: 1em;
            margin: 0 0 0 1em;
            background-color: white;
            word-break: break-all;
        }

        #semantic-result {
            border: 1px solid orange;
            padding: 0;
            margin-bottom: 20px;
        }

        #semantic-result h3 {
            background-color: orange;
            padding: 0.5em;
            color: white;
        }

        #semantic-result ul {
               padding: 1em;
            margin: 0 0 0 1em;
            background-color: white;
            word-break: break-all;
        }

        footer {
            clear: both;
            overflow: hidden;
            background: #f5f5f5;
            /*background-attachment: fixed;*/
            background: #ECECEC;
            border-top: 4px solid #3E687F;
        }

        #footeritem-container {
            margin: 0 auto 10px auto;
            padding: 4px;
        }

        .footeritem {
            display: inline-block;
            margin: 10px;
            vertical-align: top;
            padding: 5px 10px 5px 10px;
        }

        #code {
            -ms-word-break: break-all;
            word-break: break-all;
        }

        #fileName {
            font-size: 1.5em;
            line-height: 1.2em;
        }

        /* Syntax Highlighting */
        .Keyword {
            color: #0000ff;
        }

        .Identifier {
            color: #2b91af;
        }

        .StringLiteral {
            color: #a31515;
        }

        .CharacterLiteral {
            color: #d202fe;
        }

        .Comment {
            color: #008000;
        }

        .NumberLiteral {
            color: #B5CEA8;
        }

        .DisabledText {
            color: #ABABAB;
        }

        .Region {
            color: #e0e0e0;
        }
";

#endregion

        public HtmlLogger(string target)
        {
            this.targetFilePath = target;
            builder = new StringBuilder();
            syntaxDiagnosticsBuilder = new StringBuilder();
            semanticDiagnosticsBuilder = new StringBuilder();
        }

        /// <summary>
        /// 各種トークンを受け取りStringBuilderに格納します
        /// </summary>
        public void Write(TokenKind kind,string value)
        {
            switch (kind)
            {
                case TokenKind.None:
                    builder.Append(Escape(value));
                    break;
                case TokenKind.WhiteSpace:
                case TokenKind.EOL:
                    builder.Append(Escape(value));
                    break;
                default:
                    builder.Append("<span class=\"" + kind.ToString() + "\">" + Escape(value) + "</span>");
                    break;
            }
        }

        /// <summary>
        /// 構文に関する診断を格納します
        /// </summary>
        public void AppendSyntaxDiagnostic(Diagnostic item)
        {
            this.syntaxDiagnosticsBuilder.AppendLine(String.Format(DIAGNOSTIC_ITEM_TEMPLATE,
                item.Severity.ToString(),
                item.GetMessage(),
                item.Id,
                item.Location.SourceSpan.ToString()));
        }

        /// <summary>
        /// SemanticAnalysisに関する診断を格納します
        /// </summary>
        public void AppendSemanticDiagnostic(Diagnostic item)
        {
            this.semanticDiagnosticsBuilder.AppendLine(String.Format(DIAGNOSTIC_ITEM_TEMPLATE,
                item.Severity.ToString(),
                item.GetMessage(),
                item.Id,
                item.Location.SourceSpan.ToString()));
        }

        /// <summary>
        /// HTML出力用に文字列のエスケープを行います
        /// </summary>
        private string Escape(string value)
        {
            return WebUtility.HtmlEncode(value).Replace("\n","<br>").Replace(" ", "&nbsp;").Replace("\t","&nbsp;&nbsp;&nbsp;&nbsp;");
        }

        /// <summary>
        /// HTMLを生成します
        /// </summary>
        public string BuildHtml()
        {
            return String.Format(TEMPLATE, CSS,this.targetFilePath,
                syntaxDiagnosticsBuilder.ToString(),
                semanticDiagnosticsBuilder.ToString(),
                builder.ToString());
        }

    }
}
