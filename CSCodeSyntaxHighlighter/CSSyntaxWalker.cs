using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSCodeSyntaxHighlighter
{
    internal class CSSyntaxWalker : SyntaxWalker
    {
        private SemanticModel semanticModel;
        private HtmlBuilder builder;

        /// <summary>
        /// コードの解析を行い、builderに記録します
        /// </summary>
        public void Analyze(string code,HtmlBuilder builder)
        {
            this.builder = builder;
            // コードを解析して構文木を作成
            var tree = CSharpSyntaxTree.ParseText(code);
            // 解析結果から診断を取得
            foreach(var item in tree.GetDiagnostics())
            {
                this.builder.AppendSyntaxDiagnostic(item);
            }

            // コンパイル環境を用意
            var compilation = CSharpCompilation.Create("sample",
                syntaxTrees : new[] { tree },
                references: new[] { new MetadataFileReference(typeof(Object).Assembly.Location) });
            // セマンティックモデル(構文木を走査してNameSyntaxなどの意味を解釈したもの)を取得
            semanticModel = compilation.GetSemanticModel(tree);
            // 解析結果から診断を取得
            foreach(var item in compilation.GetDiagnostics())
            {
                this.builder.AppendSemanticDiagnostic(item);
            }

            // 各トークンを見ていく
            foreach (var token in tree.GetRoot().DescendantTokens())
            {
                this.VisitToken(token);
            }

            // 初期化
            this.builder = null;
            this.semanticModel = null;
        }

        protected override void VisitToken(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
            {
                foreach (var trivia in token.LeadingTrivia)
                {
                    VisitTrivia(trivia);
                }
            }

            bool isProcessed = false;

            // キーワードであるか
            if (token.IsKeyword())
            {
                this.builder.Write(TokenKind.Keyword, token.ValueText);
                isProcessed = true;

            } else
            {
                switch (token.CSharpKind())
                {
                    // 各種リテラルであるか
                    case SyntaxKind.StringLiteralToken:
                        this.builder.Write(TokenKind.StringLiteral, '"' + token.ValueText + '"');
                        isProcessed = true;
                        break;
                    case SyntaxKind.CharacterLiteralToken:
                        this.builder.Write(TokenKind.CharacterLiteral, token.ValueText);
                        isProcessed = true;
                        break;
                    case SyntaxKind.NumericLiteralToken:
                        this.builder.Write(TokenKind.NumberLiteral, token.ValueText);
                        isProcessed = true;
                        break;
                    case SyntaxKind.IdentifierToken:
                        // 何かの名前(変数等)を参照しようとした場合
                        if (token.Parent is SimpleNameSyntax)
                        {
                            var name = (SimpleNameSyntax)token.Parent;
                            // 参照先に関する情報を取得
                            var info = semanticModel.GetSymbolInfo(name);
                            if (info.Symbol != null && info.Symbol.Kind != SymbolKind.ErrorType)
                            {
                                switch (info.Symbol.Kind)
                                {
                                    case SymbolKind.NamedType:
                                        // クラスや列挙などの場合は色づけ
                                        this.builder.Write(TokenKind.Identifier, token.ValueText);
                                        isProcessed = true;
                                        break;
                                    case SymbolKind.Namespace:
                                    case SymbolKind.Parameter:
                                    case SymbolKind.Local:
                                    case SymbolKind.Field:
                                    case SymbolKind.Property:
                                        // それ以外は通常の色
                                        this.builder.Write(TokenKind.None, token.ValueText);
                                        isProcessed = true;
                                        break;
                                }
                            }
                        } else if (token.Parent is TypeDeclarationSyntax)
                        {
                            // 宣言時のStatementがヒットした場合
                            var name = (TypeDeclarationSyntax)token.Parent;
                            var info = semanticModel.GetDeclaredSymbol(name);
                            if (info != null && info.Kind != SymbolKind.ErrorType)
                            {
                                switch (info.Kind)
                                {
                                    case SymbolKind.NamedType:
                                        this.builder.Write(TokenKind.Identifier, token.ValueText);
                                        isProcessed = true;
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            // それ以外の項目 (今のところ、特殊例はすべて色づけしない)
            if (!isProcessed)
            {
                this.builder.Write(TokenKind.None, token.ValueText);
            }

            if (token.HasTrailingTrivia)
            {
                foreach(var trivia in token.TrailingTrivia)
                {
                    VisitTrivia(trivia);
                }
            }

        }

        protected override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.CSharpKind())
            {
                // コメント
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                    this.builder.Write(TokenKind.Comment, trivia.ToFullString());
                    break;
                // 無効になっているテキスト
                case SyntaxKind.DisabledTextTrivia:
                    this.builder.Write(TokenKind.DisabledText, trivia.ToFullString());
                    break;
                // ドキュメントコメント
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                    this.builder.Write(TokenKind.Comment, trivia.ToFullString());
                    break;
                // #region
                case SyntaxKind.RegionDirectiveTrivia:
                case SyntaxKind.EndRegionDirectiveTrivia:
                    this.builder.Write(TokenKind.Region, trivia.ToFullString());
                    break;
                // 空白
                case SyntaxKind.WhitespaceTrivia:
                    this.builder.Write(TokenKind.WhiteSpace, trivia.ToFullString());
                    break;
                // 改行
                case SyntaxKind.EndOfLineTrivia:
                    this.builder.Write(TokenKind.EOL, trivia.ToFullString());
                    break;
                // それ以外
                default:
                    this.builder.Write(TokenKind.None, trivia.ToFullString());
                    break;
            }
            base.VisitTrivia(trivia);
        }

    }


}
