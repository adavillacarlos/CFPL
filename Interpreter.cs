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
        private static int tokenCounter;
        private static bool foundStart;
        private static List<string> errorMessages;
        private static List<string> outputMessages;


        List<string> varDeclareList = new List<string>();
        private static Dictionary<string, object> outputMap;
        Dictionary<string, object> declaredVariables = new Dictionary<string, object>();
        private bool foundStop;
        private bool errorFound;
        /// <summary>
        /// The receiving identifer in the executable block. A = 10 + 2. A is the temp_ident
        /// </summary>
        string temp_ident = "";

        bool error;

        private int startLine, stopLine, flagIf = 0;
        List<Tokens> postfix = new List<Tokens>();
        int result;
        string stringInput;
        Operations operation;


        //FSM fsm;
        List<String> inputVariables = new List<string>();

        //mine
        Stack<bool> countStartStop = new Stack<bool>();
        bool foundWhileStart = false;
        int stopWhile = -1;

        List<int> stopInLoops = new List<int>();
        int loopCounter = -1;

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
            tokenCounter = tokenCounter = 0;
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
            tokenCounter = tokenCounter = 0;
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
            double temp_double = 0.0;
            int temp_int = 0;
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
                            errorMessages.Add("Invalid variable declaration due to START at line " + (tokens[tokenCounter].Line + 1));
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
                        //startLine = tokens[tokenCounter].Line;
                        if (!isValidPosition())
                        {
                            errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                            return 1;
                        }
                        if (!foundStart)
                            foundStart = true;
                        else
                        {
                            errorMessages.Add("Syntax Error. Incorrect usage of START at line " + (tokens[tokenCounter].Line + 1));
                            return 1;
                        }
                        tokenCounter++;
                        break;
                    case TokenType.STOP:
                        //Console.WriteLine("TOKENCOUNTER NOW AT " + tokenCounter);
                        if (!isValidPosition())
                        {
                            errorMessages.Add("Invalid syntax in STOP at line " + (tokens[tokenCounter].Line + 1));
                            return 1;
                        }
                        /*
                        if (foundWhileStart)
                        {
                            foundWhileStart = false;
                            return errorMessages.Count;
                        }
                        */
                        if (tokenCounter != tokens.Count - 1 && foundStart) // not the last token
                        {
                            if (countStartStop.Count != 0)
                                countStartStop.Pop();

                            /*
                            else
                            {
                                errorMessages.Add("Unbalanced number of start and stop at line " + (tokens[tokenCounter].Line +1));
                                return 1;
                            }*/
                        }
                        else // last token
                        {
                            // if balanced start-stop
                            if (foundStart && countStartStop.Count == 0)
                            {
                                //stopLine = tokens[tokenCounter].Line;
                                foundStop = true;
                            }
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
                        stopInLoops.Clear();
                        loopCounter = -1;
                        errorFound = parseWhile();
                        if (errorFound) return 1;
                        break;
                    case TokenType.IF:
                        tokenCounter++;
                        errorFound = getInfix();
                        if (errorFound) return 1;
                        if (infixTokens.Count != 0)
                        {
                            string output = null;
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.convertInfixToPostfix();
                            Console.WriteLine("(after infix) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + tokenCounter);
                            output = operation.evaluateExpression(postfix);
                            if (output == "error") return 1;
                            if (output == "True" || output == "False") { }
                            else
                            {
                                errorMessages.Add("Invalid expression inside if statement at line " + (tokens[tokenCounter].Line));
                                return 1;
                            }
                            infixTokens.Clear();
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                if (!isValidPosition())
                                {
                                    errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                                    return 1;
                                }

                                countStartStop.Push(true);
                                // see ahead if there's a stop
                                closingStop = foundAClosingStop();
                                // no closing stop found for this control structure
                                if (closingStop == -1) return 1;

                                if (output == "True")
                                {
                                    flagIf = 1; // tells not to execute Token ELSE
                                    tokenCounter++;
                                }
                                else // condition is false, so skip all tokens inside the IF block
                                {
                                    flagIf = -1; // tells to execute Token ELSE
                                    while (tokenCounter < closingStop)
                                        tokenCounter++;
                                }
                            } // no start after IF
                            else
                            {
                                errorMessages.Add(string.Format("(IF) Missing Start at " + (tokens[tokenCounter].Line + 1)));
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
                            if (!isValidPosition())
                            {
                                errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                                return 1;
                            }
                            countStartStop.Push(true);

                            // see ahead if there's a stop
                            closingStop = foundAClosingStop();
                            // -1 means there's an unbalanced number of start-stop
                            if (closingStop == -1) return 1;
                        }
                        else
                        {
                            errorMessages.Add(string.Format("(ELSE) Missing Start at line " + (tokens[tokenCounter].Line + 1)));
                            return 1;
                        }
                        // back to Token ELSE
                        if (flagIf == -1) // meaning IF-statement was false
                            tokenCounter++; // points to token after start

                        else // flagIf = 1, skip the else part of the IF, because the condition was TRUE
                        { // 
                            while (tokenCounter < closingStop)
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
                                        postfix = operation.convertInfixToPostfix();
                                        output = operation.evaluateExpression(postfix);
                                        if (output == "error") return 1;
                                        if (outputMap[temp_ident].GetType() == typeof(double) || outputMap[temp_ident].GetType() == typeof(int))
                                        {
                                            if (operation.isDigit(output))
                                            {
                                                temp_double = double.Parse(output);
                                                if (outputMap[temp_ident].GetType() == typeof(int))
                                                    obj = (int)temp_double;
                                                else
                                                    obj = temp_double;
                                            }
                                            else
                                            {
                                                errorMessages.Add("(I)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
                                        //boolean
                                        else if (outputMap[temp_ident].GetType() == typeof(string))
                                        {
                                            if (output == "True" || output == "False")
                                                obj = output;
                                            else
                                            {
                                                errorMessages.Add("(B)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
                                        else if (outputMap[temp_ident].GetType() == typeof(char))
                                        {
                                            if (output.Length == 1)
                                                obj = char.Parse(output);
                                            else
                                            {
                                                errorMessages.Add("(C)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
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
                                        errorMessages.Add(string.Format("(EMPTY)Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
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
                                errorMessages.Add(string.Format("(MAIN PARSE) Identifier " + temp_ident + " is not declared at line " + (tokens[tokenCounter].Line + 1)));
                                return 1;
                            }
                        }
                        else
                        { // if !foundStart, or for variable declaration
                            if (outputMap.ContainsKey(temp_ident))
                            {
                                errorMessages.Add("(identifier)Syntax error at line " + (tokens[tokenCounter].Line));
                                return errorMessages.Count;
                            }
                            tokenCounter++;
                            ParseIdentifier(temp_ident);
                        }
                        break;
                    case TokenType.INPUT:
                         tokenCounter++;
                         ParseInput();
                        break;
                    case TokenType.OUTPUT:
                        if (foundStart)
                        {
                            tokenCounter++;
                            ParseOutput();
                        }
                        else
                        {
                            errorMessages.Add("Syntax Error. There is something wrong with OUTPUT at line " + (tokens[tokenCounter].Line + 1));
                            return 1;
                        }
                        break;
                    default:
                        break; 

                }
                temp_ident = "";
                temp = null;
            }
            //|| startLine == stopLine
            if (!foundStop)
                errorMessages.Add("Program execution failed.");

            Console.WriteLine("\nMAIN PARSE COMPLETE\n");
            return errorMessages.Count;
        }
        public int ParseNestedWhile()
        {
            int closingStop = 0;
            int total_tokens = tokens.Count;
            double temp_double = 0.0;
            // para nested while, without if
            // while (tokens[tokenCounter].Type != TokenType.STOP) 
            // para if inside while
            //  while (tokenCounter != stopWhile)
            //while (tokenCounter != stopWhile)
            // while(countStartStop.Count != 0)
            while (tokenCounter < stopInLoops[loopCounter])
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
                    case TokenType.STOP:
                        Console.WriteLine("(NESTED)STACK POPPED AT " + tokenCounter);
                        countStartStop.Pop();
                        tokenCounter++;
                        break;
                    case TokenType.WHILE:
                        errorFound = parseWhile();
                        if (errorFound) return 1;
                        loopCounter--;
                        break;
                    case TokenType.IF:
                        tokenCounter++;
                        errorFound = getInfix();
                        if (errorFound) return 1;
                        if (infixTokens.Count != 0)
                        {
                            string output = null;
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.convertInfixToPostfix();
                            // Console.WriteLine("(after postfix) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + tokenCounter);
                            output = operation.evaluateExpression(postfix);
                            if (output == "error") return 1;
                            if (output == "True" || output == "False") { }
                            else
                            {
                                errorMessages.Add("Invalid expression inside if statement at line " + (tokens[tokenCounter].Line));
                                return 1;
                            }
                            infixTokens.Clear();
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                if (!isValidPosition())
                                {
                                    errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                                    return 1;
                                }
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
                                    while (tokenCounter < closingStop)
                                        tokenCounter++;
                                }
                            } // no start after IF
                            else
                            {
                                errorMessages.Add(string.Format("(IF) Missing Start at " + (tokens[tokenCounter].Line + 1)));
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
                            if (!isValidPosition())
                            {
                                errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                                return 1;
                            }
                            countStartStop.Push(true);

                            // see ahead if there's a stop
                            closingStop = foundAClosingStop();
                            // -1 means there's an unbalanced number of start-stop
                            if (closingStop == -1) return 1;
                        }
                        else
                        {
                            errorMessages.Add(string.Format("(ELSE)Missing Start at line " + (tokens[tokenCounter].Line + 1)));
                            return 1;
                        }
                        // back to Token ELSE
                        if (flagIf == -1) // meaning IF-statement was false
                            tokenCounter++; // points to token after start

                        else // flagIf = 1, skip the else part of the IF, because the condition was TRUE
                        { // 
                            while (tokenCounter < closingStop)
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
                                        postfix = operation.convertInfixToPostfix();
                                        //Console.WriteLine("(after postfix) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + tokenCounter);
                                        output = operation.evaluateExpression(postfix);
                                        if (output == "error") return 1;
                                        //Console.WriteLine("OUTPUT FROM EVALUATE EXPRFESSION IS " + output);
                                        if (outputMap[temp_ident].GetType() == typeof(double) || outputMap[temp_ident].GetType() == typeof(int))
                                        {
                                            if (operation.isDigit(output))
                                            {
                                                temp_double = double.Parse(output);
                                                if (outputMap[temp_ident].GetType() == typeof(int))
                                                    obj = (int)temp_double;
                                                else
                                                    obj = temp_double;
                                            }
                                            else
                                            {
                                                errorMessages.Add("(I)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
                                        //boolean
                                        else if (outputMap[temp_ident].GetType() == typeof(string))
                                        {
                                            if (output == "True" || output == "False")
                                                obj = output;
                                            else
                                            {
                                                errorMessages.Add("(B)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
                                        else if (outputMap[temp_ident].GetType() == typeof(char))
                                        {
                                            if (output.Length == 1)
                                                obj = char.Parse(output);
                                            else
                                            {
                                                errorMessages.Add("(C)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                                return 1;
                                            }
                                        }
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
                                        errorMessages.Add(string.Format("(EMPTY)Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
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
                                errorMessages.Add(string.Format("(NESTED WHILE)Identifier " + temp_ident + " is not declared at line " + (tokens[tokenCounter].Line + 1)));
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
                            errorMessages.Add("Syntax Error. There is something wrong with INPUT at line " + (tokens[tokenCounter].Line + 1));
                            return 1;
                        }
                        break;
                    case TokenType.OUTPUT:
                        tokenCounter++;
                        ParseOutput();
                        break;
                    default:
                        errorMessages.Add("Syntax error at line " + (tokens[tokenCounter].Line));
                        return 1;
                }
                temp_ident = "";
            }

            Console.WriteLine("\nNESTED WHILE PARSE COMPLETE");
            return errorMessages.Count;
        }
        private bool parseWhile()
        {
            stopInLoops.Add(0);
            loopCounter++;

            //countStartStop.Clear();
            tokenCounter++; // points to parenthesis after while
            // where while is found;
            int condition, i;

            condition = tokenCounter;
            //int stopWhile = 0;
            int check = 0;

            // counts the number of start-stop inside while
            int controlsCounter = 0;

            Console.WriteLine("\nTOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + condition);
            errorFound = getInfix(); // token counter points to start
            if (errorFound) return true;
            //Console.WriteLine("VALUE OF TOKEN COUNTER AFTER INFIX IN WHILE " + tokenCounter);
            //tokenCounter now points to start
            i = tokenCounter;
            bool valid;
            do
            {
                valid = false;
                if (tokens[i].Type == TokenType.STOP || tokens[i].Type == TokenType.START)
                {
                    valid = (tokens[i].Line != tokens[i - 1].Line) && (tokens[i].Line != tokens[i + 1].Line);
                    if (valid)
                    {
                        if (tokens[i].Type == TokenType.START)
                            controlsCounter++;
                        if (tokens[i].Type == TokenType.STOP)
                            controlsCounter--;
                    }
                }
                i++;
            } while (controlsCounter != 0 && i != tokens.Count - 1);
            if (i == tokens.Count)
            {
                errorMessages.Add("Unbalanced number of start-stop. (ParseWhile)");
                return true;
            }
            stopWhile = i;
            stopInLoops[loopCounter] = i;

            if (infixTokens.Count != 0)
            {
                operation = new Operations(infixTokens, errorMessages, outputMap);
                postfix = operation.convertInfixToPostfix();
                //Console.WriteLine("(after postfix) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + tokenCounter);
                string output = operation.evaluateExpression(postfix);
                if (output == "error") return true;
                if (output == "True" || output == "False")
                { }
                else
                {
                    errorMessages.Add("Invalid boolean expression inside while at " + (tokens[tokenCounter].Line));
                    return true;
                }
                while (output == "True")
                {
                    if (tokens[tokenCounter].Type == TokenType.START)
                    {
                        if (!isValidPosition())
                        {
                            errorMessages.Add("Invalid syntax in START at line " + (tokens[tokenCounter].Line + 1));
                            return true;
                        }
                        Console.WriteLine("(PARSE WHILE) PUSHED START AT " + tokenCounter);
                        //foundWhileStart = true;
                        countStartStop.Push(true);
                        tokenCounter++;
                    }
                    else
                    {
                        errorMessages.Add(string.Format("Missing Start at line" + (tokens[tokenCounter].Line + 1)));
                        return true;
                    }
                    infixTokens.Clear();
                    check = ParseNestedWhile();
                    if (check != 0) return true;
                    //stopWhile = tokenCounter;
                    // check = ParseNestedWhile();
                    //if (check != 0) return true;
                    // Console.WriteLine("STOP WHILE " + stopWhile);

                    // back to while condition
                    tokenCounter = condition;
                    Console.WriteLine("(PARSE WHILE) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + condition);
                    errorFound = getInfix();
                    operation = new Operations(infixTokens, errorMessages, outputMap);

                    if (errorFound) return true;
                    postfix = operation.convertInfixToPostfix();
                    //Console.WriteLine("(after postfix) TOKEN COUNTER POINTS " + tokens[tokenCounter].Type + ", at " + tokenCounter);
                    output = operation.evaluateExpression(postfix);
                    if (output == "error") return true;
                    if (output == "True" || output == "False")
                    { }
                    else
                    {
                        errorMessages.Add("Invalid boolean expression inside while at " + (tokens[tokenCounter].Line));
                        return true;
                    }
                    // Console.WriteLine("CURRENT VALUE OF OUTPUT " + output);
                }
            }
            // skip tokens, i is where the while's STOP
            while (tokenCounter != stopWhile)
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
                            errorMessages.Add("Type does not match at INPUT.");
                        }
                    }
                    else if (type == typeof(double))
                    {
                        try { outputMap[v] = double.Parse(inputValues[x++]); }
                        catch (Exception)
                        {
                            errorMessages.Add("Type does not match at INPUT.");
                        }
                    }
                    else if (type == typeof(char))
                    {
                        try { outputMap[v] = char.Parse(inputValues[x++]); }
                        catch (Exception)
                        {
                            errorMessages.Add("Type does not match at INPUT.");
                        }
                    }
                    else if (type == typeof(string))
                    {
                        try { outputMap[v] = inputValues[x++]; }
                        catch (Exception)
                        {
                            errorMessages.Add("Type does not match at INPUT.");
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
                errorMessages.Add("Variable not initialized at line " + (tokens[tokenCounter].Line + 1));
        }
        private void ParseIdentifier(string identifier)
        {
            int currentLine = tokens[tokenCounter].Line;
            object temp;
            if (tokens[tokenCounter].Type == TokenType.EQUALS)
            {
                tokenCounter++;
                tokenCounter = tokenCounter;
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
                                errorMessages.Add("Assigned variables are in different types at line " + (tokens[tokenCounter].Line + 1));
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
                    errorMessages.Add("Syntax Error. Variable Assignation failed at line " + (tokens[tokenCounter].Line + 1));
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
            Console.WriteLine("HERE AT PARSE OUTPUT");
            string temp_identOut = "";
            string output = "";
            int outputLine = tokens[tokenCounter].Line;
            //int currentLine = tokens[tokenCounter].Line;
            if (tokens[tokenCounter].Type == TokenType.COLON && tokens[tokenCounter + 1].Type != TokenType.AMPERSAND)
            {
                tokenCounter++;
                // tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE
                //tokenCounter2 < tokens.Count - 1
                // && (tokens[tokenCounter2].Type == TokenType.IDENTIFIER || tokens[tokenCounter2].Type == TokenType.DOUBLE_QUOTE)
                while (outputLine == tokens[tokenCounter].Line &&
                    (tokens[tokenCounter].Type == TokenType.IDENTIFIER || tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE
                    || tokens[tokenCounter].Type == TokenType.LEFT_PAREN || tokens[tokenCounter].Type == TokenType.MULT))
                {
                    switch (tokens[tokenCounter].Type)
                    {
                        case TokenType.MULT:
                            int thisline = tokens[tokenCounter].Line;
                            while (thisline == tokens[tokenCounter].Line)
                                tokenCounter++;
                            return;
                        //break;
                        case TokenType.IDENTIFIER:
                        case TokenType.LEFT_PAREN:
                            //case TokenType.IDENTIFIER:
                            errorFound = getInfix();
                            if (errorFound)
                                return;
                            if (infixTokens.Count != 0)
                            {
                                operation = new Operations(infixTokens, errorMessages, outputMap);
                                postfix = operation.convertInfixToPostfix();
                                output = operation.evaluateExpression(postfix);
                                if (output == "error")
                                {
                                    infixTokens.Clear();
                                    return;
                                }
                                outputMessages.Add(output);
                                Console.WriteLine("OUT: " + output);
                            }
                            infixTokens.Clear();
                            break;

                        // case TokenType.IDENTIFIER:
                        /*
                        errorFound = getInfix();
                        if (errorFound)
                            return;
                        if (infixTokens.Count != 0)
                        {
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.convertInfixToPostfix();
                            output = operation.evaluateExpression(postfix);
                            outputMessages.Add(output);
                            Console.WriteLine("OUT: " + output);
                        }
                        infixTokens.Clear();

                        temp_identOut = tokens[tokenCounter].Lexeme;
                        if (outputMap.ContainsKey(temp_identOut)) //checks if the identifier is inside the final outputMap
                        {
                            output = outputMap[temp_identOut].ToString();
                            Console.WriteLine("OUT: " + output);
                            outputMessages.Add(output);  //add it to the messages needed to be outputted
                            //Console.WriteLine("HERE AT LEFT PAREN");
                            errorFound = getInfix();
                            if (errorFound)
                            {
                                errorMessages.Add("error found");
                                break;
                            }
                            if (infixTokens.Count != 0)
                            {
                                operation = new Operations(infixTokens, errorMessages, outputMap);
                                postfix = operation.convertInfixToPostfix();
                                output = operation.evaluateExpression(postfix);
                                outputMessages.Add(output);
                                Console.WriteLine("OUT: " + output);
                            }
                            infixTokens.Clear();
                        }

                        else
                        {
                            errorMessages.Add("Variable not initialized at line " + (tokens[tokenCounter].Line + 1));
                            error = true;
                        }

                        if (tokens[tokenCounter].Type == TokenType.AMPERSAND)
                            tokenCounter++;
                        else if (tokens[tokenCounter].Type == TokenType.MULT)
                        {
                            break;
                        }
                        else
                        {
                            // if there's still tokens in the output, then invalid syntax
                            // OUTPUT: "HAHAHAH" "Z"
                            if (outputLine == tokens[tokenCounter].Line)
                            {
                                errorMessages.Add("Invalid expression in output at line " + (outputLine + 1));
                                return;
                            }
                            else
                                tokenCounter++;

                        }

                        break;
                        */
                        case TokenType.DOUBLE_QUOTE:
                            tokenCounter++;
                            bool open = true;
                            while (open)
                            {
                                if (tokens[tokenCounter].Type == TokenType.SHARP)
                                {
                                    outputMessages.Add("\n");
                                    Console.WriteLine("NEW LINE");
                                    tokenCounter++;
                                }
                                else if (tokens[tokenCounter].Type == TokenType.TILDE)
                                {
                                    outputMessages.Add(" ");
                                    Console.WriteLine(" ");
                                    tokenCounter++;
                                }
                                else if (tokens[tokenCounter].Type == TokenType.LEFT_BRACE)
                                { // only one character is stored inside the brackets
                                    if (!isEscapeCharacter(tokens[tokenCounter + 1].Type.ToString()))
                                        errorMessages.Add("Invalid token inside brackets at line " + (tokens[tokenCounter].Line + 1));

                                    // for escape char
                                    if (tokens[tokenCounter + 2].Type == TokenType.RIGHT_BRACE)
                                    {
                                        tokenCounter++;
                                        outputMessages.Add(tokens[tokenCounter].Lexeme);
                                        Console.WriteLine("OUT: " + tokens[tokenCounter].Lexeme);
                                        tokenCounter += 2;
                                    }
                                    else
                                    { // left brace is outputted
                                        outputMessages.Add(tokens[tokenCounter].Lexeme);
                                        Console.WriteLine("OUT: " + tokens[tokenCounter].Lexeme);
                                        tokenCounter++;
                                    }
                                }
                                else
                                {
                                    outputMessages.Add(tokens[tokenCounter].Lexeme);
                                    Console.WriteLine("OUT: " + tokens[tokenCounter].Lexeme);
                                    tokenCounter++;
                                }
                                if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                                {
                                    open = false;
                                    if (tokens[tokenCounter + 1].Type == TokenType.AMPERSAND)
                                        tokenCounter += 2;
                                    else if (tokens[tokenCounter + 1].Type == TokenType.MULT)
                                    {
                                        tokenCounter++;
                                    }
                                    else
                                    {
                                        // if there's still tokens in the output, then syntax is invalid
                                        // OUTPUT: "HAHAHAH" "Z"
                                        if (outputLine == tokens[tokenCounter + 1].Line)
                                        {
                                            errorMessages.Add("Invalid expression in output at line " + (outputLine + 1));
                                            return;
                                        }
                                        else
                                            tokenCounter++;
                                    }

                                }
                                // check if mo lapas na siyas output line
                                if (open && outputLine != tokens[tokenCounter].Line)
                                {
                                    errorMessages.Add("Missing double quotes at line " + (tokens[tokenCounter].Line));
                                    return;
                                }
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
                tokenCounter = tokenCounter;
                Console.WriteLine("AFTER OUTPUT IS " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme);
            }
            else
                errorMessages.Add("Syntax Error. Something wrong with the OUTPUT at line " + (tokens[tokenCounter].Line + 1));
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
                    errorMessages.Add("Identifier already taken at line" + (tokens[tokenCounter].Line + 1));
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
                errorMessages.Add("Invalid variable declaration. After VAR is not an identifier at line " + (tokens[tokenCounter].Line + 1));
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
                        errorMessages.Add("Identifier already taken at line" + (tokens[tokenCounter].Line + 1));
                        return;
                    }
                    else
                        varDeclareList.Add(temp_ident);

                    tokenCounter++;
                    ParseEqual();
                }
                else
                    errorMessages.Add("Syntax Error. There is an excess comma at line " + (tokens[tokenCounter].Line + 1));
                if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                    errorMessages.Add("Invalid Variable declaration at line " + (tokens[tokenCounter].Line + 1));
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
                                errorMessages.Add("Missing double quotes at line " + ((tokens[tokenCounter].Line + 1)));
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
                        errorMessages.Add("Syntax Error at line " + ((tokens[tokenCounter].Line + 1)));
                        tokenCounter++;
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
                                errorMessages.Add("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1));
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
                            if (declaredVariables[x].GetType() == typeof(char))
                            {
                                if (outputMap.ContainsKey(x))
                                    errorMessages.Add(string.Format("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (char)declaredVariables[x]);
                            }
                            else
                                errorMessages.Add("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1));
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
                                errorMessages.Add("Identifier name already taken at line " + (tokens[tokenCounter].Line + 1));
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
                                    errorMessages.Add(string.Format("(D)Identifier name already taken at line " + (tokens[tokenCounter].Line + 1)));
                                else
                                    outputMap.Add(x, (double)declaredVariables[x]);
                            }
                            else
                                errorMessages.Add("Must have a decimal part. Error at line " + (tokens[tokenCounter].Line + 1));
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
                    errorMessages.Add("Syntax Error at line " + (tokens[tokenCounter].Line + 1));
                    break;
            }
        }

        public bool getInfix()
        {
            Operations o = new Operations();
            infixTokens = new List<Tokens>();
            int line = tokens[tokenCounter - 1].Line;
            int temp_value;
            int countParen = 0; // apply pda to check balanced number if parenthesis
            double iden_value;
            //bool isFirst = true;
            bool error = false;
            while (line >= tokens[tokenCounter].Line && tokens[tokenCounter].Type != TokenType.AMPERSAND
                       && (tokens[tokenCounter].Type != TokenType.DOUBLE_QUOTE || tokens[tokenCounter + 1].Type == TokenType.BOOL_LIT))
            // "TRUE             "HAHA
            // FALSE AND TRUE    FALSE OR FALSE
            {
                // infixTokens is just a list of tokens found in the expression,
                // that is to be used in converting infix to postfix
                if (tokens[tokenCounter].Type == TokenType.MULT)
                {
                    if (tokens[tokenCounter - 1].Type == TokenType.RIGHT_PAREN || tokens[tokenCounter - 1].Type == TokenType.RIGHT_BRACE ||
                           tokens[tokenCounter - 1].Type == TokenType.FLOAT_LIT || tokens[tokenCounter - 1].Type == TokenType.INT_LIT ||
                           tokens[tokenCounter - 1].Type == TokenType.IDENTIFIER)
                    { }
                    else
                    {
                        int commentline = tokens[tokenCounter].Line; // comment's line
                        while (commentline >= tokens[tokenCounter].Line) // skip all tokens with the same line as comment's 
                            tokenCounter++;
                        return false;
                    }
                }

                // encountering unary for arithmetic expression, a = -b, a = -10, a = c + -b
                if ((tokens[tokenCounter].Type == TokenType.ADD || tokens[tokenCounter].Type == TokenType.SUBT)
                    && (tokens[tokenCounter - 1].Type == TokenType.EQUALS || o.isArithmeticOperator(tokens[tokenCounter - 1].Lexeme) || o.isRelationalOperator(tokens[tokenCounter - 1].Lexeme))
                    && (tokens[tokenCounter + 1].Type == TokenType.IDENTIFIER || tokens[tokenCounter + 1].Type != TokenType.BOOL_LIT
                    || tokens[tokenCounter + 1].Type == TokenType.INT_LIT || tokens[tokenCounter + 1].Type == TokenType.FLOAT_LIT))
                {
                    if (tokens[tokenCounter].Type == TokenType.SUBT)
                    {
                        tokenCounter++; // points to literal

                        if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                        {
                            string iden = tokens[tokenCounter].Lexeme;
                            if (outputMap.ContainsKey(iden))
                            {
                                if (outputMap[iden].GetType() == typeof(int) || outputMap[iden].GetType() == typeof(double))
                                {
                                    iden_value = Convert.ToDouble(outputMap[iden]);
                                    iden_value *= -1;
                                    if (outputMap[iden].GetType() == typeof(int))
                                    {
                                        temp_value = (int)iden_value;
                                        infixTokens.Add(new Tokens(TokenType.INT_LIT, temp_value.ToString(), temp_value, tokens[tokenCounter].Line));
                                    }
                                    else
                                        infixTokens.Add(new Tokens(TokenType.FLOAT_LIT, iden_value.ToString(), iden_value, tokens[tokenCounter].Line));

                                    //Console.WriteLine("Pushed " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme + " to infix tokens");
                                }
                                else // other data types, string or char
                                {
                                    errorMessages.Add("(UNARY)Invalid expression at line " + tokens[tokenCounter].Line);
                                    return true;
                                }
                            }
                            else
                            {
                                errorMessages.Add("Alien identifier found at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }
                        }
                        else // int_lit, float_lit
                        {
                            if (tokens[tokenCounter].Literal.GetType() == typeof(int) || tokens[tokenCounter].Literal.GetType() == typeof(double))
                            {
                                iden_value = Convert.ToDouble(tokens[tokenCounter].Lexeme);
                                iden_value *= -1;
                                if (tokens[tokenCounter].Type == TokenType.INT_LIT)
                                {
                                    temp_value = (int)iden_value;
                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, temp_value.ToString(), temp_value, tokens[tokenCounter].Line));
                                }
                                else
                                    infixTokens.Add(new Tokens(tokens[tokenCounter].Type, iden_value.ToString(), iden_value, tokens[tokenCounter].Line));

                                //Console.WriteLine("Pushed " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme + " to infix tokens");
                            }
                            else
                            {
                                errorMessages.Add("Invalid arithmetic expression at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }

                        }
                    }
                }
                // encountering unary for boolean expression
                else if (tokens[tokenCounter].Type == TokenType.NOT)
                {
                    bool logic, open, close;
                    open = close = false;
                    tokenCounter++;
                    if (tokens[tokenCounter].Type == TokenType.LEFT_PAREN)
                    {
                        countParen++;
                        open = true;
                        tokenCounter++;
                    }
                    // NOT <IDENTIFER>, NOT A
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
                                //Console.WriteLine("Pushed " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme + " to infix tokens");
                            }
                            else
                            {
                                errorMessages.Add("Invalid boolean expression type at line " + (tokens[tokenCounter].Line + 1));
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
                    } // NOT "<BOOL_LIT>", NOT "TRUE"
                    else if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                    {
                        tokenCounter++;
                        if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                        {
                            logic = bool.Parse(tokens[tokenCounter].Lexeme);
                            logic = !logic;
                            //Console.WriteLine("Pushed " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme + " to infix tokens");
                            infixTokens.Add(new Tokens(tokens[tokenCounter].Type, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));

                            tokenCounter++;
                        }
                        else // NOT 5, INT_LIT
                        {
                            errorMessages.Add(string.Format("Invalid boolean expression  at line " + (tokens[tokenCounter].Line + 1)));
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
                            countParen--;
                            if (open)
                                close = true;
                            else
                            {
                                errorMessages.Add(string.Format("Invalid usage of right parenthesis at line " + (tokens[tokenCounter].Line + 1)));
                                return true;
                            }

                        }
                        if (open && !close)
                        {
                            errorMessages.Add(string.Format("Expected right parenthesis at line " + (tokens[tokenCounter].Line + 1)));
                            return true;
                        }
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
                        // Console.WriteLine("Pushed " + tokens[tokenCounter].Type + ", " + tokens[tokenCounter].Lexeme + " to infix tokens");
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
                /*else if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                { // check identifier if declared
                    
                    Console.WriteLine("TEMP_IDENT = " + temp_ident + " VALUE: " + outputMap[temp_ident] + "TYPE: " + outputMap[temp_ident].GetType());
                    Console.WriteLine("TEMP_IDENT2 = " + temp_ident2+ " VALUE: " + outputMap[temp_ident2] + " TYPE:" + outputMap[temp_ident2].GetType());

                    if (outputMap.ContainsKey(temp_ident2))
                    {
                        // int(A) = B
                        if (outputMap[temp_ident].GetType() == typeof(int) || outputMap[temp_ident].GetType() == typeof(double))
                        {
                            if(outputMap[temp_ident2].GetType() == typeof(int) || outputMap[temp_ident2].GetType() == typeof(double))
                                infixTokens.Add(tokens[tokenCounter]);
                            else
                            {
                                errorMessages.Add("(I)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }
                        }
                        else if (outputMap[temp_ident].GetType() == typeof(char))
                        {
                            if (outputMap[temp_ident2].GetType() == typeof(char))
                                infixTokens.Add(tokens[tokenCounter]);
                            else
                            {
                                errorMessages.Add("(C)Invalid expression at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }
                        }
                        else // for boolean
                        {
                            
                        }
                            infixTokens.Add(tokens[tokenCounter]);
                    }
                    else
                    {
                        errorMessages.Add("Alien identifier at line " + tokens[tokenCounter].Line + 1);
                        return true;
                    }
                        
                }*/
                else
                {
                    if (tokens[tokenCounter].Type == TokenType.LEFT_PAREN)
                        countParen++;
                    if (tokens[tokenCounter].Type == TokenType.RIGHT_PAREN)
                        countParen--;
                    // code below will just check if the use of operators is correct.
                    // left operand and right operand must be of the same type.
                    // except when using arithmetic operation on int and float.
                    // didnt check boolean because it has plenty of valid combinations
                    string x = tokens[tokenCounter].Lexeme;

                    //Console.WriteLine("VALUE OF X IS " + x.ToString());
                    if (o.isOperator(x))
                    {

                        string leftOp, rightOp;
                        bool leftIsInt, rightIsInt;
                        leftIsInt = rightIsInt = true;

                        // get data type of left operand
                        if (tokens[tokenCounter - 1].Type == TokenType.IDENTIFIER)
                        {
                            string identifier = tokens[tokenCounter - 1].Lexeme;
                            // check first if identifier is already declared
                            if (outputMap.ContainsKey(identifier))
                            {
                                if (outputMap[identifier].GetType() == typeof(int) || outputMap[identifier].GetType() == typeof(double))
                                {
                                    leftOp = "digit";
                                    leftIsInt = outputMap[identifier].GetType() == typeof(int);
                                }
                                else if (outputMap[identifier].GetType() == typeof(string))
                                    leftOp = "bool";
                                else if (outputMap[identifier].GetType() == typeof(char))
                                    leftOp = "char";
                                else
                                {
                                    errorMessages.Add("Invalid data type at line " + tokens[tokenCounter].Line + 1);
                                    return true;
                                }
                            }
                            else
                            {
                                errorMessages.Add("Alien identifier at line " + tokens[tokenCounter].Line + 1);
                                return true;
                            }
                        }// check if left operand is a literal
                        else if (tokens[tokenCounter - 1].Type == TokenType.BOOL_LIT || tokens[tokenCounter - 1].Type == TokenType.CHAR_LIT
                             || tokens[tokenCounter - 1].Type == TokenType.INT_LIT || tokens[tokenCounter - 1].Type == TokenType.FLOAT_LIT)
                        {
                            if (tokens[tokenCounter - 1].Type == TokenType.INT_LIT || tokens[tokenCounter - 1].Type == TokenType.FLOAT_LIT)
                            {
                                leftOp = "digit";
                                leftIsInt = tokens[tokenCounter - 1].Type == TokenType.INT_LIT;
                            }
                            else if (tokens[tokenCounter - 1].Type == TokenType.BOOL_LIT)
                                leftOp = "bool";
                            else
                                leftOp = "char";
                        }
                        else if (tokens[tokenCounter - 1].Type == TokenType.RIGHT_PAREN)
                            leftOp = "right_paren";
                        else if (tokens[tokenCounter - 1].Type == TokenType.DOUBLE_QUOTE)
                            leftOp = "quotes";
                        else
                        {
                            errorMessages.Add("Invalid usage of operator at line " + (tokens[tokenCounter].Line + 1));
                            return true;
                        }

                        // get data type of right operand
                        if (tokens[tokenCounter + 1].Type == TokenType.IDENTIFIER)
                        {
                            string identifier = tokens[tokenCounter + 1].Lexeme;
                            // check first if identifier is already declared
                            if (outputMap.ContainsKey(identifier))
                            {
                                if (outputMap[identifier].GetType() == typeof(int) || outputMap[identifier].GetType() == typeof(double))
                                {
                                    rightOp = "digit";
                                    rightIsInt = outputMap[identifier].GetType() == typeof(int);
                                }
                                else if (outputMap[identifier].GetType() == typeof(string))
                                    rightOp = "bool";
                                else if (outputMap[identifier].GetType() == typeof(char))
                                    rightOp = "char";
                                else
                                {
                                    errorMessages.Add("Invalid data type at line " + (tokens[tokenCounter].Line + 1));
                                    return true;
                                }
                            }
                            else
                            {
                                errorMessages.Add("Alien identifier at line " + (tokens[tokenCounter].Line + 1));
                                return true;
                            }
                        }// check if right operand is a literal
                        else if (tokens[tokenCounter + 1].Type == TokenType.BOOL_LIT || tokens[tokenCounter + 1].Type == TokenType.CHAR_LIT
                             || tokens[tokenCounter + 1].Type == TokenType.INT_LIT || tokens[tokenCounter + 1].Type == TokenType.FLOAT_LIT)
                        {
                            if (tokens[tokenCounter + 1].Type == TokenType.INT_LIT || tokens[tokenCounter + 1].Type == TokenType.FLOAT_LIT)
                            {
                                rightOp = "digit";
                                rightIsInt = tokens[tokenCounter + 1].Type == TokenType.INT_LIT;
                            }
                            else if (tokens[tokenCounter + 1].Type == TokenType.BOOL_LIT)
                                rightOp = "bool";
                            else
                                rightOp = "char";
                        }
                        else if (tokens[tokenCounter + 1].Type == TokenType.LEFT_PAREN)
                            rightOp = "left_paren";
                        else if (tokens[tokenCounter + 1].Type == TokenType.DOUBLE_QUOTE)
                            rightOp = "quotes";
                        else if (tokens[tokenCounter + 1].Type == TokenType.SUBT || tokens[tokenCounter + 1].Type == TokenType.ADD || tokens[tokenCounter + 1].Type == TokenType.NOT)
                            rightOp = "unary";
                        else
                        {
                            errorMessages.Add("Invalid usage of operator at line " + (tokens[tokenCounter].Line + 1));
                            return true;
                        }
                        // evaluate
                        if (o.isLogicalOperator(x))
                        { }
                        else
                        {
                            if (leftOp != rightOp)
                            {
                                if (leftOp == "right_paren" || rightOp == "left_paren" || rightOp == "unary")
                                { } // just do nothing
                                else if (leftOp == "quotes" || rightOp == "quotes")
                                { }
                                else
                                {
                                    errorMessages.Add("Can't do operations between two different data types. Found error at line " + (tokens[tokenCounter].Line + 1));
                                    return true;
                                }
                            }
                            else
                            {
                                if (o.isArithmeticOperator(x))
                                {
                                    // will just check leftOp since it has the same value as rightOp
                                    if (leftOp != "digit")
                                    {// either boolean or char, which is not applicable for arithmetic operations
                                        errorMessages.Add("Invalid usage of arithmetic operator at line " + (tokens[tokenCounter].Line + 1));
                                        return true;
                                    }
                                }
                                else if (x == "==" || x == "<>")
                                {
                                    if (leftOp == "digit")
                                    {
                                        // the concept is that, left and right operand should be of the same value,
                                        // so that equals and not equals is applicable.
                                        // the left side means both of them are int, while the right side means both of them are float
                                        if ((leftIsInt && rightIsInt) || (!leftIsInt && !rightIsInt))
                                        {// just do nothing
                                        }
                                        else
                                        {
                                            errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                            return true;
                                        }
                                    }
                                }
                                else if (o.isRelationalOperator(x))
                                {
                                    if (leftOp == "digit")
                                    {
                                        // the concept is that, left and right operand should be of the same value,
                                        // so that equals and not equals is applicable.
                                        // the left side means both of them are int, while the right side means both of them are float
                                        if ((leftIsInt && rightIsInt) || (!leftIsInt && !rightIsInt))
                                        {// just do nothing
                                        }
                                        else
                                        {
                                            errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                            return true;
                                        }
                                    }
                                    else
                                    { // either boolean or char, which is not applicable for relational operations
                                        errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                        return true;
                                    }
                                }
                            }
                        }
                        /*
                        if (leftOp != rightOp)
                        {
                            errorMessages.Add("Can't do operations between two different data types. Found at line " + (tokens[tokenCounter].Line + 1));
                            return true;
                        }
                        else
                        {
                            if (o.isArithmeticOperator(x))
                            {
                                // will just check leftOp since it has the same value as rightOp
                                if (leftOp != "digit")
                                {// either boolean or char, which is not applicable for arithmetic operations
                                    errorMessages.Add("Invalid usage of arithmetic operator at line " + (tokens[tokenCounter].Line + 1));
                                    return true;
                                }
                            }
                            else if (x == "==" || x == "<>")
                            {
                                if (leftOp == "digit")
                                {
                                    // the concept is that, left and right operand should be of the same value,
                                    // so that equals and not equals is applicable.
                                    // the left side means both of them are int, while the right side means both of them are float
                                    if ((leftIsInt && rightIsInt) || (!leftIsInt && !rightIsInt))
                                    {// just do nothing
                                    }
                                    else
                                    {
                                        errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                        return true;
                                    }
                                }
                            }
                            else if (o.isRelationalOperator(x))
                            {
                                if (leftOp == "digit")
                                {
                                    // the concept is that, left and right operand should be of the same value,
                                    // so that equals and not equals is applicable.
                                    // the left side means both of them are int, while the right side means both of them are float
                                    if ((leftIsInt && rightIsInt) || (!leftIsInt && !rightIsInt))
                                    {// just do nothing
                                    }
                                    else
                                    {
                                        errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                        return true;
                                    }
                                }
                                else
                                { // either boolean or char, which is not applicable for relational operations
                                    errorMessages.Add("Invalid usage of relational operator at line " + (tokens[tokenCounter].Line + 1));
                                    return true;
                                }
                            }
                            else if (o.isLogicalOperator(x))
                            {
                                if (leftOp != "bool")
                                {
                                    errorMessages.Add("Invalid usage of logical operator at line " + (tokens[tokenCounter].Line + 1));
                                    return true;
                                }
                            }
                        }
                        */
                    }
                    infixTokens.Add(tokens[tokenCounter]);
                }
                //if (isFirst)// this is just used for identifying unary as the first element. like A = -4 + 6
                //   isFirst = false;

                tokenCounter++;
            }//end while
            if (countParen != 0)
            {
                errorMessages.Add("Unbalanced number of parenthesis in expression at line " + (tokens[tokenCounter].Line));
                return true;
            }
            if (tokens[tokenCounter].Type == TokenType.AMPERSAND)
                tokenCounter++;
            else if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
            {
                errorMessages.Add("(INFIX) Invalid expression at line " + (line + 1));
                return true;
            }
            Console.WriteLine("\nINFIX EXPRESSION: ");
            foreach (Tokens x in infixTokens)
                Console.WriteLine(x.Type + ", " + x.Lexeme);
            Console.WriteLine("END OF INFIX");
            // Console.WriteLine("(INFIX) TOKEN COUNTER NOW POINTS TO " + tokens[tokenCounter].Type);


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
            bool valid = false;
            while (i != tokens.Count - 1)
            {
                if (tokens[i].Type == TokenType.STOP)
                {
                    if (i == 0)
                        valid = (tokens[i].Line != tokens[i + 1].Line);
                    else if (i == tokens.Count - 1)
                        valid = (tokens[i].Line != tokens[i - 1].Line);
                    else
                        valid = (tokens[i].Line != tokens[i - 1].Line) && (tokens[i].Line != tokens[i + 1].Line);

                    if (valid)
                    {
                        flag = i;
                        break;
                    }
                }
                i++;
            }
            if (flag == -1)
            {
                errorMessages.Add("(FoundClosingStop) Unbalanced number of start-stop at token " + i);
                return flag;
            }
            return i;
        }
        public bool isEscapeCharacter(string token)
        {
            token = token.ToLower();
            bool flag = false;
            if (token == "ampersand")
                flag = true;
            else if (token == "sharp")
                flag = true;
            else if (token == "double_quote")
                flag = true;
            else if (token == "mult")
                flag = true;
            else if (token == "left_brace")
                flag = true;
            else if (token == "right_brace")
                flag = true;

            return flag;
        }
        /// <summary>
        /// Check if START/STOP keyword is in valid position
        /// </summary>
        /// <returns></returns>
        public bool isValidPosition()
        {
            if (tokenCounter == 0)
                return (tokens[tokenCounter].Line != tokens[tokenCounter + 1].Line);
            else if (tokenCounter == tokens.Count - 1)
                return (tokens[tokenCounter].Line != tokens[tokenCounter - 1].Line);
            else
                return (tokens[tokenCounter].Line != tokens[tokenCounter - 1].Line) && (tokens[tokenCounter].Line != tokens[tokenCounter + 1].Line);
        }

    }
}