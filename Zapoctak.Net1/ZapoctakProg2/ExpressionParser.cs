using System;
using System.Linq.Expressions;

namespace ZapoctakProg2
{
    static class ExpressionParser
    {
        static readonly string[] ParamSeparator = new[] {"=>"};
        private static readonly char[] TrimmedChars = new[] {'(', ')', ' '};

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
