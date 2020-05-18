using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassGeneric
    {
        #region Fields
        public readonly string Name;
        public readonly IReadOnlyList<ClassType> Constraints;
        #endregion

        #region Constructor
        public ClassGeneric(string name, IReadOnlyList<ClassType> constraints)
        {
            this.Name = name;
            this.Constraints = constraints;
        }
        #endregion

        #region Methods
        #endregion
    }
}