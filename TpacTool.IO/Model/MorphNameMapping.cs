namespace TpacTool.IO
{
	public static class MorphNameMapping
	{
		private static string[] _headMapping = new[]
		{
			"Basis", // 0
			"FaceWidth",
			"FaceDepth",
			"FaceRatio",
			"Cheeks",
			"CheekboneHeight",
			"CheekboneWidth",
			"CheekboneDepth",
			"LipShapeTop",
			"NoseAngle",
			"CenterHeight", // 10
			"JawLine",
			"FaceSharpness",
			"TempleWidth",
			"EyeDepth",
			"EyeShape",
			"EyeToEyeDistance",
			"EyeSize",
			"EyelidHeight",
			"MonolidEyes",
			"EyeOuterCornerHeight", // 20
			"EyeInnerCornerHeight",
			"EyebrowDepth",
			"EyePosition",
			"BrowOuterHeight",
			"BrowMiddleHeight",
			"BrowInnerHeight",
			"NoseLength",
			"NoseBridge",
			"NoseTipHeight",
			"NoseSize", // 30
			"NoseWidth",
			"NostrilHeight",
			"NoseDefinition",
			"NostrilScale",
			"MouthWidth",
			"MouthPosition",
			"NoseBump",
			"ChinForward",
			"ChinShape",
			"ChinLength", // 40
			"LipsFrown",
			"LipThickness",
			"LipsForward",
			"LipShapeBottom",
			"NoseAsymetry",
			"HeadScaling",
			"FaceAsymetry",
			"EyeAsymetry",
			"JawHeight",
			"NeckSlope", // 50
			"HideEars",
			"EarShape",
			"EyeSocketSize",
			"NoseShape",
			"Mouth",
			"EarSize",
			"OldFace",
			"KidFace",
			"Eyebump", // face end
			"EyesRight", // 60
			"EyesLeft",
			"EyesUp",
			"EyesDown", // 63
			"EyebrowRaise",
			"Frown",
			"Scowl",
			"JawLeft",
			"JawRight",
			"JawBack",
			"JawForward", // 70
			"JawDrop",
			"JawDown",
			"JawUp",
			"WrinkleNose",
			"Unhappy",
			"Sip",
			"Displeased",
			"Pout",
			"PursedLips",
			"Dislike", // 80
			"Scorn",
			"Anger",
			"Quarrel",
			"Jeer",
			"Sneer",
			"CloseRightEye",
			"CloseLeftEye",
			"CloseEyes",
			"WideEyed",
			"Unknown", //90
			"Squint",
			"Unknown",
			"LipsRightUp",
			"LipsLeftUp",
			"Laugh",
			"Sad",
			"Smile",
			"Worry",
			"Speak",
			"Yell" // 100
		};

		public static string GetHumanHeadMorphName(int index)
		{
			if (index < 0 || index >= _headMapping.Length)
				return "KeyTime_" + index;
			return _headMapping[index] + "_" + index;
		}
	}
}