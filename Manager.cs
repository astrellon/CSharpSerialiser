using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class Manager
    {
        #region Fields
        public readonly string NameSpace;

        private readonly Dictionary<ClassName, ClassObject> classMap = new Dictionary<ClassName, ClassObject>();

        public IReadOnlyDictionary<ClassName, ClassObject> ClassMap => this.classMap;
        #endregion

        #region Constructor
        public Manager(string nameSpace)
        {
            this.NameSpace = nameSpace;
        }
        #endregion

        #region Methods
        public void AddClass(ClassObject classObject)
        {
            this.classMap[classObject.FullName] = classObject;
        }

        public static ClassObject CreateObjectFromType(Type type)
        {
            var fields = new List<ClassField>();
            foreach (var field in type.GetFields())
            {
                var fieldType = (Type)null;
                try
                {
                    fieldType = field.FieldType;
                }
                catch (IOException)
                {
                    fieldType = null;
                }

                if (fieldType != null)
                {
                    fields.Add(new ClassField(field.Name, new ClassName(fieldType.FullName)));
                }
            }

            var ctors = type.GetConstructors();
            var ctor = ctors.First();
            var ctorFields = ctor.GetParameters().Select(p => p.Name).ToList();

            return new ClassObject(new ClassName(type.FullName), fields, ctorFields);
        }
        #endregion
    }
}