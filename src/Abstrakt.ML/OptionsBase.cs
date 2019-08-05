using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Abstrakt.ML
{
    /// <summary>
    /// Base class for option helpers.
    /// </summary>
    public abstract class Options
    {
        protected static string GetMemberName<TType, TRes>(Expression<Func<TType, TRes>> expr)
        {
            if (expr.Body is MemberExpression m)
                return m.Member.Name;
            if (expr.Body is UnaryExpression u)
            {
                if (u.Operand is MemberExpression mem)
                    return mem.Member.Name;
            }

            throw new InvalidOperationException("Not a member expression");
        }
    }
}
