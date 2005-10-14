using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BLToolkit.Reflection.Emit
{
	/// <summary>
	/// A wrapper around the <see cref="AssemblyBuilder"/> and <see cref="ModuleBuilder"/> classes.
	/// </summary>
	/// <include file="Examples.CS.xml" path='examples/emit[@name="Emit"]/*' />
	/// <include file="Examples.VB.xml" path='examples/emit[@name="Emit"]/*' />
	/// <seealso cref="System.Reflection.Emit.AssemblyBuilder">AssemblyBuilder Class</seealso>
	/// <seealso cref="System.Reflection.Emit.ModuleBuilder">ModuleBuilder Class</seealso>
	public class AssemblyBuilderHelper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyBuilderHelper"/> class
		/// with the specified parameters.
		/// </summary>
		/// <param name="path">The path where the assembly will be saved.</param>
		public AssemblyBuilderHelper(string path)
		{
			if (path == null) throw new ArgumentNullException("path");

			path = path.Replace("+", ".");

			string assemblyName = System.IO.Path.GetFileName     (path);
			string assemblyDir  = System.IO.Path.GetDirectoryName(path);

			_path              = path;
			_assemblyName.Name = assemblyName;

			_assemblyBuilder =
				assemblyDir == null || assemblyDir.Length == 0?
				Thread.GetDomain().DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave):
				Thread.GetDomain().DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave, assemblyDir);

			_assemblyBuilder.SetCustomAttribute(BLToolkitAttribute);
		}

		private string _path;
		/// <summary>
		/// Gets the path where the assembly will be saved.
		/// </summary>
		public  string  Path
		{
			get { return _path; }
		}

		private AssemblyName _assemblyName = new AssemblyName();
		/// <summary>
		/// Gets AssemblyName.
		/// </summary>
		public  AssemblyName  AssemblyName
		{
			get { return _assemblyName; }
		}

		private AssemblyBuilder _assemblyBuilder;
		/// <summary>
		/// Gets AssemblyBuilder.
		/// </summary>
		public  AssemblyBuilder  AssemblyBuilder
		{
			get { return _assemblyBuilder; }
		}

		private ModuleBuilder _moduleBuilder;
		/// <summary>
		/// Gets ModuleBuilder.
		/// </summary>
		public  ModuleBuilder  ModuleBuilder
		{
			get 
			{
				if (_moduleBuilder == null)
				{
					_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name);
					_moduleBuilder.SetCustomAttribute(BLToolkitAttribute);

				}

				return _moduleBuilder;
			}
		}

		private CustomAttributeBuilder _blToolkitAttribute;
		public  CustomAttributeBuilder  BLToolkitAttribute
		{
			get 
			{
				if (_blToolkitAttribute == null)
				{
					Type            at = typeof(TypeBuilder.BLToolkitGeneratedAttribute);
					ConstructorInfo ci = at.GetConstructor(Type.EmptyTypes);

					_blToolkitAttribute = new CustomAttributeBuilder(ci, new object[0]);
				}

				return _blToolkitAttribute;
			}
		}

		/// <summary>
		/// Converts the supplied <see cref="AssemblyBuilderHelper"/> to a <see cref="AssemblyBuilder"/>.
		/// </summary>
		/// <param name="assemblyBuilder">The AssemblyBuilderHelper.</param>
		/// <returns>An AssemblyBuilder.</returns>
		public static implicit operator AssemblyBuilder(AssemblyBuilderHelper assemblyBuilder)
		{
			return assemblyBuilder.AssemblyBuilder;
		}

		/// <summary>
		/// Converts the supplied <see cref="AssemblyBuilderHelper"/> to a <see cref="ModuleBuilder"/>.
		/// </summary>
		/// <param name="assemblyBuilder">The AssemblyBuilderHelper.</param>
		/// <returns>A ModuleBuilder.</returns>
		public static implicit operator ModuleBuilder(AssemblyBuilderHelper assemblyBuilder)
		{
			return assemblyBuilder.ModuleBuilder;
		}

		/// <summary>
		/// Saves this dynamic assembly to disk.
		/// </summary>
		public void Save()
		{
			_assemblyBuilder.Save(_assemblyName.Name);
		}

		#region DefineType Overrides

		/// <summary>
		/// Constructs a <see cref="TypeBuilderHelper"/> for a type with the specified name.
		/// </summary>
		/// <param name="name">The full path of the type.</param>
		/// <returns>Returns the created <b>TypeBuilderHelper</b>.</returns>
		/// <seealso cref="System.Reflection.Emit.ModuleBuilder.DefineType(string)">ModuleBuilder.DefineType Method</seealso>
		public TypeBuilderHelper DefineType(string name)
		{
			return new TypeBuilderHelper(this, ModuleBuilder.DefineType(name));
		}

		/// <summary>
		/// Constructs a <see cref="TypeBuilderHelper"/> for a type with the specified name and base type.
		/// </summary>
		/// <param name="name">The full path of the type.</param>
		/// <param name="parent">The Type that the defined type extends.</param>
		/// <returns>Returns the created <b>TypeBuilderHelper</b>.</returns>
		/// <seealso cref="System.Reflection.Emit.ModuleBuilder.DefineType(string,TypeAttributes,Type)">ModuleBuilder.DefineType Method</seealso>
		public TypeBuilderHelper DefineType(string name, Type parent)
		{
			return new TypeBuilderHelper(this, ModuleBuilder.DefineType(name, TypeAttributes.Public, parent));
		}

		/// <summary>
		/// Constructs a <see cref="TypeBuilderHelper"/> for a type with the specified name, its attributes, and base type.
		/// </summary>
		/// <param name="name">The full path of the type.</param>
		/// <param name="attrs">The attribute to be associated with the type.</param>
		/// <param name="parent">The Type that the defined type extends.</param>
		/// <returns>Returns the created <b>TypeBuilderHelper</b>.</returns>
		/// <seealso cref="System.Reflection.Emit.ModuleBuilder.DefineType(string,TypeAttributes,Type)">ModuleBuilder.DefineType Method</seealso>
		public TypeBuilderHelper DefineType(string name, TypeAttributes attrs, Type parent)
		{
			return new TypeBuilderHelper(this, ModuleBuilder.DefineType(name, attrs, parent));
		}

		/// <summary>
		/// Constructs a <see cref="TypeBuilderHelper"/> for a type with the specified name, base type,
		/// and the interfaces that the defined type implements.
		/// </summary>
		/// <param name="name">The full path of the type.</param>
		/// <param name="parent">The Type that the defined type extends.</param>
		/// <param name="interfaces">The list of interfaces that the type implements.</param>
		/// <returns>Returns the created <b>TypeBuilderHelper</b>.</returns>
		/// <seealso cref="System.Reflection.Emit.ModuleBuilder.DefineType(string,TypeAttributes,Type,Type[])">ModuleBuilder.DefineType Method</seealso>
		public TypeBuilderHelper DefineType(string name, Type parent, params Type[] interfaces)
		{
			return new TypeBuilderHelper(
				this,
				ModuleBuilder.DefineType(name, TypeAttributes.Public, parent, interfaces));
		}

		/// <summary>
		/// Constructs a <see cref="TypeBuilderHelper"/> for a type with the specified name, its attributes, base type,
		/// and the interfaces that the defined type implements.
		/// </summary>
		/// <param name="name">The full path of the type.</param>
		/// <param name="attrs">The attribute to be associated with the type.</param>
		/// <param name="parent">The Type that the defined type extends.</param>
		/// <param name="interfaces">The list of interfaces that the type implements.</param>
		/// <returns>Returns the created <b>TypeBuilderHelper</b>.</returns>
		/// <seealso cref="System.Reflection.Emit.ModuleBuilder.DefineType(string,TypeAttributes,Type,Type[])">ModuleBuilder.DefineType Method</seealso>
		public TypeBuilderHelper DefineType(string name, TypeAttributes attrs, Type parent, params Type[] interfaces)
		{
			return new TypeBuilderHelper(
				this,
				ModuleBuilder.DefineType(name, attrs, parent, interfaces));
		}

		#endregion
	}
}
