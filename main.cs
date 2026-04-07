using System;

using Rocket.API;
using Rocket.Core.Plugins;

using interception.libraries.rocketfasterpermissions.hooks;

namespace interception.libraries.rocketfasterpermissions {
    public class config : IRocketPluginConfiguration, IDefaultable {
        public bool override_HasPermission;

        public void LoadDefaults() {
            override_HasPermission = false;
        }
    }

    public class main : RocketPlugin<config> {
        internal static main instance;
        public static config cfg;
        static RocketPermissionsManager_hk rpm_hk;

        protected override void Load() {
            instance = this;
            cfg = instance.Configuration.Instance;
            rpm_hk = new RocketPermissionsManager_hk();
            rpm_hk.init(cfg.override_HasPermission);
            permissions_manager.init();
            //GC.Collect();
        }

        protected override void Unload() {
            permissions_manager.uninit();
            rpm_hk.uninit();
            cfg = null;
            instance = null;
            //GC.Collect();
        }
    }
}
