using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace ZapoctakProg2
{
    public static class ExpressionParser
    {
        static readonly string[] ParamSeparator = new[] {"=>"};
        private static readonly char[] TrimmedChars = new[] {'(', ')', ' '};
        /// <summary>
        /// Parses a single-parameter (parameter type is <code>Level</code>) lambda function in the form of a string
        /// </summary>
        /// <param name="expr">Expression to be parsed</param>
        /// <returns>Function corresponding to <code>expr</code></returns>
        public static LambdaExpression Parse(string expr)
        {
            var exprParts = expr.Split(ParamSeparator, StringSplitOptions.None);
            var paramName = exprParts[0].Trim(TrimmedChars);
            var body = exprParts[1];

            // To allow more convenient indexing instead of e.g. PlanetCountByType[(int)Planet.PlanetType.Good] ... 
            body = body.Replace("Good", "0");
            body = body.Replace("Neutral", "1");
            body = body.Replace("Bad", "2");

            var param = Expression.Parameter(typeof(Level), paramName);
            var expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] {param}, null, body);
            return expression;
        }

        /// <summary>
        /// Generates auto-description for special cases of expression
        /// </summary>
        /// <param name="expr">expr to generate description from</param>
        /// <param name="paramName">name of the parameter (expected to be level)</param>
        /// <returns>String description of <code>expr</code> if <code>expr</code>has the correct format, null otherwise</returns>

        private static readonly string PlanetCountByTypeName = nameof(Level.PlanetCountByType).Split('.').Last();
        private static readonly string PlanetCountName = nameof(Level.PlanetCount).Split('.').Last();

        private static readonly MemberInfo PlanetCountByTypeInfo = typeof(Level).GetProperty(PlanetCountByTypeName);
        private static readonly MemberInfo PlanetCountInfo = typeof(Level).GetProperty(PlanetCountName);



        private static readonly Dictionary<ExpressionType, string> OperatorDict = new Dictionary<ExpressionType, string>()
        {
            {ExpressionType.Add, " + "},
            {ExpressionType.Subtract, " - " },
            {ExpressionType.Multiply, " * " },
            {ExpressionType.Divide, " / " },
            {ExpressionType.OrElse, " OR " },
            {ExpressionType.AndAlso, " AND "},
            {ExpressionType.Equal, " required exactly " },
            {ExpressionType.GreaterThan, " required more than " },
            {ExpressionType.LessThan, " required less than " },
            {ExpressionType.LessThanOrEqual, " required at most " },
            {ExpressionType.GreaterThanOrEqual, " required at least "}
        };

        /// <summary>
        /// Generate description from lambda expression
        /// </summary>
        /// <param name="lambdaExpression"><code>LambdaExpression for which to generate description</code></param>
        /// <returns>Description of the win condition if known, <code>lambdaExpression.ToString() if uknown</code></returns>
        public static string GenerateDescription(LambdaExpression lambdaExpression)
        {
            return GenerateDescriptionInternal(lambdaExpression.Body);
        }

        private static string GenerateDescriptionInternal(Expression expr)
        {
            if (expr.NodeType == ExpressionType.ArrayIndex)
            {
                var indExpr = (BinaryExpression) expr;
                if (indExpr.Left.NodeType == ExpressionType.MemberAccess && indExpr.Right.NodeType == ExpressionType.Constant)
                {
                    var memberExpr = (MemberExpression) indExpr.Left;
                    var constExpr = (ConstantExpression) indExpr.Right;
                    if (memberExpr.Member == PlanetCountByTypeInfo)
                    {
                        switch ((int)constExpr.Value)
                        {
                            case 0:
                                return "Good planets";
                            case 1:
                                return "Neutral planets";
                            case 2:
                                return "Bad planets";
                        }
                    }
                }
            }
            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) expr;
                if (memberExpr.Member == PlanetCountInfo)
                    return "Total planets";
            }

            BinaryExpression binExpr;
            if ((binExpr = expr as BinaryExpression) != null)
            {
                if (OperatorDict.TryGetValue(binExpr.NodeType, out string op))
                {
                    return GenerateDescriptionInternal(binExpr.Left) + op 
                        + GenerateDescriptionInternal(binExpr.Right);
                }
            }

    }
}
