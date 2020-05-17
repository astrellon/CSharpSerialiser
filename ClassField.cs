namespace CSharpSerialiser
{
    public class ClassField
    {
        #region Fields
        public readonly string Name;
        public readonly ClassName TypeFullName;
        #endregion

        #region Constructor
        public ClassField(string name, ClassName typeFullName)
        {
            this.Name = name;
            this.TypeFullName = typeFullName;
        }
        #endregion

        #region Methods
        #endregion
    }
}