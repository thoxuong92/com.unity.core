using System;
using UnityEngine;

namespace Unity.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionField { get; private set; }
        public object ExpectedValue { get; private set; }
        public ComparisonOp Comparison { get; private set; }
        public BoolOperator Operator { get; private set; }
        public bool Invert { get; set; }

        public ShowIfAttribute(string conditionField)
        {
            ConditionField = conditionField;
            Comparison = ComparisonOp.Equals;
            ExpectedValue = true;
            Operator = BoolOperator.And;
        }

        public ShowIfAttribute(string conditionField, object expectedValue, ComparisonOp comparison = ComparisonOp.Equals)
        {
            ConditionField = conditionField;
            ExpectedValue = expectedValue;
            Comparison = comparison;
            Operator = BoolOperator.And;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string conditionField) : base(conditionField)
        {
            Invert = true;
        }

        public HideIfAttribute(string conditionField, object expectedValue, ComparisonOp comparison = ComparisonOp.Equals)
            : base(conditionField, expectedValue, comparison)
        {
            Invert = true;
        }
    }
}
