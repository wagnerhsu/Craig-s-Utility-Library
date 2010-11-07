﻿/*
Copyright (c) 2010 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Reflection.Emit.Interfaces;
using System.Reflection;
using System.Reflection.Emit;
#endregion

namespace Utilities.Reflection.Emit
{
    /// <summary>
    /// Helper class for defining default properties
    /// </summary>
    public class DefaultPropertyBuilder:IPropertyBuilder,IVariable
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="TypeBuilder">Type builder</param>
        /// <param name="Name">Name of the property</param>
        /// <param name="Attributes">Attributes for the property (public, private, etc.)</param>
        /// <param name="PropertyType">Property type for the property</param>
        /// <param name="Parameters">Parameter types for the property</param>
        public DefaultPropertyBuilder(TypeBuilder TypeBuilder, string Name,
            PropertyAttributes Attributes,MethodAttributes GetMethodAttributes,
            MethodAttributes SetMethodAttributes,
            Type PropertyType, List<Type> Parameters)
        {
            if (TypeBuilder==null)
                throw new ArgumentNullException("TypeBuilder");
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name");
            this.Name = Name;
            this.Type = TypeBuilder;
            this.Attributes = Attributes;
            this.GetMethodAttributes = GetMethodAttributes;
            this.SetMethodAttributes = SetMethodAttributes;
            this.PropertyType = PropertyType;
            this.Parameters = new List<ParameterBuilder>();
            if (Parameters != null)
            {
                int x = 1;
                foreach (Type Parameter in Parameters)
                {
                    this.Parameters.Add(new ParameterBuilder(Parameter, x));
                    ++x;
                }
            }
            Field = new FieldBuilder(Type, "_" + Name + "field", PropertyType, FieldAttributes.Private);
            Builder = Type.Builder.DefineProperty(Name, Attributes, PropertyType,
                (Parameters != null && Parameters.Count > 0) ? Parameters.ToArray() : System.Type.EmptyTypes);
            GetMethod = new MethodBuilder(Type, "get_" + Name, GetMethodAttributes, Parameters, PropertyType);
            GetMethod.Generator.Emit(OpCodes.Ldarg_0);
            GetMethod.Generator.Emit(OpCodes.Ldfld, Field.Builder);
            GetMethod.Generator.Emit(OpCodes.Ret);
            List<Type> SetParameters = new List<System.Type>();
            if (Parameters != null)
            {
                SetParameters.AddRange(Parameters);
            }
            SetParameters.Add(PropertyType);
            SetMethod = new MethodBuilder(Type, "set_" + Name, SetMethodAttributes, SetParameters, typeof(void));
            SetMethod.Generator.Emit(OpCodes.Ldarg_0);
            SetMethod.Generator.Emit(OpCodes.Ldarg_1);
            SetMethod.Generator.Emit(OpCodes.Stfld, Field.Builder);
            SetMethod.Generator.Emit(OpCodes.Ret);
            Builder.SetGetMethod(GetMethod.Builder);
            Builder.SetSetMethod(SetMethod.Builder);
        }

        #endregion

        #region Functions

        public void Load(ILGenerator Generator)
        {
            Generator.EmitCall(OpCodes.Callvirt,GetMethod.Builder, null);
        }

        public void Save(ILGenerator Generator)
        {
            Generator.EmitCall(OpCodes.Callvirt, SetMethod.Builder, null);
        }

        public string GetDefinition()
        {
            StringBuilder Output = new StringBuilder();

            Output.Append("\n");
            if ((GetMethodAttributes & MethodAttributes.Public) > 0)
                Output.Append("public ");
            else if ((GetMethodAttributes & MethodAttributes.Private) > 0)
                Output.Append("private ");
            if ((GetMethodAttributes & MethodAttributes.Static) > 0)
                Output.Append("static ");
            if ((GetMethodAttributes & MethodAttributes.Virtual) > 0)
                Output.Append("virtual ");
            else if ((GetMethodAttributes & MethodAttributes.Abstract) > 0)
                Output.Append("abstract ");
            else if ((GetMethodAttributes & MethodAttributes.HideBySig) > 0)
                Output.Append("override ");
            Output.Append(Reflection.GetTypeName(PropertyType));
            Output.Append(" ").Append(Name);

            string Splitter = "";
            if (Parameters != null && Parameters.Count > 0)
            {
                Output.Append("[");
                foreach (ParameterBuilder Parameter in Parameters)
                {
                    Output.Append(Splitter).Append(Parameter.GetDefinition());
                    Splitter = ",";
                }
                Output.Append("]");
            }
            Output.Append(" { get; ");
            if ((SetMethodAttributes & GetMethodAttributes) != SetMethodAttributes)
            {
                if ((SetMethodAttributes & MethodAttributes.Public) > 0)
                    Output.Append("public ");
                else if ((SetMethodAttributes & MethodAttributes.Private) > 0)
                    Output.Append("private ");
            }
            Output.Append("set; }\n");

            return Output.ToString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Method name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Parameter types
        /// </summary>
        public List<ParameterBuilder> Parameters { get; private set; }

        /// <summary>
        /// Method builder
        /// </summary>
        public System.Reflection.Emit.PropertyBuilder Builder { get; private set; }

        /// <summary>
        /// Attributes for the property
        /// </summary>
        public System.Reflection.PropertyAttributes Attributes { get; private set; }

        /// <summary>
        /// Attributes for the get method
        /// </summary>
        public System.Reflection.MethodAttributes GetMethodAttributes { get; private set; }

        /// <summary>
        /// Attributes for the set method
        /// </summary>
        public System.Reflection.MethodAttributes SetMethodAttributes { get; private set; }

        /// <summary>
        /// Method builder for the get method
        /// </summary>
        public MethodBuilder GetMethod { get; private set; }

        /// <summary>
        /// Method builder for the set method
        /// </summary>
        public MethodBuilder SetMethod { get; private set; }

        public FieldBuilder Field { get; private set; }

        private TypeBuilder Type { get; set; }

        #endregion

        #region Overridden Functions

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
