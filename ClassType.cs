using System;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassType
    {
        #region Fields
        public readonly ClassName Name;
        public readonly CollectionType CollectionType;
        public readonly IReadOnlyList<ClassType> GenericTypes;

        #endregion

        #region Constructor
        public ClassType(ClassName name, CollectionType collectionType, IReadOnlyList<ClassType> genericTypes = null)
        {
            this.Name = name;
            this.CollectionType = collectionType;
            this.GenericTypes = genericTypes ?? new ClassType[0];
        }
        #endregion

        #region Methods
        #endregion
    }
}