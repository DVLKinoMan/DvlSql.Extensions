﻿using System.Collections.Generic;
using System.CustomModels.Filters;
using System.Linq;
using static Dvl_Sql.Helpers.Expressions;

namespace Dvl_Sql.Extensions
{
    public static class CommonQueryExpressionHelpers
    {
        public static IEnumerable<DvlSqlFilter> PatternString(string fieldName, PatternString name)
        {
            if (name == null)
                yield break;
            if (!string.IsNullOrEmpty(name.Value))
                yield return new DvlSqlWhereFilter(ConstantExpCol(fieldName) == ConstantExp(name.Value, false));
            else if (!string.IsNullOrEmpty(name.Pattern))
                yield return new DvlSqlWhereFilter(LikeExp(fieldName, $"{name.Pattern}"));
        }

#nullable enable
        public static IEnumerable<DvlSqlFilter> Id(string tableAlias, IEnumerable<int>? ids)
        {
            if (ids is {} notNullIds)
                yield return new DvlSqlWhereFilter(InExp($"{tableAlias}.Id", notNullIds.Select(id => ConstantExp(id)).ToArray()));
        }
#nullable restore
    }
}
