using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassObject
    {
        #region Fields
        public readonly ClassName FullName;
        public readonly IReadOnlyList<ClassField> Fields;
        public readonly IReadOnlyList<string> CtorFields;
        #endregion

        #region Constructor
        public ClassObject(ClassName fullName, IReadOnlyList<ClassField> fields, IReadOnlyList<string> ctorFields)
        {
            this.FullName = fullName;
            this.Fields = fields;
            this.CtorFields = ctorFields;
        }
        #endregion

        #region Methods
        #endregion
    }
}