using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public static class TypedDataFactory
	{
		private static Dictionary<Guid, Type> guidToDataTypeMap = new Dictionary<Guid, Type>();
		private static Dictionary<Guid, ConstructorInfo> guidToDataConstructorMap = new Dictionary<Guid, ConstructorInfo>();
		private static Dictionary<Guid, Type> guidToLoaderTypeMap = new Dictionary<Guid, Type>();
		private static Dictionary<Guid, ConstructorInfo> guidToLoaderConstructorMap = new Dictionary<Guid, ConstructorInfo>();

		static TypedDataFactory()
		{
#if NETSTANDARD1_3
			// do nothing
#else
			RegisterType(typeof(MeshEditData));
			RegisterType(typeof(VertexStreamData));
			//RegisterType(typeof(ClothMappingData));
			RegisterType(typeof(EditmodeMiscData));
			RegisterType(typeof(TexturePixelData));
			RegisterType(typeof(TextureSourceInfoData));
			RegisterType(typeof(TextureImportSettingsData));
			RegisterType(typeof(PhysicsDescriptionData));
			RegisterType(typeof(PhysicsStaticCookData));
			RegisterType(typeof(PhysicsDynamicCookData));
			RegisterType(typeof(SkeletonDefinitionData));
			RegisterType(typeof(SkeletonUserData));
			RegisterType(typeof(MorphDefinitionData));
			RegisterType(typeof(AnimationDefinitionData));
			RegisterType(typeof(ParticleEffectData));
			RegisterType(typeof(OptimizedAnimation));
			RegisterType(typeof(ShortenedOptimizedAnimation));
			RegisterType(typeof(FbxSettingData));
			RegisterType(typeof(GeometrySettingData));
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
			Guid guid = (Guid)field.GetValue(null);
			RegisterType(guid, typeClass);
#endif
		}

		public static void RegisterType(Guid typeGuid, [NotNull] Type typeClass)
		{
#if NETSTANDARD1_3
			throw new NotImplementedException("Register custom type is unsupported in .net standard 1.3");
#else
			if (!typeof(ExternalData).IsAssignableFrom(typeClass))
				throw new ArgumentException("Registered type must extend from ExternalData");

			ConstructorInfo constructor = typeClass.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
				throw new ArgumentException("Registered type must have a param-less constructor");

			guidToDataTypeMap[typeGuid] = typeClass;
			guidToDataConstructorMap[typeGuid] = constructor;

			var loaderType = typeof(ExternalLoader<>).MakeGenericType(typeClass);
			guidToLoaderTypeMap[typeGuid] = loaderType;
			guidToLoaderConstructorMap[typeGuid] = loaderType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, 
				null, new[] { typeof(FileInfo) }, null);
#endif
		}

		public static bool CreateTypedData(Guid typeGuid, out ExternalData result)
		{
#if NETSTANDARD1_3
			if (typeGuid == MeshEditData.TYPE_GUID)
				result = new MeshEditData();
			if (typeGuid == VertexStreamData.TYPE_GUID)
				result = new VertexStreamData();
			if (typeGuid == EditmodeMiscData.TYPE_GUID)
				result = new EditmodeMiscData();
			if (typeGuid == TexturePixelData.TYPE_GUID)
				result = new TexturePixelData();
			if (typeGuid == TextureSourceInfoData.TYPE_GUID)
				result = new TextureSourceInfoData();
			if (typeGuid == TextureImportSettingsData.TYPE_GUID)
				result = new TextureImportSettingsData();
			if (typeGuid == PhysicsDescriptionData.TYPE_GUID)
				result = new PhysicsDescriptionData();
			if (typeGuid == PhysicsStaticCookData.TYPE_GUID)
				result = new PhysicsStaticCookData();
			if (typeGuid == PhysicsDynamicCookData.TYPE_GUID)
				result = new PhysicsDynamicCookData();
			if (typeGuid == SkeletonDefinitionData.TYPE_GUID)
				result = new SkeletonDefinitionData();
			if (typeGuid == SkeletonUserData.TYPE_GUID)
				result = new SkeletonUserData();
			if (typeGuid == MorphDefinitionData.TYPE_GUID)
				result = new MorphDefinitionData();
			if (typeGuid == AnimationDefinitionData.TYPE_GUID)
				result = new AnimationDefinitionData();
			if (typeGuid == ParticleEffectData.TYPE_GUID)
				result = new ParticleEffectData();
			if (typeGuid == OptimizedAnimation.TYPE_GUID)
				result = new OptimizedAnimation();
			if (typeGuid == ShortenedOptimizedAnimation.TYPE_GUID)
				result = new ShortenedOptimizedAnimation();
			if (typeGuid == FbxSettingData.TYPE_GUID)
				result = new FbxSettingData();
			if (typeGuid == GeometrySettingData.TYPE_GUID)
				result = new GeometrySettingData();
			else
			{
				result = new ExternalData(typeGuid);
				return false;
			}

			return true;
#else
			if (guidToDataTypeMap.ContainsKey(typeGuid))
			{
				var constructor = guidToDataConstructorMap[typeGuid];
				result = (ExternalData)constructor.Invoke(null);
				return true;
			}
			result = new ExternalData(typeGuid);
			return false;
#endif
		}

		public static bool CreateTypedLoader(Guid typeGuid, FileInfo file, out AbstractExternalLoader result)
		{
#if NETSTANDARD1_3
			AbstractExternalLoader loader = null;
			bool isFound = true;
			if (typeGuid == PhysicsDescriptionData.TYPE_GUID)
				loader = new ExternalLoader<PhysicsDescriptionData>(file);
			else
			{
				loader = new ExternalLoader<ExternalData>(file);
				isFound = false;
			}
			loader.TypeGuid = typeGuid;
			result = loader;

			return isFound;
#else
			AbstractExternalLoader loader = null;
			bool isFound = false;
			if (guidToLoaderTypeMap.ContainsKey(typeGuid))
			{
				var constructor = guidToLoaderConstructorMap[typeGuid];
				loader = (AbstractExternalLoader)constructor.Invoke(new [] {file});
				isFound = true;
			}
			else
			{
				loader = new ExternalLoader<ExternalData>(file);
			}
			loader.TypeGuid = typeGuid;
			result = loader;
			return isFound;
#endif
		}
	}
}