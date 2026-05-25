#include "pch.h"

#include "FbxSdkAnimationExporter.h"
#include "Utf8String.h"

/*
	FBX ANIMATION EXPORTER NOTES:
	- Exports a skeleton-only FBX (FbxNode tree with FbxSkeleton::eLimbNode attributes)
	- Only TargetKind.Node controllers are converted
*/

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace System::Numerics;

namespace GFDLibrary::Conversion::FbxSdk
{
	using namespace Models;
	using namespace Animations;

	static FbxDouble3 ConvertToFbxDouble3(Vector3 value)
	{
		return FbxDouble3(value.X, value.Y, value.Z);
	}

	static FbxQuaternion ConvertToFbxQuaternion(Quaternion value)
	{
		return FbxQuaternion(value.X, value.Y, value.Z, value.W);
	}

	FbxSdkAnimationExporter::FbxSdkAnimationExporter()
	{
		mFbxManager = FbxManager::Create();
		if (!mFbxManager)
			throw gcnew FbxSdkAnimationExporterException("Failed to create FBX Manager");

		mNameToFbxNodeLookup = gcnew Dictionary<String^, IntPtr>();
	}

	FbxSdkAnimationExporter::~FbxSdkAnimationExporter()
	{
		mFbxManager->Destroy();
	}

	void FbxSdkAnimationExporter::Reset()
	{
		mNameToFbxNodeLookup->Clear();
	}

	void FbxSdkAnimationExporter::BuildSkeletonRecursive(FbxNode* fbxParentNode, Node^ node)
	{
		auto fbxNode = FbxNode::Create(mFbxScene, Utf8String(node->Name).ToCStr());

		// Local transform from GFD node
		FbxAMatrix m;
		m.SetQ(ConvertToFbxQuaternion(node->Rotation));
		fbxNode->LclRotation.Set(m.GetR());
		fbxNode->LclScaling.Set(ConvertToFbxDouble3(node->Scale));
		fbxNode->LclTranslation.Set(ConvertToFbxDouble3(node->Translation));
		fbxNode->SetPreferedAngle(fbxNode->LclRotation.Get());

		auto fbxSkeleton = FbxSkeleton::Create(mFbxScene, "");
		fbxSkeleton->SetSkeletonType(FbxSkeleton::EType::eLimbNode);
		fbxNode->SetNodeAttribute(fbxSkeleton);

		fbxParentNode->AddChild(fbxNode);

		// If two GFD nodes share a name, use first instance and log a warning
		if (!mNameToFbxNodeLookup->ContainsKey(node->Name))
			mNameToFbxNodeLookup->Add(node->Name, (IntPtr)fbxNode);
		else
			Trace::TraceWarning(String::Format("FbxSdkAnimationExporter: duplicate node name '{0}', animation lookup will use the first occurrence", node->Name));

		if (node->HasChildren)
		{
			for each (auto childNode in node->Children)
				BuildSkeletonRecursive(fbxNode, childNode);
		}
	}

	void FbxSdkAnimationExporter::BuildSkeleton(Model^ skeleton)
	{
		// Skip the GFD root node itself
		for each (auto child in skeleton->RootNode->Children)
			BuildSkeletonRecursive(mFbxScene->GetRootNode(), child);
	}

	void FbxSdkAnimationExporter::AddPRSKeysToCurves(FbxNode* fbxNode, FbxAnimLayer* fbxAnimLayer, AnimationLayer^ layer)
	{
		// Resolve curve nodes / per-axis curves
		FbxAnimCurve* tx = nullptr;
		FbxAnimCurve* ty = nullptr;
		FbxAnimCurve* tz = nullptr;
		FbxAnimCurve* rx = nullptr;
		FbxAnimCurve* ry = nullptr;
		FbxAnimCurve* rz = nullptr;
		FbxAnimCurve* sx = nullptr;
		FbxAnimCurve* sy = nullptr;
		FbxAnimCurve* sz = nullptr;

		auto positionScale = layer->PositionScale;
		auto scaleScale = layer->ScaleScale;

		for each (Key^ baseKey in layer->Keys)
		{
			auto prsKey = dynamic_cast<PRSKey^>(baseKey);
			if (prsKey == nullptr)
				continue;

			FbxTime time;
			time.SetSecondDouble(prsKey->Time);

			if (prsKey->HasPosition)
			{
				if (!tx)
				{
					tx = fbxNode->LclTranslation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					ty = fbxNode->LclTranslation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					tz = fbxNode->LclTranslation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
					tx->KeyModifyBegin();
					ty->KeyModifyBegin();
					tz->KeyModifyBegin();
				}

				auto pos = prsKey->Position;
				auto px = pos.X * positionScale.X;
				auto py = pos.Y * positionScale.Y;
				auto pz = pos.Z * positionScale.Z;

				int kx = tx->KeyAdd(time); tx->KeySetValue(kx, (float)px); tx->KeySetInterpolation(kx, FbxAnimCurveDef::eInterpolationLinear);
				int ky = ty->KeyAdd(time); ty->KeySetValue(ky, (float)py); ty->KeySetInterpolation(ky, FbxAnimCurveDef::eInterpolationLinear);
				int kz = tz->KeyAdd(time); tz->KeySetValue(kz, (float)pz); tz->KeySetInterpolation(kz, FbxAnimCurveDef::eInterpolationLinear);
			}

			if (prsKey->HasRotation)
			{
				if (!rx)
				{
					rx = fbxNode->LclRotation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					ry = fbxNode->LclRotation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					rz = fbxNode->LclRotation.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
					rx->KeyModifyBegin();
					ry->KeyModifyBegin();
					rz->KeyModifyBegin();
				}

				// Quaternion -> Euler degrees (XYZ order, FBX default).
				FbxAMatrix m;
				m.SetQ(ConvertToFbxQuaternion(prsKey->Rotation));
				FbxVector4 euler = m.GetR();

				int kx = rx->KeyAdd(time); rx->KeySetValue(kx, (float)euler[0]); rx->KeySetInterpolation(kx, FbxAnimCurveDef::eInterpolationLinear);
				int ky = ry->KeyAdd(time); ry->KeySetValue(ky, (float)euler[1]); ry->KeySetInterpolation(ky, FbxAnimCurveDef::eInterpolationLinear);
				int kz = rz->KeyAdd(time); rz->KeySetValue(kz, (float)euler[2]); rz->KeySetInterpolation(kz, FbxAnimCurveDef::eInterpolationLinear);
			}

			if (prsKey->HasScale)
			{
				if (!sx)
				{
					sx = fbxNode->LclScaling.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					sy = fbxNode->LclScaling.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					sz = fbxNode->LclScaling.GetCurve(fbxAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
					sx->KeyModifyBegin();
					sy->KeyModifyBegin();
					sz->KeyModifyBegin();
				}

				auto sc = prsKey->Scale;
				auto sxv = sc.X * scaleScale.X;
				auto syv = sc.Y * scaleScale.Y;
				auto szv = sc.Z * scaleScale.Z;

				int kx = sx->KeyAdd(time); sx->KeySetValue(kx, (float)sxv); sx->KeySetInterpolation(kx, FbxAnimCurveDef::eInterpolationLinear);
				int ky = sy->KeyAdd(time); sy->KeySetValue(ky, (float)syv); sy->KeySetInterpolation(ky, FbxAnimCurveDef::eInterpolationLinear);
				int kz = sz->KeyAdd(time); sz->KeySetValue(kz, (float)szv); sz->KeySetInterpolation(kz, FbxAnimCurveDef::eInterpolationLinear);
			}
		}

		if (tx) { tx->KeyModifyEnd(); ty->KeyModifyEnd(); tz->KeyModifyEnd(); }
		if (rx) { rx->KeyModifyEnd(); ry->KeyModifyEnd(); rz->KeyModifyEnd(); }
		if (sx) { sx->KeyModifyEnd(); sy->KeyModifyEnd(); sz->KeyModifyEnd(); }
	}

	void FbxSdkAnimationExporter::BuildAnimation(Animation^ animation, String^ animationName)
	{
		auto stackName = Utf8String(animationName);
		auto fbxAnimStack = FbxAnimStack::Create(mFbxScene, stackName.ToCStr());

		FbxTime start; start.SetSecondDouble(0.0);
		FbxTime stop; stop.SetSecondDouble(animation->Duration);
		FbxTimeSpan span(start, stop);
		fbxAnimStack->SetLocalTimeSpan(span);

		auto fbxAnimLayer = FbxAnimLayer::Create(mFbxScene, "Base Layer");
		fbxAnimStack->AddMember(fbxAnimLayer);

		for each (AnimationController^ controller in animation->Controllers)
		{
			if (controller->TargetKind != TargetKind::Node)
			{
				Trace::TraceWarning(String::Format("FbxSdkAnimationExporter: skipping controller with unsupported target kind {0} on '{1}'", controller->TargetKind.ToString(), controller->TargetName));
				continue;
			}

			IntPtr fbxNodePtr;
			if (!mNameToFbxNodeLookup->TryGetValue(controller->TargetName, fbxNodePtr))
			{
				Trace::TraceWarning(String::Format("FbxSdkAnimationExporter: target bone '{0}' not found in skeleton, skipping controller", controller->TargetName));
				continue;
			}

			auto fbxNode = (FbxNode*)fbxNodePtr.ToPointer();

			for each (AnimationLayer^ layer in controller->Layers)
			{
				// only convert PRS key types, skip keys like Material animation keys
				AddPRSKeysToCurves(fbxNode, fbxAnimLayer, layer);
			}
		}
	}

	void FbxSdkAnimationExporter::ExportFbxScene(String^ path)
	{
		auto fbxExporter = FbxExporter::Create(mFbxManager, "");
		if (!fbxExporter->SetFileExportVersion(FBX_2014_00_COMPATIBLE))
			throw gcnew FbxSdkAnimationExporterException("Failed to set FBX export version");

		const bool isAsciiFbx = path->EndsWith(".ascii.fbx", StringComparison::OrdinalIgnoreCase);
		int pFileFormat = -1;
		if (isAsciiFbx)
		{
			int lFormatIndex, lFormatCount = mFbxManager->GetIOPluginRegistry()->GetWriterFormatCount();
			for (lFormatIndex = 0; lFormatIndex < lFormatCount; lFormatIndex++)
			{
				if (mFbxManager->GetIOPluginRegistry()->WriterIsFBX(lFormatIndex))
				{
					FbxString lDesc = mFbxManager->GetIOPluginRegistry()->GetWriterFormatDescription(lFormatIndex);
					if (lDesc.Find("ascii") >= 0)
					{
						pFileFormat = lFormatIndex;
						break;
					}
				}
			}
			if (pFileFormat == -1)
				throw gcnew FbxSdkAnimationExporterException("Failed to find FBX ASCII export format.");
		}

		if (!fbxExporter->Initialize(Utf8String(path).ToCStr(), pFileFormat, mFbxManager->GetIOSettings()))
		{
			auto errorMsg = gcnew String(fbxExporter->GetStatus().GetErrorString());
			throw gcnew FbxSdkAnimationExporterException("Failed to initialize FBX exporter: " + errorMsg);
		}

		fbxExporter->Export(mFbxScene);
		fbxExporter->Destroy();
	}

	void FbxSdkAnimationExporter::Export(Animation^ animation, Model^ skeleton, String^ animationName, String^ path, FbxSdkAnimationExporterConfig^ config)
	{
		Reset();
		mConfig = config;

		if (animation == nullptr)
			throw gcnew FbxSdkAnimationExporterException("Animation is null");
		if (skeleton == nullptr || skeleton->RootNode == nullptr)
			throw gcnew FbxSdkAnimationExporterException("Skeleton model is null or has no root node");

		auto fIos = FbxIOSettings::Create(mFbxManager, IOSROOT);
		fIos->SetBoolProp(EXP_FBX_MATERIAL, false);
		fIos->SetBoolProp(EXP_FBX_TEXTURE, false);
		fIos->SetBoolProp(EXP_FBX_EMBEDDED, false);
		fIos->SetBoolProp(EXP_FBX_SHAPE, false);
		fIos->SetBoolProp(EXP_FBX_GOBO, false);
		fIos->SetBoolProp(EXP_FBX_ANIMATION, true);
		fIos->SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);
		mFbxManager->SetIOSettings(fIos);

		mFbxScene = FbxScene::Create(mFbxManager, "");
		if (!mFbxScene)
			throw gcnew FbxSdkAnimationExporterException("Failed to create FBX scene");

		auto& fbxGlobalSettings = mFbxScene->GetGlobalSettings();
		fbxGlobalSettings.SetAxisSystem(FbxAxisSystem::DirectX);
		fbxGlobalSettings.SetSystemUnit(FbxSystemUnit::m);

		BuildSkeleton(skeleton);
		BuildAnimation(animation, String::IsNullOrEmpty(animationName) ? "Take 001" : animationName);
		ExportFbxScene(path);
	}

	void FbxSdkAnimationExporter::ExportFile(Animation^ animation, Model^ skeleton, String^ path, FbxSdkAnimationExporterConfig^ config)
	{
		ExportFile(animation, skeleton, nullptr, path, config);
	}

	void FbxSdkAnimationExporter::ExportFile(Animation^ animation, Model^ skeleton, String^ animationName, String^ path, FbxSdkAnimationExporterConfig^ config)
	{
		auto exp = gcnew FbxSdkAnimationExporter();
		exp->Export(animation, skeleton, animationName, path, config);
	}
}
