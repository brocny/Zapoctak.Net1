using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ZapoctakProg2
{
    public static class ExpressionParser
    {
        static readonly string[] ParamSeparator = new[] {"=>"};
        private static readonly char[] TrimmedChars = new[] {'(', ')', ' '};
        /// <summary>
        /// Parser a single-parameter (parameter type is <code>Level</code>) lambda function in the form of a string
        /// </summary>
        /// <param name="expr">Expression to be parsed</param>
        /// <returns>Function corresponding to <code>expr</code></returns>
        public static Func<Level, bool> Parse(string expr)
        {
            var exprParts = expr.Split(ParamSeparator, StringSplitOptions.None);
            var paramName = exprParts[0].Trim(TrimmedChars);
            var body = exprParts[1];
            body = body.Replace("Good", "0");
            body = body.Replace("Neutral", "1");
            body = body.Replace("Bad", "2");

            var param = Expression.Parameter(typeof(Level), paramName);
            var expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] {param}, null, body);

            return (Func<Level, bool>) expression.Compile();
        }
    }

}
