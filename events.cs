using System;

using Rocket.API;

namespace interception.libraries.rocketbetterpermissions {
    public delegate void on_before_player_added_to_group_callback(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result);
    public delegate void on_after_player_added_to_group_callback(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result);

    public delegate void on_before_player_removed_from_group_callback(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result);
    public delegate void on_after_player_removed_from_group_callback(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result);

    public delegate void on_permissions_reloaded_callback();
    
    public static class events {
        public static on_before_player_added_to_group_callback on_before_player_added_to_group_global;
        public static on_after_player_added_to_group_callback on_after_player_added_to_group_global;

        public static on_before_player_removed_from_group_callback on_before_player_removed_from_group_global;
        public static on_after_player_removed_from_group_callback on_after_player_removed_from_group_global;

        public static on_permissions_reloaded_callback on_permissions_reloaded_global;

        internal static void trigger_on_before_player_added_to_group_global(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result) {
            if (on_before_player_added_to_group_global != null)
                on_before_player_added_to_group_global(group_id, player, ref allow, ref override_result);
        }

        internal static void trigger_on_after_player_added_to_group_global(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result) {
            if (on_after_player_added_to_group_global != null)
                on_after_player_added_to_group_global(group_id, player, result);
        }

        internal static void trigger_on_before_player_removed_from_group_global(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result) {
            if (on_before_player_removed_from_group_global != null)
                on_before_player_removed_from_group_global(group_id, player, ref allow, ref override_result);
        }

        internal static void trigger_on_after_player_removed_from_group_global(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result) {
            if (on_after_player_removed_from_group_global != null)
                on_after_player_removed_from_group_global(group_id, player, result);
        }

        internal static void trigger_on_permissions_reloaded_global() {
            if (on_permissions_reloaded_global != null)
                on_permissions_reloaded_global();
        }
    }
}
