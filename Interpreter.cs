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
        Dictionary<string, object> declaredVariables = new Dictionary<string, object>();
        private int startCount = 0;
        private int stopCount;
        private bool foundStop;
        string temp_ident = "";
        string msg = "";
        bool error;
        int result;
        string stringInput;
        FSM fsm;
        List<String> inputVariables = new List<string>();
        private List<Tokens> infixTokens;
        private Operations operation;

        public Interpreter(List<Tokens> t, string input)
        {
            tokens = new List<Tokens>(t);
            errorMessages = new List<string>();
            outputMessages = new List<string>();
            tokenCounter = tokenCounter2 = 0;
            foundStart = foundStop = false;
            outputMap = new Dictionary<string, object>();
            error = false;
            fsm = new FSM(tokens);
            stringInput = input;
        }

        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> OutputMessages { get { return outputMessages; } }

        public int Parse()
        {
            object temp;
            int startLine=0; 
            if(tokens[tokens.Count-1].Type != TokenType.STOP)
            {
                errorMessages.Add("Missing STOP keyword"); 
            }
            while (tokenCounter < tokens.Count) //counts it token by token
            {
                switch (tokens[tokenCounter].Type)
                {
                    case TokenType.MULT:
                        // where multiplication token is correctly placed
                        if (tokens[tokenCounter - 1].Type == TokenType.RIGHT_PAREN || tokens[tokenCounter - 1].Type == TokenType.RIGHT_BRACE ||
                            tokens[tokenCounter - 1].Type == TokenType.FLOAT_LIT || tokens[tokenCounter - 1].Type == TokenType.INT_LIT ||
                            tokens[tokenCounter - 1].Type == TokenType.IDENTIFIER)
                        {

                        }
                        else
                        {
                            int line = tokens[tokenCounter].Line; // comment's line
                            while (line >= tokens[tokenCounter].Line) // skip all tokens with the same line as comment's 
                            {
                                tokenCounter++;
                            }
                        }
                        break;
                    case TokenType.VAR:
                        result = fsm.Declaration(tokens, tokenCounter);
                        if (foundStart)
                        {
                            msg = "Invalid variable declaration due to START at line " + (tokens[tokenCounter].Line + 1);
                            Console.WriteLine(msg);
                            errorMessages.Add(msg);
                            tokenCounter++;
                            break;
                        }
                        else
                        {
                            tokenCounter++; //iterate to get the variable name
                            ParseDeclaration();
                            if (error)
                                break;
                        }
                        break;
                    case TokenType.AS:
                        tokenCounter++;
                        ParseAs();
                        break;
                    case TokenType.START: //ERROR DETECTING NOT WORKING HUHU
                        startCount++;
                        startLine = tokens[tokenCounter].Line;

                        if (!foundStart)
                        {
                            foundStart = true;
                        }
                        else
                        {
                            msg = "Syntax Error. Incorrect usage of START at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                        tokenCounter++;
                        break;
                    case TokenType.STOP:
                        stopCount++;
                        //this doesn't really work well yet, need fixing 🙁 
                        if (startLine != tokens[tokenCounter].Line && !foundStop && foundStart)
                        {
                            foundStop = true;
                        }
                        else
                        {
                            msg = "Incorrect usage of STOP at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                        tokenCounter++;
                        break;
                    case TokenType.IDENTIFIER:
                        temp_ident = tokens[tokenCounter].Lexeme;
                        tokenCounter++;
                        if (foundStart)
                        {
                            List<Tokens> postfix = new List<Tokens>();
                            // check if variable is declared
                            if (outputMap.ContainsKey(temp_ident))
                            {
                               if(tokens[tokenCounter].Type == TokenType.EQUALS)
                               {
                                    tokenCounter++;
                                    infixTokens = new List<Tokens>();
                                    int line = tokens[tokenCounter-1].Line;
                                    int n;
                                    double m;
                                    bool isFirst = true;
                                    while (line >= tokens[tokenCounter].Line)
                                    {
                                        // infixTokens is a list of tokens found in the expression,
                                        // that is to be used in converting infix to postfix

                                        // a = -2, first element is unary
                                        if (isFirst && (tokens[tokenCounter].Type == TokenType.ADD || tokens[tokenCounter].Type == TokenType.SUBT)
                                            && (tokens[tokenCounter + 1].Type == TokenType.FLOAT_LIT || tokens[tokenCounter + 1].Type == TokenType.INT_LIT))
                                        {
                                            if(tokens[tokenCounter].Type == TokenType.SUBT)
                                            {
                                                tokenCounter++; // points to the literal
                                                if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                                {
                                                    n = (int)tokens[tokenCounter].Literal;
                                                    n *= -1;
                                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, n.ToString(), n, tokens[tokenCounter].Line));
                                                }
                                                else if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                                                {
                                                    m = (double)tokens[tokenCounter].Literal;
                                                    m *= -1;
                                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, m.ToString(), m, tokens[tokenCounter].Line));
                                                }
                                            }
                                        }
                                        // b = a * -4, identified unary. most unary operators are found between operator and identifier.
                                        else if((tokens[tokenCounter].Type == TokenType.SUBT || tokens[tokenCounter].Type == TokenType.ADD)
                                            && isOperator(tokens[tokenCounter-1].Lexeme[0]) 
                                            && ((tokens[tokenCounter + 1].Type == TokenType.FLOAT_LIT || tokens[tokenCounter + 1].Type == TokenType.INT_LIT)))
                                        {
                                            if(tokens[tokenCounter].Type == TokenType.SUBT)
                                            {
                                                tokenCounter++; // points to the literal
                                                if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                                {
                                                    n = (int)tokens[tokenCounter].Literal;
                                                    n *= -1;
                                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, n.ToString(), n, tokens[tokenCounter].Line));
                                                }
                                                else if(tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                                                {
                                                    m = (double)tokens[tokenCounter].Literal;
                                                    m *= -1;
                                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, m.ToString(), m, tokens[tokenCounter].Line));
                                                }
                                            }
                                        }
                                       // the NOT token will not be added into the infixTokens list.
                                        else if (tokens[tokenCounter].Type == TokenType.NOT)
                                        {
                                            bool logic, open, close ;
                                            open = close = false;
                                            tokenCounter++;
                                            if(tokens[tokenCounter].Type == TokenType.LEFT_PAREN)
                                            {
                                                open = true;
                                                tokenCounter++;
                                            }
                                            if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                                            {// check if identifier is declared, and is of type string
                                                string temp_ident2 = tokens[tokenCounter].Lexeme;
                                                if (outputMap.ContainsKey(temp_ident2))
                                                {
                                                    if (outputMap[temp_ident2].GetType() == typeof(string))
                                                    {
                                                        logic = Convert.ToBoolean(outputMap[temp_ident2]);
                                                        logic = !logic;
                                                        // Even though this is an identifier, the TokenType should be bool_lit, 
                                                        // bcs temp_ident2's  value is not inverted, and temp_ident2 has no Tokens.Literal value
                                                        infixTokens.Add(new Tokens(TokenType.BOOL_LIT, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));
                                                        Console.WriteLine(infixTokens.Count);
                                                    }
                                                    else 
                                                        errorMessages.Add("Invalid identifier type at line " + tokens[tokenCounter].Line +1);
                                                }
                                                else
                                                    errorMessages.Add("Alien identifier at line " + tokens[tokenCounter].Line + 1);
                                                if (open)
                                                    tokenCounter++;
                                            }
                                            else if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                                            {
                                                logic = bool.Parse(tokens[tokenCounter].Lexeme);
                                                logic = !logic;
                                                Console.WriteLine("BOOLEAN FIRST!!!");
                                                infixTokens.Add(new Tokens(tokens[tokenCounter].Type, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));
                                                Console.WriteLine("OPEN " + open);
                                                if(open)
                                                    tokenCounter++;
                                            }
                                            else // NOT 5
                                                errorMessages.Add(string.Format("Invalid expression after NOT at line " + tokens[tokenCounter].Line + 1));
                                            if(tokens[tokenCounter].Type == TokenType.RIGHT_PAREN)
                                            {
                                                if (open)
                                                    close = true;
                                                else
                                                    errorMessages.Add(string.Format("Invalid usage of right parenthesis at line " + tokens[tokenCounter].Line + 1));
                                            }
                                            if(open && !close)
                                                errorMessages.Add(string.Format("Expected right parenthesis at line " + tokens[tokenCounter].Line + 1));
                                        }
                                        else if(tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                                        { // check identifier if declared
                                            string temp_ident2 = tokens[tokenCounter].Lexeme;
                                            if (outputMap.ContainsKey(temp_ident2))
                                                infixTokens.Add(tokens[tokenCounter]);
                                            else
                                                errorMessages.Add("Alien identifier at line " + tokens[tokenCounter].Line + 1);
                                        }
                                        else
                                            infixTokens.Add(tokens[tokenCounter]);

                                        if (isFirst)// this is just used for identifying unary as the first element. like A = -4 + 6
                                            isFirst = false;
                                        tokenCounter++;
                                    }
                                    Console.WriteLine("INFIX: ");
                                    for (int i = 0; i < infixTokens.Count; i++)
                                        Console.WriteLine(infixTokens[i].Type + ", " + infixTokens[i].Lexeme + ", " + infixTokens[i].Literal);
                                    Console.WriteLine("END INFIX");

                                    if (infixTokens.Count != 0)
                                    {
                                        // expects arithmetic/binary expression
                                        if (outputMap[temp_ident].GetType() == typeof(int) || outputMap[temp_ident].GetType() == typeof(double))
                                        {
                                            operation = new Operations(infixTokens, errorMessages, outputMap);
                                            postfix = operation.infixToPostFix();
                                            if (outputMap[temp_ident].GetType() == typeof(int))
                                            {
                                                int res = operation.evaluateIntegerExpression(postfix);
                                                outputMap[temp_ident] = res;
                                            }
                                            else
                                            {
                                                double d_res = operation.evaluateFloatExpression(postfix);
                                                outputMap[temp_ident] = d_res;
                                            }
                                            infixTokens.Clear();
                                        }
                                        // expects boolean expression
                                        else if (outputMap[temp_ident].GetType() == typeof(bool))
                                        {

                                            infixTokens.Clear();
                                        }
                                        // expects char expression. A = 'x'
                                        else if (outputMap[temp_ident].GetType() == typeof(char))
                                        {
                                            char t = infixTokens[0].Lexeme[0];
                                            outputMap[temp_ident] = t;
                                            infixTokens.Clear();
                                        }
                                        else
                                            errorMessages.Add(string.Format("Unidentified type at line " + tokens[tokenCounter].Line + 1));
                                    }
                                    else // infix tokens is empty
                                        errorMessages.Add(string.Format("Invalid expression at line " + tokens[tokenCounter - 1].Line + 1));
                                }
                                else
                                    errorMessages.Add(string.Format("Expected '=' after identifier at line " + tokens[tokenCounter].Line + 1));
                            }
                            else
                                errorMessages.Add(string.Format("{0} identifier not declared ", temp_ident + "at line " + tokens[tokenCounter].Line + 1));
                        }
                        else
                        // if !foundStart, or for variable declaration
                        {
                            tokenCounter++;
                            ParseIdentifier(temp_ident);
                        }
                        break;
                    case TokenType.SUBT: // for pre decrement
                        if (foundStart)
                        {
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.SUBT && tokens[tokenCounter + 1].Type == TokenType.IDENTIFIER)
                            {
                                tokenCounter++; // points to the identifier
                                string temp_iden = tokens[tokenCounter].Lexeme;
                                if (outputMap.ContainsKey(temp_iden))
                                {
                                    int value = (int)outputMap[temp_iden];
                                    outputMap[temp_iden] = value - 1;
                                }
                            }
                        }
                        else
                        {
                            errorMessages.Add(string.Format("Syntax error at line  " + tokens[tokenCounter].Line + 1));
                        }
                        break;
                    case TokenType.ADD: // for pre increment; does not work with float. 
                        if (foundStart)
                        {
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.ADD && tokens[tokenCounter + 1].Type == TokenType.IDENTIFIER)
                            {
                                tokenCounter++; // points to the identifier
                                string temp_iden = tokens[tokenCounter].Lexeme;
                                if (outputMap.ContainsKey(temp_iden))
                                {
                                    int value = (int)outputMap[temp_iden];
                                    outputMap[temp_iden] = value + 1;
                                }
                            }
                        }
                        else
                        {
                            errorMessages.Add(string.Format("Syntax error at line  " + tokens[tokenCounter].Line + 1));
                        }
                        break;
                    case TokenType.INPUT:
                        result = fsm.Input(tokens, tokenCounter);
                        if (result == 1)
                        {
                            tokenCounter++;
                            ParseInput();
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with INPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                            tokenCounter++;
                        }
                        break;
                    case TokenType.OUTPUT:
                        result = fsm.Output(tokens, tokenCounter);
                        if (result == 1)
                        {
                            tokenCounter++;
                            ParseOutput();
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                            tokenCounter++;
                        }

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

        private bool isOperator(char v)
        {
            return (v == '+' || v == '-' || v == '*' || v == '/' || v == '%');
        }

        /* 
         * ParseInput:
         * Reads and Saves the inputted values to the outputMap
         */
        private void ParseInput()
        {
            if (tokens[tokenCounter].Type == TokenType.COLON)
            {
                tokenCounter++;
                if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                {
                    ParseInputVariables();
                    if (tokens[tokenCounter].Type == TokenType.COMMA)
                    {
                        while (tokens[tokenCounter].Type == TokenType.COMMA)
                        {
                            tokenCounter++;
                            ParseInputVariables();
                        }
                    }
                    ParseInputValues();

                }

            }
        }

        /* 
         * Helper Function for ParseInput: 
         * Saving the strings in input textbox to get saved to outputMap
         */
        private void ParseInputValues()
        {
            String[] inputValues = stringInput.Split(','); //Values
            Type type;
            if (inputValues.Length == inputVariables.Count)
            {
                int x = 0;
                for (int i = 0; i < inputVariables.Count; i++)
                {
                    string v = inputVariables[i];
                    type = outputMap[v].GetType();
                    if (type == typeof(int))
                    {
                        try { outputMap[v] = int.Parse(inputValues[x++]); }
                        catch (Exception)
                        {
                            msg = "Type does not match at INPUT.";
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                    }
                    else if (type == typeof(double))
                    {
                        try { outputMap[v] = double.Parse(inputValues[x++]); }
                        catch (Exception)
                        {
                            msg = "Type does not match at INPUT.";
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                    }
                    else if (type == typeof(char))
                    {
                        try { outputMap[v] = char.Parse(inputValues[x++]); }
                        catch (Exception)
                        {
                            msg = "Type does not match at INPUT.";
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                    }
                    else if (type == typeof(string))
                    {
                        try { outputMap[v] = inputValues[x++]; }
                        catch (Exception)
                        {
                            msg = "Type does not match at INPUT.";
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                        }
                    }
                }

            }
        }

        /* 
         * Helper Function for ParseInput: 
         * Get the variables in the Compiler and save it to the input variables list for later checking
         */
        private void ParseInputVariables()
        {
            if (outputMap.ContainsKey(tokens[tokenCounter].Lexeme))
            {
                inputVariables.Add(tokens[tokenCounter].Lexeme);
                tokenCounter++;
            }
            else
            {
                msg = "Variable not initialized at line " + (tokens[tokenCounter].Line + 1);
                errorMessages.Add(msg);
                Console.WriteLine(msg);
            }
        }

        //Mostly used if identifier is declaredVariables inside the START keyword
        private void ParseIdentifier(string identifier)
        {
            int currentLine = tokens[tokenCounter].Line;
            object temp;

            if (tokens[tokenCounter].Type == TokenType.EQUALS)
            {
                tokenCounter++;
                tokenCounter2 = tokenCounter;
                List<string> expression = new List<string>();
                string a = "";

                if (outputMap.ContainsKey(identifier)) //if there is an variable inside the final outputMap 
                {
                    //while relations 

                    switch (tokens[tokenCounter].Type)
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
                        case TokenType.IDENTIFIER:
                            if (outputMap[tokens[tokenCounter].Lexeme].GetType() == outputMap[identifier].GetType())
                            {
                                temp = outputMap[tokens[tokenCounter].Lexeme];
                                outputMap[identifier] = temp; 
                            } else
                            {
                                msg = "Assigned variables are in different types at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                            }
                            break;
                        default:
                            msg = "Identifier does not exist at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            Console.WriteLine(msg);
                            break; 
                    }
                }
                else
                {
                    msg = "Syntax Error. Variable Assignation failed at line " + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    Console.WriteLine(msg);
                    error = true;
                }

            }
        }


        /*
         * ParseOutput:
         * Saves the data inside the outputMap to the outputMessages for printing
         */
        private void ParseOutput()
        {
            string temp_identOut = "";
            string output = "";
            tokenCounter2 = tokenCounter;
            if (tokens[tokenCounter2].Type == TokenType.COLON && tokens[tokenCounter2 + 1].Type != TokenType.AMPERSAND)
            {
                tokenCounter2++;
                // tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE
                // tokenCounter2 < tokens.Count - 1
                while (tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE)
                {
                    switch (tokens[tokenCounter2].Type)
                    {
                        case TokenType.IDENTIFIER:
                            temp_identOut = tokens[tokenCounter2].Lexeme;
                            Console.WriteLine(temp_identOut); 
                            if (outputMap.ContainsKey(temp_identOut)) //checks if the identifier is inside the final outputMap
                            {
                                output = outputMap[temp_identOut].ToString();
                                Console.WriteLine(output);
                                outputMessages.Add(output);  //add it to the messages needed to be outputted
                            }
                            else
                            {
                                msg = "Variable not initialized at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                                error = true;
                            }
                            if (tokens[tokenCounter2 + 1].Type == TokenType.AMPERSAND)
                                tokenCounter2 += 2;
                            else
                                tokenCounter2++;
                            break;
                        case TokenType.DOUBLE_QUOTE:
                            tokenCounter2++;
                            if (tokens[tokenCounter2].Type == TokenType.SHARP)
                            {
                                outputMessages.Add("\n");
                                tokenCounter2++;
                            }
                            else if (tokens[tokenCounter2].Type == TokenType.TILDE)
                            {
                                outputMessages.Add(" ");
                                tokenCounter2++;
                            }
                            else if (tokens[tokenCounter2].Type == TokenType.LEFT_BRACE)
                            {
                                tokenCounter2++;
                                if (tokens[tokenCounter2].Type == TokenType.RIGHT_BRACE && tokens[tokenCounter2 + 1].Type == TokenType.RIGHT_BRACE)
                                {
                                    outputMessages.Add(tokens[tokenCounter2].Lexeme);
                                    tokenCounter2++;
                                }
                                else
                                {
                                    while (tokens[tokenCounter2].Type != TokenType.RIGHT_BRACE)
                                    {
                                        outputMessages.Add(tokens[tokenCounter2].Lexeme);
                                        tokenCounter2++;
                                    }
                                }
                                tokenCounter2++;
                            }
                            else
                            {
                                outputMessages.Add(tokens[tokenCounter2].Lexeme);
                                tokenCounter2++;
                            }
                            if (tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE)
                            {
                                if (tokens[tokenCounter2 + 1].Type == TokenType.AMPERSAND)
                                    tokenCounter2 += 2;
                                else
                                    tokenCounter2++;
                            }
                            else
                            {
                                msg = "Missing double quotes at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                                error = true;
                            }
                            break;
                        default:

                            break;
                    }

                    if (error)
                    {
                        error = false;
                        break;
                    }
                }
                tokenCounter = tokenCounter2;
            }
            else
            {
                msg = "Syntax Error. Something wrong with the OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                errorMessages.Add(msg);
                Console.WriteLine(msg);
            }

        }

        //Checks the token type after the keyword AS  
        //Also saves to the outputmap 
        private void ParseAs()
        {

            switch (tokens[tokenCounter].Type)
            {
                case TokenType.INT:
                    for (int i = 0; i < varDeclareList.Count; i++) //go through the variable declaredVariables
                    {
                        string x = varDeclareList[i];
                        if (declaredVariables.ContainsKey(x)) //checks if it is being declaredVariables together with its value
                        {
                            if (declaredVariables[x].GetType() == typeof(int))
                            {
                                outputMap.Add(x, (int)declaredVariables[x]); //add it to the outputMap dictionary serves as final list for output
                            }
                            else
                            {
                                msg = "Type Error at Line: " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                            }
                        }
                        else //if not declaredVariables just store 0 temporarily
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
                        if (declaredVariables.ContainsKey(x))
                        {
                            if (declaredVariables[x].GetType() == typeof(Char))
                            {
                                outputMap.Add(x, (char)declaredVariables[x]);
                                Console.WriteLine(declaredVariables[x]);
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
                case TokenType.BOOL:
                    for (int i = 0; i < varDeclareList.Count; i++)
                    {
                        string x = varDeclareList[i];
                        if (declaredVariables.ContainsKey(x))
                        {
                            if (declaredVariables[x].GetType() == typeof(string))
                            {
                                outputMap.Add(x, (string)declaredVariables[x]);
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

                        if (declaredVariables.ContainsKey(x))
                        {
                            if (declaredVariables[x].GetType() == typeof(double))
                            {
                                outputMap.Add(x, (double)declaredVariables[x]);
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

        /* 
         * ParseDeclaration: 
         * Gets the declared variable name then saves it to the declaredVariables dictionary 
         * If it does have a declared value, it gets passed to the ParseEqual() function
         */
        private void ParseDeclaration()
        {
            if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
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

        /* 
         * Helper Function for ParseDeclaration: 
         * Used in Commas 
         */
        private void ParseCommas()
        {
            while (tokens[tokenCounter].Type == TokenType.COMMA)
            {
                tokenCounter++;
                if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                {
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
                if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                {
                    msg = "Invalid Variable declaration at line " + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    Console.WriteLine(msg);
                }
            }
        }

        /* 
         * Helper Function for ParseDeclaration: 
         * If EQUAL sign is available, save the variable together with its value
         */
        private void ParseEqual()
        {
            if (tokens[tokenCounter].Type == TokenType.EQUALS) //if the value is going to get declaredVariables as well
            {
                
                temp_ident = tokens[tokenCounter - 1].Lexeme;
                tokenCounter++;
                switch (tokens[tokenCounter].Type) //Check what type
                {
                    case TokenType.INT_LIT:
                        //save the variable together with its value 
                        declaredVariables.Add(temp_ident, (int)tokens[tokenCounter].Literal);
                        tokenCounter++;
                        break;
                    case TokenType.CHAR_LIT:
                        declaredVariables.Add(temp_ident, Convert.ToChar(tokens[tokenCounter].Literal));
                        tokenCounter++;
                        break;
                    case TokenType.BOOL_LIT: //Not yet working have to fix the declaration of TRUE and FALSE 
                        declaredVariables.Add(temp_ident, (string)(tokens[tokenCounter].Literal));
                        tokenCounter++;
                        if(tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                        {
                            tokenCounter++; 
                        } else
                        {
                            msg = "Missing double quotes at line " + ((tokens[tokenCounter].Line + 1));
                            errorMessages.Add(msg);
                        }
                        break;
                    case TokenType.FLOAT_LIT:
                        //save the variable together with its value 
                        declaredVariables.Add(temp_ident, (double)tokens[tokenCounter].Literal);
                        tokenCounter++;
                        break;
                    case TokenType.SUBT:
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            declaredVariables.Add(temp_ident, ((double)tokens[tokenCounter++].Literal) * -1);
                        else
                        {
                            if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                declaredVariables.Add(temp_ident, ((int)tokens[tokenCounter++].Literal) * -1);
                        }
                        break;
                    case TokenType.ADD:
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            declaredVariables.Add(temp_ident, ((double)tokens[tokenCounter++].Literal));
                        else
                        {
                            if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                declaredVariables.Add(temp_ident, ((int)tokens[tokenCounter++].Literal));
                        }
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
