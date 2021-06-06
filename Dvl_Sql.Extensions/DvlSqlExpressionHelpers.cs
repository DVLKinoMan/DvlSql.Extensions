using Dvl_Sql.Expressions;
using System.CustomModels.Filters;
using static Dvl_Sql.Helpers.Expressions;

namespace Dvl_Sql.Extensions
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
