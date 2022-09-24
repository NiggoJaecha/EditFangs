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
    public class EditFangs : BaseUnityPlugin
    {
#if KK
        public const string PluginName = "KK_EditFangs";
#elif KKS
        public const string PluginName = "KKS_EditFangs";
#endif
        public const string GUID = "org.njaecha.plugins.editfangs";
        public const string Version = "1.1.0";

        internal new static ManualLogSource Logger;
        internal static EditFangs Instance;

        internal static MakerSlider fangSizeSliderL;
        internal static MakerSlider fangSizeSliderR;
        internal static MakerSlider fangSpacingSliderL;
        internal static MakerSlider fangSpacingSliderR;

        void Awake()
        {
            EditFangs.Logger = base.Logger;
            EditFangs.Instance = this;
            CharacterApi.RegisterExtraBehaviour<CharacterController>(GUID);
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
        }

        private static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            fangSizeSliderL = e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Left Fang Length", 0f, 1f, 0.1f, EditFangs.Instance));
            fangSpacingSliderL = e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Left Fang Spacing", 0f, 1.3f, 1f, EditFangs.Instance));
            fangSizeSliderR = e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Right Fang Length", 0f, 1f, 0.1f, EditFangs.Instance));
            fangSpacingSliderR = e.AddControl(new MakerSlider(MakerConstants.Face.Mouth, "Right Fang Spacing", 0f, 1.3f, 1f, EditFangs.Instance));
            fangSizeSliderL.ValueChanged.Subscribe(i => adjustFangMaker(i, fangSpacingSliderL.Value, fangSizeSliderR.Value, fangSpacingSliderR.Value));
            fangSizeSliderR.ValueChanged.Subscribe(i => adjustFangMaker(fangSizeSliderL.Value, fangSpacingSliderL.Value, i, fangSpacingSliderR.Value));
            fangSpacingSliderL.ValueChanged.Subscribe(i => adjustFangMaker(fangSizeSliderL.Value, i, fangSizeSliderR.Value, fangSpacingSliderR.Value, true));
            fangSpacingSliderR.ValueChanged.Subscribe(i => adjustFangMaker(fangSizeSliderL.Value, fangSpacingSliderL.Value, fangSizeSliderR.Value, i, true));

            Singleton<ChaCustom.CustomBase>.Instance.actUpdateCvsMouth += registerFangs;
        }

        public static void adjustFangMaker(float scaleL, float spacingL, float scaleR, float spacingR, bool readjust = false)
        {
            MakerAPI.GetCharacterControl().GetComponent<CharacterController>().adjustFang(scaleL, spacingL, scaleR, spacingR, readjust);
        }

        private static void registerFangs()
        {
            MakerAPI.GetCharacterControl().GetComponent<CharacterController>().registerFangs();
        }
    }
}
