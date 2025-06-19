using System;
using System.Collections.Generic;
using System.Text;

public static class ExpressionEvaluator
{
    public static bool Evaluate(string expression, Dictionary<string, bool> variables)
    {
        var tokens = Tokenize(expression);
        var ast = ParseExpression(tokens);
        return ast.Evaluate(variables);
    }

    // Token types
    private enum TokenType { And, Or, Not, LParen, RParen, Identifier }

    private class Token
    {
        public TokenType Type;
        public string Value;
        public Token(TokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }
    }

    // Node types
    private abstract class Node
    {
        public abstract bool Evaluate(Dictionary<string, bool> vars);
    }

    private class VariableNode : Node
    {
        public string Name;
        public VariableNode(string name) => Name = name;
        public override bool Evaluate(Dictionary<string, bool> vars) => vars[Name];
    }

    private class BinaryNode : Node
    {
        public Node Left;
        public Node Right;
        public TokenType Op;
        public BinaryNode(Node left, Node right, TokenType op)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        public override bool Evaluate(Dictionary<string, bool> vars)
        {
            bool l = Left.Evaluate(vars);
            bool r = Right.Evaluate(vars);
            return Op switch
            {
                TokenType.And => l && r,
                TokenType.Or => l || r,
                _ => throw new InvalidOperationException("Unknown operator")
            };
        }
    }

    private class UnaryNode : Node
    {
        public Node Operand;
        public UnaryNode(Node operand) => Operand = operand;

        public override bool Evaluate(Dictionary<string, bool> vars)
        {
            return !Operand.Evaluate(vars);
        }
    }

    // Tokenizer
    private static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < input.Length)
        {
            char c = input[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '(') { tokens.Add(new Token(TokenType.LParen)); i++; }
            else if (c == ')') { tokens.Add(new Token(TokenType.RParen)); i++; }
            else if (c == '&' && i + 1 < input.Length && input[i + 1] == '&')
            {
                tokens.Add(new Token(TokenType.And));
                i += 2;
            }
            else if (c == '|' && i + 1 < input.Length && input[i + 1] == '|')
            {
                tokens.Add(new Token(TokenType.Or));
                i += 2;
            }
            else if (c == '!') { tokens.Add(new Token(TokenType.Not)); i++; }
            else if (char.IsLetterOrDigit(c) || c == '_')
            {
                var sb = new StringBuilder();
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                {
                    sb.Append(input[i]);
                    i++;
                }
                tokens.Add(new Token(TokenType.Identifier, sb.ToString()));
            }
            else
            {
                throw new Exception($"Unexpected character: {c}");
            }
        }

        return tokens;
    }

    // Parser: Recursive descent
    private static Node ParseExpression(List<Token> tokens)
    {
        int index = 0;

        Node ParseOr()
        {
            Node left = ParseAnd();
            while (index < tokens.Count && tokens[index].Type == TokenType.Or)
            {
                index++;
                Node right = ParseAnd();
                left = new BinaryNode(left, right, TokenType.Or);
            }
            return left;
        }

        Node ParseAnd()
        {
            Node left = ParsePrimary();
            while (index < tokens.Count && tokens[index].Type == TokenType.And)
            {
                index++;
                Node right = ParsePrimary();
                left = new BinaryNode(left, right, TokenType.And);
            }
            return left;
        }

        Node ParsePrimary()
        {
            if (index >= tokens.Count) throw new Exception("Unexpected end of input");

            Token token = tokens[index++];

            if (token.Type == TokenType.Identifier)
                return new VariableNode(token.Value);

            if (token.Type == TokenType.LParen)
            {
                Node expr = ParseOr();
                if (index >= tokens.Count || tokens[index].Type != TokenType.RParen)
                    throw new Exception("Expected ')'");
                index++;
                return expr;
            }

            if (token.Type == TokenType.Not)
            {
                Node operand = ParsePrimary();
                return new UnaryNode(operand);
            }

            throw new Exception($"Unexpected token: {token.Type}");
        }

        return ParseOr();
    }
}

