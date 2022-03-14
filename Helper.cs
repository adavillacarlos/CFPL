using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Helper
    {
        private List<Tokens> tokens;
        int state;
        int input; 

        public Helper(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
        }

        public int Declaration(List<Tokens> tokens, int tokenCounter)
        {
            int[,] stateTable = new int[,]
            {
                {1, 7, 7, 7, 7, 7, 7 },
                {7, 2, 7, 7, 7, 7, 7 },
                {7, 7, 1, 5, 7, 3, 7 },
                {7, 7, 7, 7, 7, 7, 4 },
                {7, 7, 7, 7, 7, 7, 7 },
                {7, 7, 7, 7, 6, 7, 7 },
                {7, 7, 1, 7, 7, 3, 7 },
                {7, 7, 7, 7, 7, 7, 7 }, 
            };

            input = 0;
            state = 0; 

            while (tokens[tokenCounter].Lexeme != "START")
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

            int[,] stateTable = new int[,]
            {
                { 1, 4, 4, 4, 4 },
                { 4, 2, 4, 4, 4},
                { 4, 4, 3, 4, 3 },
                { 4, 4, 4, 2, 4},
                { 4, 4, 4, 4, 4 }
            };

            input = 0;
            state = 0;
            return 1;
        }
    }
}
