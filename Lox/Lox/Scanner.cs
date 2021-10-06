using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lox.TokenType;

namespace Lox
{
    public class Scanner
    {
        private static readonly Dictionary<String, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {"and",    AND },
            { "class",  CLASS  }  ,
            { "else",   ELSE }  ,
            { "false",  FALSE  }  ,
            { "for",    FOR  }  ,
            { "fun",    FUN  }  ,
            { "if",     IF   }  ,
            { "nil",    NIL  }  ,
            { "or",     OR   }  ,
            { "print",  PRINT  }  ,
            { "return", RETURN  } ,
            { "super",  SUPER  }  ,
            { "this",   THIS  }  ,
            { "true",   TRUE  }  ,
            { "var",    VAR  }  ,
            { "while",  WHILE  }
        };

        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                scanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(': addToken(LEFT_PAREN); break;
                case ')': addToken(RIGHT_PAREN); break;
                case '{': addToken(LEFT_BRACE); break;
                case '}': addToken(RIGHT_BRACE); break;
                case ',': addToken(COMMA); break;
                case '.': addToken(DOT); break;
                case '-': addToken(MINUS); break;
                case '+': addToken(PLUS); break;
                case ';': addToken(SEMICOLON); break;
                case '*': addToken(STAR); break;
                case '!':
                    addToken(match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    addToken(match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    addToken(match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    addToken(match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case '/':
                    if (match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else
                    {
                        addToken(SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;
                case '"': String(); break;
                default:
                    if (isDigit(c))
                    {
                        number();
                    }
                    else if (isAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        Lox.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void identifier()
        {
            while (isAlphaNumeric(peek())) advance();
            String text = source.Substring(start, current - start);
            if(!keywords.TryGetValue(text, out TokenType type))
                type = IDENTIFIER;
            addToken(type);
        }

        private void number()
        {
            while (isDigit(peek())) advance();

            // Look for a fractional part.
            if (peek() == '.' && isDigit(peekNext()))
            {
                // Consume the "."
                advance();

                while (isDigit(peek())) advance();
            }

            addToken(NUMBER,
                Double.Parse(source.Substring(start, current - start)));
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source.ElementAtOrDefault(current + 1);
        }

        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void String() {
            while (peek() != '"' && !isAtEnd()) {
                if (peek() == '\n') line++;
                advance();
            }

            if (isAtEnd()) {
                Lox.error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            advance();

            // Trim the surrounding quotes.
            String value = source.Substring(start + 1, current - 1);
            addToken(STRING, value);
        }

        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source.ElementAtOrDefault(current);
        }

        private bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source.ElementAtOrDefault(current) != expected) return false;

            current++;
            return true;
        }
        private char advance()
        {
            return source.ElementAtOrDefault(current++);
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, Object literal)
        {
            String text = source.Substring(start, current -start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }
    }
}
