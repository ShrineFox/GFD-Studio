#pragma once
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Numerics;
namespace GFDLibrary::Conversion::FbxSdk
{
	using namespace Models;
	using namespace Animations;
	public ref class FbxSdkAnimationExporterConfig
	{
	public:
		inline FbxSdkAnimationExporterConfig()
		{
		}
	};
	public ref class FbxSdkAnimationExporterException : public Exception
	{
	public:
		inline FbxSdkAnimationExporterException(String^ message) : Exception(message) {}
	};
	public ref class FbxSdkAnimationExporter sealed
	{
	public:
		FbxSdkAnimationExporter();
		~FbxSdkAnimationExporter();
		static void ExportFile(Animation^ animation, Model^ skeleton, String^ path, FbxSdkAnimationExporterConfig^ config);
		static void ExportFile(Animation^ animation, Model^ skeleton, String^ animationName, String^ path, FbxSdkAnimationExporterConfig^ config);
		void Export(Animation^ animation, Model^ skeleton, String^ animationName, String^ path, FbxSdkAnimationExporterConfig^ config);

		static void AddPRSKeysToCurves(FbxNode* fbxNode, FbxAnimLayer* fbxAnimLayer, AnimationLayer^ layer);
		static void AddMorphKeysToCurves(FbxBlendShapeChannel* fbxChannel, FbxAnimLayer* fbxAnimLayer, AnimationLayer^ layer);

	private:
		void Reset();
		void BuildSkeleton(Model^ skeleton);
		void BuildSkeletonRecursive(FbxNode* fbxParentNode, Node^ node);
		void BuildMorphTargetMeshes(Model^ model);
		void BuildAnimation(Animation^ animation, String^ animationName);
		void ExportFbxScene(String^ path);
		FbxManager* mFbxManager;
		FbxScene* mFbxScene;
		Dictionary<String^, IntPtr>^ mNameToFbxNodeLookup;
		Dictionary<String^, IntPtr>^ mMorphTargetMeshLookup;

		FbxSdkAnimationExporterConfig^ mConfig;
	};
}
