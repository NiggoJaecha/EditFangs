using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker.UI;
using UniRx;
using KKAPI.Maker;

namespace EditFangs
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class EditFangsPlugin : BaseUnityPlugin
    {
#if KK
        public const string PluginName = "KK_EditFangs";
#elif KKS
        public const string PluginName = "KKS_EditFangs";
#endif
        public const string GUID = "org.njaecha.plugins.editfangs";
        public const string Version = "1.1.2";

        internal new static ManualLogSource Logger;
        internal static EditFangsPlugin Instance;

        void Awake()
        {
            EditFangsPlugin.Logger = base.Logger;
            EditFangsPlugin.Instance = this;
            CharacterApi.RegisterExtraBehaviour<EditFangsController>(GUID);
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
        }

        private static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Left Fang Length", 0f, 1f, 0.1f, EditFangsPlugin.Instance))
             .BindToFunctionController<EditFangsController, float>(
                 controller => controller.fangData.scaleL, 
                 (controller, value) => controller.adjustFang(value, controller.fangData.spacingL, controller.fangData.scaleR, controller.fangData.spacingR));

            e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Left Fang Spacing", 0f, 1.3f, 1f, EditFangsPlugin.Instance))
             .BindToFunctionController<EditFangsController, float>(
                 controller => controller.fangData.spacingL, 
                 (controller, value) => controller.adjustFang(controller.fangData.scaleL, value, controller.fangData.scaleR, controller.fangData.spacingR, true));
            
            e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Right Fang Length", 0f, 1f, 0.1f, EditFangsPlugin.Instance))
             .BindToFunctionController<EditFangsController, float>(
                 controller => controller.fangData.scaleR, 
                 (controller, value) => controller.adjustFang(controller.fangData.scaleL, controller.fangData.spacingL, value, controller.fangData.spacingR));
            
            e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Right Fang Spacing", 0f, 1.3f, 1f, EditFangsPlugin.Instance))
             .BindToFunctionController<EditFangsController, float>(
                 controller => controller.fangData.spacingR, 
                 (controller, value) => controller.adjustFang(controller.fangData.scaleL, controller.fangData.spacingL, controller.fangData.scaleR, value, true));

            Singleton<ChaCustom.CustomBase>.Instance.actUpdateCvsMouth += registerFangs;
        }

        private static void registerFangs()
        {
            MakerAPI.GetCharacterControl().GetComponent<EditFangsController>().registerFangs();
        }
    }
}
