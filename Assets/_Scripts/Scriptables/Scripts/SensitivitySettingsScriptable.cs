using SpellFlinger.Enum;
using SpellFlinger.PlayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.Scriptables
{
    [CreateAssetMenu(fileName = "Sensitivity Settings Scriptable", menuName = "Sensitivitiy Settings Scriptable")]
    class SensitivitySettingsScriptable : ScriptableObject
    {
        private static SensitivitySettingsScriptable _instance;

        public static SensitivitySettingsScriptable Instance { get => _instance; private set => _instance = value; }

        public void Init()
        {
            if (Instance == null) _instance = this;
        }

        [SerializeField] private float _upDownSensitivity = 0f;
        [SerializeField] private float _leftRightSensitivity = 0f;
        private float _upDownMultiplyer = 1f;
        private float _leftRightMultiplyer = 1f;

        public float UpDownSensitivity => _upDownSensitivity * _upDownMultiplyer;
        public float LeftRightSensitivity => _leftRightSensitivity * _leftRightMultiplyer;

        public void SetUpDownMultiplyer(float multiplyer) => _upDownMultiplyer = multiplyer;
        public void SetLeftRightMultiplyer(float multiplyer) => _leftRightMultiplyer = multiplyer;
    }
}
