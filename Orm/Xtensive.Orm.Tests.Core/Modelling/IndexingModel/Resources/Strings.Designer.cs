﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xtensive.Modelling.IndexingModel.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xtensive.Modelling.IndexingModel.Resources.Strings", typeof(Strings).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to , .
        /// </summary>
        internal static string Comma {
            get {
                return ResourceManager.GetString("Comma", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not find reference to column &quot;{0}&quot;..
        /// </summary>
        internal static string ExCanNotFindReferenceToColumnX {
            get {
                return ResourceManager.GetString("ExCanNotFindReferenceToColumnX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Column &quot;{0}&quot; contains both key and value collections..
        /// </summary>
        internal static string ExColumnXContainsBothKeyAndValueCollections {
            get {
                return ResourceManager.GetString("ExColumnXContainsBothKeyAndValueCollections", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to KeyColumns collection is empty..
        /// </summary>
        internal static string ExEmptyKeyColumnsCollection {
            get {
                return ResourceManager.GetString("ExEmptyKeyColumnsCollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Direction value (Direction.None)..
        /// </summary>
        internal static string ExInvalidDirectionValue {
            get {
                return ResourceManager.GetString("ExInvalidDirectionValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid ForeignKey structure: its column sequence do not match PrimaryKey column sequence..
        /// </summary>
        internal static string ExInvalidForeignKeyStructure {
            get {
                return ResourceManager.GetString("ExInvalidForeignKeyStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IncludedColumns collection is invalid..
        /// </summary>
        internal static string ExInvalidIncludedColumnsCollection {
            get {
                return ResourceManager.GetString("ExInvalidIncludedColumnsCollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PrimaryKeyColumns collection is invalid..
        /// </summary>
        internal static string ExInvalidPrimaryKeyColumnsCollection {
            get {
                return ResourceManager.GetString("ExInvalidPrimaryKeyColumnsCollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid PrimaryKeyInfo structure: sequence KeyColumns and ValueColumns do not match sequence of all the Columns of the table..
        /// </summary>
        internal static string ExInvalidPrimaryKeyStructure {
            get {
                return ResourceManager.GetString("ExInvalidPrimaryKeyStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IncludedColumns collection contains more then one reference to column &quot;{0}&quot;..
        /// </summary>
        internal static string ExMoreThenOneIncludedColumnReferenceToColumnX {
            get {
                return ResourceManager.GetString("ExMoreThenOneIncludedColumnReferenceToColumnX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to KeyColumns collection contains more then one reference to column &quot;{0}&quot;..
        /// </summary>
        internal static string ExMoreThenOneKeyColumnReferenceToColumnX {
            get {
                return ResourceManager.GetString("ExMoreThenOneKeyColumnReferenceToColumnX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ValueColumns collection contains more then one reference to column &quot;{0}&quot;..
        /// </summary>
        internal static string ExMoreThenOneValueColumnReferenceToColumnX {
            get {
                return ResourceManager.GetString("ExMoreThenOneValueColumnReferenceToColumnX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Primary key column can not be nullable..
        /// </summary>
        internal static string ExPrimaryKeyColumnCanNotBeNullable {
            get {
                return ResourceManager.GetString("ExPrimaryKeyColumnCanNotBeNullable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Referenced column &quot;{0}&quot; does not belong to index &quot;{1}&quot;..
        /// </summary>
        internal static string ExReferencedColumnXDoesNotBelongToIndexY {
            get {
                return ResourceManager.GetString("ExReferencedColumnXDoesNotBelongToIndexY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ForeignKey is undefined..
        /// </summary>
        internal static string ExUndefinedForeignKey {
            get {
                return ResourceManager.GetString("ExUndefinedForeignKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PrimaryKey is undefined..
        /// </summary>
        internal static string ExUndefinedPrimaryKey {
            get {
                return ResourceManager.GetString("ExUndefinedPrimaryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type of column &quot;{0}&quot; is undefined..
        /// </summary>
        internal static string ExUndefinedTypeOfColumnX {
            get {
                return ResourceManager.GetString("ExUndefinedTypeOfColumnX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Length.
        /// </summary>
        internal static string Length {
            get {
                return ResourceManager.GetString("Length", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ?.
        /// </summary>
        internal static string NullableMark {
            get {
                return ResourceManager.GetString("NullableMark", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Precision.
        /// </summary>
        internal static string Precision {
            get {
                return ResourceManager.GetString("Precision", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}: {1}.
        /// </summary>
        internal static string PropertyPairFormat {
            get {
                return ResourceManager.GetString("PropertyPairFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scale.
        /// </summary>
        internal static string Scale {
            get {
                return ResourceManager.GetString("Scale", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type.
        /// </summary>
        internal static string Type {
            get {
                return ResourceManager.GetString("Type", resourceCulture);
            }
        }
    }
}
