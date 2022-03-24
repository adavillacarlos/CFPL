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
        private List<Tokens> infixTokens;
        private static int tokenCounter, tokenCounter2;
        private static bool foundStart;
        private static List<string> errorMessages;
        private static List<string> outputMessages;


        List<string> varDeclareList = new List<string>();
        private static Dictionary<string, object> outputMap;
        Dictionary<string, object> declaredVariables = new Dictionary<string, object>();
        private int startCount = 0, ifCount = 0, stopCount = 0;
        private bool foundStop;
        private bool errorFound;
        string temp_ident = "";
        string msg = "";
        bool error;

        private int startLine, stopLine, flagIf = 0;
        private int ifStart = 0, ifStop = 0;
        List<Tokens> postfix = new List<Tokens>();
        private int nested;
        int result;
        string stringInput;
        Operations operation;
        //FSM fsm;
        List<String> inputVariables = new List<string>();

        //mine
        Stack<bool> countStartStop = new Stack<bool>();

        /// <summary>
        /// Counts the number of START keyword found in control structures: if,else,while
        /// </summary>
        int startInControl = 0;
        /// <summary>
        /// Counts the number of STOP keyword found in control structures: if,else,while
        /// </summary>
        int stopInControl = 0;
        public Interpreter(List<Tokens> t, string input)
        {
            tokens = new List<Tokens>(t);
            errorMessages = new List<string>();
            outputMessages = new List<string>();
            tokenCounter = tokenCounter2 = 0;
            foundStart = foundStop = false;
            outputMap = new Dictionary<string, object>();
            error = false;
            //fsm = new FSM(tokens);
            stringInput = input;
        }
        public Interpreter(List<Tokens> t)
        {
            tokens = new List<Tokens>(t);
            errorMessages = new List<string>();
            outputMessages = new List<string>();
            tokenCounter = tokenCounter2 = 0;
            foundStart = foundStop = false;
            outputMap = new Dictionary<string, object>();
            error = false;
            //fsm = new FSM(tokens);
        }
        public List<string> ErrorMessages { get { return errorMessages; } }
        public List<string> OutputMessages { get { return outputMessages; } }
        public int Parse()
        {
            int closingStop = 0;
            object temp;
            int total_tokens = tokens.Count;
            if (tokens[total_tokens - 1].Type != TokenType.STOP)
            {
                errorMessages.Add("Missing STOP keyword");
                return 1;
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
                        { }
                        else
                        {
                            int line = tokens[tokenCounter].Line; // comment's line
                            while (line >= tokens[tokenCounter].Line) // skip all tokens with the same line as comment's 
                                tokenCounter++;
                        }
                        break;
                    case TokenType.VAR:
                        //result = fsm.Declaration(tokens, tokenCounter);
                        if (foundStart)
                        {
                            msg = "Invalid variable declaration due to START at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
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
                    case TokenType.START:
                        startCount++;
                        startLine = tokens[tokenCounter].Line;
                        if (!foundStart)
                            foundStart = true;
                        else
                        {
                            msg = "Syntax Error. Incorrect usage of START at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
                        }
                        tokenCounter++;
                        break;
                    case TokenType.STOP:
                        if (tokenCounter != tokens.Count - 1 && foundStart) // not the last token
                        {
                            if (countStartStop.Count != 0)
                                countStartStop.Pop();
                            else
                            {
                                errorMessages.Add("Unbalanced number of start and stop at line " + (tokens[tokenCounter].Line + 1));
                                return 1;
                            }
                        }
                        else // last token
                        {
                            // if balanced start-stop
                            if (foundStart && countStartStop.Count == 0)
                                foundStop = true;
                        }
                        tokenCounter++;
                        /*
                        stopCount++;
                        stopLine = tokens[tokenCounter].Line;
                        if ((startLine != stopLine && foundStart)
                            && (startInControl == stopInControl))  
                            // if startInControl != stopInControl, it means that the STOP token scanned 
                            // is still part the control structure, either if,else, or while.
                        {
                            foundStop = true;
                            return errorMessages.Count;
                        }
                        if (startInControl != stopInControl)
                        {
                            tokenCounter++;
                            stopInControl++;
                        }
                        else
                        {
                            //tokenCounter++;
                             msg = "Syntax Error. Incorrect usage of STOP at line " + (tokens[tokenCounter].Line + 1);
                             errorMessages.Add(msg);
                             return 1;
                        }
                        */
                        break;
                    case TokenType.WHILE:
                        errorFound = parseWhile();
                        if (errorFound) return 1;
                        break;
                    case TokenType.IF:
                        tokenCounter++;
                        errorFound = getInfix();
                        if (errorFound) return 1;
                        if (infixTokens.Count != 0)
                        {
                            object obj = null;
                            string output = null;
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.logicInfixToPostFix();
                            output = operation.evaluateExpression(postfix);
                            infixTokens.Clear();
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                countStartStop.Push(true);
                                // see ahead if there's a stop
                                closingStop = foundAClosingStop();
                                // no closing stop found for this control structure
                                if (closingStop == -1) return 1;

                                if (output == "True")
                                {
                                    tokenCounter++;
                                    flagIf = 1; // tells not to execute Token ELSE
                                }
                                else // condition is false, so skip all tokens inside the IF block
                                {
                                    flagIf = -1; // tells to execute Token ELSE
                                    while (tokens[tokenCounter].Type != TokenType.STOP)
                                        tokenCounter++;
                                }
                            } // no start after IF
                            else
                            {
                                errorMessages.Add(string.Format("(if)Missing Start at " + (tokens[tokenCounter].Line + 1)));
                                return 1;
                            }
                        }
                        else
                        {
                            errorMessages.Add(string.Format("Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
                            return 1;
                        }
                        break;
                    case TokenType.ELSE: // else is a must execute control structure
                        tokenCounter++;
                        // check first if theres a start and stop
                        if (tokens[tokenCounter].Type == TokenType.START)
                        {
                            countStartStop.Push(true);

                            // see ahead if there's a stop
                            closingStop = foundAClosingStop();
                            if (closingStop == -1) return 1;
                        }
                        else
                        {
                            errorMessages.Add(string.Format("Missing Start at " + (tokens[tokenCounter].Line + 1)));
                            return 1;
                        }
                        // back to Token ELSE
                        if (flagIf == -1) // meaning IF-statement was false
                            tokenCounter++; // points to token after start

                        else // flagIf = 1, skip the else part of the IF, 
                        { // because the condition was TRUE
                            while (tokens[tokenCounter].Type != TokenType.STOP)
                                tokenCounter++;
                        }
                        break;
                    case TokenType.IDENTIFIER:
                        temp_ident = tokens[tokenCounter].Lexeme;
                        tokenCounter++;
                        if (foundStart)
                        {
                            postfix = new List<Tokens>();
                            // check if variable is declared
                            if (outputMap.ContainsKey(temp_ident))
                            {
                                if (tokens[tokenCounter].Type == TokenType.EQUALS)
                                {
                                    tokenCounter++;
                                    errorFound = getInfix();
                                    if (errorFound) return 1;

                                    // Evaluate Infix Expression
                                    if (infixTokens.Count != 0)
                                    {
                                        object obj = null;
                                        string output = null;
                                        operation = new Operations(infixTokens, errorMessages, outputMap);
                                        postfix = operation.logicInfixToPostFix();


                                        output = operation.evaluateExpression(postfix);

                                        if (outputMap[temp_ident].GetType() == typeof(double))
                                            obj = double.Parse(output);
                                        else if (outputMap[temp_ident].GetType() == typeof(int))
                                            obj = int.Parse(output);
                                        else if (outputMap[temp_ident].GetType() == typeof(string))
                                            obj = output;
                                        else if (outputMap[temp_ident].GetType() == typeof(char))
                                            obj = char.Parse(output);
                                        else
                                        {
                                            errorMessages.Add(string.Format("Unidentified type at line " + (tokens[tokenCounter].Line + 1)));
                                            return 1;
                                        }
                                        outputMap[temp_ident] = obj;
                                        if (operation.multipleIden.Count != 0)
                                        {
                                            // example multiple declaration: a = b = c = d = 2
                                            // b, c, and d are stored in identifier
                                            foreach (string identifier in operation.multipleIden)
                                                outputMap[identifier] = obj;
                                        }
                                        infixTokens.Clear();
                                    }
                                    else // infix tokens is empty
                                    {
                                        errorMessages.Add(string.Format("Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
                                        return 1;
                                    }
                                }
                                else
                                {
                                    errorMessages.Add(string.Format("Expected '=' after identifier at line " + (tokens[tokenCounter].Line + 1)));
                                    return 1;
                                }
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Identifier " + temp_ident + " is not declared at line " + (tokens[tokenCounter].Line + 1)));
                                return 1;
                            }
                        }
                        else
                        { // if !foundStart, or for variable declaration
                            if (outputMap.ContainsKey(temp_ident))
                            {
                                errorMessages.Add("Syntax error at line " + (tokens[tokenCounter].Line));
                                return errorMessages.Count;
                            }
                            tokenCounter++;
                            ParseIdentifier(temp_ident);
                        }
                        break;
                    case TokenType.INPUT:
                        if (result == 1)
                        {
                            tokenCounter++;
                            ParseInput();
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with INPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
                        }
                        break;
                    case TokenType.OUTPUT:
                        //result = fsm.Output(tokens, tokenCounter);
                        if (foundStart)
                        {
                            tokenCounter++;
                            ParseOutput();
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
                        }
                        break;
                    default:
                        errorMessages.Add("Syntax error at line " + (tokens[tokenCounter].Line));
                        return 1;
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
            Console.WriteLine("\nMAIN PARSE COMPLETE\n");
            return errorMessages.Count;
        }
        private bool parseWhile()
        {
            tokenCounter++; // points to parenthesis after while
            // where while is found;
            int n, i;
            n = tokenCounter;
            int stopWhile = 0;
            int check = 0;
            int controlsCounter = 0;

            Console.WriteLine("\nVALUE OF TOKEN COUNTER " + n);
            errorFound = getInfix(); // token counter points to start
            if (errorFound) return true;
            Console.WriteLine("VALUE OF TOKEN COUNTER AFTER INFIX IN WHILE " + tokenCounter);
            i = tokenCounter;

            do
            {
                if (tokens[i].Type == TokenType.START)
                    controlsCounter++;
                if (tokens[i].Type == TokenType.STOP)
                    controlsCounter--;
                i++;
            } while (controlsCounter != 0 && i != tokens.Count - 1);
            if (i == tokens.Count)
            {
                errorMessages.Add("Unbalanced number of start-stop. (W)");
                return true;
            }

            if (infixTokens.Count != 0)
            {
                operation = new Operations(infixTokens, errorMessages, outputMap);
                postfix = operation.logicInfixToPostFix();
                string output = operation.evaluateExpression(postfix);
                Console.WriteLine("TOKEN COUNTER NOW POINTS AT " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme);
                while (output == "True")
                {
                    if (tokens[tokenCounter].Type == TokenType.START)
                    {
                        countStartStop.Push(true);
                        tokenCounter++;
                    }
                    else
                    {
                        errorMessages.Add(string.Format("Missing Start at " + (tokens[tokenCounter].Line + 1)));
                        return true;
                    }
                    infixTokens.Clear();
                    check = Parse();
                    if (check != 0) return true;
                    stopWhile = tokenCounter;

                    // back to while condition
                    tokenCounter = n;
                    Console.WriteLine("\nVALUE OF TOKEN COUNTER" + tokenCounter);
                    errorFound = getInfix();
                    operation = new Operations(infixTokens, errorMessages, outputMap);

                    if (errorFound) return true;
                    postfix = operation.logicInfixToPostFix();

                    output = operation.evaluateExpression(postfix);
                    Console.WriteLine("CURRENT VALUE OF OUTPUT " + output);
                }
            }
            // skip tokens, i is where the while's STOP
            while (tokenCounter != i)
                tokenCounter++;
            return false;
        }
        /// <summary>
        /// ParseInput:
        /// Reads and Saves the inputted values to the outputMap
        /// </summary>
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
        /// <summary>
        /// Helper Function for ParseInput: 
        /// Saving the strings in input textbox to get saved to outputMap
        /// </summary>
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
        /// <summary>
        /// Helper Function for ParseInput: 
        /// Get the variables in the Compiler and save it to the input variables list for later checking
        /// </summary>
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
                    switch (tokens[tokenCounter].Type)
                    {
                        case TokenType.INT_LIT when outputMap[identifier].GetType() == typeof(Int32):
                            //temp = (int)tokens[tokenCounter].Literal;
                            temp = int.Parse(tokens[tokenCounter].Lexeme);
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.CHAR_LIT when outputMap[identifier].GetType() == typeof(char):
                            temp = Convert.ToChar(tokens[tokenCounter].Literal);
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.BOOL_LIT when outputMap[identifier].GetType() == typeof(string):
                            Console.WriteLine("Inside Boolean");  //add value to the variable; BOOLEAN NOT WORKING YET HUHU
                            temp = Convert.ToString(tokens[tokenCounter].Lexeme);
                            //tokens[tokenCounter].Literal = temp;
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.FLOAT_LIT when outputMap[identifier].GetType() == typeof(double):
                            //temp = (double)(tokens[tokenCounter].Literal);
                            temp = double.Parse(tokens[tokenCounter].Lexeme);
                            outputMap[identifier] = temp;
                            break;
                        case TokenType.IDENTIFIER:
                            if (outputMap[tokens[tokenCounter].Lexeme].GetType() == outputMap[identifier].GetType())
                            {
                                temp = outputMap[tokens[tokenCounter].Lexeme];
                                outputMap[identifier] = temp;
                            }
                            else
                            {
                                msg = "Assigned variables are in different types at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                                Console.WriteLine(msg);
                            }
                            break;
                        case TokenType.SUBT:
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            {
                                //temp = (double)tokens[tokenCounter].Literal * -1;
                                temp = double.Parse(tokens[tokenCounter].Lexeme) * -1;
                                //temp *= -1;
                                outputMap[identifier] = temp;
                            }
                            else
                            {
                                if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                {
                                    //temp = (int)(tokens[tokenCounter].Literal) * -1;
                                    temp = int.Parse(tokens[tokenCounter].Lexeme) * -1;
                                    outputMap[identifier] = temp;
                                }
                            }
                            break;
                        case TokenType.ADD:
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            {
                                //temp = (double)tokens[tokenCounter].Literal;
                                temp = double.Parse(tokens[tokenCounter].Lexeme);
                                outputMap[identifier] = temp;
                            }
                            else
                            {
                                if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                {
                                    //temp = (int)(tokens[tokenCounter].Literal);
                                    temp = int.Parse(tokens[tokenCounter].Lexeme);
                                    outputMap[identifier] = temp;
                                }
                            }
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
            int currentLine = tokens[tokenCounter].Line;
            if (tokens[tokenCounter2].Type == TokenType.COLON && tokens[tokenCounter2 + 1].Type != TokenType.AMPERSAND)
            {
                tokenCounter2++;
                // tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE
                //tokenCounter2 < tokens.Count - 1
                while (currentLine == tokens[tokenCounter2].Line
                    && (tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE))
                {
                    switch (tokens[tokenCounter2].Type)
                    {
                        case TokenType.IDENTIFIER:
                            temp_identOut = tokens[tokenCounter2].Lexeme;
                            if (outputMap.ContainsKey(temp_identOut)) //checks if the identifier is inside the final outputMap
                            {
                                output = outputMap[temp_identOut].ToString();
                                outputMessages.Add(output);  //add it to the messages needed to be outputted
                            }
                            else
                            {
                                msg = "Variable not initialized at line " + (tokens[tokenCounter2].Line + 1);
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
                                msg = "Missing double quotes at line " + (tokens[tokenCounter2].Line + 1);
                                errorMessages.Add(msg);
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
                Console.WriteLine("AFTER OUTPUT IS " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme);
            }
            else
            {
                msg = "Syntax Error. Something wrong with the OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                errorMessages.Add(msg);
                Console.WriteLine(msg);
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
                temp_ident = tokens[tokenCounter].Lexeme;
                if (varDeclareList.Contains(temp_ident))
                {
                    msg = "Identifier already taken at line" + (tokens[tokenCounter].Line + 1);
                    errorMessages.Add(msg);
                    return;
                }
                else
                    varDeclareList.Add(temp_ident); //Add the variable to the variable List
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
                    //varDeclareList.Add(tokens[tokenCounter].Lexeme); //Add the variable to the variable List
                    //temp_ident= tokens[tokenCounter].Lexeme; //get the variable name 

                    temp_ident = tokens[tokenCounter].Lexeme;
                    if (varDeclareList.Contains(temp_ident))
                    {
                        msg = "Identifier already taken at line" + (tokens[tokenCounter].Line + 1);
                        errorMessages.Add(msg);
                        return;
                    }
                    else
                        varDeclareList.Add(temp_ident);

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
                        //declaredVariables.Add(temp_ident, (int)tokens[tokenCounter].Literal);
                        declaredVariables.Add(temp_ident, int.Parse(tokens[tokenCounter].Lexeme));
                        tokenCounter++;
                        break;
                    case TokenType.CHAR_LIT:
                        //declaredVariables.Add(temp_ident, Convert.ToChar(tokens[tokenCounter].Literal));
                        declaredVariables.Add(temp_ident, Convert.ToChar(tokens[tokenCounter].Lexeme));
                        tokenCounter++;
                        break;
                    case TokenType.DOUBLE_QUOTE:
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                        {
                            string temp2 = tokens[tokenCounter].Lexeme;
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                            {
                                declaredVariables.Add(temp_ident, temp2);
                                //tokens[tokenCounter - 1].Literal = temp2;
                                tokenCounter++;
                            }
                            else
                            {
                                msg = "Missing double quotes at line " + ((tokens[tokenCounter].Line + 1));
                                errorMessages.Add(msg);
                            }
                        }
                        else
                            errorMessages.Add(string.Format("Invalid variable declaration at line " + tokens[tokenCounter].Line + 1)); ;
                        break;
                    case TokenType.FLOAT_LIT:
                        //save the variable together with its value 
                        //declaredVariables.Add(temp_ident, (double)tokens[tokenCounter].Literal);
                        declaredVariables.Add(temp_ident, double.Parse(tokens[tokenCounter].Lexeme));
                        tokenCounter++;
                        break;
                    case TokenType.SUBT:
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            declaredVariables.Add(temp_ident, (double.Parse(tokens[tokenCounter++].Lexeme) * -1));
                        //declaredVariables.Add(temp_ident, ((double)tokens[tokenCounter++].Literal) * -1);
                        else
                        {
                            if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                declaredVariables.Add(temp_ident, (int.Parse(tokens[tokenCounter++].Lexeme)) * -1);
                            //declaredVariables.Add(temp_ident, ((int)tokens[tokenCounter++].Literal) * -1);
                        }
                        break;
                    case TokenType.ADD:
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.FLOAT_LIT)
                            declaredVariables.Add(temp_ident, ((double)tokens[tokenCounter++].Literal));
                        //declaredVariables.Add(temp_ident, ((double)tokens[tokenCounter++].Literal));
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
                                if (outputMap.ContainsKey(x))
                                    errorMessages.Add(string.Format("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (int)declaredVariables[x]); //add it to the outputMap dictionary serves as final list for output
                            else
                            {
                                msg = "Identifier name already taken at line " + (tokens[tokenCounter].Line + 1);
                                errorMessages.Add(msg);
                            }
                        }
                        else //if not declaredVariables just store 0 temporarily
                        {
                            declaredVariables.Add(x, 0);
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
                                if (outputMap.ContainsKey(x))
                                    errorMessages.Add(string.Format("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (char)declaredVariables[x]);
                            }
                            else
                            {
                                msg = "Identifier name already taken at line " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                            }
                        }
                        else
                        {
                            declaredVariables.Add(x, ' ');
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
                        Console.WriteLine("BOOLVAR: " + x);
                        if (declaredVariables.ContainsKey(x))
                        {
                            if (declaredVariables[x].GetType() == typeof(string))
                            {
                                if (outputMap.ContainsKey(x))
                                    errorMessages.Add(string.Format("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (string)declaredVariables[x]);
                            }
                            else
                            {
                                msg = "Identifier name already taken at line " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                            }
                        }
                        else
                        {
                            declaredVariables.Add(x, "False");
                            outputMap.Add(x, "False");
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
                                if (outputMap.ContainsKey(x))
                                    errorMessages.Add(string.Format("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (double)declaredVariables[x]);
                            }
                            else
                            {
                                msg = "Identifier name already taken at line " + tokens[tokenCounter].Line;
                                errorMessages.Add(msg);
                            }
                        }
                        else
                        {
                            declaredVariables.Add(x, 0.0);
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
        public bool isOperator(char x)
        {
            return (x == '+' || x == '-' || x == '*' || x == '/' || x == '%');
        }
        public bool getInfix()
        {
            infixTokens = new List<Tokens>();
            int line = tokens[tokenCounter - 1].Line;
            int n;
            double m;
            bool isFirst = true;
            bool error = false;
            while (line >= tokens[tokenCounter].Line)
            {
                // infixTokens is a list of tokens found in the expression,
                // that is to be used in converting infix to postfix

                // unary, a = -2, a = a + -2
                if ((tokens[tokenCounter].Type == TokenType.ADD || tokens[tokenCounter].Type == TokenType.SUBT)
                    && (isFirst || isOperator(tokens[tokenCounter - 1].Lexeme[0]))
                    && (tokens[tokenCounter + 1].Type == TokenType.IDENTIFIER || tokens[tokenCounter + 1].Type != TokenType.BOOL_LIT
                    || tokens[tokenCounter + 1].Type == TokenType.INT_LIT || tokens[tokenCounter + 1].Type != TokenType.FLOAT_LIT))
                {
                    if (tokens[tokenCounter].Type == TokenType.SUBT)
                    {
                        tokenCounter++; // points to literal

                        m = Convert.ToDouble(tokens[tokenCounter].Lexeme);
                        m *= -1;
                        if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                        {
                            n = (int)m;
                            infixTokens.Add(new Tokens(tokens[tokenCounter].Type, n.ToString(), n, tokens[tokenCounter].Line));
                        }
                        else
                        {
                            infixTokens.Add(new Tokens(tokens[tokenCounter].Type, m.ToString(), m, tokens[tokenCounter].Line));
                            return true;
                        }

                    }

                }
                // the NOT token will not be added into the infixTokens list.
                else if (tokens[tokenCounter].Type == TokenType.NOT)
                {
                    bool logic, open, close;
                    open = close = false;
                    tokenCounter++;
                    if (tokens[tokenCounter].Type == TokenType.LEFT_PAREN)
                    {
                        open = true;
                        tokenCounter++;
                    }
                    // NOT A
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
                                //Console.WriteLine(infixTokens.Count);
                            }
                            else
                            {
                                errorMessages.Add("Invalid identifier type at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }

                        }
                        else
                        {
                            errorMessages.Add("Alien identifier at line " + (tokens[tokenCounter].Line + 1));
                            return true;
                        }
                        if (open)
                            tokenCounter++;
                    } // NOT "TRUE"
                    else if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                    {
                        tokenCounter++;

                        if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                        {
                            logic = bool.Parse(tokens[tokenCounter].Lexeme);
                            logic = !logic;
                            infixTokens.Add(new Tokens(tokens[tokenCounter].Type, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));

                            tokenCounter++;
                        }
                        else // NOT 5, INT_LIT
                        {
                            errorMessages.Add(string.Format("Invalid expression after NOT at line " + (tokens[tokenCounter].Line + 1)));
                            return true;
                        }
                        if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                        {
                            if (open)
                                tokenCounter++;
                        }
                        else
                        {
                            errorMessages.Add(string.Format("Missing quote at line " + (tokens[tokenCounter].Line + 1)));
                            return true;
                        }
                        if (tokens[tokenCounter].Type == TokenType.RIGHT_PAREN)
                        {
                            if (open)
                                close = true;
                            else
                            {
                                errorMessages.Add(string.Format("Invalid usage of right parenthesis at line " + (tokens[tokenCounter].Line + 1)));
                                return true;
                            }

                        }
                        if (open && !close)
                            errorMessages.Add(string.Format("Expected right parenthesis at line " + (tokens[tokenCounter].Line + 1)));
                    }
                    else
                    {
                        errorMessages.Add(string.Format("Incorrect usage of bool literal at line " + (tokens[tokenCounter].Line + 1)));
                        return true;
                    }

                }
                else if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                {
                    tokenCounter++;
                    bool logic;
                    if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                    {
                        logic = bool.Parse(tokens[tokenCounter].Lexeme);
                        infixTokens.Add(new Tokens(tokens[tokenCounter].Type, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));
                        tokenCounter++;
                    }
                    else // NOT 5
                    {
                        errorMessages.Add(string.Format("Invalid expression after NOT at line " + (tokens[tokenCounter].Line + 1)));
                        return true;
                    }
                    if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                        Console.WriteLine("CLOSING QUOTE");
                    else
                    {
                        errorMessages.Add(string.Format("Missing quote at line " + (tokens[tokenCounter].Line + 1)));
                        return true;
                    }
                }
                // A = TRUE
                else if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                {
                    errorMessages.Add(string.Format("Invalid usage of bool literal at line " + (tokens[tokenCounter].Line + 1)));
                    return true;
                }
                else if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                { // check identifier if declared
                    string temp_ident2 = tokens[tokenCounter].Lexeme;
                    if (outputMap.ContainsKey(temp_ident2))
                        infixTokens.Add(tokens[tokenCounter]);
                    else
                    {
                        errorMessages.Add("Alien identifier at line " + tokens[tokenCounter].Line + 1);
                        return true;
                    }

                }
                else
                    infixTokens.Add(tokens[tokenCounter]);

                if (isFirst)// this is just used for identifying unary as the first element. like A = -4 + 6
                    isFirst = false;

                tokenCounter++;
            }//end while
            Console.WriteLine("\nINFIX EXPRESSION: ");
            foreach (Tokens x in infixTokens)
                Console.WriteLine(x.Type + ", " + x.Lexeme);
            Console.WriteLine("END OF INFIX");
            Console.WriteLine("TOKEN COUNTER NOW POINTS TO " + tokens[tokenCounter].Type);
            return error;
        }
        /// <summary>
        /// Skips all tokens until it sees a STOP.
        /// This is used only in control structures.
        /// Returns -1 if it sees the last STOP of the program, 
        /// else, it returns counter where the stop was found.
        /// </summary>
        /// <returns></returns>
        int foundAClosingStop()
        {
            int flag = -1;
            int i = tokenCounter;
            while (i != tokens.Count - 1)
            {
                if (tokens[i].Type == TokenType.STOP)
                {
                    flag = i;
                    break;
                }
                i++;
            }
            if (flag == -1)
            {
                errorMessages.Add("Unbalanced number of start-stop");
                return flag;
            }
            return i;
        }
        /// <summary>
        /// Returns true if start or stop's position is valid.
        /// Accepts which string to evaluate. (start|stop)
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>

        /*public bool isPositionValid(string str)
        {
            if(str.ToLower() == "start")
                return tokens[tokenCounter - 1]
            if(tokens[tokenCounter - 1].Line != tokens[tokenCounter].Line)
        }
        */
    }
}