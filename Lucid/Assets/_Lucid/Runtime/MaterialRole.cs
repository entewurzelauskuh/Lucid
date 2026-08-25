using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Which skin role paints this renderer. The builder records the role a
    /// spec asked for; a SkinSet resolves it to a material at load time, which
    /// is what lets one cube read as a bedroom or a dungeon (docs/SPEC.md §16).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialRole : MonoBehaviour
    {
        [SerializeField] string _role;

        /// <summary>A role name (wall, floor, ceiling, trim…) or an explicit path.</summary>
        public string Role
        {
            get => _role;
            set => _role = value;
        }
    }
}
