﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MPMFEVRP.Properties
{
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources
    {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MPMFEVRP.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap data
        {
            get
            {
                object obj = ResourceManager.GetObject("data", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to // N = number of jobs : integer
        ///10
        ///// P = processing times of jobs : integer array of N
        ///1 2 3 4 5 6 7 8 9 10
        ///// D = due dates of jobs : integer array of N
        ///13 29 23 25 27 15 17 19 21 11
        ///// Desc = descriptions of jobs : string of N lines
        ///Wakeup
        ///Brush your teeth
        ///Eat something
        ///Clean your shoes
        ///Wear something
        ///Prepare coffee
        ///Start the car
        ///Drive to work
        ///Do your work
        ///Go back home.
        /// </summary>
        internal static string data1
        {
            get
            {
                return ResourceManager.GetString("data1", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to // N = number of jobs : integer
        ///15
        ///// P = processing times of jobs : integer array of N
        ///1 1 1 1 1 1 1 1 1 1 1 1 1 1 1
        ///// D = due dates of jobs : integer array of N
        ///12 1 11 2 10 3 9 4 8 5 7 6 6 7 13.
        /// </summary>
        internal static string data2
        {
            get
            {
                return ResourceManager.GetString("data2", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap multi
        {
            get
            {
                object obj = ResourceManager.GetObject("multi", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap single
        {
            get
            {
                object obj = ResourceManager.GetObject("single", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
