﻿/*
Copyright (c) 2014 <a href="http://www.gutgames.com">James Craig</a>

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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Utilities.DataTypes;
using Utilities.DataTypes.Patterns.BaseClasses;
using Utilities.ORM.Manager.QueryProvider.Interfaces;
using Utilities.ORM.Manager.Schema.Interfaces;

#endregion Usings

namespace Utilities.ORM.Manager.QueryProvider
{
    /// <summary>
    /// Profiler manager
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Manager()
        {
            Providers = AppDomain.CurrentDomain
                                 .GetAssemblies()
                                 .Objects<Interfaces.IQueryProvider>()
                                 .ToDictionary(x => x.ProviderName);
        }

        /// <summary>
        /// Providers
        /// </summary>
        protected IDictionary<string, Interfaces.IQueryProvider> Providers { get; private set; }

        /// <summary>
        /// Creates a batch object
        /// </summary>
        /// <param name="SchemaType">Schema type</param>
        /// <returns>The batch object</returns>
        public IBatch Batch(string SchemaType)
        {
            if (string.IsNullOrEmpty(SchemaType))
                SchemaType = "System.Data.SqlClient";
            return Providers.ContainsKey(SchemaType) ? Providers[SchemaType].Batch() : null;
        }

        /// <summary>
        /// Outputs the provider information as a string
        /// </summary>
        /// <returns>The provider information as a string</returns>
        public override string ToString()
        {
            return Providers.ToString(x => x.Key);
        }
    }
}