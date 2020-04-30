using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public static class TypedAssetFactory
	{
		private static Dictionary<Guid, Type> classMap = new Dictionary<Guid, Type>();
		private static Dictionary<Guid, ConstructorInfo> constructorMap = new Dictionary<Guid, ConstructorInfo>();

		static TypedAssetFactory()
		{
#if NETSTANDARD1_3
			// do nothing
#else
			RegisterType(typeof(Geometry));
			RegisterType(typeof(Material));
			RegisterType(typeof(Metamesh));
			RegisterType(typeof(MorphAnimation));
			RegisterType(typeof(Particle));
			RegisterType(typeof(PhysicsShape));
			RegisterType(typeof(ProceduralVectorField));
			RegisterType(typeof(Shader));
			RegisterType(typeof(Skeleton));
			RegisterType(typeof(SkeletalAnimation));
			RegisterType(typeof(Texture));
#endif
		}

		public static void RegisterType([NotNull] Type typeClass)
		{
#if NETSTANDARD1_3
			throw new NotImplementedException("Register custom type is unsupported in .net standard 1.3");
#else
			var field = typeClass.GetField("TYPE_GUID", BindingFlags.Static | BindingFlags.Public) ??
				throw new ArgumentException("Cannot find public static field \"TYPE_GUID\" from class " + typeClass.FullName);
			if (field.FieldType != typeof(Guid))
				throw new ArgumentException("\"TYPE_GUID\" must be Guid");
			Guid guid = (Guid) field.GetValue(null);
			RegisterType(guid, typeClass);
#endif
		}

		public static void RegisterType(Guid typeGuid, [NotNull] Type typeClass)
		{
#if NETSTANDARD1_3
			throw new NotImplementedException("Register custom type is unsupported in .net standard 1.3");
#else
			if (!typeof(AssetItem).IsAssignableFrom(typeClass))
				throw new ArgumentException("Registered type must extend from AssetItem");

			ConstructorInfo constructor = typeClass.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
				throw new ArgumentException("Registered type must have a param-less constructor");

			classMap[typeGuid] = typeClass;
			constructorMap[typeGuid] = constructor;
#endif
		}

		public static bool CreateTypedAsset(Guid typeGuid, out AssetItem result)
		{
#if NETSTANDARD1_3
			if (typeGuid == Geometry.TYPE_GUID)
				result = new Geometry();
			else if (typeGuid == Material.TYPE_GUID)
				result = new Material();
			else if (typeGuid == Metamesh.TYPE_GUID)
				result = new Metamesh();
			else if (typeGuid == MorphAnimation.TYPE_GUID)
				result = new MorphAnimation();
			else if (typeGuid == Particle.TYPE_GUID)
				result = new Particle();
			else if (typeGuid == PhysicsShape.TYPE_GUID)
				result = new PhysicsShape();
			else if (typeGuid == ProceduralVectorField.TYPE_GUID)
				result = new ProceduralVectorField();
			else if (typeGuid == Shader.TYPE_GUID)
				result = new Shader();
			else if (typeGuid == Skeleton.TYPE_GUID)
				result = new Skeleton();
			else if (typeGuid == SkeletalAnimation.TYPE_GUID)
				result = new SkeletalAnimation();
			else if (typeGuid == Texture.TYPE_GUID)
				result = new Texture();
			else
			{
				result = new AssetItem(typeGuid);
				return false;
			}

			return true;
#else
			if (classMap.ContainsKey(typeGuid))
			{
				var constructor = constructorMap[typeGuid];
				result = (AssetItem) constructor.Invoke(null);
				return true;
			}
			result = new AssetItem(typeGuid);
			return false;
#endif
		}
	}
}