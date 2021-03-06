using DvlSql.Expressions;
using System.CustomModels.Filters;
using static DvlSql.Extensions.ExpressionHelpers;

namespace DvlSql.Extensions
{
    public static class DvlSqlExpressionHelpers
    {
        public static DvlSqlAndExpression RangeExp<TValue>(string col, Range<TValue> range)
        {
            if (range.IncludingEnds)
                return ConstantExp(range.Start) <= ConstantExp(col) &
                       ConstantExp(col) <= ConstantExp(range.End);

            return ConstantExp(range.Start) < ConstantExp(col) &
                   ConstantExp(col) < ConstantExp(range.End);
        }
    }
}
