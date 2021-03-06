using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassBaseObject
    {
        public struct SubclassPair
        {
            public readonly ClassObject Subclass;
            public readonly object TypeDiscriminatorValue;

            public SubclassPair(ClassObject subclass, object typeDiscriminatorValue)
            {
                this.Subclass = subclass;
                this.TypeDiscriminatorValue = typeDiscriminatorValue;
            }
        }

        #region Fields
        public readonly ClassName FullName;
        public readonly ClassField TypeDiscriminator;
        public readonly IReadOnlyList<ClassGeneric> Generics;
        public readonly IReadOnlyList<SubclassPair> Subclasses;
        public readonly ClassName InterfaceName;
        #endregion

        #region Constructor
        public ClassBaseObject(ClassName fullName, ClassField typeDiscriminator, IReadOnlyList<ClassGeneric> generics, IReadOnlyList<SubclassPair> subclasses, ClassName interfaceName)
        {
            this.FullName = fullName;
            this.TypeDiscriminator = typeDiscriminator;
            this.Generics = generics;
            this.Subclasses = subclasses;
            this.InterfaceName = interfaceName;
        }
        #endregion

        #region Methods
        #endregion
    }
}