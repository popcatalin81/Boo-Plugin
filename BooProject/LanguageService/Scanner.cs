﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Boo.Lang.Parser;
using System.IO;

namespace Hill30.BooProject.LanguageService
{
    class Scanner : IScanner
    {
        antlr.TokenStream lexer;
        antlr.IToken stashed_token = null;
        int offset;
        int current;

        #region IScanner Members

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            var token = stashed_token;
            if (token == null)
                try
                {
                    token = lexer.nextToken();
                }
                catch (Exception)
                {
                    return false;
                }

            if (current < token.getColumn() - 1)
            {
                tokenInfo.StartIndex = current;
                tokenInfo.EndIndex = offset + token.getColumn() - 2;
                tokenInfo.Type = TokenType.Comment;
                tokenInfo.Color = TokenColor.Comment;
                current = tokenInfo.EndIndex + 1;
                stashed_token = token;
                return true;
            }

            stashed_token = null;
            int quotes = 0;
            switch (token.Type)
            {
                case BooLexer.EOL:
                case BooLexer.EOF:
                    return false;

                case BooLexer.TRIPLE_QUOTED_STRING:
                    quotes = 6;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case BooLexer.DOUBLE_QUOTED_STRING:
                case BooLexer.SINGLE_QUOTED_STRING:
                    quotes = 2;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case BooLexer.WS:
                    tokenInfo.Type = TokenType.WhiteSpace;
                    tokenInfo.Color = TokenColor.Text;
                    break;
                
                case BooLexer.ID:
                    tokenInfo.Type = TokenType.Identifier;
                    tokenInfo.Color = TokenColor.Identifier;
                    break;
                
                case BooLexer.PUBLIC:
                case BooLexer.DEF:
                case BooLexer.CLASS:
                case BooLexer.IMPORT:
                case BooLexer.NAMESPACE:
                    tokenInfo.Type = TokenType.Keyword;
                    tokenInfo.Color = TokenColor.Keyword;
                    break;
                
                default:
                    tokenInfo.Color = TokenColor.Text;
                    break;
            }

            tokenInfo.StartIndex = offset + token.getColumn() - 1;
            tokenInfo.EndIndex = offset + quotes + token.getColumn() - 1 + token.getText().Length - 1;
            current = tokenInfo.EndIndex + 1;
            return true;
        }

        public void SetSource(string source, int offset)
        {
            current = this.offset = offset;
            lexer = BooParser.CreateBooLexer(1, "Line Scanner", new StringReader(source.Substring(offset)));
        }

        #endregion
    }
}