using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class ClassObject
    {
        public class FieldToCtorPair
        {
            public readonly ClassField Field;
            public readonly ClassField CtorField;

            public FieldToCtorPair(ClassField field, ClassField ctorField)
            {
                this.Field = field;
                this.CtorField = ctorField;
            }
        }

        #region Fields
        public readonly ClassName FullName;
        public readonly IReadOnlyList<ClassField> Fields;
        public readonly IReadOnlyList<FieldToCtorPair> CtorFields;
        public readonly IReadOnlyList<ClassGeneric> Generics;
        public readonly ClassBaseObject BaseObject;
        public readonly bool InlineFields;
        #endregion

        #region Constructor
        public ClassObject(ClassName fullName, IReadOnlyList<ClassField> fields, IReadOnlyList<FieldToCtorPair> ctorFields, IReadOnlyList<ClassGeneric> generics, ClassBaseObject baseObject = null, bool inlineFields = false)
        {
            this.FullName = fullName;
            this.Fields = fields;
            this.CtorFields = ctorFields;
            this.Generics = generics;
            this.BaseObject = baseObject;
            this.InlineFields = inlineFields;
        }
        #endregion

        #region Methods
        public static IReadOnlyList<FieldToCtorPair> FindCtor(IReadOnlyList<ClassField> fields, IReadOnlyList<IReadOnlyList<ClassField>> allCtorFields)
        {
            var finalFieldOrder = (FieldToCtorPair[])null;
            foreach (var ctorFields in allCtorFields)
            {
                var fieldOrder = new FieldToCtorPair[ctorFields.Count];
                for (var i = 0; i < ctorFields.Count; i++)
                {
                    var ctorField = ctorFields[i];
                    foreach (var field in fields)
                    {
                        if (field.Name.Equals(ctorField.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            fieldOrder[i] = new FieldToCtorPair(field, ctorField);
                            break;
                        }
                    }
                }

                if (fieldOrder.Any(fo => fo == null))
                {
                    for (var i = 0; i < ctorFields.Count; i++)
                    {
                        if (fieldOrder[i] != null)
                        {
                            continue;
                        }

                        var ctorField = ctorFields[i];
                        foreach (var field in fields)
                        {
                            if (field.Type.Name == ctorField.Type.Name)
                            {
                                fieldOrder[i] = new FieldToCtorPair(field, ctorField);
                                break;
                            }
                        }
                    }
                }

                if (fieldOrder.All(fo => fo != null))
                {
                    finalFieldOrder = fieldOrder;
                    break;
                }
            }

            if (finalFieldOrder == null)
            {
                throw new Exception($"Unable to determin ctor parameters");
            }

            return finalFieldOrder;
        }
        #endregion
    }
}