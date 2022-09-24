﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using ExtensibleSaveFormat;
using MessagePack;
using System.Collections;

namespace EditFangs
{
    [MessagePackObject]
    public class FangData
    {
        [Key(0)]
        public float scaleL { get; set; }
        [Key(1)]
        public float scaleR { get; set; }
        [Key(2)]
        public float spacingL { get; set; }
        [Key(3)]
        public float spacingR { get; set; }
    }
    class CharacterController : CharaCustomFunctionController
    {
        internal static ManualLogSource Logger = EditFangs.Logger;

        internal Vector3[] fangsBaseVertices = new Vector3[42];
        internal Vector3[] fangsBaseNormals = new Vector3[42];
        private bool registered = false;
        private bool loaded = false;
        public FangData fangData = new FangData();
        private int[] tips = new int[2];
        private int[] rotp = new int[2];
        private int headID = 1;

        protected override void Start()
        {
            base.Start();
            if (!loaded)
            {
                if (fangData.scaleL == 0) fangData.scaleL = 0.1f;
                if (fangData.scaleR == 0) fangData.scaleR = 0.1f;
                if (fangData.spacingL == 0) fangData.spacingL = 1f;
                if (fangData.spacingR == 0) fangData.spacingR = 1f;
            }
            registerFangs();
        }

        internal void registerFangs()
        {
            var fangs = ChaControl.transform.Find(
                "BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/cf_O_canine").gameObject;
            Mesh mesh = fangs.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            if (mesh == null) return;
            float lowestY = 100;
            List<int> tipI = new List<int>();
            if (ChaControl.fileFace.headId != headID)
            {
                headID = ChaControl.fileFace.headId;
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    fangsBaseVertices[i] = new Vector3(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z);
                    fangsBaseNormals[i] = new Vector3(mesh.normals[i].x, mesh.normals[i].y, mesh.normals[i].z);
                    if (mesh.vertices[i].y < lowestY) lowestY = mesh.vertices[i].y;
                }
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    if (mesh.vertices[i].y == lowestY) tipI.Add(i);
                    if (mesh.vertices[i] == new Vector3(-0.007559223f, 0.01637148f, 0.08396365f)) rotp[0] = i;
                    if (mesh.vertices[i] == new Vector3(0.007559223f, 0.01637148f, 0.08396365f)) rotp[1] = i;
                }
                if (tips.Length == 2)
                {
                    tips[0] = tipI[0];
                    tips[1] = tipI[1];
                }
                else return;
            }
            registered = true;
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            base.OnCoordinateBeingLoaded(coordinate);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            base.OnCoordinateBeingSaved(coordinate);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            PluginData data = new PluginData();

            data.data.Add("fangData", MessagePackSerializer.Serialize<FangData>(fangData));

            Logger.LogDebug($"Saving extended data for {ChaControl.chaFile.parameter.fullname}: ({fangData.scaleL} | {fangData.spacingL} | {fangData.scaleR} | {fangData.spacingR})");

            SetExtendedData(data);
        }

        protected override void OnReload(GameMode currentGameMode, Boolean maintainState)
        {
            var data = GetExtendedData();
            if (data == null && currentGameMode == GameMode.Maker)
            {
                EditFangs.fangSizeSliderL.SetValue(0.1f);
                EditFangs.fangSizeSliderR.SetValue(0.1f);
                EditFangs.fangSpacingSliderL.SetValue(1f);
                EditFangs.fangSpacingSliderR.SetValue(1f);
                return;
            }
            if (data == null) return;

            Logger.LogDebug($"Loading extended data for {ChaControl.chaFile.parameter.fullname}");

            // clone the loaded mesh to make sure that it's mesh is not shared with other characters
            var fangs = ChaControl.transform.Find(
                "BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/cf_O_canine").gameObject;
            fangs.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = (Mesh)Instantiate(fangs.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh);

            FangData newfangData = new FangData();

            if (data.data.TryGetValue("fangData", out var fangDataSerialised) && fangDataSerialised != null)
            {
                newfangData = MessagePackSerializer.Deserialize<FangData>((byte[])fangDataSerialised);
            }
            if (currentGameMode == GameMode.Maker)
            {
                float scaleL = newfangData.scaleL;
                float scaleR = newfangData.scaleR;
                float spacingL = newfangData.spacingL;
                float spacingR = newfangData.spacingR;
                EditFangs.fangSizeSliderL.SetValue(scaleL);
                EditFangs.fangSizeSliderR.SetValue(scaleR);
                EditFangs.fangSpacingSliderL.SetValue(spacingL);
                EditFangs.fangSpacingSliderR.SetValue(spacingR);
            }
            else
            {
                fangData.scaleL = newfangData.scaleL;
                fangData.scaleR = newfangData.scaleR;
                fangData.spacingL = newfangData.spacingL;
                fangData.spacingR = newfangData.spacingR;
            }
            loaded = true;
            StartCoroutine(adjustFang(0.5f, true));
        }


        /// <summary>
        /// Adjusts the fang's mesh according to the parameters
        /// </summary>
        /// <param name="scaleL">Length of the left fang [0 ; 1]</param>
        /// <param name="spacingL">Spacing of the left fang [0 ; 1.3]</param>
        /// <param name="scaleR">Length of the right fang [0 ; 1]</param>
        /// <param name="spacingR">Spacing of the right fang [0 ; 1.3]</param>
        /// <param name="readjust">Whenever the fang should be adjusted twice (fixes glitchy rotation)</param>
        public void adjustFang(float scaleL, float spacingL, float scaleR, float spacingR, bool readjust = false)
        {
            if (!registered) return;
            var fangs = ChaControl.transform.Find(
                "BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/cf_O_canine").gameObject;
            if (fangs == null) return;
            Mesh mesh = fangs.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            if (mesh.vertexCount != 42) return;

            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker) //dont spam the log to hard
            {
                Logger.LogDebug($"adjusting {ChaControl.chaFile.parameter.fullname}: ({scaleL} | {spacingL} | {scaleR} | {spacingR})");
            }

            var vertices = new Vector3[42];
            var normals = new Vector3[42];

            // special factors used to move and rotate the fang according to the spacing
            float spacingModL = (float)Math.Cos(spacingL * Math.PI / 2) * (float)((spacingL <= 1f) ? 1 : 2.3);
            float spacingModR = (float)Math.Cos(spacingR * Math.PI / 2) * (float)((spacingR <= 1f) ? 1 : 2.3);

            // point around which the left fang rotates
            Vector3 rotationPointL = mesh.vertices[rotp[0]] * 0.99f;
            // left rotation
            Quaternion rotateL = Quaternion.Euler(0, spacingModL * 30, 0);

            // point around which the rigth fang rotates
            Vector3 rotationPointR = mesh.vertices[rotp[1]] * 0.99f;
            // right rotation
            Quaternion rotateR = Quaternion.Euler(0, spacingModR * -30, 0);

            // calculation
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                normals[i] = new Vector3(fangsBaseNormals[i].x, fangsBaseNormals[i].y, fangsBaseNormals[i].z);
                if (i >= 21) // right fang
                {
                    // set vector
                    vertices[i] = new Vector3(fangsBaseVertices[i].x, fangsBaseVertices[i].y, fangsBaseVertices[i].z);
                    // position
                    vertices[i].x = vertices[i].x - 0.01f * (1 - spacingR);
                    vertices[i].y = vertices[i].y - 0.001f * spacingModR;
                    vertices[i].z = vertices[i].z + 0.0014f * spacingModR;
                    // rotation
                    vertices[i] = rotationPointR + rotateR * (vertices[i] - rotationPointR);
                    normals[i] = rotateR * normals[i];

                    if (i == tips[1]) // fang tip length
                    {
                        vertices[i].y = vertices[i].y + 0.01f * (0.1f - scaleR);
                        vertices[i].z = vertices[i].z + 0.002f * (0.1f - scaleR);
                    }
                }
                else // left fang
                {
                    // set vector
                    vertices[i] = new Vector3(fangsBaseVertices[i].x, fangsBaseVertices[i].y, fangsBaseVertices[i].z);
                    // postition
                    vertices[i].x = vertices[i].x + 0.01f * (1 - spacingL);
                    vertices[i].y = vertices[i].y - 0.001f * spacingModL;
                    vertices[i].z = vertices[i].z + 0.0014f * spacingModL;
                    // rotation
                    vertices[i] = rotationPointL + rotateL * (vertices[i] - rotationPointL);
                    normals[i] = rotateL * normals[i];

                    if (i == tips[0]) // fang tip length
                    {
                        vertices[i].y = vertices[i].y + 0.01f * (0.1f - scaleL);
                        vertices[i].z = vertices[i].z + 0.002f * (0.1f - scaleL);
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            fangs.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = mesh;

            // set fangData
            fangData.scaleL = scaleL;
            fangData.scaleR = scaleR;
            fangData.spacingL = spacingL;
            fangData.spacingR = spacingR;

            // readjust (because my math sucks)
            if (readjust)
                adjustFang(fangData);
        }
        /// <summary>
        /// Adjusts the fang's mesh according to the passed FangData
        /// </summary>
        /// <param name="newFangData">FangData containing the 4 parameteres</param>
        /// <param name="readjust">Whenever the fang should be adjusted twice (fixes glitchy rotation)</param>
        public void adjustFang(FangData newFangData, bool readjust = false)
        {
            adjustFang(newFangData.scaleL, newFangData.spacingL, newFangData.scaleR, newFangData.spacingR, readjust);
        }
        /// <summary>
        /// Adjusts the fang's mesh according to the current fangData
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <param name="readjust">Whenever the fang should be adjusted twice (fixes glitchy rotation)</param>
        /// <returns></returns>
        IEnumerator adjustFang(float delay, bool readjust = false)
        {
            yield return new WaitForSeconds(delay);
            adjustFang(fangData, readjust);
        }
        /// <summary>
        /// Adjusts the fang's mesh according to the current fangData
        /// </summary>
        public void adjustFang()
        {
            adjustFang(fangData);
        }
    }
}
