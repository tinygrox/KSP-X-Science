using ToolbarControl_NS;
using UnityEngine;
namespace ScienceChecklist
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {

        void Start()
        {
            ToolbarControl.RegisterMod(ScienceChecklistAddon.MODID, ScienceChecklistAddon.MODNAME);
            ToolbarControl.RegisterMod(ScienceChecklistAddon.MODID+"2", ScienceChecklistAddon.WINDOW_CHECKLIST);
        }
    }
}
