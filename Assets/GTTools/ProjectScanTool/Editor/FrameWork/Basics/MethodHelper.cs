/*
 * @Description: 打印日志函数接口辅助
 */
using System;
using System.Linq.Expressions;

namespace CasualEngine.ProjectScanTool
{
    public class MethodHelper
    {
        /// <summary>
        /// 获得变量名称
        /// </summary>
        public static string GetVarName(Expression<Func<System.Object>> expr)
        {
            Expression e = expr.Body;
            MemberExpression me = e as MemberExpression;
            if (me == null)
            {
                UnaryExpression ue = e as UnaryExpression;
                me = ue.Operand as MemberExpression;
            }
            return me.Member.Name;
        }
    }
}

