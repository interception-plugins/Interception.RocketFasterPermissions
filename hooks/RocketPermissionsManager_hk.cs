using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.Core;

using interception.hooks;
using interception.libraries.rocketfasterpermissions.types;

namespace interception.libraries.rocketfasterpermissions.hooks {
    // todo Add/Remove/SaveGroup hook
    internal class RocketPermissionsManager_hk {
        delegate RocketPermissionsProviderResult AddPlayerToGroup_def(string groupId, IRocketPlayer player);
        delegate RocketPermissionsProviderResult RemovePlayerFromGroup_def(string groupId, IRocketPlayer player);
        delegate void Reload_def();
        delegate bool HasPermission_def(IRocketPlayer player, List<string> permissions);

        bool do_HasPermission_hook;

        public RocketPermissionsProviderResult AddPlayerToGroup_hk(string groupId, IRocketPlayer player) {
            bool flag = true;
            RocketPermissionsProviderResult result = RocketPermissionsProviderResult.UnspecifiedError;
            permissions_manager.trigger_on_before_player_added_to_group_global(groupId, player, ref flag, ref result);
            if (!flag)
                return result;
            result = (RocketPermissionsProviderResult)hook_manager.call_original(R.Permissions, new object[] { groupId, player });
            permissions_manager.add_player_to_group(groupId, ulong.Parse(player.Id));
            permissions_manager.trigger_on_after_player_added_to_group_global(groupId, player, result);
            return result;
        }

        public RocketPermissionsProviderResult RemovePlayerFromGroup_hk(string groupId, IRocketPlayer player) {
            bool flag = true;
            RocketPermissionsProviderResult result = RocketPermissionsProviderResult.UnspecifiedError;
            permissions_manager.trigger_on_before_player_removed_from_group_global(groupId, player, ref flag, ref result);
            if (!flag)
                return result;
            result = (RocketPermissionsProviderResult)hook_manager.call_original(R.Permissions, new object[] { groupId, player });
            permissions_manager.remove_player_from_group(groupId, ulong.Parse(player.Id));
            permissions_manager.trigger_on_after_player_removed_from_group_global(groupId, player, result);
            return result;
        }

        public void Reload_hk() {
            hook_manager.call_original(R.Permissions, null);
            permissions_manager.reload();
            permissions_manager.trigger_on_permissions_reloaded_global();
        }

        // todo
        public bool HasPermission_hk(IRocketPlayer player, List<string> permissions) {
            if (permissions.Count == 0) return false;
            // original method returns true if player has ANY permission presented in the list
            return permissions_manager.has_permission(player, permissions[0]);
        }

        public void init(bool do_HasPermission_hook) {
            hook_manager.create_hook<AddPlayerToGroup_def>(R.Permissions.AddPlayerToGroup, AddPlayerToGroup_hk);
            hook_manager.enable_hook<AddPlayerToGroup_def>(AddPlayerToGroup_hk);

            hook_manager.create_hook<RemovePlayerFromGroup_def>(R.Permissions.RemovePlayerFromGroup, RemovePlayerFromGroup_hk);
            hook_manager.enable_hook<RemovePlayerFromGroup_def>(RemovePlayerFromGroup_hk);

            hook_manager.create_hook<Reload_def>(R.Permissions.Reload, Reload_hk);
            hook_manager.enable_hook<Reload_def>(Reload_hk);

            this.do_HasPermission_hook = do_HasPermission_hook;
            if (this.do_HasPermission_hook) {
                hook_manager.create_hook<HasPermission_def>(R.Permissions.HasPermission, HasPermission_hk);
                hook_manager.enable_hook<HasPermission_def>(HasPermission_hk);
            }
        }

        public void uninit() {
            hook_manager.remove_hook<AddPlayerToGroup_def>(AddPlayerToGroup_hk);
            hook_manager.remove_hook<RemovePlayerFromGroup_def>(RemovePlayerFromGroup_hk);
            hook_manager.remove_hook<Reload_def>(Reload_hk);
            if (do_HasPermission_hook)
                hook_manager.remove_hook<HasPermission_def>(HasPermission_hk);
        }
    }
}
