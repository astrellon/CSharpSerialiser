using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassObject
    {
        #region Fields
        public readonly ClassName FullName;
        public readonly IReadOnlyList<ClassField> Fields;
        public readonly IReadOnlyList<ClassField> CtorFields;
        public readonly IReadOnlyList<ClassGeneric> Generics;
        #endregion

        #region Constructor
        public ClassObject(ClassName fullName, IReadOnlyList<ClassField> fields, IReadOnlyList<ClassField> ctorFields, IReadOnlyList<ClassGeneric> generics)
        {
            this.FullName = fullName;
            this.Fields = fields;
            this.CtorFields = ctorFields;
            this.Generics = generics;
        }
        #endregion

        #region Methods
        #endregion
    }
}