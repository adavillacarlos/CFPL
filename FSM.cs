using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class FSM
    {
        private List<Tokens> tokens;
        int state, input;
        int [,] stateTable; 

        public FSM(List<Tokens> tokens)
        {
            this.tokens = tokens;
        }

        public int Declaration(List<Tokens> tokens, int tokenCounter)
        {
           stateTable = new int [,] {
                { 1, 7, 7, 7, 7, 7, 7},
                { 7, 2, 7, 7, 7, 7, 7},
                { 7, 7, 1, 5, 7, 3, 7},
                { 7, 7, 7, 7, 7, 7, 4},
                { 7, 7, 7, 7, 7, 7, 7},
                { 7, 7, 7, 7, 6, 7, 7},
                { 7, 7, 1, 7, 7, 3, 7},
                { 7, 7, 7, 7, 7, 7, 7},
            };
            state = 0;
            input = 0;
            int line = tokens[tokenCounter].Line + 1; 
            while (tokens[tokenCounter].Line < line)
            {
                switch (tokens[tokenCounter].Type)
                {
                    case TokenType.VAR:
                        input = 0;
                        break;
                    case TokenType.IDENTIFIER:
                        input = 1;
                        break;
                    case TokenType.COMMA:
                        input = 2;
                        break;
                    case TokenType.EQUALS:
                        input = 3;
                        break;
                    case TokenType.INT_LIT:
                        input = 4;
                        break;
                    case TokenType.FLOAT_LIT:
                        input = 4;
                        break;
                    case TokenType.CHAR_LIT:
                        input = 4;
                        break;
                    case TokenType.BOOL_LIT:
                        input = 4;
                        tokenCounter++; 
                        break;
                    case TokenType.AS:
                        input = 5;
                        break;
                    case TokenType.INT:
                        input = 6;
                        break;
                    case TokenType.FLOAT:
                        input = 6;
                        break;
                    case TokenType.CHAR:
                        input = 6;
                        break;
                    case TokenType.BOOL:
                        input = 6;
                        break;
                }
                state = stateTable[state, input]; 
                if (state == 7)
                    break;
                tokenCounter++;
            }
            if (state == 4)
                return 1;
            else
                return 0; 
        }

        public int Output(List<Tokens> tokens, int tokenCounter)
        {
            stateTable = new int[,] {
              { 1, 4, 4, 4, 4, 4, 4, 4, 4 },
              { 4, 2, 4, 4, 4, 4, 4, 4, 4 },
              { 4, 4, 3, 4, 5, 4, 4, 4, 4 },
              { 4, 4, 4, 2, 4, 4, 4, 4, 4 },
              { 4, 4, 4, 4, 4, 4, 4, 4, 4 },
              { 4, 4, 6, 4, 4, 6, 7, 4, 6 },
              { 4, 4, 4, 4, 3, 4, 4, 4, 4 },
              { 8, 8, 8, 8, 8, 8, 8, 8, 8 },
              { 4, 4, 4, 4, 3, 4, 4, 6, 4 },
            };
            state = 0;
            input = 0;
            int line = tokens[tokenCounter].Line + 1;
            while (tokens[tokenCounter].Line < line)
            {
                switch (tokens[tokenCounter].Type)
                {
                    case TokenType.OUTPUT:
                        input = 0;
                        break;
                    case TokenType.COLON:
                        input = 1;
                        break;
                    case TokenType.IDENTIFIER:
                        input = 2;
                        break;
                    case TokenType.AMPERSAND:
                        input = 3;
                        break;
                    case TokenType.DOUBLE_QUOTE:
                        input = 4;
                        break;
                    case TokenType.TILDE:
                        input = 5;
                        break;
                    case TokenType.SHARP:
                        input = 5;
                        break;
                    case TokenType.LEFT_BRACE:
                        input = 6;
                        break;
                    case TokenType.RIGHT_BRACE:
                        input = 7;
                        break;
                    default:
                        input= 8; 
                        break; 

                }
                state = stateTable[state, input];
                if (state == 4)
                    break;
                tokenCounter++;
            }
            if (state == 3)
                return 1;
            else
                return 0;
        }

        public int Input(List<Tokens> tokens, int tokenCounter)
        {
            stateTable = new int[,] {
                { 1, 4, 4, 4 },
                { 4, 2, 4, 4 },
                { 4, 4, 3, 4 },
                { 4, 4, 4, 2 },
                { 4, 4, 4, 4 }
            };
            state = 0;
            input = 0;

            int line = tokens[tokenCounter].Line + 1;
            while (tokens[tokenCounter].Line < line)
            {
                switch (tokens[tokenCounter].Type)
                {
                    case TokenType.INPUT:
                        input = 0;
                        break;
                    case TokenType.COLON:
                        input = 1;
                        break;
                    case TokenType.IDENTIFIER:
                        input = 2;
                        break;
                    case TokenType.COMMA:
                        input = 3;
                        break;
                }

                state = stateTable[state, input];
                if (state == 4)
                    break;
                tokenCounter++;
            }
            if (state == 3)
                return 1;
            else
                return 0;
        }
    }
}
