using DvlSql.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using static DvlSql.Extensions.ExpressionHelpers;

namespace DvlSql.Extensions
{
    public enum JoinType
    {
        Left,
        Right,
        Inner,
        Full
    }

    public abstract class DvlSqlFilter
    {
        public static readonly DvlSqlFilter Empty = new DvlSqlEmptyFilter();
    }

    public class DvlSqlJoinFilter : DvlSqlFilter
    {
        public readonly JoinType JoinType;
        public readonly string TableName;
        public readonly DvlSqlComparisonExpression CompExpression;

        public DvlSqlJoinFilter(JoinType joinType, string tableName, string firstTableMatchingCol, string secondTableMatchingCol)
        {
            this.JoinType = joinType;
            this.TableName = tableName;
            this.CompExpression = ConstantExpCol(firstTableMatchingCol) == ConstantExpCol(secondTableMatchingCol);
        }

        public DvlSqlJoinFilter(JoinType joinType, string tableName, DvlSqlComparisonExpression compExpression)
        {
            this.JoinType = joinType;
            this.TableName = tableName;
            this.CompExpression = compExpression;
        }

        public ISelector Join(ISelector selector) =>
            this.JoinType switch
            {
                JoinType.Full => selector.FullJoin(this.TableName, this.CompExpression),
                JoinType.Inner => selector.Join(this.TableName, this.CompExpression),
                JoinType.Left => selector.LeftJoin(this.TableName, this.CompExpression),
                JoinType.Right => selector.RightJoin(this.TableName, this.CompExpression),
                _ => throw new NotImplementedException($"{this.JoinType} not implemented")
            };

        public override int GetHashCode()
        {
            return this.TableName.GetHashCode() + this.JoinType.GetHashCode();
        }
    }

    public class DvlSqlJoinFilterEqualityComparer : IEqualityComparer<DvlSqlJoinFilter>
    {
        public bool Equals(DvlSqlJoinFilter? x, DvlSqlJoinFilter? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);
            if (x.JoinType != y.JoinType)
                return false;
            return x.TableName == y.TableName;
        }

        public int GetHashCode(DvlSqlJoinFilter obj)
        {
            return obj.GetHashCode();
        }
    }

    public class DvlSqlWhereFilter(DvlSqlBinaryExpression binaryExpression, params DvlSqlParameter[] parameters) : DvlSqlFilter
    {
        public DvlSqlBinaryExpression BinaryExpression { get; private set; } = binaryExpression;
        public IEnumerable<DvlSqlParameter> Parameters { get; set; } = parameters;

        public static DvlSqlWhereFilter operator &(DvlSqlWhereFilter @this, DvlSqlWhereFilter other)
        {
            @this.BinaryExpression &= other.BinaryExpression;
            @this.Parameters = @this.Parameters.Concat(other.Parameters);
            return @this;
        }
    }

    public class DvlSqlEmptyFilter : DvlSqlFilter
    {

    }

}
