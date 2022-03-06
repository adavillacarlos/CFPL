using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    public enum TokenType
    {
        //Valid character tokens
        LEFT_PAREN, RIGHT_PAREN, //( )
        LEFT_BRACE, RIGHT_BRACE, //[]  
        COMMA, EQUALS, COLON,  // , =  :



        //Esacape character
        SHARP, AMPERSAND, QUOTE,//# & "

        //Operation
        MULT, ADD, SUBT, DIV, MOD, //* + - / %

        //Logical
        GREATER, LESSER, // > <
        GREATER_EQUAL, LESSER_EQUAL, // >= <=

        EQUAL, NOT_EQUAL, // = == <>

        //Variables
        IDENTIFIER, //^([A-Za-z+_+$][A-Za-z+_+$]*)
        CHAR_LIT, //^('.*')
        INT_LIT,  //^([+-]?[0-9]+)
        FLOAT_LIT,  //^([+-]?([0-9]*[.])?[0-9]+) 
        BOOL_LIT,  //^(TRUE|FALSE)

        //RESERVED WORDS
        VAR, AS, OUTPUT, IF, ELSE, ELIF,
        WHILE, START, STOP, INT,
        BOOL, FLOAT, CHAR,
        AND, OR, NOT, INPUT, TRUE, FALSE
    }
}
