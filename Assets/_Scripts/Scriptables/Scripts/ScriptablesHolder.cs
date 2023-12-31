using SpellFlinger.Scriptables;
using UnityEngine;

namespace Scriptables
{
    class ScriptablesHolder : SingletonPersistent<ScriptablesHolder>
    {
        [SerializeField] private LevelDataScriptable _levelDataScriptable;
        [SerializeField] private WeaponDataScriptable _weaponDataScriptable;

        public LevelDataScriptable LevelDataScriptable => _levelDataScriptable;
        public WeaponDataScriptable WeaponDataScriptable => _weaponDataScriptable;

        private void Awake()
        {
            base.Awake();
            _levelDataScriptable.Init();
            _weaponDataScriptable.Init();
        }
    }
}
