using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSCodeSyntaxHighlighter
{
    internal enum TokenKind
    {
        None,
        Keyword,
        Identifier,
        StringLiteral,
        CharacterLiteral,
        Comment,
        DisabledText,
        Region,
        NumberLiteral,
        WhiteSpace,
        EOL
    }
}
