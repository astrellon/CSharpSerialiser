using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class Config
    {
        public class FindBaseClass
        {
            public readonly string TypeNameRegex;
            public readonly string TypeField;

            public FindBaseClass(string typeNameRegex, string typeField)
            {
                this.TypeNameRegex = typeNameRegex;
                this.TypeField = typeField;
            }
        }

        public class FindClass
        {
            public readonly string TypeNameRegex;

            public FindClass(string typeNameRegex)
            {
                this.TypeNameRegex = typeNameRegex;
            }
        }

        #region Fields
        public readonly IReadOnlyList<string> NameSpace;
        public readonly string BaseSerialiserClassName;
        public readonly IReadOnlyList<FindBaseClass> FindBaseClasses;
        public readonly IReadOnlyList<FindClass> FindClasses;
        #endregion

        #region Constructor
        public Config(IReadOnlyList<string> nameSpace, string baseSerialiserClassName, IReadOnlyList<FindBaseClass> findBaseClasses, IReadOnlyList<FindClass> findClasses)
        {
            this.NameSpace = nameSpace;
            this.BaseSerialiserClassName = baseSerialiserClassName;
            this.FindBaseClasses = findBaseClasses;
            this.FindClasses = findClasses;
        }
        #endregion

        #region Methods
        #endregion
    }
}