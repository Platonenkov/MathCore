using System;
using MathCore.Annotations;
using MathCore.MathParser.ExpressionTrees.Nodes;
// ReSharper disable UnusedMember.Global

namespace MathCore.MathParser
{
    /// <summary>�������� ������� ��������������� ���������</summary>
    internal sealed class NumberTerm : Term
    {
        /// <summary>��������� �������� ��������</summary>
        private int _IntValue;

        /// <summary>��������� �������� ��������</summary>
        public int Value
        {
            get => _IntValue;
            set
            {
                _IntValue = value;
                _Value = value.ToString();
            }
        }

        /// <summary>����� ��������� ������� ���.���������</summary>
        /// <param name="Str">��������� �������� ��������</param>
        public NumberTerm([NotNull] string Str) : base(Str) => _IntValue = int.Parse(Str);

        public NumberTerm(int Value) : base(Value.ToString()) => _IntValue = Value;

        /// <summary>������ ���������</summary>
        /// <param name="Parser">������</param>
        /// <param name="Expression">�������������� ���������</param>
        /// <returns>���� ������������ ��������</returns>
        public override ExpressionTreeNode GetSubTree(ExpressionParser Parser, MathExpression Expression)
            => new ConstValueNode(_IntValue);

        /// <summary>���������� �������� ������� �������� �����</summary>
        /// <param name="node">���� ���������</param>
        /// <param name="SeparatorTerm">���� �����������</param>
        /// <param name="DecimalSeparator">���� � ����� ������ �����</param>
        /// <param name="FrationPartTerm">���� � ������� ������ �����</param>
        /// <returns>������, ���� �������� ��������� �������. ����, ���� � ����������� ������ �� ���������� ������ ����������</returns>
        public static bool TryAddFractionPart(ref ExpressionTreeNode node, Term SeparatorTerm, char DecimalSeparator, Term FrationPartTerm)
        {
            if(!(node is ConstValueNode value)) throw new ArgumentException("�������� ��� ���� ������");
            if(!(SeparatorTerm is CharTerm separator) || separator.Value != DecimalSeparator) return false;
            if(!(FrationPartTerm is NumberTerm fraction)) return false;

            var v_value = fraction.Value;
            if(v_value == 0) return true;
            node = new ConstValueNode(value.Value + v_value / Math.Pow(10, Math.Truncate(Math.Log10(v_value)) + 1));
            return true;
        }
    }
}