using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class HarmonyUtil
    {
        public static void Patch(this Harmony harmony, MethodInfo original, Expression<Action> prefix = default, Expression<Action> postfix = default)
        {
            harmony.Patch(original, GetHarmonyMethod(prefix), GetHarmonyMethod(postfix));
        }

        public static HarmonyMethod GetHarmonyMethod(Expression<Action> expression)
        {
            if (expression == null)
                return null;
            if (expression.Body is MethodCallExpression member)
                return new HarmonyMethod(member.Method);
            throw new ArgumentException("Expression is not a method", nameof(expression));
        }
    }
}
