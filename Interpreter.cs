using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFPL
{
    class Interpreter
    {
        private List<Tokens> tokens;
        private static int tokenCounter, tokenCounter2;
        private static bool foundStart;
        private static List<string> errorMessages;
        private static List<string> outputMessages; 

        List<string> varDeclareList = new List<string>();

        private static Dictionary<string, object> outputMap;
        Dictionary<string, object> declared = new Dictionary<string, object>();
        private int startCount=0;
        private int stopCount;
        private bool foundStop;
        string temp_ident = "";
        string msg = "";
        bool error; 

        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            errorMessages = new List<string>();
            outputMessages = new List<string>();
            tokenCounter = tokenCounter2 = 0; 
            foundStart = foundStop = false;
            outputMap = new Dictionary<string, object>();
            error = false; 
        }

        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> OutputMessages { get { return outputMessages;  } }

        public int Parse()
        {
            object temp;
            {
                while (tokenCounter < tokens.Count) //counts it token by token
                {
                    //Console.WriteLine("Lexeme: " + tokens[tokenCounter].Lexeme); 
                    switch (tokens[tokenCounter].Type) 
                    {
                        case TokenType.VAR:
                            //error messages does not work NGANO MAN KAIRIT HA
                            if (foundStart)
                            {
                                msg = "Invalid variable declaration. Declaration after START at line " + (tokens[tokenCounter].Line + 1);
                                Console.WriteLine(msg); 
                                errorMessages.Add(msg);
                                tokenCounter++;
                                break; 
                            } else
                            {
                                tokenCounter++; //iterate to get the variable name
                                ParseDeclaration();
                            }
                            break;
                         case TokenType.AS:
                            tokenCounter++;
                            ParseAs(); 
                            break; 
                        case TokenType.START: //ERROR DETECTING NOT WORKING HUHU
                            startCount++;
                            if (!foundStart)
                            {
                                foundStart = true; 
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of start at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg); 
                            }
                            tokenCounter++; 
                            break;
                        case TokenType.STOP:
                            stopCount++;
                            //this doesn't really work well yet, need fixing :( 
                            if (!foundStop && foundStart)
                            {
                               
                                foundStop = true;
                            } else
                            {
                                msg = "Syntax Error. Incorrect usage of stop at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg); 
                            }
                            tokenCounter++; 
                            break;
                        case TokenType.IDENTIFIER:
                            //should happen after the var
                            //happens after variable declaration
                            temp_ident = tokens[tokenCounter++].Lexeme;
                            ParseIdentifier(temp_ident); 
                            break;
                        case TokenType.OUTPUT:
                            tokenCounter++;
                            ParseOutput();
                            break; 
                        case TokenType.INT_LIT:
                            temp = (int)tokens[tokenCounter].Literal; //have to check if everything is valid as well
                            tokenCounter++;
                            break;
                        case TokenType.CHAR_LIT:
                            temp = Convert.ToChar(tokens[tokenCounter].Literal);
                            tokenCounter++;
                            break;
                        case TokenType.BOOL_LIT:
                            temp = (string)tokens[tokenCounter].Literal;
                            tokenCounter++;
                            break;
                        case TokenType.FLOAT_LIT:
                            temp = (double)tokens[tokenCounter].Literal;
                            tokenCounter++; 
                            break; 
                        default:
                            tokenCounter++; 
                            break; 
                    }
                    temp_ident = "";
                    temp = null; 
                }
                if (!foundStop)
                {
                    msg = "Program execution failed."; 
                    errorMessages.Add(msg);
                    Console.WriteLine(msg); 
                }
                    
                return errorMessages.Count; 
            }
        }

        //Mostly used if identifier is declared inside the START keyword
        private void ParseIdentifier(string identifier)
        {
            /*
             *   outputMap.Select(i => $"{i.Key}").ToList().ForEach(Console.WriteLine);
             *    Console.WriteLine(tokens[tokenCounter].Lexeme);
             */

            int currentLine = tokens[tokenCounter].Line;
            object temp;
            if(tokens[tokenCounter].Type == TokenType.EQUALS) 
            {
                tokenCounter++;
                tokenCounter2 = tokenCounter;
                List <string> expression= new List<string>(); 
                string a = "";
                if (outputMap.ContainsKey(identifier)) //if there is an variable inside the final outputMap 
                {
                    switch (tokens[tokenCounter].Type )
                    {
                        case TokenType.INT_LIT when outputMap[identifier].GetType() == typeof(Int32):
                            temp = (int)tokens[tokenCounter].Literal;
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.CHAR_LIT when outputMap[identifier].GetType() == typeof(char):
                            temp = Convert.ToChar(tokens[tokenCounter].Literal);
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.BOOL_LIT when outputMap[identifier].GetType() == typeof(string):
                            Console.WriteLine("Inside Boolean");  //add value to the variable; BOOLEAN NOT WORKING YET HUHU
                            temp = Convert.ToString(tokens[tokenCounter].Literal);
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.FLOAT_LIT when outputMap[identifier].GetType() == typeof(double):
                            temp = (double)(tokens[tokenCounter].Literal);
                            outputMap[identifier] = temp;
                            break;
                    }
                } 
                else
                {
                    msg = "Syntax Error. Variable Assignation failed at line " + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    Console.WriteLine(msg); 
                }
                
            }
        }

        private void ParseOutput()
        {
            string temp_identOut = "";
            string output = ""; 
            tokenCounter2 = tokenCounter;
            if (tokens[tokenCounter2].Type == TokenType.COLON) 
            {
                tokenCounter2++;
                Console.WriteLine("Output: "); 
                while(tokens[tokenCounter2].Type == TokenType.IDENTIFIER) {
                    switch(tokens[tokenCounter2].Type)
                    {
                        case TokenType.IDENTIFIER:
                            temp_identOut = tokens[tokenCounter2].Lexeme;
                            Console.WriteLine(temp_identOut); 
                            if (outputMap.ContainsKey(temp_identOut)) //checks if the identifier is inside the final outputMap
                            {
                                output = outputMap[temp_identOut].ToString(); 
                                Console.WriteLine(output);
                                outputMessages.Add(output);  //add it to the messages needed to be outputted
                            } else
                            {
                                msg = "Variable not initialized at line "  + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                                error = true; 
                            }
                            tokenCounter2++;
                            break; 
                    }
                    if (error)
                    {
                        error = false; 
                        break;
                    }
                         
                    
                }
            }
            
        }

        //Checks the token type after the keyword AS  
        //Also saves to the outputmap 
        private void ParseAs()
        {

            switch (tokens[tokenCounter].Type)
            {
                case TokenType.INT:
                    for (int i = 0; i < varDeclareList.Count; i++) //go through the variable declared
                    {
                        string x = varDeclareList[i];
                        if (declared.ContainsKey(x)) //checks if it is being declared together with its value
                        {
                            if (declared[x].GetType() == typeof(int))
                            {
                                outputMap.Add(x, (int)declared[x]); //add it to the outputMap dictionary serves as final list for output
                            }
                            else
                            {
                                msg = "Type Error at Line: " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                                Console.WriteLine(msg); 
                            }
                        }
                        else //if not declared just store 0 temporarily
                        {
                            outputMap.Add(x, 0);
                        }

                    }
                    tokenCounter++;
                    varDeclareList.Clear(); //clear the variable list
                    break;
                case TokenType.CHAR:
                    for (int i = 0; i < varDeclareList.Count; i++)
                    {
                        string x = varDeclareList[i];
                        if (declared.ContainsKey(x)) 
                        {
                            if (declared[x].GetType() == typeof(Char))
                            {
                                outputMap.Add(x, (char)declared[x]);
                                Console.WriteLine(declared[x]); 
                            }
                            else
                            {
                                msg = "Type Error at Line: " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                                Console.WriteLine(msg); 
                            }
                        }
                        else
                        {
                            outputMap.Add(x, ' ');
                        }
                    }
                    tokenCounter++;
                    varDeclareList.Clear(); 
                    break;
                case TokenType.BOOL: //Not Working
                    for (int i = 0; i < varDeclareList.Count; i++)
                    {
                        string x = varDeclareList[i];
                        Console.WriteLine("VAR: " + x); 
                        if (declared.ContainsKey(x))
                        {
                            if (declared[x].GetType() == typeof(string))
                            {
                                outputMap.Add(x, (string)declared[x]);
                                Console.WriteLine(declared[x]);
                            }
                            else
                            {
                                msg = "Type Error at Line: " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                                Console.WriteLine(msg); 
                            }
                        }
                        else
                        {
                            outputMap.Add(x, "FALSE");
                        }
                    }
                    tokenCounter++;
                    varDeclareList.Clear();
                    break;
                case TokenType.FLOAT:
                    for (int i = 0; i < varDeclareList.Count; i++) 
                    {
                        string x = varDeclareList[i];
                        
                        if (declared.ContainsKey(x)) 
                        {
                            if (declared[x].GetType() == typeof(double))
                            {
                                outputMap.Add(x, (double)declared[x]); 
                            }
                            else
                            {
                                msg = "Type Error at Line: " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                            }
                        }
                        else
                        {
                            outputMap.Add(x, 0.0);
                        }

                    }
                    tokenCounter++;
                    varDeclareList.Clear(); //clear the variable list
                    break;
                default:
                    msg = "Syntax Error at line " + ((tokens[tokenCounter].Line + 1));
                    Console.WriteLine(msg); 
                    errorMessages.Add(msg);
                    break; 
            }
        }

        //Get the variable name declared and save it to the declared dictionary
        
       
        
        private void ParseDeclaration()
        {
            if(tokens[tokenCounter].Type == TokenType.IDENTIFIER) 
            {
                varDeclareList.Add(tokens[tokenCounter].Lexeme); //Add the variable to the variable List
                //temp_ident= tokens[tokenCounter].Lexeme; //get the variable name 
                tokenCounter++;
                ParseEqual(); 
                if (tokens[tokenCounter].Type == TokenType.COMMA)
                {
                    ParseCommas(); 
                }
            } 
            else
            {
                msg = "Invalid variable declaration. After VAR is not an identifier at line " + (tokens[tokenCounter].Line + 1);
                errorMessages.Add(msg);
                Console.WriteLine(msg); 
            }

        }

        private void ParseCommas()
        {
            while(tokens[tokenCounter].Type == TokenType.COMMA)
            {
                tokenCounter++;
                if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                {
                    Console.WriteLine("Added to Var Declared List: " + tokens[tokenCounter].Lexeme); 
                    varDeclareList.Add(tokens[tokenCounter].Lexeme); //Add the variable to the variable List
                                                                     //temp_ident= tokens[tokenCounter].Lexeme; //get the variable name 
                    tokenCounter++;
                    ParseEqual(); 
                }
                else
                {
                    msg = "Syntax Error. There is an excess comma at line " + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    Console.WriteLine(msg);
                }
                if(tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                {
                    msg = "Invalid Variable declaration at line " + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    Console.WriteLine(msg);
                }
            }
        }

        private void ParseEqual()
        {
            if (tokens[tokenCounter].Type == TokenType.EQUALS) //if the value is going to get declared as well
            {
                temp_ident = tokens[tokenCounter - 1].Lexeme;
                tokenCounter++;
                switch (tokens[tokenCounter].Type) //Check what type
                {
                    case TokenType.INT_LIT:
                        //save the variable together with its value 
                        declared.Add(temp_ident, (int)tokens[tokenCounter].Literal);
                        tokenCounter++;
                        break;
                    case TokenType.CHAR_LIT:
                        declared.Add(temp_ident, Convert.ToChar(tokens[tokenCounter].Literal));
                        tokenCounter++;
                        break;
                    case TokenType.BOOL_LIT: //Not yet working have to fix the declaration of TRUE and FALSE 
                        declared.Add(temp_ident, (string)(tokens[tokenCounter].Literal));
                        tokenCounter++;
                        break;
                    case TokenType.FLOAT_LIT:
                        //save the variable together with its value 
                        declared.Add(temp_ident, (double)tokens[tokenCounter].Literal);
                        tokenCounter++;
                        break;
                    default:
                        msg = "Syntax Error at line " + ((tokens[tokenCounter].Line + 1));
                        errorMessages.Add(msg);
                        tokenCounter++;
                        Console.WriteLine(msg);
                        break;
                }
            }
        }
    }
}
