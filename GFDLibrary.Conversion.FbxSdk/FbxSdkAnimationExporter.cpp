#include "pch.h"

#include "FbxSdkAnimationExporter.h"
#include "Utf8String.h"

/*
	FBX ANIMATION EXPORTER NOTES:
	- Exports a skeleton-only FBX (FbxNode tree with FbxSkeleton::eLimbNode attributes)
	- TargetKind.Node controllers are converted (PRS keys)
	- TargetKind.Morph / MorphIndexed controllers are converted (SingleKey -> blend shape weight curves)
	- Morph controllers are SHARED across all meshes on the same node: TargetId directly
	  indexes into each mesh's own blend shape channels (not split across meshes).
*/

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace System::Numerics;

namespace GFDLibrary::Conversion::FbxSdk
{
	using namespace Models;
	using namespace Animations;
	using namespace Conversion;

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
		mMorphTargetMeshLookup = gcnew Dictionary<String^, IntPtr>();


	}

	FbxSdkAnimationExporter::~FbxSdkAnimationExporter()
	{
		mFbxManager->Destroy();
	}

	void FbxSdkAnimationExporter::Reset()
	{
		mNameToFbxNodeLookup->Clear();
		mMorphTargetMeshLookup->Clear();

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
		for each (auto child in skeleton->RootNode->Children)
			BuildSkeletonRecursive(mFbxScene->GetRootNode(), child);
	}

	void FbxSdkAnimationExporter::BuildMorphTargetMeshes(Model^ model)
	{
		// Create a placeholder mesh node per mesh attachment that has morph targets.
		// Each placeholder is named EXACTLY like the model FBX mesh node so 3ds Max
		// matches animation curves to the correct Morpher modifier when merging.
		for each (Node^ node in model->Nodes)
		{
			if (!node->HasAttachments)
				continue;

			List<Mesh^>^ morphMeshes = gcnew List<Mesh^>();
			List<int>^ morphMeshTypeIndices = gcnew List<int>();
			int meshCount = 0;
			for each (NodeAttachment^ attachment in node->Attachments)
			{
				if (attachment->Type == NodeAttachmentType::Mesh)
				{
					auto mesh = safe_cast<Mesh^>(attachment->GetValue());
					if (mesh->MorphTargets != nullptr && mesh->MorphTargets->Count > 0)
					{
						morphMeshes->Add(mesh);
						morphMeshTypeIndices->Add(meshCount);
					}
					meshCount++;
				}
			}

			if (morphMeshes->Count == 0)
				continue;

			IntPtr boneFbxNodePtr;
			FbxNode* fbxBoneNode = nullptr;
			if (mNameToFbxNodeLookup->TryGetValue(node->Name, boneFbxNodePtr))
				fbxBoneNode = (FbxNode*)boneFbxNodePtr.ToPointer();

			auto countKey = String::Format("{0}_count", node->Name);
			mMorphTargetMeshLookup[countKey] = IntPtr(morphMeshes->Count);

			Trace::TraceInformation(String::Format("FbxSdkAnimationExporter: node '{0}' has {1} morph mesh(es)",
				node->Name, morphMeshes->Count));

			for (int m = 0; m < morphMeshes->Count; m++)
			{
				auto mesh = morphMeshes[m];
				int typeIndex = morphMeshTypeIndices[m];
				auto meshExportName = ModelConversionHelpers::GetMeshExportName(node->Name, typeIndex);

				auto fbxMeshNode = FbxNode::Create(mFbxScene, Utf8String(meshExportName).ToCStr());
				if (fbxBoneNode != nullptr)
					fbxBoneNode->AddChild(fbxMeshNode);
				else
					mFbxScene->GetRootNode()->AddChild(fbxMeshNode);

				auto lookupKey = String::Format("{0}_mesh{1}", node->Name, m);
				mMorphTargetMeshLookup[lookupKey] = (IntPtr)fbxMeshNode;

				auto fbxMesh = FbxMesh::Create(mFbxScene, "");
				fbxMeshNode->SetNodeAttribute(fbxMesh);
				fbxMesh->InitControlPoints(1);
				fbxMesh->SetControlPointAt(FbxVector4(0, 0, 0), 0);

				auto fbxBlendShape = FbxBlendShape::Create(mFbxScene, "");
				for (int t = 0; t < mesh->MorphTargets->Count; t++)
				{
					auto channelName = Utf8String(
						String::Format("{0}_MorphTarget{1}", meshExportName, t));
					auto fbxChannel = FbxBlendShapeChannel::Create(mFbxScene, channelName.ToCStr());
					fbxBlendShape->AddBlendShapeChannel(fbxChannel);
				}
				fbxMesh->AddDeformer(fbxBlendShape);
			}
		}
	}

	void FbxSdkAnimationExporter::AddPRSKeysToCurves(FbxNode* fbxNode, FbxAnimLayer* fbxAnimLayer, AnimationLayer^ layer, bool alignEulers)
	{
		FbxAnimCurve* tx = nullptr;
		FbxAnimCurve* ty = nullptr;
		FbxAnimCurve* tz = nullptr;
		FbxAnimCurve* rx = nullptr;
		FbxAnimCurve* ry = nullptr;
		FbxAnimCurve* rz = nullptr;
		FbxAnimCurve* sx = nullptr;
		FbxAnimCurve* sy = nullptr;
		FbxAnimCurve* sz = nullptr;

		double prevEx = 0.0, prevEy = 0.0, prevEz = 0.0;
		bool hasFirstRotationKey = false;

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

				FbxAMatrix m;
				m.SetQ(ConvertToFbxQuaternion(prsKey->Rotation));
				FbxVector4 euler = m.GetR();
				float ex = (float)euler[0];
				float ey = (float)euler[1];
				float ez = (float)euler[2];

				// taken from https://github.com/Pherakki/BlenderToolsForGFS/blob/develop/src/BlenderIO/modelUtilsTest/Skeleton/Transform/Animation/Transform.py#L55
				// unwraps euler angles to prevent huge jumps i.e. 180 -> -180
				// this fixes interpolation bug in softwares like 3ds max
				if (alignEulers)
				{
					if (hasFirstRotationKey)
					{
						double shiftX = 360.0 * round((prevEx - ex) / 360.0);
						double shiftY = 360.0 * round((prevEy - ey) / 360.0);
						double shiftZ = 360.0 * round((prevEz - ez) / 360.0);
						ex = (float)(ex + shiftX);
						ey = (float)(ey + shiftY);
						ez = (float)(ez + shiftZ);
					}

					prevEx = ex;
					prevEy = ey;
					prevEz = ez;
					hasFirstRotationKey = true;
				}

				int kx = rx->KeyAdd(time); rx->KeySetValue(kx, ex); rx->KeySetInterpolation(kx, FbxAnimCurveDef::eInterpolationLinear);
				int ky = ry->KeyAdd(time); ry->KeySetValue(ky, ey); ry->KeySetInterpolation(ky, FbxAnimCurveDef::eInterpolationLinear);
				int kz = rz->KeyAdd(time); rz->KeySetValue(kz, ez); rz->KeySetInterpolation(kz, FbxAnimCurveDef::eInterpolationLinear);
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

	void FbxSdkAnimationExporter::AddMorphKeysToCurves(
		FbxBlendShapeChannel* fbxChannel,
		FbxAnimLayer* fbxAnimLayer,
		AnimationLayer^ layer)
	{
		if (!layer->HasSingleKeyFrames)
			return;

		auto curve = fbxChannel->DeformPercent.GetCurve(fbxAnimLayer, true);
		if (curve == nullptr)
			return;

		curve->KeyModifyBegin();

		for each (Key^ baseKey in layer->Keys)
		{
			auto singleKey = dynamic_cast<SingleKey^>(baseKey);
			if (singleKey == nullptr)
				continue;

			FbxTime time;
			time.SetSecondDouble(singleKey->Time);

			float weightPercent = singleKey->Value * 100.0f;

			int ki = curve->KeyAdd(time);
			curve->KeySetValue(ki, weightPercent);
			curve->KeySetInterpolation(ki, FbxAnimCurveDef::eInterpolationLinear);
		}

		curve->KeyModifyEnd();
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

		// Export node (PRS) controllers
		for each (AnimationController^ controller in animation->Controllers)
		{
			if (controller->TargetKind != TargetKind::Node)
				continue;

			IntPtr fbxNodePtr;
			if (!mNameToFbxNodeLookup->TryGetValue(controller->TargetName, fbxNodePtr))
			{
				Trace::TraceWarning(String::Format("FbxSdkAnimationExporter: target bone '{0}' not found in skeleton, skipping controller", controller->TargetName));
				continue;
			}

			auto fbxNode = (FbxNode*)fbxNodePtr.ToPointer();

			for each (AnimationLayer^ layer in controller->Layers)
			{
				AddPRSKeysToCurves(fbxNode, fbxAnimLayer, layer, mConfig->AlignEulers);
			}
		}

		// Export morph controllers
		// Morph controllers are SHARED across all meshes on the same node.
		// TargetId directly indexes into each mesh's own blend shape channels.
		// Apply the curve to EVERY mesh that has this channel index.
		for each (AnimationController^ controller in animation->Controllers)
		{
			if (controller->TargetKind != TargetKind::Morph &&
				controller->TargetKind != TargetKind::MorphIndexed)
				continue;

			IntPtr countPtr;
			int mMeshCount = 0;
			auto countKey = String::Format("{0}_count", controller->TargetName);
			if (mMorphTargetMeshLookup->TryGetValue(countKey, countPtr))
				mMeshCount = countPtr.ToInt32();

			if (mMeshCount == 0)
			{
				Trace::TraceWarning(
					String::Format("FbxSdkAnimationExporter: morph target mesh '{0}' not found, skipping controller",
						controller->TargetName));
				continue;
			}

			int channelIndex = controller->TargetId;

			for (int m = 0; m < mMeshCount; m++)
			{
				auto meshKey = String::Format("{0}_mesh{1}", controller->TargetName, m);
				IntPtr fbxMeshNodePtr;
				if (!mMorphTargetMeshLookup->TryGetValue(meshKey, fbxMeshNodePtr))
					continue;

				auto fbxMeshNode = (FbxNode*)fbxMeshNodePtr.ToPointer();
				auto fbxMesh = fbxMeshNode->GetMesh();
				if (fbxMesh == nullptr)
					continue;

				FbxBlendShape* fbxBlendShape = nullptr;
				int deformerCount = fbxMesh->GetDeformerCount();
				for (int d = 0; d < deformerCount; d++)
				{
					auto deformer = fbxMesh->GetDeformer(d);
					if (deformer->GetDeformerType() == FbxDeformer::EDeformerType::eBlendShape)
					{
						fbxBlendShape = static_cast<FbxBlendShape*>(deformer);
						break;
					}
				}

				if (fbxBlendShape == nullptr)
					continue;

				if (channelIndex < 0 || channelIndex >= fbxBlendShape->GetBlendShapeChannelCount())
					continue;

				auto fbxChannel = fbxBlendShape->GetBlendShapeChannel(channelIndex);

				for each (AnimationLayer^ layer in controller->Layers)
				{
					AddMorphKeysToCurves(fbxChannel, fbxAnimLayer, layer);
				}
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
		fIos->SetBoolProp(EXP_FBX_SHAPE, true);
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
		BuildMorphTargetMeshes(skeleton);
		BuildAnimation(animation, String::IsNullOrEmpty(animationName) ? "Animation 0" : animationName);
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
