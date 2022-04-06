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


        TILDE,
        //Escape character
        SPACE, SHARP, AMPERSAND, DOUBLE_QUOTE, SINGLE_QUOTE,//  # & " '

        //Arithmetic
        MULT, ADD, SUBT, DIV, MOD, //* + - / %

        //Logical
        GREATER, LESSER, // > <
        GREATER_EQUAL, LESSER_EQUAL, // >= <=
        EQUAL, NOT_EQUAL, // == <>
        AND, OR, NOT, // AND OR NOT

        //Variables
        IDENTIFIER, //^([A-Za-z+_+$][A-Za-z+_+$]*)
        CHAR_LIT, //^('.*')
        INT_LIT,  //^([+-]?[0-9]+)
        FLOAT_LIT,  //^([+-]?([0-9]*[.])?[0-9]+) 
        BOOL_LIT,  //^(TRUE|FALSE)

        //RESERVED WORDS
        VAR, AS, OUTPUT,
        IF, ELSE, ELIF, DO, WHILE, START, STOP,
        INT, BOOL, FLOAT, CHAR,
        INPUT, TRUE, FALSE,

    }
   
}
