using System;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public struct ClassName : IEquatable<string>
    {
        #region Fields
        public static readonly ClassName Empty = new ClassName("<EMPTY>");

        public readonly string Value;

        public bool IsEmpty => this.Value == Empty.Value;
        #endregion

        #region Constructor
        public ClassName(string value)
        {
            this.Value = string.Intern(ProcessTypeName(value));
        }
        #endregion

        #region Methods
        public string TrimNameSpace(IReadOnlyList<string> nameSpace)
        {
            var result = this.Value;
            foreach (var ns in nameSpace)
            {
                if (result.StartsWith(ns))
                {
                    result = result.Substring(ns.Length + 1);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public bool Equals(ClassName other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ClassName))
            {
                return false;
            }

            return ((ClassName)obj).Value == this.Value;
        }

        public bool Equals(string other)
        {
            return this.Value == other;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value;
        }

        public static ClassName FromValue(string input)
        {
            return new ClassName(input);
        }

        public static bool operator==(ClassName input1, ClassName input2)
        {
            return input1.Value == input2.Value;
        }

        public static bool operator!=(ClassName input1, ClassName input2)
        {
            return input1.Value != input2.Value;
        }

        public static string ProcessTypeName(string typeName)
        {
            typeName = typeName.Replace("+", ".");
            var backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                return typeName.Substring(0, backtickIndex);
            }

            return typeName;
        }
        #endregion
    }
}