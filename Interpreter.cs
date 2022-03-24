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
        private int startCount = 0, ifCount = 0;
        private int stopCount = 0;
        private bool foundStop;
        string temp_ident = "";
        string msg = "";
        bool error;
        int result;
        string stringInput;
        string output;
        object obj;
        Operations operation;
        //FSM fsm;
        List<String> inputVariables = new List<string>();
        private int startLine, stopLine, flagIf = 0;
        private int ifStart = 0, ifStop = 0;
        private bool errorFound;
        List<Tokens> postfix = new List<Tokens>();
        private int nested;
        private int whileCounter;
        private bool whileStart;

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
                        startLine = (tokens[tokenCounter].Line + 1);

                        if (!foundStart)
                            foundStart = true;
                        else
                        {
                            msg = "Syntax Error. Incorrect usage of START at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                        }
                        tokenCounter++;
                        break;
                    case TokenType.STOP:
                        stopCount++;
                        stopLine = (tokens[tokenCounter].Line + 1);
                        if ((startLine != stopLine) && foundStart) //Bug on IF() Start only 
                        {
                            foundStop = true;

                        }
                        else
                        {
                            msg = "Syntax Error. Incorrect usage of STOP at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
                        }
                        tokenCounter++;
                        break;
                    case TokenType.IDENTIFIER:
                        temp_ident = tokens[tokenCounter].Lexeme;
                        tokenCounter++;
                        if (foundStart)
                        {
                            if (outputMap.ContainsKey(temp_ident))
                            {
                                if (tokens[tokenCounter].Type == TokenType.EQUALS)
                                {
                                    tokenCounter++;
                                    errorFound = getInfix();
                                    if (errorFound) return 1;
                                    Console.WriteLine("\nINFIX EXPRESSION: ");
                                    foreach (Tokens x in infixTokens)
                                        Console.WriteLine(x.Type + ", " + x.Lexeme);
                                    Console.WriteLine("END OF INFIX");
                                    // Evaluate Infix Expression
                                    if (infixTokens.Count != 0)
                                    {
                                        object obj = null;
                                        string output = null;
                                        operation = new Operations(infixTokens, errorMessages, outputMap);
                                        postfix = operation.logicInfixToPostFix();
                                        output = operation.evaluateExpression(postfix);

                                        if (outputMap[temp_ident].GetType() == typeof(double))
                                            try
                                            {
                                                obj = double.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line " + (tokens[tokenCounter].Line + 1)));
                                                return 1;
                                            }

                                        else if (outputMap[temp_ident].GetType() == typeof(int))

                                            try
                                            {
                                                obj = int.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line  " + (tokens[tokenCounter].Line + 1)));
                                                return 1;
                                            }
                                        else if (outputMap[temp_ident].GetType() == typeof(string))

                                            try
                                            {
                                                obj = bool.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line " + (tokens[tokenCounter].Line + 1)));
                                                return 1;
                                            }
                                        else if (outputMap[temp_ident].GetType() == typeof(char))


                                            try
                                            {
                                                obj = char.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line  " + (tokens[tokenCounter].Line + 1)));
                                                return 1;
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
                            tokenCounter++;
                            ParseIdentifier(temp_ident);
                        }
                        break;
                    case TokenType.OUTPUT:
                        if (foundStart)
                        {
                            tokenCounter++;
                            ParseOutput();
                            Console.WriteLine("Inside Parse Output: ");
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            return 1;
                        }

                        break;
                    case TokenType.INPUT:
                        tokenCounter++;
                        ParseInput();
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
                                startCount++;
                                ifStart++;
                                tokenCounter++;
                                int i = tokenCounter;

                                if (output == "True")
                                {

                                    flagIf = 1;
                                    //To do if output is true 
                                    Console.WriteLine(tokens[tokenCounter].Lexeme);
                                    //Checking if STOP is there
                                    while (i != tokens.Count)
                                    {
                                        if (tokens[i].Type == TokenType.STOP)
                                        {
                                            ifStop++;
                                            stopCount++;
                                            break;
                                        }
                                        i++;
                                    }

                                }
                                else
                                {
                                    flagIf = -1;
                                    //TO DO IF OUTPUT IS FALSE; skip the tokens until the next stop
                                    while (tokenCounter != tokens.Count)
                                    {
                                        if (tokens[tokenCounter].Type == TokenType.STOP)
                                        {
                                            ifStop++;
                                            stopCount++;
                                            break;
                                        }
                                        tokenCounter++;
                                    }
                                }
                                Console.WriteLine("Start Count: " + ifCount + " Stop Count: " + ifStop);

                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at " + (tokens[tokenCounter - 1].Line + 1)));
                                return 1;
                            }

                        }
                        else
                        {
                            errorMessages.Add(string.Format("Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
                            return 1;
                        }
                        break;
                    case TokenType.ELSE:
                        if (flagIf == -1)
                        {
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                startCount++;
                                tokenCounter++;
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at Line: " + (tokens[tokenCounter - 1].Line + 1)));
                                return 1;
                            }
                        }
                        break;
                    case TokenType.WHILE:
                        tokenCounter++;
                        tokenCounter2 = tokenCounter;//GET THE STARTING FOR THE CONDITION
                        Console.WriteLine("Hello While: " + tokens[tokenCounter2].Lexeme);
                        errorFound = getInfix();
                        if (errorFound) return 1;
                        if (infixTokens.Count != 0)
                        {
                            obj = null;
                            output = null;
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.logicInfixToPostFix();
                            output = operation.evaluateExpression(postfix);
                            infixTokens.Clear();
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                whileStart = true;
                                tokenCounter++;

                                ParseWhile(tokenCounter2, tokenCounter);
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at Line: " + (tokens[tokenCounter - 1].Line + 1)));
                                return 1;
                            }
                        }
                        if (error)
                            break;
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

        private void ParseWhile(int conditionToken, int afterStartToken)
        {
            //tokencounter2 holds the left param
            int startCounter = tokenCounter; //TOKEN AFTER THE START
            int origCounter = afterStartToken;  //holds the original counter
            Console.WriteLine("Start Counter: " + startCounter + "Token Counter: " + tokenCounter + "Statement Counter: " + tokenCounter2);

            //tokenCounter 2 should not be changed since it is for the condition
            while (output == "True")
            {


                //TOKENCOUNTER AFTER THE START 
                Console.WriteLine("Lexeme: " + tokens[tokenCounter].Lexeme);
                ParseInterpreterWhile(); //Like the Parse
                origCounter = tokenCounter;

                tokenCounter = conditionToken; //to check the condition
                Console.WriteLine("Left Param: " + tokens[tokenCounter - 1].Lexeme); //It should be the leexeme: 

                errorFound = getInfix();
                if (errorFound) error = true;
                //Get the new condition once again. 
                operation = new Operations(infixTokens, errorMessages, outputMap);
                postfix = operation.logicInfixToPostFix();
                output = operation.evaluateExpression(postfix);

                infixTokens.Clear();



                tokenCounter = afterStartToken; //Should be on the start
                Console.WriteLine("Original Counter after: " + tokens[tokenCounter].Lexeme);

            }
            //Take the tokenCounter back to the stop
            while (tokens[tokenCounter].Type != TokenType.STOP)
                tokenCounter++;
        }

        private void ParseInterpreterWhile()
        {
            object temp;
            while (tokens[tokenCounter].Type != TokenType.STOP)
            {
                Console.WriteLine("Inside Parse Interpreter While: " + tokens[tokenCounter].Lexeme);
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
                    case TokenType.IDENTIFIER:
                        temp_ident = tokens[tokenCounter].Lexeme;
                        tokenCounter++;
                        if (whileStart)
                        {
                            if (outputMap.ContainsKey(temp_ident))
                            {
                                if (tokens[tokenCounter].Type == TokenType.EQUALS)
                                {
                                    tokenCounter++;
                                    errorFound = getInfix();
                                    if (errorFound) error = true;
                                    Console.WriteLine("\nINFIX EXPRESSION: ");
                                    foreach (Tokens x in infixTokens)
                                        Console.WriteLine(x.Type + ", " + x.Lexeme);
                                    Console.WriteLine("END OF INFIX");
                                    // Evaluate Infix Expression
                                    if (infixTokens.Count != 0)
                                    {
                                        object obj = null;
                                        string output = null;
                                        operation = new Operations(infixTokens, errorMessages, outputMap);
                                        postfix = operation.logicInfixToPostFix();
                                        output = operation.evaluateExpression(postfix);

                                        if (outputMap[temp_ident].GetType() == typeof(double))
                                            try
                                            {
                                                obj = double.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line " + (tokens[tokenCounter].Line + 1)));
                                                error = true; 
                                            }

                                        else if (outputMap[temp_ident].GetType() == typeof(int))

                                            try
                                            {
                                                obj = int.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line  " + (tokens[tokenCounter].Line + 1)));
                                                error = true;
                                            }
                                        else if (outputMap[temp_ident].GetType() == typeof(string))

                                            try
                                            {
                                                obj = bool.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line " + (tokens[tokenCounter].Line + 1)));
                                                error = true;
                                            }
                                        else if (outputMap[temp_ident].GetType() == typeof(char))


                                            try
                                            {
                                                obj = char.Parse(output);
                                            }
                                            catch (Exception e)
                                            {
                                                errorMessages.Add(string.Format("Data types does not match at line  " + (tokens[tokenCounter].Line + 1)));
                                                error = true;
                                            }
                                        else
                                        {
                                            errorMessages.Add(string.Format("Unidentified type at line " + (tokens[tokenCounter].Line + 1)));
                                            error = true;
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
                                        error = true;
                                    }
                                }
                                else
                                {
                                    errorMessages.Add(string.Format("Expected '=' after identifier at line " + (tokens[tokenCounter].Line + 1)));
                                    error = true;
                                }
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Identifier " + temp_ident + " is not declared at line " + (tokens[tokenCounter].Line + 1)));
                                error = true;
                            }
                        }
                        else
                        { // if !foundStart, or for variable declaration
                            tokenCounter++;
                            ParseIdentifier(temp_ident);
                        }
                        break;
                    case TokenType.OUTPUT:
                        if (whileStart)
                        {
                            tokenCounter++;
                            ParseOutput();
                        }
                        else
                        {
                            msg = "Syntax Error. There is something wrong with OUTPUT at line " + (tokens[tokenCounter].Line + 1);
                            errorMessages.Add(msg);
                            error = true;
                        }
                        break;
                    case TokenType.INPUT:
                        tokenCounter++;
                        ParseInput();
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
                    case TokenType.IF:
                        tokenCounter++;
                        errorFound = getInfix();
                        if (errorFound) error = true;
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
                                startCount++;
                                ifStart++;
                                tokenCounter++;
                                int i = tokenCounter;

                                if (output == "True")
                                {

                                    flagIf = 1;
                                    //To do if output is true 
                                    Console.WriteLine(tokens[tokenCounter].Lexeme);
                                    //Checking if STOP is there
                                    while (i != tokens.Count)
                                    {
                                        if (tokens[i].Type == TokenType.STOP)
                                        {
                                            ifStop++;
                                            stopCount++;
                                            break;
                                        }
                                        i++;
                                    }

                                }
                                else
                                {
                                    flagIf = -1;
                                    //TO DO IF OUTPUT IS FALSE; skip the tokens until the next stop
                                    while (tokenCounter != tokens.Count)
                                    {
                                        if (tokens[tokenCounter].Type == TokenType.STOP)
                                        {
                                            ifStop++;
                                            stopCount++;
                                            break;
                                        }
                                        tokenCounter++;
                                    }
                                }
                                Console.WriteLine("Start Count: " + ifCount + " Stop Count: " + ifStop);

                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at " + (tokens[tokenCounter - 1].Line + 1)));
                                error = true;
                            }

                        }
                        else
                        {
                            errorMessages.Add(string.Format("Invalid expression at line " + (tokens[tokenCounter - 1].Line + 1)));
                            error = true;
                        }
                        break;
                    case TokenType.ELSE:
                        if (flagIf == -1)
                        {
                            tokenCounter++;
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                startCount++;
                                tokenCounter++;
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at Line: " + (tokens[tokenCounter - 1].Line + 1)));
                                error = true;
                            }
                        }
                        break;
                    case TokenType.WHILE:
                        tokenCounter++;
                        tokenCounter2 = tokenCounter;//GET THE STARTING FOR THE CONDITION
                        Console.WriteLine("Hello While: " + tokens[tokenCounter2].Lexeme);
                        errorFound = getInfix();
                        if (errorFound) error = true;
                        if (infixTokens.Count != 0)
                        {
                            obj = null;
                            output = null;
                            operation = new Operations(infixTokens, errorMessages, outputMap);
                            postfix = operation.logicInfixToPostFix();
                            output = operation.evaluateExpression(postfix);
                            infixTokens.Clear();
                            if (tokens[tokenCounter].Type == TokenType.START)
                            {
                                whileStart = true;
                                tokenCounter++;

                                ParseWhile(tokenCounter2, tokenCounter);
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Missing Start at Line: " + (tokens[tokenCounter - 1].Line + 1)));
                                error = true;
                            }
                        }
                        if (error)
                            break;
                        break;
                    default:
                        tokenCounter++;
                        break;
                }
            }


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
                    msg = "Syntax Error. There is something wrong with the declaration at line " + (tokens[tokenCounter].Line + 1);
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
                    if (tokens[tokenCounter].Type == TokenType.DOUBLE_QUOTE)
                    {
                        tokenCounter++;
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
                            tokenCounter++;
                        }
                        else if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
                        {
                            logic = bool.Parse(tokens[tokenCounter].Lexeme);
                            logic = !logic;
                            infixTokens.Add(new Tokens(tokens[tokenCounter].Type, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));

                            tokenCounter++;
                        }
                        else // NOT 5
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
                    if (tokens[tokenCounter].Type == TokenType.IDENTIFIER)
                    {// check if identifier is declared, and is of type string
                        string temp_ident2 = tokens[tokenCounter].Lexeme;
                        if (outputMap.ContainsKey(temp_ident2))
                        {
                            if (outputMap[temp_ident2].GetType() == typeof(string))
                            {
                                logic = Convert.ToBoolean(outputMap[temp_ident2]);
                                infixTokens.Add(new Tokens(TokenType.BOOL_LIT, logic.ToString(), logic.ToString(), tokens[tokenCounter].Line));
                                tokenCounter++;
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

                    }
                    else if (tokens[tokenCounter].Type == TokenType.BOOL_LIT)
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
                        errorMessages.Add("Alien identifier at line " + (tokens[tokenCounter].Line + 1));
                        return true;
                    }

                }
                else
                    infixTokens.Add(tokens[tokenCounter]);

                if (isFirst)// this is just used for identifying unary as the first element. like A = -4 + 6
                    isFirst = false;

                tokenCounter++;
            }//end while
            return error;
        }

    }
}