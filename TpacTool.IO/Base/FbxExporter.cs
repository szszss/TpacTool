using System;
using System.IO;
using TpacTool.Lib;
#if FBX_SHARPIE
	using UkooLabs.FbxSharpie;
	using UkooLabs.FbxSharpie.Tokens;
	using UkooLabs.FbxSharpie.Tokens.Value;
#endif

namespace TpacTool.IO
{
	public class FbxExporter : AbstractModelExporter
	{
		private const int FBX_HEADER_VERSION = 1003;

		private const int FBX_VERSION = 7400;

		private DateTime _exportTime = DateTime.UtcNow;

		private string AnimationClipName
		{
			get => "Take 001";
		}

		public override string Extension => "fbx";

		public override bool SupportsSecondMaterial => false;

		public override bool SupportsSecondUv => false;

		public override bool SupportsSecondColor => false;

		public override bool SupportsSkeleton => true;

		public override bool SupportsMorph => true;

		public override bool SupportsSkeletalAnimation => true;

		public override bool SupportMorphAnimation => false;

		public override void Export(Stream writeStream)
		{
#if FBX_SHARPIE
			CheckStreamAndTexture(writeStream);

			FbxDocument document = new FbxDocument();

			document.Version = FbxVersion.v7_5;
			
			FbxBinaryWriter writer = new FbxBinaryWriter(writeStream);
			writer.Write(document);
#else
			throw new NotImplementedException("Fbx exporting is not supported in " + 
											TpacToolVersion.GetLibraryTargetPlatform());
#endif
		}

#if FBX_SHARPIE

		#region Header

		private FbxNode CreateFBXHeaderExtension()
		{
			FbxNode FBXHeaderExtension = new FbxNode(new IdentifierToken("FBXHeaderExtension"));
			{
				FbxNode FBXHeaderVersion = new FbxNode(new IdentifierToken("FBXHeaderVersion"));
				FBXHeaderVersion.AddProperty(new IntegerToken(FBX_HEADER_VERSION));
				FBXHeaderExtension.AddNode(FBXHeaderVersion);

				FbxNode FBXVersion = new FbxNode(new IdentifierToken("FBXVersion"));
				FBXVersion.AddProperty(new IntegerToken(FBX_VERSION));
				FBXHeaderExtension.AddNode(FBXVersion);

				FbxNode CreationTimeStamp = new FbxNode(new IdentifierToken("CreationTimeStamp"));
				{
					var time = _exportTime;

					FbxNode Version = new FbxNode(new IdentifierToken("Version"));
					Version.AddProperty(new IntegerToken(1000));
					CreationTimeStamp.AddNode(Version);

					FbxNode Year = new FbxNode(new IdentifierToken("Year"));
					Year.AddProperty(new IntegerToken(time.Year));
					CreationTimeStamp.AddNode(Year);

					FbxNode Month = new FbxNode(new IdentifierToken("Month"));
					Month.AddProperty(new IntegerToken(time.Month));
					CreationTimeStamp.AddNode(Month);

					FbxNode Day = new FbxNode(new IdentifierToken("Day"));
					Day.AddProperty(new IntegerToken(time.Day));
					CreationTimeStamp.AddNode(Day);

					FbxNode Hour = new FbxNode(new IdentifierToken("Hour"));
					Hour.AddProperty(new IntegerToken(time.Hour));
					CreationTimeStamp.AddNode(Hour);

					FbxNode Minute = new FbxNode(new IdentifierToken("Minute"));
					Minute.AddProperty(new IntegerToken(time.Minute));
					CreationTimeStamp.AddNode(Minute);

					FbxNode Second = new FbxNode(new IdentifierToken("Second"));
					Second.AddProperty(new IntegerToken(time.Second));
					CreationTimeStamp.AddNode(Second);

					FbxNode Millisecond = new FbxNode(new IdentifierToken("Millisecond"));
					Millisecond.AddProperty(new IntegerToken(time.Millisecond));
					CreationTimeStamp.AddNode(Millisecond);
				}
				FBXHeaderExtension.AddNode(CreationTimeStamp);

				FbxNode Creator = new FbxNode(new IdentifierToken("Creator"));
				Creator.AddProperty(new StringToken("TpacTool FBX Exporter"));
				FBXHeaderExtension.AddNode(Creator);

				FbxNode SceneInfo = new FbxNode(new IdentifierToken("SceneInfo"));
				SceneInfo.AddProperty(new StringToken("SceneInfo::GlobalInfo"));
				SceneInfo.AddProperty(new StringToken("UserData"));
				{
					FbxNode Type = new FbxNode(new IdentifierToken("Type"));
					Type.AddProperty(new StringToken("UserData"));
					SceneInfo.AddNode(Type);

					FbxNode Version = new FbxNode(new IdentifierToken("Version"));
					Version.AddProperty(new IntegerToken(100));
					SceneInfo.AddNode(Version);

					FbxNode MetaData = new FbxNode(new IdentifierToken("MetaData"));
					{
						FbxNode Version2 = new FbxNode(new IdentifierToken("Version"));
						Version2.AddProperty(new IntegerToken(100));
						MetaData.AddNode(Version2);

						FbxNode Title = new FbxNode(new IdentifierToken("Title"));
						Title.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Title);

						FbxNode Subject = new FbxNode(new IdentifierToken("Subject"));
						Subject.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Subject);

						FbxNode Author = new FbxNode(new IdentifierToken("Author"));
						Author.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Author);

						FbxNode Keywords = new FbxNode(new IdentifierToken("Keywords"));
						Keywords.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Keywords);

						FbxNode Revision = new FbxNode(new IdentifierToken("Revision"));
						Revision.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Revision);

						FbxNode Comment = new FbxNode(new IdentifierToken("Comment"));
						Comment.AddProperty(new StringToken(string.Empty));
						MetaData.AddNode(Comment);
					}
					SceneInfo.AddNode(MetaData);

					FbxNode Properties70 = new FbxNode(new IdentifierToken("Properties70"));
					{
						Properties70.AddNode(CreatePItem("DocumentUrl", "KString", "Url", null, null));
						Properties70.AddNode(CreatePItem("SrcDocumentUrl", "KString", "Url", null, null));
						Properties70.AddNode(CreatePItem("Original", "Compound", null, null));
						Properties70.AddNode(CreatePItem(
							"Original|ApplicationVendor", "KString", null, null, "TpacTool"));
						Properties70.AddNode(CreatePItem(
							"Original|ApplicationName", "KString", null, null, "TpacTool"));
						Properties70.AddNode(CreatePItem(
							"Original|ApplicationVersion", "KString", null, null, ""));
						Properties70.AddNode(CreatePItem(
							"Original|DateTime_GMT", "DateTime", null, null,
							_exportTime.ToString("dd/MM/yyyy HH:mm:ss.fff")));
						Properties70.AddNode(CreatePItem(
							"Original|FileName", "KString", null, null, ""));
						Properties70.AddNode(CreatePItem("LastSaved", "Compound", null, null));
						Properties70.AddNode(CreatePItem(
							"LastSaved|ApplicationVendor", "KString", null, null, "TpacTool"));
						Properties70.AddNode(CreatePItem(
							"LastSaved|ApplicationName", "KString", null, null, "TpacTool"));
						Properties70.AddNode(CreatePItem(
							"LastSaved|ApplicationVersion", "KString", null, null, ""));
						Properties70.AddNode(CreatePItem(
							"LastSaved|DateTime_GMT", "DateTime", null, null,
							_exportTime.ToString("dd/MM/yyyy HH:mm:ss.fff")));
						Properties70.AddNode(CreatePItem(
							"LastSaved|FileName", "KString", null, null, ""));
						Properties70.AddNode(CreatePItem(
							"Original|ApplicationActiveProject", "KString", null, null, ""));
					}
					SceneInfo.AddNode(Properties70);
				}
				FBXHeaderExtension.AddNode(SceneInfo);
			}
			return FBXHeaderExtension;
		}

		private FbxNode CreateGlobalSettings()
		{
			FbxNode GlobalSettings = new FbxNode(new IdentifierToken("GlobalSettings"));
			{
				GlobalSettings.AddNode(CreateNode("Version", 1000));

				FbxNode Properties70 = CreateNode("Properties70");
				{
					Properties70.AddNode(CreatePItem("UpAxis", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("UpAxisSign", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("FrontAxis", "int", "Integer", null, 2));
					Properties70.AddNode(CreatePItem("FrontAxisSign", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("CoordAxis", "int", "Integer", null, 0));
					Properties70.AddNode(CreatePItem("CoordAxisSign", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("OriginalUpAxis", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("OriginalUpAxisSign", "int", "Integer", null, 1));
					Properties70.AddNode(CreatePItem("UnitScaleFactor", "double", "Number", null, 1));
					Properties70.AddNode(CreatePItem("OriginalUnitScaleFactor", "double", "Number", null, 1));
					Properties70.AddNode(CreatePItem("AmbientColor", "ColorRGB", "Color", null, 0, 0, 0));
					Properties70.AddNode(CreatePItem("DefaultCamera", "KString", null, null, "Producer Perspective"));
					Properties70.AddNode(CreatePItem("TimeMode", "enum", null, null, 11));
					Properties70.AddNode(CreatePItem("TimeProtocol", "enum", null, null, 2));
					Properties70.AddNode(CreatePItem("SnapOnFrameMode", "enum", null, null, 0));
					Properties70.AddNode(CreatePItem("TimeSpanStart", "KTime", "Time", null, 0));
					Properties70.AddNode(CreatePItem("TimeSpanStop", "KTime", "Time", null, int.MaxValue));
					Properties70.AddNode(CreatePItem("CustomFrameRate", "double", "Number", null, -1));
					Properties70.AddNode(CreatePItem("TimeMarker", "Compound", null, null));
					Properties70.AddNode(CreatePItem("CurrentTimeMarker", "int", "Integer", null, -1));
				}
				GlobalSettings.AddNode(Properties70);
			}
			return GlobalSettings;
		}

		private FbxNode CreateDocuments()
		{
			FbxNode Documents = new FbxNode(new IdentifierToken("Documents"));
			{
				Documents.AddNode(CreateNode("Count", 1));

				FbxNode Document = CreateNode("Document");
				Document.AddProperty(new LongToken(GenerateGuid()));
				Document.AddProperty(new StringToken(""));
				Document.AddProperty(new StringToken("Scene"));
				{
					FbxNode Properties70 = CreateNode("Properties70");
					{
						Properties70.AddNode(CreatePItem("SourceObject", "object", null, null));
						Properties70.AddNode(CreatePItem(
							"ActiveAnimStackName", "KString", null, null, AnimationClipName));
					}
					Document.AddNode(Properties70);

					Documents.AddNode(CreateNode("RootNode", 0));
				}
				Documents.AddNode(Document);
			}
			return Documents;
		}

		private FbxNode CreateReferences()
		{
			FbxNode References = new FbxNode(new IdentifierToken("References"));
			return References;
		}

		private FbxNode CreateDefinitions()
		{
			FbxNode Definitions = new FbxNode(new IdentifierToken("Definitions"));
			{
				Definitions.AddNode(CreateNode("Version", 100));
				Definitions.AddNode(CreateNode("Count", 6));

				FbxNode GlobalSettings = CreateNode("ObjectType", "GlobalSettings");
				{
					GlobalSettings.AddNode(CreateNode("Count", 1));
				}
				Definitions.AddNode(GlobalSettings);

				FbxNode AnimationStack = CreateNode("ObjectType", "AnimationStack");
				{
					AnimationStack.AddNode(CreateNode("Count", 1));

					FbxNode PropertyTemplate = CreateNode("PropertyTemplate", "FbxAnimStack");
					{
						FbxNode Properties70 = CreateNode("Properties70");
						{
							Properties70.AddNode(CreatePItem("Description", "KString", null, null, null));
							Properties70.AddNode(CreatePItem("LocalStart", "KTime", "Time", null, 0));
							Properties70.AddNode(CreatePItem("LocalStop", "KTime", "Time", null, 0));
							Properties70.AddNode(CreatePItem("ReferenceStart", "KTime", "Time", null, 0));
							Properties70.AddNode(CreatePItem("ReferenceStop", "KTime", "Time", null, 0));
						}
						PropertyTemplate.AddNode(Properties70);
					}
					AnimationStack.AddNode(PropertyTemplate);
				}
				Definitions.AddNode(AnimationStack);

				FbxNode AnimationLayer = CreateNode("ObjectType", "AnimationLayer");
				{
					AnimationLayer.AddNode(CreateNode("Count", 1));

					FbxNode PropertyTemplate = CreateNode("PropertyTemplate", "FbxAnimLayer");
					{
						FbxNode Properties70 = CreateNode("Properties70");
						{
							Properties70.AddNode(CreatePItem("Weight", "Number", null, "A", 100));
							Properties70.AddNode(CreatePItem("Mute", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("Solo", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("Lock", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("Color", "ColorRGB", "Color", null, 0.8f, 0.8f, 0.8f));
							Properties70.AddNode(CreatePItem("BlendMode", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationAccumulationMode", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("ScaleAccumulationMode", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("BlendModeBypass", "ULongLong", null, null, 0));
						}
						PropertyTemplate.AddNode(Properties70);
					}
					AnimationLayer.AddNode(PropertyTemplate);
				}
				Definitions.AddNode(AnimationLayer);

				FbxNode Geometry = CreateNode("ObjectType", "Geometry");
				{
					Geometry.AddNode(CreateNode("Count", 1));

					FbxNode PropertyTemplate = CreateNode("PropertyTemplate", "FbxMesh");
					{
						FbxNode Properties70 = CreateNode("Properties70");
						{
							Properties70.AddNode(CreatePItem("Color", "ColorRGB", "Color", null, 0.8f, 0.8f, 0.8f));
							Properties70.AddNode(CreatePItem("BBoxMin", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("BBoxMax", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("Primary Visibility", "bool", null, null, 1));
							Properties70.AddNode(CreatePItem("Casts Shadows", "bool", null, null, 1));
							Properties70.AddNode(CreatePItem("Receive Shadows", "bool", null, null, 1));
						}
						PropertyTemplate.AddNode(Properties70);
					}
					Geometry.AddNode(PropertyTemplate);
				}
				Definitions.AddNode(Geometry);

				FbxNode Material = CreateNode("ObjectType", "Material");
				{
					Material.AddNode(CreateNode("Count", 1));

					FbxNode PropertyTemplate = CreateNode("PropertyTemplate", "FbxSurfaceLambert");
					{
						FbxNode Properties70 = CreateNode("Properties70");
						{
							Properties70.AddNode(CreatePItem("ShadingModel", "KString", null, null, "Lambert"));
							Properties70.AddNode(CreatePItem("MultiLayer", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("EmissiveColor", "Color", null, "A", 0, 0, 0));
							Properties70.AddNode(CreatePItem("EmissiveFactor", "Number", null, "A", 1));
							Properties70.AddNode(CreatePItem("AmbientColor", "Color", null, "A", 0.2f, 0.2f, 0.2f));
							Properties70.AddNode(CreatePItem("AmbientFactor", "Number", null, "A", 1));
							Properties70.AddNode(CreatePItem("DiffuseColor", "Color", null, "A", 0.8f, 0.8f, 0.8f));
							Properties70.AddNode(CreatePItem("DiffuseFactor", "Number", null, "A", 1));
							Properties70.AddNode(CreatePItem("Bump", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("NormalMap", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("BumpFactor", "double", "Number", null, 1));
							Properties70.AddNode(CreatePItem("TransparentColor", "Color", null, "A", 0, 0, 0));
							Properties70.AddNode(CreatePItem("TransparencyFactor", "Number", null, "A", 0));
							Properties70.AddNode(CreatePItem("DisplacementColor", "ColorRGB", "Color", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("DisplacementFactor", "double", "Number", null, 1));
							Properties70.AddNode(CreatePItem("VectorDisplacementColor", "ColorRGB", "Color", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("VectorDisplacementFactor", "double", "Number", null, 1));
						}
						PropertyTemplate.AddNode(Properties70);
					}
					Material.AddNode(PropertyTemplate);
				}
				Definitions.AddNode(Material);

				FbxNode Model = CreateNode("ObjectType", "Model");
				{
					Model.AddNode(CreateNode("Count", 1));

					FbxNode PropertyTemplate = CreateNode("PropertyTemplate", "FbxNode");
					{
						FbxNode Properties70 = CreateNode("Properties70");
						{
							Properties70.AddNode(CreatePItem("QuaternionInterpolate", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationOffset", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("RotationPivot", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("ScalingOffset", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("ScalingPivot", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("TranslationActive", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMin", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("TranslationMax", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("TranslationMinX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMinY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMinZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMaxX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMaxY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("TranslationMaxZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationOrder", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationSpaceForLimitOnly", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationStiffnessX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("RotationStiffnessY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("RotationStiffnessZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("AxisLen", "double", "Number", null, 10));
							Properties70.AddNode(CreatePItem("PreRotation", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("PostRotation", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("RotationActive", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMin", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("RotationMax", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("RotationMinX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMinY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMinZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMaxX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMaxY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("RotationMaxZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("InheritType", "enum", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingActive", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMin", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("ScalingMax", "Vector3D", "Vector", null, 1, 1, 1));
							Properties70.AddNode(CreatePItem("ScalingMinX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMinY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMinZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMaxX", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMaxY", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("ScalingMaxZ", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("GeometricTranslation", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("GeometricRotation", "Vector3D", "Vector", null, 0, 0, 0));
							Properties70.AddNode(CreatePItem("GeometricScaling", "Vector3D", "Vector", null, 1, 1, 1));
							Properties70.AddNode(CreatePItem("MinDampRangeX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MinDampRangeY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MinDampRangeZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampRangeX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampRangeY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampRangeZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MinDampStrengthX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MinDampStrengthY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MinDampStrengthZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampStrengthX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampStrengthY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("MaxDampStrengthZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("PreferedAngleX", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("PreferedAngleY", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("PreferedAngleZ", "double", "Number", null, 0));
							Properties70.AddNode(CreatePItem("LookAtProperty", "object", null, null));
							Properties70.AddNode(CreatePItem("UpVectorProperty", "object", null, null));
							Properties70.AddNode(CreatePItem("Show", "bool", null, null, 1));
							Properties70.AddNode(CreatePItem("NegativePercentShapeSupport", "bool", null, null, 1));
							Properties70.AddNode(CreatePItem("DefaultAttributeIndex", "int", "Integer", null, -1));
							Properties70.AddNode(CreatePItem("Freeze", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("LODBox", "bool", null, null, 0));
							Properties70.AddNode(CreatePItem("Lcl Translation", "Lcl Translation", null, "A", 0, 0, 0));
							Properties70.AddNode(CreatePItem("Lcl Rotation", "Lcl Rotation", null, "A", 0, 0, 0));
							Properties70.AddNode(CreatePItem("Lcl Scaling", "Lcl Scaling", null, "A", 1, 1, 1));
							Properties70.AddNode(CreatePItem("Visibility", "Visibility", null, "A", 1));
							Properties70.AddNode(CreatePItem("Visibility Inheritance", "Visibility Inheritance", null, null, 1));
						}
						PropertyTemplate.AddNode(Properties70);
					}
					Model.AddNode(PropertyTemplate);
				}
				Definitions.AddNode(Model);
			}
			return Definitions;
		}

		#endregion

		private static FbxNode CreateNode(string ident)
		{
			FbxNode node = new FbxNode(new IdentifierToken(ident));
			return node;
		}

		private static FbxNode CreateNode(string ident, int value)
		{
			FbxNode node = new FbxNode(new IdentifierToken(ident));
			node.Value = new IntegerToken(value);
			return node;
		}

		private static FbxNode CreateNode(string ident, string value)
		{
			FbxNode node = new FbxNode(new IdentifierToken(ident));
			node.Value = new StringToken(value);
			return node;
		}

		private static FbxNode CreatePItem(params object[] tokens)
		{
			FbxNode p = new FbxNode(new IdentifierToken("P"));
			foreach (var token in tokens)
			{
				Token t = null;
				if (token == null)
					t = new StringToken("");
				else if (token is string)
					t = new StringToken((string) token);
				else if (token is int)
					t = new IntegerToken((int) token);
				else if (token is long)
					t = new LongToken((long) token);
				else if (token is bool)
					t = new BooleanToken((bool) token);
				else if (token is float)
					t = new FloatToken((float) token);
				else
					throw new Exception("Unsupported P-Item param; " + token + " (Type: " + token.GetType() + ")");
				p.AddProperty(t);
			}
			return p;
		}

		private static long GenerateGuid()
		{
			return (long)Guid.NewGuid().GetHashCode() - (long)int.MinValue;
		}
#endif

		public static bool IsFbxSupported()
		{
#if FBX_SHARPIE
			/*FbxBinaryReader br = new FbxBinaryReader(File.OpenRead("C:\\Users\\szszss\\Desktop\\TEMP\\Mugetsu\\test3\\human_skeleton2.fbx"));
			var node = br.Read();

			using (var stream = File.OpenWrite("I:\\mbpose2.fbx"))
			{
				FbxDocument document = new FbxDocument();
				document.Version = FbxVersion.v7_4;
				
				FbxExporter exp = new FbxExporter();
				document.AddNode(exp.CreateFBXHeaderExtension());
				document.AddNode(exp.CreateGlobalSettings());
				document.AddNode(exp.CreateDocuments());
				document.AddNode(exp.CreateReferences());
				document.AddNode(exp.CreateDefinitions());
				//FBXHeaderExtension.AddProperty(Token.CreateAsterix());

				FbxBinaryWriter writer = new FbxBinaryWriter(stream);
				writer.Write(document);
			}*/
			return true;
#else
			return false;
#endif
		}
	}
}